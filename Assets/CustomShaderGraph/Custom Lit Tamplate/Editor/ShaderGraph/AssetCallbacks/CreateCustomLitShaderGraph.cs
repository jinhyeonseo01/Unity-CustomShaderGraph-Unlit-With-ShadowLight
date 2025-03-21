using System;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    static class CreateCustomLitShaderGraph
    {
        //[GenerateBlocks]
        //public struct VertexDescription
        //{
        //    public static string name = "VertexDescription";
        //    public static BlockFieldDescriptor Position = new BlockFieldDescriptor(VertexDescription.name, "Position", "VERTEXDESCRIPTION_POSITION",
        //        new PositionControl(CoordinateSpace.Object), ShaderStage.Vertex);
        //    public static BlockFieldDescriptor Normal = new BlockFieldDescriptor(VertexDescription.name, "Normal", "VERTEXDESCRIPTION_NORMAL",
        //        new NormalControl(CoordinateSpace.Object), ShaderStage.Vertex);
        //    public static BlockFieldDescriptor Tangent = new BlockFieldDescriptor(VertexDescription.name, "Tangent", "VERTEXDESCRIPTION_TANGENT",
        //        new TangentControl(CoordinateSpace.Object), ShaderStage.Vertex);
        //}

        //[GenerateBlocks]
        //public struct SurfaceDescription
        //{
        //    public static string name = "SurfaceDescription";
        //    public static BlockFieldDescriptor BaseColor = new BlockFieldDescriptor(SurfaceDescription.name, "BaseColor", "Base Color", "SURFACEDESCRIPTION_BASECOLOR",
        //        new ColorControl(UnityEngine.Color.grey, false), ShaderStage.Fragment);
        //    public static BlockFieldDescriptor NormalTS = new BlockFieldDescriptor(SurfaceDescription.name, "NormalTS", "Normal (Tangent Space)", "SURFACEDESCRIPTION_NORMALTS",
        //        new NormalControl(CoordinateSpace.Tangent), ShaderStage.Fragment);
        //    public static BlockFieldDescriptor NormalOS = new BlockFieldDescriptor(SurfaceDescription.name, "NormalOS", "Normal (Object Space)", "SURFACEDESCRIPTION_NORMALOS",
        //        new NormalControl(CoordinateSpace.Object), ShaderStage.Fragment);
        //}

        [MenuItem("Assets/Create/Shader Graph/URP/Custom Lit Shader Graph", priority = CoreUtils.Priorities.assetsCreateShaderMenuPriority)]
        public static void CreateCustomLitGraph()
        {
            var target = (UniversalTarget)Activator.CreateInstance(typeof(UniversalTarget));
            target.TrySetActiveSubTarget(typeof(UniversalCustomLitSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.Metallic,
                BlockFields.SurfaceDescription.Smoothness,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Occlusion,
            };

            GraphUtil.CreateNewGraphWithOutputs(new[] { target }, blockDescriptors);
        }
    }
}
