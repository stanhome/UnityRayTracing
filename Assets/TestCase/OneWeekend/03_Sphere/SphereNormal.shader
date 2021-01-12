Shader "Unlit/SphereNormal"
{
	Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
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
            };

            struct v2f
            {
                UNITY_FOG_COORDS(0)
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
		pass {
			Name "MyRayTracing"
			Tags { "LightMode" = "RayTracing"}

			HLSLPROGRAM

			#pragma raytracing test
			#include "UnityRaytracingMeshUtils.cginc"
			#include "./../../Common/RaygenCommon.hlsl"

			//cbuffer UnityPerMaterial {
			cbuffer UnityMaterial {
				float4 _Color;
			}

			struct IntersectionVertex {
				float3 normalInModel;
			};

			void fetchIntersectionVertex(uint vertexIdx, out IntersectionVertex outVertex) {
				outVertex.normalInModel = UnityRayTracingFetchVertexAttribute3(vertexIdx, kVertexAttributeNormal);
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

				rayIntersection.color = float4(0.5f * (normalInWorld + 1.0), 0.0f);
			}

			ENDHLSL
		}
	}
}
