using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;
using static Codice.CM.Common.Purge.PurgeReport;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Matching Light Layer")]// "Lighting", 
    class MatchingLightLayerNode : CodeFunctionNode
    {
        public MatchingLightLayerNode()
        {
            name = "Matching Light Layer";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("MatchingLightLayer", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string MatchingLightLayer(
            [Slot(0, Binding.None)] DynamicDimensionVector colorIn,
            [Slot(1, Binding.None)] Vector1 layerMask,
            [Slot(2, Binding.None)] out DynamicDimensionVector colorOut)
        {
            colorOut = default;

            return
@"
{
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    colorOut = colorIn;

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

        colorOut = 0;
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(uint(layerMask), GetMeshRenderingLayer()))
        #endif
        {
            colorOut = colorIn;
        }

    #endif
}
";

        }
    }


}