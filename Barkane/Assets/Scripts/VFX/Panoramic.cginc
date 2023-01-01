//UNITY_SHADER_NO_UPGRADE
#ifndef PANORAMIC_INCLUDE
#define PANORAMIC_INCLUDE

// Includes referenced from generated shader
#ifdef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

//// Builtin-shader Skybox-Panoramic.shader

const static float pi = 3.1415927;

inline float2 ToRadialCoords(float3 coords)
{
    float3 normalizedCoords = normalize(coords);
    float latitude = acos(normalizedCoords.y);
    float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
    float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / pi, 1.0 / pi);
    return float2(0.5, 1.0) - sphereCoords;
}

float3 RotateAroundYInDegrees(float3 vertex, float degrees)
{
    float alpha = degrees * pi / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return float3(mul(m, vertex.xz), vertex.y).xzy;
}
////

//// migrated from vert and frag shaders for built-in panoramic skybox
void
panoramic_float
(
	float3 v,
	UnitySamplerState panoState,
	UnityTexture2D pano,
	out float4 c
) {

    // Calculate constant horizontal scale and cutoff for 180 (vs 360) image type
    // if (_ImageType == 0)  // 360 degree
    float2 image180ScaleAndCutoff = float2(1.0, 1.0);
    // else  // 180 degree
    //    o.image180ScaleAndCutoff = float2(2.0, _MirrorOnBack ? 1.0 : 0.5);
    // Calculate constant scale and offset for 3D layouts
    // if (_Layout == 0) // No 3D layout
    float4    layout3DScaleAndOffset = float4(0, 0, 1, 1);
    //else if (_Layout == 1) // Side-by-Side 3D layout
    //    o.layout3DScaleAndOffset = float4(unity_StereoEyeIndex, 0, 0.5, 1);
    //else // Over-Under 3D layout
    //    o.layout3DScaleAndOffset = float4(0, 1 - unity_StereoEyeIndex, 1, 0.5);

    float2 tc = ToRadialCoords(v);
    if (tc.x > image180ScaleAndCutoff[1]) {
        c = float4(0, 0, 0, 1);
        return;
    }
    tc.x = fmod(tc.x * image180ScaleAndCutoff[0], 1);
    tc = (tc + layout3DScaleAndOffset.xy) * layout3DScaleAndOffset.zw;

    float4 tex = tex2D(pano, tc);
    // float3 c_ = tex; // DecodeHDR(tex, pano);
    // c_ = c_ * _Tint.rgb * unity_ColorSpaceDouble.rgb;
    // c_ *= _Exposure;

    // c = float4(c_, 1);
    c = tex;
}

#endif