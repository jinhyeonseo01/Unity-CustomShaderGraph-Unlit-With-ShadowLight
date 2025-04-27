using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;


namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Main Light Direct")]// "Lighting", 
    class MainLightDirectNode : CodeFunctionNode
    {
        public MainLightDirectNode()
        {
            name = "Main Light Direct";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("MainLightDirect", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string MainLightDirect(
            [Slot(0, Binding.WorldSpacePosition)] Vector3 PositionWS,
            [Slot(1, Binding.None)] Vector4 shadowCoord,
            [Slot(2, Binding.None)] Vector4 shadowMask,
            [Slot(3, Binding.None)] out Vector3 Direction,
            [Slot(4, Binding.None)] out Vector4 Color,
            [Slot(5, Binding.None)] out Vector1 DistanceAttenuation,
            [Slot(6, Binding.None)] out Vector1 ShadowAttenuation,
            [Slot(7, Binding.None)] out Vector1 layerMask)
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


    Light light = GetMainLight(shadowCoord, PositionWS, shadowMask);


    $precision2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(TransformWorldToHClip(PositionWS));
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