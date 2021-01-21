
struct ONB {
	float3 u;
	float3 v;
	float3 w;
};

void buildONBFromNormal(inout ONB uvw, float3 n) {
	uvw.w = normalize(n);
	float3 a;
	if (abs(uvw.w.x) > 0.9f) {
		a = float3(0, 1, 0);
	} else {
		a = float3(1, 0, 0);
	}

	uvw.v = cross(uvw.w, a);
	uvw.u = cross(uvw.w, uvw.v);
}

float3 localONB(inout ONB uvw, float3 a) {
	return a.x * uvw.u + a.y * uvw.v + a.z * uvw.w;
}


