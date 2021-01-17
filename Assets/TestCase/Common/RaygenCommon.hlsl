

#define INTERPOLATE_BY_BARYCENTRIC(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)

cbuffer CameraBuffer {
float4x4 _InvCameraViewProj;
float3 _WorldSpaceCameraPos;
float _CameraFarDistance;
// focus camera
float3 _FocusCameraLeftBottomCorner;
float3 _FocusCameraRight;
float3 _FocusCameraUp;
float2 _FocusCameraSize;
float _FocusCameraHalfApertureV;
};


struct RayIntersection {
	float4 color;
	uint4 PRNGStates;
	int remainingDepth;
};

struct AttributeData {
	float2 barycentrics;
};


RaytracingAccelerationStructure _AccelerationStructure;


inline float3 bgColor(float3 origin, float3 direction) {
	float t = 0.5f * (direction.y + 1.0f);
	return lerp(float3(1.0f, 1.0f, 1.0f), float3(0.5f, 0.7f, 1.0f), t);
}

inline void generateCameraRay(out float3 origin, out float3 direction) {
	// center in the middle of the pixel
	float2 xy = DispatchRaysIndex().xy + 0.5f;
	float2 posInNDC = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

	// unproject the pixel coordinate into a ray
	float4 worldPos = mul(_InvCameraViewProj, float4(posInNDC, 0, 1));
	worldPos.xyz /= worldPos.w;

	origin = _WorldSpaceCameraPos.xyz;
	direction = normalize(worldPos.xyz - origin);
}

inline void generateCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset) {
	// random offset for pixel
	float2 xy = DispatchRaysIndex().xy + offset;
	float2 posInNDC = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

	// unproject the pixel coordinate into a ray
	float4 worldPos = mul(_InvCameraViewProj, float4(posInNDC, 0, 1));
	worldPos.xyz /= worldPos.w;

	origin = _WorldSpaceCameraPos.xyz;
	direction = normalize(worldPos.xyz - origin);
}

inline void generateFocusCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset, float2 apertureOffset) {
	// random offset for pixel
	float2 xy = DispatchRaysIndex().xy + offset;
	float2 uv = xy / DispatchRaysDimensions().xy;

	float3 worldPos = _FocusCameraLeftBottomCorner + uv.x * _FocusCameraSize.x * _FocusCameraRight + uv.y * _FocusCameraSize.y * _FocusCameraUp;
	origin = _WorldSpaceCameraPos.xyz + _FocusCameraHalfApertureV * apertureOffset.x * _FocusCameraRight + _FocusCameraHalfApertureV * apertureOffset.y * _FocusCameraUp;

	direction = normalize(worldPos.xyz - origin);
}