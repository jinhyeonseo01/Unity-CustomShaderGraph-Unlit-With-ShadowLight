using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;


namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Main Light")]// "Lighting", 
    class MainLightNode : CodeFunctionNode
    {
        public MainLightNode()
        {
            name = "Main Light";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("MainLight", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string MainLight(
            [Slot(0, Binding.WorldSpacePosition)] Vector3 PositionWS,
            [Slot(1, Binding.MeshUV1)] Vector2 lightmapUV,
            [Slot(2, Binding.None)] out Vector3 Direction,
            [Slot(3, Binding.None)] out Vector4 Color,
            [Slot(4, Binding.None)] out Vector1 DistanceAttenuation,
            [Slot(5, Binding.None)] out Vector1 ShadowAttenuation,
            [Slot(6, Binding.None)] out Vector1 layerMask)
        {
            Direction = default;
            Color = default;
            DistanceAttenuation = default;
            ShadowAttenuation = default;
            layerMask = default;

            return
@"
{
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    Color = $precision4(1, 1, 1, 1);
    Direction = $precision3(-0.707, 0.707, 0);
    ShadowAttenuation = 1;
    DistanceAttenuation = 1;
    layerMask = 0;

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"" // Added first
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl""

    $precision4 positionCS = TransformWorldToHClip(PositionWS);

    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        $precision4 shadowCoord = ComputeScreenPos(positionCS);
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        $precision4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
    #else
        $precision4 shadowCoord = $precision4(0, 0, 0, 0);
    #endif
   
    
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
        $precision4 shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
    #elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
        OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
        $precision4 shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
    #elif !defined (LIGHTMAP_ON)
        $precision4 shadowMask = unity_ProbesOcclusion;
    #else
        $precision4 shadowMask = $precision4(1, 1, 1, 1); // Fallback shadowmask, fully unoccluded
    #endif

    Light light = GetMainLight(shadowCoord, PositionWS, shadowMask);


    $precision2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(normalizedScreenSpaceUV, 1);
        if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
            light.color *= aoFactor.directAmbientOcclusion;
    #endif


    Color = $precision4(light.color.xyz, 1);
    Direction = light.direction;
    ShadowAttenuation = light.shadowAttenuation;
    DistanceAttenuation = light.distanceAttenuation;
    layerMask = light.layerMask;

    #endif
}
";

        }
    }


}