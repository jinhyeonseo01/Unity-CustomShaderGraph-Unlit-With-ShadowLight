using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;
using static Codice.CM.Common.Purge.PurgeReport;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Ambient SampleSH")]// "Lighting", 
    class AmbientSampleSHNode : CodeFunctionNode
    {
        public AmbientSampleSHNode()
        {
            name = "Ambient SampleSH";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("AmbientSampleSH", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string AmbientSampleSH(
            [Slot(0, Binding.WorldSpaceNormal)] Vector3 normalWS,
            [Slot(1, Binding.None)] out Vector3 ambient)
        {
            ambient = default;

            return
@"
{
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    ambient = $precision3(1,1,1);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

    ambient = SampleSH(normalWS);

    #endif
}
";

        }
    }


}