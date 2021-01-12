#define M_PI (3.14159265358979323846264338327950288)

RWStructuredBuffer<uint4> _PRNGStates;

inline float cbrt(float d)
{
	return pow(d, 1.0f / 3.0f);
}

uint tausStep(inout uint z, int S1, int S2, int S3, uint M)
{
	uint b = (((z << S1) ^ z) >> S2);
	return z = (((z & M) << S3) ^ b);
}

uint LCGStep(inout uint z)
{
	return z = (1664525 * z + 1013904223);
}

float getRandomValueTauswortheUniform(inout uint4 states)
{
	uint taus = tausStep(states.x, 13, 19, 12, 4294967294UL) ^ tausStep(states.y, 2, 25, 4, 4294967288UL) ^ tausStep(states.z, 3, 11, 17, 4294967280UL);
	uint lcg = LCGStep(states.w);

	return 2.3283064365387e-10f * (taus ^ lcg); // taus+
}

float getRandomValue(inout uint4 states)
{
	float rand = getRandomValueTauswortheUniform(states);
	return rand;
}

float3 getRandomInUnitSphere(inout uint4 states)
{
	float u = getRandomValue(states);
	float v = getRandomValue(states);
	float theta = u * 2.f * (float)M_PI;
	float phi = acos(2.f * v - 1.f);
	float r = cbrt(getRandomValue(states));
	float sinTheta = sin(theta);
	float cosTheta = cos(theta);
	float sinPhi = sin(phi);
	float cosPhi = cos(phi);
	float x = r * sinPhi * cosTheta;
	float y = r * sinPhi * sinTheta;
	float z = r * cosPhi;
	return float3(x, y, z);
}

float3 getRandomOnUnitSphere(inout uint4 states)
{
	float r1 = getRandomValue(states);
	float r2 = getRandomValue(states);
	float x = cos(2.0f * (float)M_PI * r1) * 2.0f * sqrt(r2 * (1.0f - r2));
	float y = sin(2.0f * (float)M_PI * r1) * 2.0f * sqrt(r2 * (1.0f - r2));
	float z = 1.0f - 2.0f * r2;
	return float3(x, y, z);
}

float2 getRandomInUnitDisk(inout uint4 states) {
	float a = getRandomValue(states) * 2.0f * (float)M_PI;
	float r = sqrt(getRandomValue(states));

	return float2(r * cos(a), r * sin(a));
}

float3 getRandomCosineDirection(inout uint4 states) {
	float r1 = getRandomValue(states);
	float r2 = getRandomValue(states);
	float z = sqrt(1.0f - r2);
	float phi = 2.0f * M_PI * r1;
	float x = cos(phi) * sqrt(r2);
	float y = sin(phi) * sqrt(r2);
	return float3(x, y, z);
}
