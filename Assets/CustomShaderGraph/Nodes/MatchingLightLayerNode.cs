using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;


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
    // occlusion ���� ��� �Է����� ���� ���� ������ ���⼭�� ���� 1.0(=���� ����)���� ����


    colorOut = colorIn;

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (������ ���)

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