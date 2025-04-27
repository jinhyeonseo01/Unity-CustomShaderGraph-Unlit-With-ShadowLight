using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Additional Light")]// "Lighting", 
    class AdditionalLightNode : CodeFunctionNode
    {
        public AdditionalLightNode()
        {
            name = "Additional Light";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("AdditionalLight", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string AdditionalLight(
            [Slot(0, Binding.None)] Vector1 index,
            [Slot(1, Binding.WorldSpacePosition)] Vector3 PositionWS,
            [Slot(2, Binding.None)] Vector4 shadowCoord,
            [Slot(3, Binding.None)] Vector4 shadowMask,
            [Slot(4, Binding.None)] out Vector3 Direction,
            [Slot(5, Binding.None)] out Vector4 Color,
            [Slot(6, Binding.None)] out Vector1 DistanceAttenuation,
            [Slot(7, Binding.None)] out Vector1 ShadowAttenuation,
            [Slot(8, Binding.None)] out Vector1 layerMask)
        {
            Direction = default;
            Color = default;
            DistanceAttenuation = default;
            ShadowAttenuation = default;
            layerMask = default;

            return
@"
{
    // occlusion ���� ��� �Է����� ���� ���� ������ ���⼭�� ���� 1.0(=���� ����)���� ����


    Color = $precision4(1, 1, 1, 1);
    Direction = $precision3(-0.707, 0.707, 0);
    ShadowAttenuation = 1;
    DistanceAttenuation = 1;
    layerMask = 0;

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (������ ���)

    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"" // Added first
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl""


    Light light = GetAdditionalLight(uint(index), PositionWS, shadowMask);

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