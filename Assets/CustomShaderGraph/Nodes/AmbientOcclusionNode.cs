using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;
using static Codice.CM.Common.Purge.PurgeReport;
using Codice.Client.BaseCommands;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Ambient Occlusion")]// "Lighting", 
    class AmbientOcclusionNode : CodeFunctionNode
    {
        public AmbientOcclusionNode()
        {
            name = "Ambient Occlusion";
            synonyms = new string[] { "occlusion" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("AmbientOcclusion", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string AmbientOcclusion(
            [Slot(1, Binding.ScreenPosition)] Vector4 screenUV,
            [Slot(2, Binding.None)] out Vector1 DirectAO,
            [Slot(3, Binding.None)] out Vector1 IndirectAO)
        {
            DirectAO = default;
            IndirectAO = default;

            return
@"
{
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시

    DirectAO = 1;
    IndirectAO = 1;

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"" // Added first
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl""

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        AmbientOcclusionFactor aoFactor2 = GetScreenSpaceAmbientOcclusion(screenUV);
        DirectAO = aoFactor2.indirectAmbientOcclusion;
        IndirectAO = aoFactor2.directAmbientOcclusion;
    #endif

    #endif
}
";

        }
    }


}