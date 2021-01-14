Shader "RayTracing/DielectricObj"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
		// refraction index
		_RefIdx ("Refraction Index", float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 normal : NORMAL;
            };

            struct v2f
            {
				float3 normal : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			half4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

	SubShader {
		Pass {
			Name "MyRayTracing"
			Tags { "LightMode" = "RayTracing" }

			HLSLPROGRAM

			#pragma raytracing test
			
			#include "UnityRaytracingMeshUtils.cginc"
			#include "./../../Common/RaygenCommon.hlsl"
			#include "./../../Common/PRNG.hlsl"

			cbuffer UnityMaterial {
				float4 _Color;
				float _RefIdx;
			}

			struct IntersectionVertex {
				float3 normalInModel;
			};

			void fetchIntersectionVertex(uint vertexIdx, out IntersectionVertex outVertex) {
				outVertex.normalInModel = UnityRayTracingFetchVertexAttribute3(vertexIdx, kVertexAttributeNormal);
			}

			inline float schlick(float cosine, float refIdx) {
				float r0 = (1 - refIdx) / (1 + refIdx);
				r0 = r0 * r0;
	
				return r0 + (1 - r0) * pow((1 - cosine), 5);
			}

			inline bool refractFunc(float3 v, float3 n, float niOverNt, out float3 refracted) {
				float3 vNormalized = normalize(v);
				float dt = dot(vNormalized, n);

				//by Snell's Law, n sin(theta) = n sin(theta)
				float discriminant = 1.0 - niOverNt * niOverNt * (1 - dt * dt);
				if (discriminant > 0) {
					// refraction
					refracted = niOverNt * (v + abs(dt) * n) - n * sqrt(discriminant);
					return true;
				} else {
					// reflection
					return false;
				}
			}

			[shader("closesthit")]
			void closestHitShader(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes) {
				// fetch the indices of the current triangle
				uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

				// fetch the vertices
				IntersectionVertex v0, v1, v2;
				fetchIntersectionVertex(triangleIndices.x, v0);
				fetchIntersectionVertex(triangleIndices.y, v1);
				fetchIntersectionVertex(triangleIndices.z, v2);

				// compute the full barycentric coordinates
				float3 barycentricCoordinates = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);

				float3 normalInModel = INTERPOLATE_BY_BARYCENTRIC(v0.normalInModel, v1.normalInModel, v2.normalInModel, barycentricCoordinates);
				float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
				float3 normalInWorld = normalize(mul(objectToWorld, normalInModel));

				float4 outColor = float4(0, 0, 0, 1);
				if (rayIntersection.remainingDepth > 0) {
					// get pos in world space
					float3 origin = WorldRayOrigin();
					float3 direction = WorldRayDirection();
					float t = RayTCurrent();
					float3 posInWorld = origin + direction * t;

					// handle scatter (reflection & refraction)
					float3 outwardNormal;
					float niOverNt;
					float3 refracted;
					float reflectProb;
					float cosine;

					if (dot(direction, normalInWorld) > 0) {
						//the ray from water to sky.
						outwardNormal = -normalInWorld;
						niOverNt = _RefIdx;
						cosine = dot(direction, normalInWorld);
					} else {
						//the ray from sky to water
						outwardNormal = normalInModel;
						niOverNt = 1.0 / _RefIdx;
						cosine = -dot(direction, normalInWorld);
					}

					if (refractFunc(direction, outwardNormal, niOverNt, refracted)) {
						reflectProb = schlick(cosine, _RefIdx);
					} else {
						// the full reflection
						reflectProb = 1.0f;
					}

					float3 scatteredDir;
					if (getRandomValue(rayIntersection.PRNGStates) < reflectProb) {
						scatteredDir = reflect(direction, normalInWorld);
					} else {
						scatteredDir = refracted;
					}

					// create reflection ray
					RayDesc rayDescriptor;
					rayDescriptor.Origin = posInWorld + 1e-5f * scatteredDir;
					// direction = normalize(pos + N + randomOnSphere - pos)
					rayDescriptor.Direction = scatteredDir;
					rayDescriptor.TMin = 1e-5f;
					rayDescriptor.TMax = _CameraFarDistance;

					// tracing reflection
					RayIntersection reflectionRayIntersection;
					reflectionRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
					reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
					reflectionRayIntersection.color = float4(0, 0, 0, 0);

					TraceRay(_AccelerationStructure, RAY_FLAG_NONE, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);

					rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
					outColor = reflectionRayIntersection.color;
				}

				rayIntersection.color = _Color * outColor;
			}

			ENDHLSL
		}
	}
}
