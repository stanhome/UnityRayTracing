Shader "Unlit/NewUnlitShader"
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
			#include "./../../Common/RaygenCommon.hlsl"

			//cbuffer UnityPerMaterial {
			cbuffer UnityMaterial {
				float4 _Color;
			}

			[shader("closesthit")]
			void closestHitShader(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes) {
				rayIntersection.color = _Color;
			}


			ENDHLSL
		}
	}

}
