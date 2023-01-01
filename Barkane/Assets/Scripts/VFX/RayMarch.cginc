//UNITY_SHADER_NO_UPGRADE
#ifndef RAYMARCH_INCLUDE
#define RAYMARCH_INCLUDE

// Includes referenced from generated shader
#ifdef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

#include "./Panoramic.cginc"
#include "./noiseSimplex.cginc"

const static int bayer_n = 4;
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
const static float bayer_matrix_4x4[][bayer_n] = {
	{    -0.5,       0,  -0.375,   0.125 },
	{    0.25,   -0.25,   0.375, -0.125 },
	{ -0.3125,  0.1875, -0.4375,  0.0625 },
	{  0.4375, -0.0625,  0.3125, -0.1875 },
};

const static uint steps = 7;
const static float r = .5; // unity uses [-1, 1] object space, but unity's default sphere is .5 radius

float densityDrop(
	float3 pos,
	float dropoff
) {
	float sq = dot(pos, pos);
	float t = sq / r;
	return max(0, exp(-dropoff * t) - exp(-dropoff)); // the subtraction makes sure it is zeroed at 1
}

float densityAt(
	UnitySamplerState worleyState,
	UnityTexture3D worley,
	// float4 frequencies,
	float4 weights,
	float3 pos,
	float scl,
	float offset,
	float baseDensity,
	float densityDropoff
) {
	// originally the 1 - sample is built into the worley generation shader
	// it is moved here to make the worley noise texture itself more usable elsewhere

	// the single sample actually looks better lol
	//float4 samples = float4(
		//1 - worley.Sample(worleyState, pos * frequencies.x * scl + offset).r,
		//1 - worley.Sample(worleyState, pos * frequencies.y * scl + offset).g,
		//1 - worley.Sample(worleyState, pos * frequencies.z * scl + offset).b,
		//1 - worley.Sample(worleyState, pos * frequencies.w * scl + offset).a
		//);

	float3 pWorld = mul(unity_ObjectToWorld, float4(pos.x, pos.y, pos.z, 1));
	pWorld = pWorld * scl + offset;

	return baseDensity 
		* (weights.x + weights.y
			- weights.x * worley.Sample(worleyState, pWorld).r
			- weights.y * worley.Sample(worleyState, pWorld).g)
		// * dot(weights, samples) / (weights.x + weights.y + weights.z + weights.x)
		* densityDrop(pos, densityDropoff);
}

// Beer's law
float beer(float l) {
	return exp(-l);
}

float hg(float cosa, float g) {
	float g2 = g * g;
	return (1 - g2) / (4 * 3.1415 * pow(1 + g2 - 2 * g * cosa, 1.5));
}

// https://www.oceanopticsbook.info/view/scattering/level-2/the-henyey-greenstein-phase-function
float twoTermHG(float t, float g1, float g2, float cosa, float gBias) {
	return lerp(hg(cosa, g1), hg(cosa, -g2), t) + gBias;
}

float bayer(float2 px, float t) {
	int2 ipx = floor(px);
	return t > bayer_matrix_4x4[ipx.x % 4][ipx.y % 4] ? 1 : 0;
}

float march(
	UnitySamplerState worleyState,
	UnityTexture3D worley,
	// float4 frequencies,
	float4 weights,
	float scl,
	float offset,
	float3 p,
	// constant, must be in sync with Monolight constant!
	// the shadergraph translates (-0.3213938, -0.7660444, 0.5566705) to object space
	float3 l,
	float sunAbsorption,
	float baseDensity,
	float densityDropoff,

	out float exitLambertian
) {
	// same indicator as below
	float I = dot(l, p);
	float end = -I;

	I = I * I - dot(p, p) + r * r;
	if (I < 0) return 1;

	// length between base point and exit point of the light ray in the sphere
	end += sqrt(I);
	const float stepSize = end / steps;
	float mass = 0;

	exitLambertian = dot(l, normalize(p + l * end));

	for (uint i = 0; i < steps; i++) {
		float t = stepSize * (i + 1);
		float3 samplePt = p + l * t;

		mass += stepSize * densityAt(
			worleyState,
			worley,
			weights,
			samplePt,
			scl,
			offset,
			baseDensity,
			densityDropoff
		);
	}

	return beer(mass * sunAbsorption);
}

float3 weigh(float3 weights, float4 color) {
	return float3(weights.x * color.x, weights.y * color.y, weights.z * color.z);
}

float3 weigh(float3 weights, float3 color) {
	return float3(weights.x * color.x, weights.y * color.y, weights.z * color.z);
}

void RaySampler_float(
	float3 p,
	float3 v,
	float3 l,

	UnitySamplerState worleyState,
	UnityTexture3D worley,
	UnityTexture2D panoramic,

	float scl,
	float offset,
	float4 weights,

	float sunAbsorption,
	float cloudAbsorption,
	float g1,
	float g2,
	float gBias,
	float ltBias,

	float baseDensity,
	float densityDropoff,

	float cutoff,

	float2 px,
	float2 px01,

	out float transmittance,
	out float opacity,
	out float3 cloudColor,
	out float4 debug
) {
	v = -v;

	float I = dot(v, p);
	float end = -I;

	I = I * I - dot(p, p) + r * r;
	if (I < 0) discard;
	end += sqrt(I);

	// borrows from Sebastian Lague's
	// https://github.com/SebLague/Clouds/blob/master/Assets/Scripts/Clouds/Shaders/Clouds.shader
	float3 energy = 0;
	transmittance = 1;

	float cosa = dot(v, l);
	float phaseVal = twoTermHG(0.5, g1, g2, cosa, gBias);

	const float stepSize = end / steps;

	for (uint i = 0; i < steps; i++) {
		float t = stepSize * (i + 1);
		float3 pos = p + t * v;

		float density = densityAt(
			worleyState,
			worley,
			weights,
			pos,
			scl,
			offset,
			baseDensity,
			densityDropoff
		);

		float mass = density * stepSize;

		float exitLambertian;
		float lightTransmittance = march(
			worleyState,
			worley,

			scl,
			offset,
			weights,

			pos,
			l,

			sunAbsorption,
			baseDensity,
			densityDropoff,

			exitLambertian
		);

		energy += stepSize * density * transmittance * (lightTransmittance * (1 - ltBias) + ltBias) * phaseVal;
		transmittance *= beer(mass * cloudAbsorption);

		// if (transmittance < 0.01) break; // early exit
	}

	debug = 0;

	float3 pWorld = mul(unity_ObjectToWorld, p);

	if (transmittance > cutoff) {
		int2 ipx = floor(px);
		transmittance = bayer(px, transmittance - 0.1);
	}
	opacity = 1 - transmittance;

	// energy = max(energy, opacity * 0.01);
	
	float4 backgroundColor;
	panoramic_float(mul(unity_ObjectToWorld, v), worleyState, panoramic, backgroundColor);

	cloudColor = backgroundColor * transmittance + energy;

	// cloudColor = transmittance;
}

#endif 