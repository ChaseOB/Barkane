//UNITY_SHADER_NO_UPGRADE
#ifndef RAYMARCH_INCLUDE
#define RAYMARCH_INCLUDE

// Includes referenced from generated shader
#ifdef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

const static uint steps = 4;
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
	float4 frequencies,
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

	return baseDensity 
		* (1 - dot(weights, worley.Sample(worleyState, pos * scl + offset)) / (weights.x + weights.y + weights.z + weights.x))
		// * dot(weights, samples) / (weights.x + weights.y + weights.z + weights.x)
		* densityDrop(pos, densityDropoff);
}

// from http://killzone.dl.playstation.net/killzone/horizonzerodawn/presentations/Siggraph15_Schneider_Real-Time_Volumetric_Cloudscapes_of_Horizon_Zero_Dawn.pdf
float powder(float l) {
	return max(0, 1 - exp(-2 * l));
}

// Beer's law
float beer(float l) {
	return exp(-l);
}

float combinedAbsorption(float l) {
	return lerp(powder(l), beer(l), 0.6);
}

float march(
	UnitySamplerState worleyState,
	UnityTexture3D worley,
	float4 frequencies,
	float4 weights,
	float scl,
	float offset,
	float3 p,
	// constant, must be in sync with Monolight constant!
	// the shadergraph translates (-0.3213938, -0.7660444, 0.5566705) to object space
	float3 l,
	float sunAbsorption,
	float baseDensity,
	float densityDropoff
) {
	// same indicator as below
	float I = dot(l, p);
	float end = -I;

	I = I * I - dot(p, p) + r * r;

	// length between base point and exit point of the light ray in the sphere
	end += sqrt(I);
	float stepSize = end / steps;
	float density = 0;
	float3 samplePt = p;

	for (uint i = 0; i < steps; i++) {
		float t = stepSize * (i + 1);
		samplePt = p + l * t;

		density += max(0, stepSize * densityAt(
			worleyState,
			worley,
			frequencies,
			weights,
			samplePt,
			scl,
			offset,
			baseDensity,
			densityDropoff
		));
	}

	// transmittance formula borrowed from
	// http://killzone.dl.playstation.net/killzone/horizonzerodawn/presentations/Siggraph15_Schneider_Real-Time_Volumetric_Cloudscapes_of_Horizon_Zero_Dawn.pdf
	// on pg 54 it mentions Beer's Law
	float effectiveLength = density * sunAbsorption;
	return combinedAbsorption(effectiveLength);
}

// Henyey-Greenstein formula borrowed from Sebastian Lague
// to simulate scattering
float hg(float cosa, float g) {
	float g2 = g * g;
	return (1 - g2) / (4 * 3.1415 * pow(1 + g2 - 2 * g * cosa, 1.5));
}

void RaySampler_float(
	float3 p,
	float3 v,
	float3 l,

	UnitySamplerState worleyState,
	UnityTexture3D worley,
	float scl,
	float offset,
	float4 frequencies,
	float4 weights,

	float sunAbsorption,
	float cloudAbsorption,

	float baseDensity,
	float densityDropoff,

	// phong-ish
	float ambient,
	float lambertian,
	float HG,

	float cutoff,
	float hardborder,

	out float transmittance,
	out float energy,
	out float border,
	out float4 debug
) {
	float I = dot(v, p);
	float end = -I;

	I = I * I - dot(p, p) + r * r;
	end += sqrt(I);

	// borrows from Sebastian Lague's
	// https://github.com/SebLague/Clouds/blob/master/Assets/Scripts/Clouds/Shaders/Clouds.shader
	energy = 0;
	transmittance = 1;

	float cosAngle = dot(v, l); // switched to hard coded light
	float phaseVal = hg(cosAngle, .9);

	const float stepSize = end / steps;

	float lambertianK = saturate(dot(-l, v));

	for (uint i = 0; i < steps; i++) {
		float t = stepSize * (i + 1);
		float3 pos = p + t * v;

		float density = densityAt(
			worleyState,
			worley,
			frequencies,
			weights,
			pos,
			scl,
			offset,
			baseDensity,
			densityDropoff
		);

		float lightTransmittance = march(
			worleyState,
			worley,
			scl,
			offset,
			frequencies,
			weights,
			pos,
			l,
			sunAbsorption,
			baseDensity,
			densityDropoff
		);

		energy += 
			density * stepSize * transmittance 
			* lightTransmittance * 
			(ambient * combinedAbsorption(t) + lambertianK * lambertian + HG * phaseVal);

		transmittance *= exp(-density * stepSize * cloudAbsorption);

		if (transmittance < 0.01) break; // early exit
	}

	debug = 0;
	border = 0;

	if (transmittance > cutoff) {
		transmittance = 1;
		energy = 0;
	}
	else if (cutoff - transmittance < hardborder) {
		transmittance = 0;
		border = 1;
	}
}

#endif 