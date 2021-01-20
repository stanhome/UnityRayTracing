Shader "RayTracing/Isotropic"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Density ("Density", Float) = 0.01
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
				float _Density;
			}

			[shader("closesthit")]
			void closestHitShader(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes) {
				if (rayIntersection.remainingDepth < 0) {
					// HACK: this is a inner ray to test the outer boundary.
					rayIntersection.innerRayHitT = RayTCurrent();
					return;
				}

				float t1 = RayTCurrent();
				RayDesc rayDescriptor;
				rayDescriptor.Origin = WorldRayOrigin();
				rayDescriptor.Direction = WorldRayDirection();
				rayDescriptor.TMin = t1 + 1e-5f;
				rayDescriptor.TMax = _CameraFarDistance;

				RayIntersection innerRayIntersection;
				innerRayIntersection.remainingDepth = -1;
				innerRayIntersection.PRNGStates = rayIntersection.PRNGStates;
				innerRayIntersection.color = float4(0, 0, 0, 0);
				innerRayIntersection.innerRayHitT = 0;
				// trace inner ray
				TraceRay(_AccelerationStructure, RAY_FLAG_CULL_FRONT_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, innerRayIntersection);

				float t2 = innerRayIntersection.innerRayHitT;
				float distanceInsideBoundary = t2 - t1;
				float hitDistance = -(1.0f / _Density) * log(getRandomValue(rayIntersection.PRNGStates));

				if (hitDistance < distanceInsideBoundary) {
					const float t = t1 + hitDistance;
					rayDescriptor.Origin = rayDescriptor.Origin + t * rayDescriptor.Direction;
					rayDescriptor.Direction = getRandomOnUnitSphere(rayIntersection.PRNGStates);
					rayDescriptor.TMin = 1e-5f;
					rayDescriptor.TMax = _CameraFarDistance;

					RayIntersection scatteredRayIntersection;
					scatteredRayIntersection.remainingDepth = rayIntersection.remainingDepth -1;
					scatteredRayIntersection.PRNGStates = rayIntersection.PRNGStates;
					scatteredRayIntersection.color = float4(0, 0, 0, 0);

					TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, scatteredRayIntersection);

					rayIntersection.PRNGStates = scatteredRayIntersection.PRNGStates;
					rayIntersection.color = _Color * scatteredRayIntersection.color;
				} else {
					const float t = t2 + 1e-5f;
					rayDescriptor.Origin = rayDescriptor.Origin + t * rayDescriptor.Direction;
					rayDescriptor.Direction = rayDescriptor.Direction;
					rayDescriptor.TMin = 1e-5f;
					rayDescriptor.TMax = _CameraFarDistance;

					RayIntersection continueRayIntersection;
					continueRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
					continueRayIntersection.PRNGStates = rayIntersection.PRNGStates;
					continueRayIntersection.color = float4(0, 0, 0, 0);
					
					TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, continueRayIntersection);

					rayIntersection.PRNGStates = continueRayIntersection.PRNGStates;
					rayIntersection.color = continueRayIntersection.color;
				}
			}

			ENDHLSL
		}
	}
}
