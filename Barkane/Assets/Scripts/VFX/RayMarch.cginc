//UNITY_SHADER_NO_UPGRADE
#ifndef RAYMARCH_INCLUDE
#define RAYMARCH_INCLUDE

// Includes referenced from generated shader
#ifdef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

const static uint steps = 4;
const static float r = 1; // unity uses [-1, 1] object space meaning radius is always 1

float densityDrop(
	float3 pos,
	float dropoff,
	float cutoff
) {
	float sq = dot(pos, pos);
	return max(0, 1 - dropoff * sq);
}

float densityAt(
	UnitySamplerState worleyState,
	UnityTexture3D worley,
	float3 pos,
	float scl,
	float offset,
	float baseDensity,
	float densityDropoff,
	float densityCutoff
) {
	float4 weights = float4(0.3, 0.3, 0.2, 0.2);
	float4 frequencies = float4(1, 1.5, 2.25, 3.375) * scl;

	// originally the 1 - sample is built into the worley generation shader
	// it is moved here to make the worley noise texture itself more usable elsewhere

	float4 samples = float4(
		1 - worley.Sample(worleyState, pos * frequencies.x + offset).r,
		1 - worley.Sample(worleyState, pos * frequencies.y + offset).g,
		1 - worley.Sample(worleyState, pos * frequencies.z + offset).b,
		1 - worley.Sample(worleyState, pos * frequencies.w + offset).a
		);

	return baseDensity * dot(weights, samples) * densityDrop(pos, densityDropoff, densityCutoff);
}

float march(
	UnitySamplerState worleyState,
	UnityTexture3D worley,
	float scl,
	float offset,
	float3 base,
	// constant, must be in sync with Monolight constant!
	// the shadergraph translates (-0.3213938, -0.7660444, 0.5566705) to object space
	float3 l,
	float sunAbsorption,
	float cloudAbsorption,
	float baseAbsorption,
	float baseDensity,
	float densityDropoff,
	float densityCutoff
) {
	// same indicator as below
	float I = dot(base, l);
	I = I * I - dot(base, base) + r * r;
	if (I < 0) return baseAbsorption;

	// length between base point and exit point of the light ray in the sphere
	float end = -dot(base, l) + sqrt(I);

	float stepSize = end / (steps - 1);

	float density = 0;

	for (uint i = 0; i < steps; i++) {
		float t = (float) i / (steps - 1);
		float3 samplePt = base + l * t;

		density += max(0, stepSize * densityAt(
			worleyState,
			worley,
			samplePt,
			scl,
			offset,
			baseDensity,
			densityDropoff,
			densityCutoff
		));
	}
	
	// transmittance formula borrowed from
	// http://killzone.dl.playstation.net/killzone/horizonzerodawn/presentations/Siggraph15_Schneider_Real-Time_Volumetric_Cloudscapes_of_Horizon_Zero_Dawn.pdf
	// on pg 54 it mentions Beer's Law
	return baseAbsorption + (1 - baseAbsorption) * exp(density * sunAbsorption);
}

// Henyey-Greenstein formula borrowed from Sebastian Lague
// to simulate scattering
float hg(float a, float g) {
	float g2 = g * g;
	return (1 - g2) / (4 * 3.1415 * pow(abs(1 + g2 - 2 * g * (a)), 1.5)); // abs added per suggestion of shader graph warnings
}

float phase(float a, float backScattering, float frontScattering) {
	return lerp(hg(a, backScattering), hg(a, -frontScattering), .5);
}

void RaySampler_float(
	float3 c2p,
	float3 c,
	UnitySamplerState worleyState,
	UnityTexture3D worley,
	float scl,
	float offset,
	float3 lightDir,

	// not really sure how the absorption constants work
	float sunAbsorption,
	float cloudAbsorption,
	float baseAbsorption,

	// HG scattering stuff
	float backScattering,
	float frontScattering,

	float baseDensity,
	float densityDropoff,
	float densityCutoff,

	out float transmittance,
	out float energy
) {
	float I = dot(c2p, c);
	I = I * I - dot(c, c) + r * r;
	float start = 0;
	float end = 0;

	if (I < 0)
		discard;
	else {
		start = -dot(c2p, c) - sqrt(I);
		end = -dot(c2p, c) + sqrt(I);
	}

	// borrows from Sebastian Lague's
	// https://github.com/SebLague/Clouds/blob/master/Assets/Scripts/Clouds/Shaders/Clouds.shader
	energy = 0;
	transmittance = 1;

	float cosAngle = dot(c2p, lightDir); // switched to hard coded light
	float phaseVal = phase(cosAngle, backScattering, frontScattering);

	const float stepSize = (end - start) / (steps - 1);

	for (uint i = 0; i < steps; i++) {
		float t = start + stepSize * i;
		float3 pos = c + t * c2p;

		float density = densityAt(
			worleyState,
			worley,
			pos,
			scl,
			offset,
			baseDensity,
			densityDropoff,
			densityCutoff
		);

		if (density > 0) {
			float lightTransmittance = march(
				worleyState,
				worley,
				scl,
				offset,
				pos,
				lightDir,
				sunAbsorption,
				cloudAbsorption,
				baseAbsorption,
				baseDensity,
				densityDropoff,
				densityCutoff
			);

			energy += density * stepSize * transmittance * lightTransmittance * phaseVal;
			transmittance *= exp(-density * stepSize * cloudAbsorption);
		}

		if (transmittance < 0.01) break; // early exit
	}
}

#endif 