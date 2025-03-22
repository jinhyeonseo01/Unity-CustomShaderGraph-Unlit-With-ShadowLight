using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;
using static Codice.CM.Common.Purge.PurgeReport;



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
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    shadowCoord = $precision4(0,0,0,0);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

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