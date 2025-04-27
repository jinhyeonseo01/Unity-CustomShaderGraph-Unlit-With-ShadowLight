using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Shadow Coord")]// "Lighting", 
    class ShadowCoordNode : CodeFunctionNode
    {
        public ShadowCoordNode()
        {
            name = "Shadow Coord";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("ShadowCoord", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string ShadowCoord(
            [Slot(0, Binding.WorldSpacePosition)] Vector3 positionWS,
            [Slot(1, Binding.None)] out Vector4 shadowCoord)
        {
            shadowCoord = default;

            return
@"
{
    // occlusion ���� ��� �Է����� ���� ���� ������ ���⼭�� ���� 1.0(=���� ����)���� ����


    shadowCoord = $precision4(0,0,0,0);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (������ ���)

    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"" // Added first
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl""

    
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        shadowCoord = ComputeScreenPos(TransformWorldToHClip(positionWS));
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        shadowCoord = TransformWorldToShadowCoord(positionWS);
    #else
        shadowCoord = $precision4(0, 0, 0, 0);
    #endif

    #endif
}
";

        }
    }


}