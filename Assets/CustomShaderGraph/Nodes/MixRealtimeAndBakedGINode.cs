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
    // occlusion 값을 노드 입력으로 받을 수도 있지만 여기서는 고정 1.0(=차폐 없음)으로 예시


    result = $precision3(1,1,1);

    #ifndef SHADERGRAPH_PREVIEW  // SHADERGRAPH_PREVIEW (프리뷰 모드)

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