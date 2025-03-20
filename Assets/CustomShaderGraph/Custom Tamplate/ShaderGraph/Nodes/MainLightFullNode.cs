using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;
using static Codice.CM.Common.Purge.PurgeReport;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Main Light Full")]// "Lighting", 
    class MainLightFullNode : CodeFunctionNode
    {
        public MainLightFullNode()
        {
            name = "Main Light Full";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("MainLightFull", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string MainLightFull(
            [Slot(0, Binding.WorldSpacePosition)] Vector3 PositionWS,
            [Slot(1, Binding.ScreenPosition)] Vector4 screenUV,
            [Slot(2, Binding.None)] Vector1 occlusion,
            [Slot(3, Binding.None)] out Vector3 Direction,
            [Slot(4, Binding.None)] out Vector4 Color,
            [Slot(5, Binding.None)] out Vector1 DistanceAttenuation,
            [Slot(6, Binding.None)] out Vector1 ShadowAttenuation,
            [Slot(7, Binding.None)] out Vector1 DirectAO,
            [Slot(8, Binding.None)] out Vector1 IndirectAO)
        {
            Direction = default;
            Color = default;
            DistanceAttenuation = default;
            ShadowAttenuation = default;
            DirectAO = default;
            IndirectAO = default;

            return
@"
{
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    Color = $precision4(1, 1, 1, 1);
    Direction = $precision3(-0.707, 0.707, 0);
    ShadowAttenuation = 1;
    DistanceAttenuation = 1;
    DirectAO = 1;
    IndirectAO = 1;

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
   
    $precision2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
    #if !defined (LIGHTMAP_ON)
        $precision4 shadowMask = unity_ProbesOcclusion; // legacy probes의 그림자 마스크(차폐) 샘플링
    #else
        $precision4 shadowMask = $precision4(1, 1, 1, 1); // fallback, 전부 차폐되지 않음
    #endif

    Light light = GetMainLight(shadowCoord, PositionWS, shadowMask);


    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)

        AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(normalizedScreenSpaceUV, occlusion);

        if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
        {
            light.color *= aoFactor.directAmbientOcclusion;
        }

        AmbientOcclusionFactor aoFactor2 = GetScreenSpaceAmbientOcclusion(screenUV);
        DirectAO = aoFactor2.indirectAmbientOcclusion;
        IndirectAO = aoFactor2.directAmbientOcclusion;
    #endif


    Color = $precision4(light.color.xyz, 1);
    Direction = light.direction;
    ShadowAttenuation = light.shadowAttenuation;
    DistanceAttenuation = light.distanceAttenuation;

    #endif
}
";

        }
    }


}