
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Unlit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"

void InitializeInputData(Varyings input,SurfaceDescription surfaceDescription, out InputData inputData)
{
    /*
    inputData = (InputData)0;

    // InputData is only used for DebugDisplay purposes in Unlit, so these are not initialized.
    #if defined(DEBUG_DISPLAY)
    inputData.positionWS = input.positionWS;
    inputData.positionCS = input.positionCS;
    inputData.normalWS = input.normalWS;
    #else
    inputData.positionWS = half3(0, 0, 0);
    inputData.normalWS = half3(0, 0, 1);
    inputData.viewDirectionWS = half3(0, 0, 1);
    #endif
    inputData.shadowCoord = 0;
    //inputData.fogCoord = 0;
    //inputData.vertexLighting = half3(0, 0, 0);
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = half3(0, 0, 0);
    //inputData.normalizedScreenSpaceUV = 0;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = half4(1, 1, 1, 1);
    */

    inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

    #ifdef _NORMALMAP
        // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
        float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        float3 bitangent = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

        inputData.tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        #if _NORMAL_DROPOFF_TS
            inputData.normalWS = TransformTangentToWorld(surfaceDescription.NormalTS, inputData.tangentToWorld);
        #elif _NORMAL_DROPOFF_OS
            inputData.normalWS = TransformObjectToWorldNormal(surfaceDescription.NormalOS);
        #elif _NORMAL_DROPOFF_WS
            inputData.normalWS = surfaceDescription.NormalWS;
        #endif
    #else
        inputData.normalWS = input.normalWS;
    #endif
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.sh;
    #endif

    #if defined(USE_APV_PROBE_OCCLUSION)
    inputData.probeOcclusion = input.probeOcclusion;
    #endif

    inputData.positionCS = input.positionCS;
    #endif
}

void InitializeBakedGIData(Varyings input, inout InputData inputData)
{
#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV.xy, input.sh, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(input.sh,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        input.positionCS.xy,
        input.probeOcclusion,
        inputData.shadowMask);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.sh, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#endif
}


PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    output = BuildVaryings(input);
    PackedVaryings packedOutput = PackVaryings(output);
    return packedOutput;
}

FragmentOutput frag(PackedVaryings packedInput)
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.alpha = 1;

#if defined(_ALPHATEST_ON)
    surfaceData.alpha = AlphaDiscard(surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold);
#endif

#if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
    LODFadeCrossFade(unpacked.positionCS);
#endif

#if defined(_ALPHAMODULATE_ON)
    surfaceData.albedo = AlphaModulate(surfaceDescription.BaseColor, surfaceData.alpha);
#else
    surfaceData.albedo = surfaceDescription.BaseColor;
#endif

#if defined(_DBUFFER)
    ApplyDecalToBaseColor(unpacked.positionCS, surfaceData.albedo);
#endif

    InputData inputData;
    InitializeInputData(unpacked, surfaceDescription, inputData);
    InitializeBakedGIData(unpacked, inputData);

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(unpacked.positionCS);
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
        surfaceData.albedo.rgb *= aoFactor.directAmbientOcclusion;
    #else
        surfaceData.occlusion = 1;
    #endif

    surfaceData.albedo = MixFog(surfaceData.albedo, inputData.fogCoord);
    
    //surfaceData.alpha = OutputAlpha(surfaceData.alpha, isTransparent);


    return SurfaceDataToGbuffer(surfaceData, inputData, float3(0,0,0), kLightingInvalid);
}
