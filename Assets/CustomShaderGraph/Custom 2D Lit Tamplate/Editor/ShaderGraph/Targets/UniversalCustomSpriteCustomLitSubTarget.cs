using System;
using UnityEditor.ShaderGraph;
using Unity.Rendering.Universal;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    sealed class UniversalCustomSpriteCustomLitSubTarget : UniversalSubTarget
    {
        static readonly GUID kSourceCodeGuid = new GUID("69e608b3e7e0405bbc2f259ad9cfa120"); // UniversalUnlitSubTarget.cs

        public UniversalCustomSpriteCustomLitSubTarget()
        {
            displayName = "Custom Sprite Custom Lit";
        }

        protected override ShaderUtils.ShaderID shaderID => ShaderUtils.ShaderID.SG_SpriteLit;

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            base.Setup(ref context);
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);

            var universalRPType = typeof(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset);
            var gui = typeof(ShaderGraphSpriteGUI);
#if HAS_VFX_GRAPH
            if (TargetsVFX())
                gui = typeof(VFXGenericShaderGraphMaterialGUI);
#endif
            context.AddCustomEditorForRenderPipeline(gui.FullName, universalRPType);
            context.AddSubShader(PostProcessSubShader(SubShaders.SpriteLit(target)));
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            base.GetFields(ref context);

            SpriteSubTargetUtility.AddDefaultFields(ref context, target);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            SpriteSubTargetUtility.GetDefaultActiveBlocks(ref context, target);

            context.AddBlock(UniversalBlockFields.SurfaceDescription.SpriteMask);
            context.AddBlock(BlockFields.SurfaceDescription.NormalTS);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
            SpriteSubTargetUtility.AddDefaultPropertiesGUI(ref context, onChange, registerUndo, target);
        }

        #region SubShader
        static class SubShaders
        {
            public static SubShaderDescriptor SpriteLit(UniversalTarget target)
            {
                SubShaderDescriptor result = new SubShaderDescriptor()
                {
                    pipelineTag = UniversalTarget.kPipelineTag,
                    customTags = UniversalTarget.kLitMaterialTypeTag,
                    renderType = $"{RenderType.Transparent}",
                    renderQueue = $"{UnityEditor.ShaderGraph.RenderQueue.Transparent}",
                    generatesPreview = true,
                    passes = new PassCollection
                    {
                        { SpriteLitPasses.Lit(target) },
                        { SpriteLitPasses.Normal(target) },
                        // Currently neither of these passes (selection/picking) can be last for the game view for
                        // UI shaders to render correctly. Verify [1352225] before changing this order.
                        { CorePasses._2DSceneSelection(target) },
                        { CorePasses._2DScenePicking(target) },
                        { SpriteLitPasses.Forward(target) },
                    },
                };
                if (target.castShadows || target.allowMaterialOverride)
                {
                    var pass = CorePasses.ShadowCaster(target);
                    pass.pragmas = CorePragmas.Instanced;
                    result.passes.Add(pass);
                }

                return result;
            }
        }
        #endregion

        #region Passes
        static class SpriteLitPasses
        {
            public static PassDescriptor Lit(UniversalTarget target)
            {
                var result = new PassDescriptor()
                {
                    // Definition
                    displayName = "Sprite Lit",
                    referenceName = "SHADERPASS_SPRITELIT",
                    lightMode = "Universal2D",
                    useInPreview = true,

                    // Template
                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = SpriteLitBlockMasks.FragmentLit,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = SpriteLitRequiredFields.Lit,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = SpriteSubTargetUtility.GetDefaultRenderState(target),
                    pragmas = CorePragmas._2DDefault,
                    defines = new DefineCollection(),
                    keywords = SpriteLitKeywords.Lit,
                    includes = SpriteLitIncludes.Lit,
                };

                if (target.disableTint)
                    result.defines.Add(Canvas.ShaderGraph.CanvasSubTarget<Target>.CanvasKeywords.DisableTint, 1);

                SpriteSubTargetUtility.AddAlphaClipControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor Normal(UniversalTarget target)
            {
                var result = new PassDescriptor()
                {
                    // Definition
                    displayName = "Sprite Normal",
                    referenceName = "SHADERPASS_SPRITENORMAL",
                    lightMode = "NormalsRendering",
                    useInPreview = true,

                    // Template
                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = SpriteLitBlockMasks.FragmentNormal,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = SpriteLitRequiredFields.Normal,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.Default,
                    pragmas = CorePragmas._2DDefault,
                    defines = new DefineCollection(),
                    includes = SpriteLitIncludes.Normal,
                };

                if (target.disableTint)
                    result.defines.Add(Canvas.ShaderGraph.CanvasSubTarget<Target>.CanvasKeywords.DisableTint, 1);

                SpriteSubTargetUtility.AddAlphaClipControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor Forward(UniversalTarget target)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "Sprite Forward",
                    referenceName = "SHADERPASS_SPRITEFORWARD",
                    lightMode = "UniversalForward",
                    useInPreview = true,

                    // Template
                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = SpriteLitBlockMasks.FragmentForward,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = SpriteLitRequiredFields.Forward,
                    keywords = SpriteLitKeywords.Forward,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.Default,
                    pragmas = CorePragmas._2DDefault,
                    defines = new DefineCollection(),
                    includes = SpriteLitIncludes.Forward,

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                if (target.disableTint)
                    result.defines.Add(Canvas.ShaderGraph.CanvasSubTarget<Target>.CanvasKeywords.DisableTint, 1);

                return result;

            }
        }
        #endregion

        #region PortMasks
        static class SpriteLitBlockMasks
        {
            public static BlockFieldDescriptor[] FragmentLit = new BlockFieldDescriptor[]
            {
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescriptionLegacy.SpriteColor,
                BlockFields.SurfaceDescription.Alpha,
                UniversalBlockFields.SurfaceDescription.SpriteMask,
                BlockFields.SurfaceDescription.AlphaClipThreshold,
            };

            public static BlockFieldDescriptor[] FragmentNormal = new BlockFieldDescriptor[]
            {
                BlockFields.SurfaceDescription.Alpha,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.AlphaClipThreshold,
            };

            public static BlockFieldDescriptor[] FragmentForward = new BlockFieldDescriptor[]
            {
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescriptionLegacy.SpriteColor,
                BlockFields.SurfaceDescription.Alpha,
                BlockFields.SurfaceDescription.NormalTS,
            };
        }
        #endregion

        #region RequiredFields
        static class SpriteLitRequiredFields
        {
            public static FieldCollection Lit = new FieldCollection()
            {
                StructFields.Varyings.color,
                StructFields.Varyings.positionWS,
                StructFields.Varyings.texCoord0,
                StructFields.Varyings.screenPosition,
            };

            public static FieldCollection Normal = new FieldCollection()
            {
                StructFields.Varyings.color,
                StructFields.Varyings.normalWS,
                StructFields.Varyings.tangentWS,
            };

            public static FieldCollection Forward = new FieldCollection()
            {
                StructFields.Varyings.color,
                StructFields.Varyings.positionWS,
                StructFields.Varyings.texCoord0,
            };
        }
        #endregion

        #region Keywords

        static class SpriteLitKeywords
        {
            public static KeywordCollection Lit = new KeywordCollection
            {
                { CoreKeywordDescriptors.StaticLightmap },
                { CoreKeywordDescriptors.DynamicLightmap },
                { CoreKeywordDescriptors.DirectionalLightmapCombined },
                { CoreKeywordDescriptors.UseLegacyLightmaps },
                { CoreKeywordDescriptors.MainLightShadows },
                { CoreKeywordDescriptors.AdditionalLights },
                { CoreKeywordDescriptors.AdditionalLightShadows },
                { CoreKeywordDescriptors.ShadowsSoft },
                { CoreKeywordDescriptors.LightmapShadowMixing },
                { CoreKeywordDescriptors.ShadowsShadowmask },
                { CoreKeywordDescriptors.LightLayers },
                { CoreKeywordDescriptors.LightCookies },

                { CoreKeywordDescriptors.ShapeLightType0 },
                { CoreKeywordDescriptors.ShapeLightType1 },
                { CoreKeywordDescriptors.ShapeLightType2 },
                { CoreKeywordDescriptors.ShapeLightType3 },
                { CoreKeywordDescriptors.DebugDisplay },
            };

            public static KeywordCollection Forward = new KeywordCollection
            {
                { CoreKeywordDescriptors.StaticLightmap },
                { CoreKeywordDescriptors.DynamicLightmap },
                { CoreKeywordDescriptors.DirectionalLightmapCombined },
                { CoreKeywordDescriptors.UseLegacyLightmaps },
                { CoreKeywordDescriptors.MainLightShadows },
                { CoreKeywordDescriptors.AdditionalLights },
                { CoreKeywordDescriptors.AdditionalLightShadows },
                { CoreKeywordDescriptors.ShadowsSoft },
                { CoreKeywordDescriptors.LightmapShadowMixing },
                { CoreKeywordDescriptors.ShadowsShadowmask },
                { CoreKeywordDescriptors.LightLayers },
                { CoreKeywordDescriptors.LightCookies },

                { CoreKeywordDescriptors.DebugDisplay },
            };
        }
        #endregion

        #region Includes
        static class SpriteLitIncludes
        {

            const string k2DLightingUtil = "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl";
            const string kSpriteLitPass = "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteLitPass.hlsl";
            const string kSpriteUnlitPass = "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl";
            const string k2DNormal = "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl";
            const string kSpriteNormalPass = "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteNormalPass.hlsl";
            const string kSpriteForwardPass = "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteForwardPass.hlsl";

            const string kShadows = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl";

            public static IncludeCollection Lit = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.WriteRenderLayersPregraph },
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { kShadows, IncludeLocation.Pregraph },
                { k2DLightingUtil, IncludeLocation.Pregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kSpriteLitPass, IncludeLocation.Postgraph },
            };

            public static IncludeCollection Normal = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { k2DNormal, IncludeLocation.Pregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kSpriteNormalPass, IncludeLocation.Postgraph },
            };

            public static IncludeCollection Forward = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.WriteRenderLayersPregraph },
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { kShadows, IncludeLocation.Pregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kSpriteForwardPass, IncludeLocation.Postgraph },
            };
        }
        #endregion
    }
}
