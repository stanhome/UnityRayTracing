﻿Shader "RayTracing/DiffuseObj"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
		_Tex("Main Texture", 2D) = "white" {}
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
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float3 normal : TEXCOORD0;
				float2 uv : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			half4 _Color;
			sampler2D _Tex;
			float4 _Tex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Tex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
				col *= tex2D(_Tex, i.uv);
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
			#include "./../../Common/ONB.hlsl"

			cbuffer UnityMaterial {
				float4 _Color;
			}

			Texture2D _Tex;
			SamplerState sampler_Tex;

			struct IntersectionVertex {
				float3 normalInModel;
				float2 uv;
			};

			void fetchIntersectionVertex(uint vertexIdx, out IntersectionVertex outVertex) {
				outVertex.normalInModel = UnityRayTracingFetchVertexAttribute3(vertexIdx, kVertexAttributeNormal);
				outVertex.uv = UnityRayTracingFetchVertexAttribute2(vertexIdx, kVertexAttributeTexCoord0);
			}

			float scatteringPDF(float3 inOrigin, float3 inDirection, float3 hitNormal, float3 scatteredDir) {
				float cosVal = dot(hitNormal, scatteredDir);
				return max(0.0f, cosVal / M_PI);
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

				float2 uv = INTERPOLATE_BY_BARYCENTRIC(v0.uv, v1.uv, v2.uv, barycentricCoordinates);
				float4 texColor = _Color * _Tex.SampleLevel(sampler_Tex, uv, 0);

				float4 outColor = float4(0, 0, 0, 1);
				if (rayIntersection.remainingDepth > 0) {
					// get pos in world space
					float3 origin = WorldRayOrigin();
					float3 direction = WorldRayDirection();
					float t = RayTCurrent();
					float3 posInWorld = origin + direction * t;

					ONB uvw;
					buildONBFromNormal(uvw, normalInWorld);

					// create reflection ray
					RayDesc rayDescriptor;
					rayDescriptor.Origin = posInWorld + 0.001f * normalInWorld;
					// direction = normalize(pos + N + randomOnSphere - pos)
					rayDescriptor.Direction = localONB(uvw, getRandomCosineDirection(rayIntersection.PRNGStates));
					rayDescriptor.TMin = 1e-5f;
					rayDescriptor.TMax = _CameraFarDistance;

					// tracing reflection
					RayIntersection reflectionRayIntersection;
					reflectionRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
					reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
					reflectionRayIntersection.color = float4(0, 0, 0, 0);

					float pdf = dot(normalInWorld, rayDescriptor.Direction) / M_PI;

					TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);

					rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
					outColor = scatteringPDF(origin, direction, normalInWorld, rayDescriptor.Direction) * reflectionRayIntersection.color / pdf;
				}

				rayIntersection.color = texColor * 0.5f * outColor;
			}

			ENDHLSL
		}
	}
}
