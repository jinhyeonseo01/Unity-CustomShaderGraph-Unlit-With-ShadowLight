using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;
using static Codice.CM.Common.Purge.PurgeReport;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Shadow Mask")]// "Lighting", 
    class ShadowMaskNode : CodeFunctionNode
    {
        public ShadowMaskNode()
        {
            name = "Shadow Mask";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("ShadowMask", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string ShadowMask(
            [Slot(0, Binding.MeshUV1)] Vector2 lightmapUV,
            [Slot(1, Binding.None)] out Vector4 shadowMask)
        {
            shadowMask = default;

            return
@"
{
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    shadowMask = $precision4(1,1,1,1);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"" // Added first
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl""
    #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl""

    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
    shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
    #elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
    shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
    #elif !defined (LIGHTMAP_ON)
    shadowMask = unity_ProbesOcclusion;
    #else
    shadowMask = $precision4(1, 1, 1, 1); // Fallback shadowmask, fully unoccluded
    #endif

    #endif
}
";

        }
    }


}