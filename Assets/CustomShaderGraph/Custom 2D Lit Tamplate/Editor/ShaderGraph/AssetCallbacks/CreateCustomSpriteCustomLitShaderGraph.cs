using System;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    static class CreateCustomSpriteCustomLitShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/URP/Custom Sprite Custom Lit Shader Graph", priority = CoreUtils.Priorities.assetsCreateShaderMenuPriority + 100 + 4)]
        public static void CreateCustomSpriteLitGraph()
        {
            var target = (UniversalTarget)Activator.CreateInstance(typeof(UniversalTarget));
            target.TrySetActiveSubTarget(typeof(UniversalCustomSpriteCustomLitSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
                UniversalBlockFields.SurfaceDescription.SpriteMask,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.Alpha,
            };

            GraphUtil.CreateNewGraphWithOutputs(new[] { target }, blockDescriptors);
        }
    }
}
