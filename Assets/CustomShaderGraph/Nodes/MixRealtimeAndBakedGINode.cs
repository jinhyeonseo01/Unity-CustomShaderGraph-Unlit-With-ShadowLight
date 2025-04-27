using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;



namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Lighting", "Mix Realtime And BakedGI")]// "Lighting", 
    class MixRealtimeAndBakedGINode : CodeFunctionNode
    {
        public MixRealtimeAndBakedGINode()
        {
            name = "Mix Realtime And BakedGI";
            synonyms = new string[] { "main_light" };
        }

        public override bool hasPreview { get { return false; } }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("MixRealtimeAndBakedGIFunc", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string MixRealtimeAndBakedGIFunc(
            [Slot(0, Binding.None)] Vector1 shadowAtten,
            [Slot(1, Binding.WorldSpaceNormal)] Vector3 normalWS,
            [Slot(2, Binding.None)] Vector3 bakedGI,
            [Slot(3, Binding.None)] out Vector3 result)
        {
            result = default;

            return
@"
{
    // occlusion ���� ��� �Է����� ���� ���� ������ ���⼭�� ���� 1.0(=���� ����)���� ����


    result = $precision3(1,1,1);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (������ ���)

        Light light = GetMainLight();
		light.shadowAttenuation = shadowAtten;
		MixRealtimeAndBakedGI(light, normalWS, bakedGI);
		result = bakedGI;

    #endif
}
";

        }
    }


}