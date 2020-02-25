#ifndef COMMON_FUNCTIONS
#define COMMON_FUNCTIONS

float inverseLerp(float a, float b, float v) {
	return (v - a) / (b - a);
}

float2 rotate(float2 o, float r) {
	float c = cos(r);
	float s = sin(r);
	return float2(o.x * c - o.y * s, o.x * s + o.y * c);
}

float3 rgb2hsv(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}


float3 hsv2rgb(float3 c)
{
	float4 K = float4 (1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float N21(float2 p) {
	p = frac(p*float2(123.34, 456.21));
	p += dot(p, p + 45.32);
	return frac(p.x*p.y);
}

#endif