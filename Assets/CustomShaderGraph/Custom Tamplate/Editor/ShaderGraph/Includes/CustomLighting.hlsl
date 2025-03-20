#ifndef __INCLUDE_CUSTOM_LIGHTING
#define __INCLUDE_CUSTOM_LIGHTING


#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
	#if (SHADERPASS != SHADERPASS_FORWARD)
		#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	#endif
#endif

// half4 CalculateShadowMask(InputData inputData)
// {
//     // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
//     #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
//     half4 shadowMask = inputData.shadowMask; // Shadowmask was sampled from lightmap
//     #elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
//     half4 shadowMask = inputData.shadowMask; // Shadowmask (probe occlusion) was sampled from APV
//     #elif !defined (LIGHTMAP_ON)
//     half4 shadowMask = unity_ProbesOcclusion; // Sample shadowmask (probe occlusion) from legacy probes
//     #else
//     half4 shadowMask = half4(1, 1, 1, 1); // Fallback shadowmask, fully unoccluded
//     #endif

//     return shadowMask;
// }

// half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData)
// {
//     inputData = (InputData)0;
//     inputData.shadowMask = half4(1, 1, 1, 1);
//     inputData.positionWS = input.positionWS;
   
//     #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
//         inputData.shadowCoord = input.shadowCoord;
//     #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
//         inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
//     #else
//         inputData.shadowCoord = float4(0, 0, 0, 0);
//     #endif
   
//     inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
   
//     #if defined(DEBUG_DISPLAY)
//     #if defined(DYNAMICLIGHTMAP_ON)
//     inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
//     #endif
//     #if defined(LIGHTMAP_ON)
//     inputData.staticLightmapUV = input.staticLightmapUV;
//     #else
//     inputData.vertexSH = input.sh;
//     #endif
   
//     #if defined(USE_APV_PROBE_OCCLUSION)
//     inputData.probeOcclusion = input.probeOcclusion;
//     #endif
   
//     inputData.positionCS = input.positionCS;
//     #endif

//     // SurfaceData surface;
//     // surface.albedo              = surfaceDescription.BaseColor;
//     // surface.metallic            = saturate(metallic);
//     // surface.specular            = specular;
//     // surface.smoothness          = saturate(surfaceDescription.Smoothness),
//     // surface.occlusion           = surfaceDescription.Occlusion,
//     // surface.emission            = surfaceDescription.Emission,
//     // surface.alpha               = saturate(alpha);
//     // surface.normalTS            = normalTS;
//     // surface.clearCoatMask       = 0;
//     // surface.clearCoatSmoothness = 1;

//     // #ifdef _CLEARCOAT
//     //     surface.clearCoatMask       = saturate(surfaceDescription.CoatMask);
//     //     surface.clearCoatSmoothness = saturate(surfaceDescription.CoatSmoothness);
//     // #endif

//     // surface.albedo = AlphaModulate(surface.albedo, surface.alpha);


//     half4 shadowMask = CalculateShadowMask(inputData);
//     Light light = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

//     AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
//     #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
//     if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
//     {
//         light.color *= aoFactor.directAmbientOcclusion;
//     }
//     #endif
// }


void MainLight_float(float3 positionWS, float occlusion, out float3 direction, out float4 color, out float shadowAtten, out float distanceAtten)
{
	#ifdef SHADERGRAPH_PREVIEW
        direction = normalize(float3(-1, 1, 0));
        color = float4(1, 1, 1, 1);
        shadowAtten = 1;
        distanceAtten = 1;
	#else


    float4 positionCS = TransformWorldToHClip(positionWS);

    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        float4 shadowCoord = ComputeScreenPos(positionCS);
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    #else
        float4 shadowCoord = float4(0, 0, 0, 0);
    #endif
   
    float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
   
    // #if defined(DEBUG_DISPLAY)
    // #if defined(DYNAMICLIGHTMAP_ON)
    // inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    // #endif
    // #if defined(LIGHTMAP_ON)
    // inputData.staticLightmapUV = input.staticLightmapUV;
    // #else
    // inputData.vertexSH = input.sh;
    // #endif
   
    //     #if defined(USE_APV_PROBE_OCCLUSION)
    //     inputData.probeOcclusion = input.probeOcclusion;
    //     #endif
    
    //     inputData.positionCS = input.positionCS;
    //     #endif
    // }

    //Culculeate Shadow Mask
    #if !defined (LIGHTMAP_ON)
    float4 shadowMask = unity_ProbesOcclusion; // Sample shadowmask (probe occlusion) from legacy probes
    #else
    float4 shadowMask = half4(1, 1, 1, 1); // Fallback shadowmask, fully unoccluded
    #endif

    Light light = GetMainLight(shadowCoord, positionWS, shadowMask);

    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(normalizedScreenSpaceUV, occlusion);
    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
    #endif

    color = float4(light.color.xyz, 1);
    direction = light.direction;
    shadowAtten = light.shadowAttenuation;
    distanceAtten = light.distanceAttenuation;

    #endif
}



void MainLight_half(half3 positionWS, half occlusion, out half3 direction, out half4 color, out half shadowAtten, out half distanceAtten)
{
	#ifdef SHADERGRAPH_PREVIEW
        direction = normalize(half3(-1, 1, 0));
        color = half4(1, 1, 1, 1);
        shadowAtten = 1;
        distanceAtten = 1;
	#else


    half4 positionCS = TransformWorldToHClip(positionWS);

    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        half4 shadowCoord = ComputeScreenPos(positionCS);
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        half4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    #else
        half4 shadowCoord = half4(0, 0, 0, 0);
    #endif
   
    half2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
   
    // #if defined(DEBUG_DISPLAY)
    // #if defined(DYNAMICLIGHTMAP_ON)
    // inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    // #endif
    // #if defined(LIGHTMAP_ON)
    // inputData.staticLightmapUV = input.staticLightmapUV;
    // #else
    // inputData.vertexSH = input.sh;
    // #endif
   
    //     #if defined(USE_APV_PROBE_OCCLUSION)
    //     inputData.probeOcclusion = input.probeOcclusion;
    //     #endif
    
    //     inputData.positionCS = input.positionCS;
    //     #endif
    // }

    //Culculeate Shadow Mask
    #if !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion; // Sample shadowmask (probe occlusion) from legacy probes
    #else
    half4 shadowMask = half4(1, 1, 1, 1); // Fallback shadowmask, fully unoccluded
    #endif

    Light light = GetMainLight(shadowCoord, positionWS, shadowMask);

    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(normalizedScreenSpaceUV, occlusion);
    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
    #endif

    color = half4(light.color.xyz, 1);
    direction = light.direction;
    shadowAtten = light.shadowAttenuation;
    distanceAtten = light.distanceAttenuation;

    #endif
}

#endif // __INCLUDE_CUSTOM_LIGHTING