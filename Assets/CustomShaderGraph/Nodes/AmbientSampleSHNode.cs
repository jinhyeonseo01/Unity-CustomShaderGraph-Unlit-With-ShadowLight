using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;



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
    // occlusion ���� ��� �Է����� ���� ���� ������ ���⼭�� ���� 1.0(=���� ����)���� ����


    ambient = $precision3(1,1,1);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (������ ���)

    ambient = SampleSH(normalWS);

    #endif
}
";

        }
    }


}