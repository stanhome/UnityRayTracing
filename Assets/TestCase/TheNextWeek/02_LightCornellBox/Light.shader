Shader "RayTracing/Light"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Intensity ("Intensity", Float) = 10.0
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
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
				float _Intensity;
			}

			[shader("closesthit")]
			void closestHitShader(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes) {
				rayIntersection.color = float4(_Color.rgb * _Intensity, 1.0f);
			}

			ENDHLSL
		}
	}
}
