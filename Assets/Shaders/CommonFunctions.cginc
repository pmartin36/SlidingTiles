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