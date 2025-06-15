using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using stencilTestHelper;
using UnityEditor.AnimatedValues;

namespace UnityEditor
{
    public class ParticleBaseGUI : ShaderGUI
    {
        private ShaderGUIHelper helper = new ShaderGUIHelper();
        public List<Material> mats = new List<Material>();
        private Shader shader;
        private MaterialEditor matEditor;
        public List<W9ParticleShaderFlags> shaderFlags = new List<W9ParticleShaderFlags>();

        private int lastFlagBit;
        private bool isCustomedStencil = false;

        private StencilValuesConfig _stencilValuesConfig;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            //一定要初始化在第一行
            mats.Clear();
            shaderFlags.Clear();
            for (int i = 0; i < materialEditor.targets.Length; i++)
            {
                var _targetMat = materialEditor.targets[i] as Material;
                
                mats.Add(_targetMat);
                shaderFlags.Add(new W9ParticleShaderFlags(mats[i]));
            }
            matEditor = materialEditor;
            
            if (!_stencilValuesConfig)
            {
                _stencilValuesConfig = AssetDatabase.LoadAssetAtPath<StencilValuesConfig>("Packages/com.xuanxuan.nb.shaders/Shader/StencilConfig.asset");
            }
            matEditor = materialEditor;
            isCustomedStencil = false;
            helper.Init(materialEditor, props, shaderFlags.ToArray(), mats);
            
            DrawBigBlockFoldOut(W9ParticleShaderFlags.foldOutBitMeshOption,3,"模式设置", () => DrawMeshOptions());
            DrawBigBlockFoldOut(W9ParticleShaderFlags.foldOutBitMainTexOption,3,"主贴图功能", () => DrawMainTexOptions());
            DrawBigBlockFoldOut(W9ParticleShaderFlags.foldOutBitBaseOption,3,"基本全局功能", () => DrawBaseOptions());
            DrawBigBlockFoldOut(W9ParticleShaderFlags.foldOutBitFeatureOption,3,"特别功能", () => DrawFeatureOptions());
            DrawBigBlockFoldOut(W9ParticleShaderFlags.foldOutTaOption,4,"TA调试", () => DrawTaOptions());
            
            //遍历整个场景，看哪些 粒子系统 用了这个材质。会填充m_RenderersUsingThisMaterial
            if (mats.Count == 1)
            {
                CacheRenderersUsingThisMaterial(mats[0], 0);
            

                if (!_uieffectEnabled||_uiParticleEnabled)
                {
                    DoVertexStreamsArea(mats[0], m_RenderersUsingThisMaterial, 0);//填充stream和stremList
                }
                else
                {
                    mats[0].DisableKeyword("_CUSTOMDATA");
                }
            }
            
            DoAfterDraw();


            // int flagBit = mat.GetInteger(W9ParticleShaderFlags.FlagsId);
            // if (flagBit != lastFlagBit)
            // {
            //     FlagBitTest.Log(mat);
            //     lastFlagBit = flagBit;
            // }
            // Debug.Log(mat.GetInt(W9ParticleShaderFlags.FlagsId));
            // Debug.Log(shaderFlag.CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_USETEXCOORD2));
        }

        bool _uieffectEnabled = false;
        bool _uiParticleEnabled = false;
        bool _noiseEnabled = false;//扭曲
        // bool _uieffectSpriteMode = false;
        private MeshSourceMode _meshSourceMode;
        private TransparentMode _transparentMode;
        public void DrawMeshOptions()
        {
            // if (shaderFlag.CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.DefaultUVChannel))
            // {
            //     EditorGUILayout.LabelField("UVMode.DefaultUVChannel");
            // }
            // if (shaderFlag.CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.SpecialUVChannel))
            // {
            //     EditorGUILayout.LabelField("UVMode.SpecialUVChannel");
            // }
            // if (shaderFlag.CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.PolarOrTwirl))
            // {
            //     EditorGUILayout.LabelField("UVMode.PolarOrTwirl");
            // }
            // if (shaderFlag.CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.Cylinder))
            // {
            //     EditorGUILayout.LabelField("UVMode.Cylinder");
            // }
            // SetMeshSourceModeToOriginSet();//防止就旧数据被复写。
            
            // SetUVModeByOldSettings();
            
            helper.DrawPopUp("Mesh来源模式","_MeshSourceMode",_meshSourceModeNames,drawBlock: mode =>
            {
                
                _meshSourceMode = (MeshSourceMode)mode;
                if (_meshSourceMode == MeshSourceMode.UIEffectRawImage || _meshSourceMode == MeshSourceMode.UIEffectSprite || _meshSourceMode == MeshSourceMode.UIEffectBaseMap||_meshSourceMode == MeshSourceMode.UIParticle)
                {
                    _uieffectEnabled = true;
                }
                else
                {
                    _uieffectEnabled = false;
                }

                if (_meshSourceMode == MeshSourceMode.UIParticle)
                {
                    _uiParticleEnabled = true;
                }
                else
                {
                    _uiParticleEnabled = false;
                }

                if (checkIsParicleSystem)
                {
                    if (!(_meshSourceMode != MeshSourceMode.Particle || !_uiParticleEnabled))
                    {
                        EditorGUILayout.HelpBox("检测到材质用在粒子系统上，和设置不匹配",MessageType.Error);
                    }
                }
                else
                {
                    //这个不能Log，因为在Project面板下打开是不知道在不在粒子系统里的。
                    // if (_meshSourceMode == MeshSourceMode.Particle)
                    // {
                    //     EditorGUILayout.HelpBox("检测到材质没有用在粒子系统上，和设置不匹配",MessageType.Error);
                    // }
                }
            });
            
            // helper.DrawToggle("2D/UI模式", "_UIEffect_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UIEFFECT_ON ,drawBlock:(isToggle) =>
            // {
            //     _uieffectEnabled = isToggle;
            //     if (isToggle)
            //     {
            //         matEditor.ShaderProperty(helper.GetProperty("_Color"), "贴图颜色叠加");
            //         // mat.renderQueue = 3000 + (int)helper.GetProperty("_QueueBias").floatValue;
            //         helper.DrawToggle("精灵模式",flagBitsName:W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,flagIndex:1,drawBlock:
            //             isSpriteModeToggle =>
            //             {
            //                 _uieffectSpriteMode = isSpriteModeToggle;
            //             });
            //     }
            //     else
            //     {
            //         shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,null,1);
            //         EditorGUILayout.Space();
            //         // mat.renderQueue = 3100 + (int)helper.GetProperty("_QueueBias").floatValue; //3D粒子永远最前显示
            //     }
            // });
        
            helper.DrawPopUp("透明模式","_TransparentMode",transparentModeNames);
            if (mats.Count == 1)
            {
                _transparentMode = (TransparentMode)mats[0].GetFloat("_TransparentMode");
                if (_transparentMode == TransparentMode.CutOff)
                {
                    matEditor.ShaderProperty(helper.GetProperty("_Cutoff"),"裁剪位置");
                }
    
                if (_transparentMode == TransparentMode.Transparent)
                {
                    helper.DrawPopUp("混合模式","_Blend",blendModeNames);
                }
            }
        }

        public void DrawMainTexOptions()
        {
            Action drawAfterMainTex = ()=>
            {
                if (_meshSourceMode != MeshSourceMode.UIEffectSprite)
                {
                    bool hasMainTex = mats[0].GetTexture("_MainTex") || mats[0].GetTexture("_BaseMap");
                    DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeMainTex,4,"主贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_MAINTEX,0,hasMap:hasMainTex);
                }

                if (!_uieffectEnabled||_uiParticleEnabled)
                {
                    DrawCustomDataSelect("主贴图X轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_X,0);
                    DrawCustomDataSelect("主贴图Y轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_Y,0);
                }
                if (_meshSourceMode != MeshSourceMode.UIEffectSprite)
                {

                    helper.DrawVector4In2Line("_BaseMapMaskMapOffset", "偏移速度");

                    helper.DrawSlider("主贴图旋转", "_BaseMapUVRotation", 0f, 360f);
                }

                DrawNoiseAffectBlock(() =>
                {
                    helper.DrawSlider("主贴图扭曲强度","_TexDistortion_intensity",-1.0f,1.0f);

                });


                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitHueShift,3,"主贴图色相偏移","_HueShift_Toggle",W9ParticleShaderFlags.FLAG_BIT_HUESHIFT_ON,isIndentBlock:true,drawBlock:(isToggle)=>{
                        helper.DrawSlider("色相","_HueShift",0,1);
                        DrawCustomDataSelect("色相自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_HUESHIFT,0);
                });
                
                        
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitSaturability,3,"主贴图饱和度","_ChangeSaturability_Toggle",W9ParticleShaderFlags.FLAG_BIT_SATURABILITY_ON,isIndentBlock:true,drawBlock:(isToggle)=>{
                    helper.DrawSlider("饱和度","_Saturability",0,1);
                    DrawCustomDataSelect("饱和度强度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_SATURATE,1);
                });
                
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutMianTexContrast,4,"主贴图对比度","_Contrast_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_MAINTEX_CONTRAST,1,isIndentBlock:true,drawBlock:(isToggle)=>{
                    matEditor.ShaderProperty(helper.GetProperty("_ContrastMidColor"),"对比度中值颜色");
                    helper.DrawSlider("对比度","_Contrast",0,5);
                    DrawCustomDataSelect("对比度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_MAINTEX_CONTRAST,2);
                });
                
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutMainTexColorRefine,4,"主贴图颜色修正","_BaseMapColorRefine_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_MAINTEX_COLOR_REFINE,1,isIndentBlock:true,drawBlock:
                    (isToggle) =>
                    {
                        helper.DrawVector4Componet("A:主颜色相乘","_BaseMapColorRefine","x",false);
                        helper.DrawVector4Componet("B:主颜色Power","_BaseMapColorRefine","y",false);
                        helper.DrawVector4Componet("B:主颜色Power后相乘","_BaseMapColorRefine","z",false);
                        helper.DrawVector4Componet("A/B线性差值","_BaseMapColorRefine","w",true,0f,1f);
                    });
            };

            if (!_uieffectEnabled || _uiParticleEnabled || _meshSourceMode == MeshSourceMode.UIEffectBaseMap)
            {
                DrawTextureFoldOut(W9ParticleShaderFlags.foldOutBitBaseMap,3,"主贴图","_BaseMap","_BaseColor",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_BASEMAP,flagIndex:2,drawBlock:
                    theBaseMap =>
                    {
                        drawAfterMainTex();
                    });
                // helper.DrawTexture("主贴图","_BaseMap","_BaseColor",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_BASEMAP,flagIndex:2);
            }
            else
            {
                //实际上贴图来自_MainTex
                matEditor.ShaderProperty(helper.GetProperty("_Color"), "贴图颜色叠加");
                helper.DrawVector4In2Line("_UI_MainTex_ST", "Tilling","Offset");
                drawAfterMainTex();
            }

        }

        public void DrawBaseOptions()
        {
            helper.DrawFloat("整体颜色强度","_BaseColorIntensityForTimeline");
            helper.DrawSlider("整体透明度","_AlphaAll",0f,1f);
            if (!_uieffectEnabled)
            {
                helper.DrawPopUp("深度测试","_ZTest",Enum.GetNames(typeof(CompareFunction)));
            }
            else
            {
                helper.GetProperty("_ZTest").floatValue = 4.0f;//UI层使用默认值LessEqual
            }
            
            // helper.DrawPopUp("时间模式","_TimeMode",Enum.GetNames(typeof(TimeMode)));
            helper.DrawPopUp("渲染面向","_Cull",Enum.GetNames(typeof(RenderFace)));
                
            
            
            if (!_uieffectEnabled)
            {
                if (mats.Count == 1)
                {
                    if (_transparentMode == TransparentMode.Transparent)
                    {
                        bool isBackFirstPass = false;
                        helper.DrawToggle("预渲染反面", "_BackFristPassToggle", drawBlock: (isToggle) =>
                        {
                            mats[0].SetShaderPassEnabled("SRPDefaultUnlit", isToggle);
                            isBackFirstPass = isToggle;
                        });

                        if (isBackFirstPass)
                        {
                            EditorGUILayout.HelpBox("预渲染反面会导致打断动态合批，请谨慎使用。",MessageType.Warning);
                            mats[0].SetFloat("_Cull", (float)RenderFace.Front);
                        }

                        helper.DrawToggle("强制深度写入", "_ForceZWriteToggle");
                    }
                }



                EditorGUILayout.BeginHorizontal();
                helper.DrawToggle("背面颜色","_BaseBackColor_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_BACKCOLOR,drawBlock:
                    (isToggle) =>
                    {
                        matEditor.ColorProperty(helper.GetProperty("_BaseBackColor"), "");
                    });
                EditorGUILayout.EndHorizontal();
            
            
            }
            // helper.DrawToggle("使用3U作为UV来源","_UseUV1_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_USETEXCOORD2);
            
     
      
            
            if (!_uieffectEnabled)
            {
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitDistanceFade, 3,"近距离透明","_DistanceFade_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_DISTANCEFADE_ON,isIndentBlock:true,drawBlock:(isToggle) =>
                {
                    helper.DrawVector4In2Line("_Fade","透明过度范围");
                });
            }
            else
            {
                for (int i = 0; i < mats.Count; i++)
                {
                    shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_DISTANCEFADE_ON);
                }
            }

            if (!_uieffectEnabled)
            {
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitSoftParticles,3,"软粒子","_SoftParticlesEnabled",shaderKeyword:"_SOFTPARTICLES_ON",isIndentBlock:true,drawBlock:
                (isToggle) =>
                {
                    helper.DrawVector4In2Line("_SoftParticleFadeParams","远近裁剪面");
                });
                
           
                
                helper.DrawToggle("剔除主角色",shaderKeyword:"_STENCIL_WITHOUT_PLAYER", drawBlock: isToggle =>
                {
                    if (isToggle)
                    {
                        for (int i = 0; i < mats.Count; i++)
                        {
                            StencilTestHelper.SetMaterialStencil(mats[i], "ParticleWithoutPlayer", _stencilValuesConfig,
                                out int queue);
                        }

                        isCustomedStencil = true;
                    }
                },drawEndChangeCheck: isToggle =>
                {
                    if (!isToggle)
                    {
                        helper.GetProperty("_CustomStencilTest").floatValue = 0f;
                    }
                });
                helper.DrawToggle("忽略顶点色","_IgnoreVetexColor_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_IGNORE_VERTEX_COLOR,flagIndex:1);
                helper.DrawSlider("雾影响强度","_fogintensity",0f,1f);
            }
            else
            {
                helper.GetProperty("_fogintensity").floatValue = 0;
            }
        }

        public void DrawFeatureOptions()
        {
            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitMask,3,"遮罩","_Mask_Toggle",shaderKeyword:"_MASKMAP_ON",fontStyle:FontStyle.Bold,drawBlock:(isToggle) =>{
            // helper.DrawToggle("遮罩","_Mask_Toggle",shaderKeyword:"_MASKMAP_ON",fontStyle:FontStyle.Bold,drawBlock: (isToggle) =>{
                // if (isToggle)
                // {
                DrawTextureFoldOut(W9ParticleShaderFlags.foldOutBitMaskMap,3,"遮罩贴图","_MaskMap",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_MASKMAP,flagIndex:2,drawBlock:
                    theMaskMap =>
                {
                    // if (theMaskMap)
                    // {
                        DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeMaskMap,4,"遮罩贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_MASKMAP,0,hasMap:theMaskMap);
                        DrawCustomDataSelect("Mask图X轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_X,0);
                        DrawCustomDataSelect("Mask图Y轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_Y,0);
                        helper.DrawVector4Componet("遮罩强度","_MaskMapVec","x",false);
                        helper.DrawVector4In2Line("_MaskMapOffsetAnition","遮罩偏移速度");
                        helper.DrawFloat("遮罩旋转","_MaskMapUVRotation");
                        
                        DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitMaskRotate,3,"遮罩旋转速度","_Mask_RotationToggle",W9ParticleShaderFlags
                            .FLAG_BIT_PARTILCE_MASKMAPROTATIONANIMATION_ON,isIndentBlock:false,drawBlock: (isToggle2) =>{
                                helper.DrawFloat("旋转速度", "_MaskMapRotationSpeed");
                        });
                        // EditorGUI.BeginDisabledGroup(!_noiseEnabled);
                        //     
                        // EditorGUI.EndDisabledGroup();
                        DrawNoiseAffectBlock(() => {helper.DrawSlider("遮罩扭曲强度","_MaskDistortion_intensity",-2,2);});
                        //没有必要自动归位
                        // if(!_noiseEnabled)
                        // {
                        //     helper.GetProperty("_MaskDistortion_intensity").floatValue = 0f;
                        // }
                        
                    // }
                    
                });
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitMask2,3,"遮罩2","_Mask2_Toggle",flagBitsName:W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_MASK_MAP2,flagIndex:1,isIndentBlock:true,drawBlock:
                    (isToggle) =>
                    {
                            helper.DrawTexture("遮罩2贴图","_MaskMap2",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_MASKMAP2,flagIndex:2,
                                drawBlock:theMaskMap2Texture =>
                                {
                                    DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeMaskMap2,4,"遮罩2UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_MASKMAP_2,0,hasMap:theMaskMap2Texture);
                                    helper.DrawVector4In2Line("_MaskMapOffsetAnition",secondLineLabel:"遮罩2偏移速度");
                                });
                            // helper.DrawTexture("遮罩2贴图","_MaskMap2",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_MASKMAP2,flagIndex:2);
                        
                    });
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitMask3,3,"遮罩3",flagBitsName:W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_MASK_MAP3,flagIndex:1,isIndentBlock:true,drawBlock:
                    (isToggle) =>
                    {
                            helper.DrawTexture("遮罩3贴图","_MaskMap3",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_MASKMAP3,flagIndex:2,drawBlock:theMaskMap3Texture=>
                            {
                                DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeMaskMap3,4,"遮罩3UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_MASKMAP_3,0,hasMap:theMaskMap3Texture);
                                helper.DrawVector4In2Line("_MaskMap3OffsetAnition",firstLineLabel:"遮罩3偏移速度");
                            });
                    });
                    // helper.DrawTexture("遮罩贴图","_MaskMap",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_MASKMAP,flagIndex:2);
                    // if (mat.GetTexture("_MaskMap"))
                // }
            });
            
            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitNoise,3,"扭曲","_noisemapEnabled",shaderKeyword:"_NOISEMAP",fontStyle:FontStyle.Bold,drawBlock:(isToggle) => {

                _noiseEnabled = isToggle;
            
                helper.DrawToggle("用于屏幕扰动",shaderKeyword:"_SCREEN_DISTORT_MODE",drawBlock: isScreenDistortToggle =>
                {
                    if (isScreenDistortToggle)
                    {
                        //强制设置为Clamp模式。
                        for (int i = 0; i < mats.Count; i++)
                        {
                            shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_BASEMAP, index: 2);
                        }
                    }
                });
                // if (isToggle)
                // {
                // EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("扭曲贴图RG双通道则为FlowMap,FlowMap贴图设置应该去掉sRGB勾选");
                     DrawTextureFoldOut(W9ParticleShaderFlags.foldOutBitNoiseMap,3,"扭曲贴图","_NoiseMap",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_NOISEMAP,flagIndex:2,drawBlock:
                         theNoiseMap =>
                         {
                             DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeNoiseMap,4,"扭曲贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_NOISE_MAP,0);
                             helper.DrawSlider("主贴图扭曲强度","_TexDistortion_intensity",-1.0f,1.0f);
                             DrawCustomDataSelect("扭曲强度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_NOISE_INTENSITY,1);
                             helper.DrawVector4In2Line("_DistortionDirection","扭曲方向强度");
                             DrawCustomDataSelect("扭曲方向强度X自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_NOISE_DIRECTION_X,2);
                             DrawCustomDataSelect("扭曲方向强度Y自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_NOISE_DIRECTION_Y,2);
                             
                             helper.DrawSlider("扭曲旋转","_NoiseMapUVRotation",0f,360f);
                             helper.DrawVector4In2Line("_NoiseOffset","扭曲偏移速度");
                             helper.DrawToggle("0.5为中值，双向扭曲","_DistortionBothDirection_Toggle",flagBitsName:W9ParticleShaderFlags.FLAG_BIT_PARTICLE_NOISEMAP_NORMALIZEED_ON,isIndentBlock:false);
                         });
                     
                     DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitNoiseMaskToggle,3,"扭曲遮罩","_noiseMaskMap_Toggle",flagBitsName:W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_NOISE_MASKMAP,flagIndex:1,drawBlock:
                         isNoiseMaskToggle =>
                         {
                             // if (isNoiseMaskToggle)
                             // {
                                helper.DrawTexture("扭曲遮罩贴图","_NoiseMaskMap",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_NOISE_MASKMAP,drawBlock:
                                    theNoiseMaskMap =>
                                    {
                                        DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeNoiseMaskMap,4,"扭曲遮罩贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_NOISE_MASK_MAP,0,theNoiseMaskMap);
                                    });
                             // }
                         });
                // }
            });
            
            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitDistortionChoraticaberrat, 3,"扭曲色散","_Distortion_Choraticaberrat_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CHORATICABERRAT,isIndentBlock:true,fontStyle:FontStyle.Bold,drawBlock:
             (is_Choraticaberrat_Toggle) =>
             {
                 // if (is_Choraticaberrat_Toggle)
                 // {
                    DrawNoiseAffectBlock(() => { helper.DrawToggle("色散强度受扭曲强度影响","_Distortion_Choraticaberrat_WithNoise_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_NOISE_CHORATICABERRAT_WITH_NOISE);});
                    helper.DrawVector4Componet("色散强度", "_DistortionDirection", "z", false);
                    DrawCustomDataSelect("色散强度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_CHORATICABERRAT_INTENSITY,0);
                 // }
             });
            
            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitEmission,3,"流光","_EmissionEnabled",shaderKeyword:"_EMISSION",isIndentBlock:true,fontStyle:FontStyle.Bold,drawBlock: (isToggle) =>{
                // if (isToggle)
                // {
                    helper.DrawTexture("流光贴图","_EmissionMap","_EmissionMapColor",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_EMISSIONMAP,flagIndex:2,drawBlock:theEmissionMap=>
                    {
                        DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeEmissionMap,4,"流光贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_EMISSION_MAP,0,theEmissionMap);
                    });
                    helper.DrawFloat("流光颜色强度","_EmissionMapColorIntensity");
                    helper.DrawSlider("流光贴图旋转","_EmissionMapUVRotation",0f,360f);
                    DrawNoiseAffectBlock(() => {helper.DrawFloat("流光贴图扭曲强度","_Emi_Distortion_intensity"); });
                    //没有必要自动归位
                    // if (!_noiseEnabled)
                    // {
                    //     helper.GetProperty("_Emi_Distortion_intensity").floatValue = 0;
                    // }

                    helper.DrawVector4In2Line("_EmissionMapUVOffset", "流光贴图偏移速度");
                    // helper.DrawSlider("LiuuvRapSoft","_uvRapSoft",0f,1f);
                    // helper.DrawFloat("_CustomData2X","_CustomData2X");
                    // }
            });
            
           DrawToggleFoldOut(W9ParticleShaderFlags.foldOutDissolve,3,"溶解","_Dissolve_Toggle",shaderKeyword:"_DISSOLVE",isIndentBlock:true,fontStyle:FontStyle.Bold,drawBlock:(isToggle) =>{
                // if (isToggle)
                // {
                    DrawTextureFoldOut(W9ParticleShaderFlags.foldOutDissolveMap,3,"溶解贴图","_DissolveMap",drawScaleOffset:false,drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_DISSOLVE_MAP,flagIndex:2,drawBlock:(dissolveTex)=>
                    {
                        matEditor.TextureScaleOffsetProperty(helper.GetProperty("_DissolveMap"));
                        DrawCustomDataSelect("溶解贴图X轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_X,1);
                        DrawCustomDataSelect("溶解贴图Y轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_Y,1);
                        DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeDissolveMap,4,"溶解贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_DISSOLVE_MAP,0);
                        helper.DrawVector4In2Line("_DissolveOffsetRotateDistort","溶解贴图偏移速度");
                        helper.DrawVector4Componet("溶解贴图旋转","_DissolveOffsetRotateDistort","z",true,0f,360f);
                    });
                    helper.DrawToggle("溶解度黑白值测试","_Dissolve_Test_Toggle",shaderKeyword:"_DISSOLVE_EDITOR_TEST");
                    DrawToggleFoldOut(W9ParticleShaderFlags.foldOutDissolveVoronoi,3,"程序化噪波叠加","_DissolveVoronoi_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_DISSOVLE_VORONOI,flagIndex:1,isIndentBlock:true,drawBlock:isVoronoiToggle=>{
                        // if (isVoronoiToggle)
                        // {
                            // helper.DrawVector4Componet("噪波1缩放","_DissolveVoronoi_Vec","x",false);
                            helper.DrawVector4In2Line("_DissolveVoronoi_Vec","噪波1缩放");
                            helper.DrawVector4Componet("噪波1速度","_DissolveVoronoi_Vec2","z",false);
                            helper.DrawVector4In2Line("_DissolveVoronoi_Vec4","噪波1偏移");
                            DrawCustomDataSelect("噪波1偏移速度X自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE1_OFFSET_X,2);
                            DrawCustomDataSelect("噪波1偏移速度Y自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE1_OFFSET_Y,2);
                            helper.DrawVector4In2Line("_DissolveVoronoi_Vec3","噪波1偏移速度");
                            EditorGUILayout.Space();
                            helper.DrawVector4In2Line("_DissolveVoronoi_Vec",secondLineLabel:"噪波2缩放");
                            // helper.DrawVector4Componet("噪波2缩放","_DissolveVoronoi_Vec","z",false);
                            helper.DrawVector4Componet("噪波2速度","_DissolveVoronoi_Vec2","w",false);
                            helper.DrawVector4In2Line("_DissolveVoronoi_Vec4",secondLineLabel: "噪波2偏移");
                            DrawCustomDataSelect("噪波2偏移速度X自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE2_OFFSET_X,2);
                            DrawCustomDataSelect("噪波2偏移速度Y自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE2_OFFSET_Y,2);
                            helper.DrawVector4In2Line("_DissolveVoronoi_Vec3",secondLineLabel: "噪波2偏移速度");
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            helper.DrawVector4Componet("噪波1和噪波2混合系数(圆尖)","_DissolveVoronoi_Vec2","x",true);
                            helper.DrawVector4Componet("噪波整体和溶解贴图混合系数","_DissolveVoronoi_Vec2","y",true);
                            EditorGUILayout.Space();
                        // }
                        
                    });
                    
                    DrawNoiseAffectBlock(()=>{
                        helper.DrawVector4Componet("溶解贴图扭曲强度","_DissolveOffsetRotateDistort","w",false);
                    });
                    
                    helper.DrawVector4In2Line("_Dissolve_Vec2","溶解丝滑度（溶解值黑白调整）");
                    helper.DrawVector4Componet("溶解强度","_Dissolve","x",true,-1f,2f);
                    DrawCustomDataSelect("溶解强度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_DISSOLVE_INTENSITY,0);
                    helper.DrawVector4Componet("溶解硬度","_Dissolve","w",true,0f,1f);

                    // else
                    // {
                    //     
                    //     Vector4 value = helper.GetProperty("_DissolveOffsetRotateDistort").vectorValue;
                    //     value = new Vector4(value.x, value.y, value.z, 0);
                    //     helper.GetProperty("_DissolveOffsetRotateDistort").vectorValue = value;
                    // }

                    helper.DrawVector4Componet("溶解描边范围","_Dissolve","y",true,0f,1f);
                    matEditor.ColorProperty(helper.GetProperty("_DissolveLineColor"),"溶解描边颜色");
                    DrawToggleFoldOut(W9ParticleShaderFlags.foldOutDissolveRampMap,3,"溶解Ramp图功能","_Dissolve_useRampMap_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_DISSOVLE_USE_RAMP,flagIndex:1,isIndentBlock:true,drawBlock:
                        isDissolveUseRampToggle =>
                        {
                            // if (isDissolveUseRampToggle)
                            // {
                                helper.DrawTexture("溶解Ramp图","_DissolveRampMap","_DissolveRampColor",drawScaleOffset:true,drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_DISSOLVE_RAMPMAP,flagIndex:2);                       
                            // }
                        });
                    
                    
                    DrawToggleFoldOut(W9ParticleShaderFlags.foldOutDissolveMask,3,"局部溶解","_DissolveMask_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_DISSOLVE_MASK,drawBlock:
                        (isToggle) =>
                        {
                            // if (isToggle)
                            // {
                                helper.DrawTexture("局部溶解蒙版","_DissolveMaskMap",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_DISSOLVE_MASKMAP,flagIndex:2);
                                DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeDissolveMaskMap,4,"局部溶解蒙板UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_DISSOLVE_MASK_MAP,0);
                                helper.DrawVector4Componet("局部控制强度","_Dissolve","z",false);
                                DrawCustomDataSelect("局部溶解强度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_MASK_INTENSITY,1);

                            // }

                        });
                    
                // }
                
            });
            
            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutColorBlend,3,"颜色渐变","_ColorBlendMap_Toggle",shaderKeyword:"_COLORMAPBLEND",isIndentBlock:true,fontStyle:FontStyle.Bold,drawBlock:(isToggle) =>
            {
                // if (isToggle)
                // {
                    helper.DrawTexture("颜色渐变贴图","_ColorBlendMap",drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_COLORBLENDMAP,flagIndex:2);
                    matEditor.ColorProperty(helper.GetProperty("_ColorBlendColor"), "颜色渐变叠加");
                    DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeColorBlendMap,4,"颜色渐变贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_COLOR_BLEND_MAP,0);
                    helper.DrawVector4In2Line("_ColorBlendMapOffset","颜色渐变贴图偏移速度");
                // }
            });
            
           DrawToggleFoldOut(W9ParticleShaderFlags.foldOutFresnel,3,"菲涅尔","_fresnelEnabled",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_FRESNEL_ON,isIndentBlock:true,fontStyle:FontStyle.Bold,drawBlock:
                (isToggle) =>
                {
                    // if (isToggle)
                    // {
                        helper.DrawPopUp("菲涅尔模式","_FresnelMode",Enum.GetNames(typeof(FresnelMode)));
                        helper.DrawVector4Componet("菲涅尔强度","_FresnelUnit","z",true);

                        if (mats.Count == 1)
                        {
                            FresnelMode fresnelMode = (FresnelMode)mats[0].GetFloat("_FresnelMode");
                            switch (fresnelMode)
                            {
                                case FresnelMode.Color:
                                    matEditor.ColorProperty(helper.GetProperty("_FresnelColor"), "菲涅尔颜色");
                                    shaderFlags[0].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_FRESNEL_COLOR_ON);
                                    shaderFlags[0].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_FRESNEL_FADE_ON);
                                    break;
                                case FresnelMode.Fade:
                                    shaderFlags[0].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_FRESNEL_COLOR_ON);
                                    shaderFlags[0].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_FRESNEL_FADE_ON);
                                    break;
                            }
                        }

                        helper.DrawVector4Componet("菲涅尔位置","_FresnelUnit","x",true,-1f,1f);
                        DrawCustomDataSelect("菲尼尔位置自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_FRESNEL_OFFSET,0);
                        helper.DrawVector4Componet("菲涅尔范围","_FresnelUnit","y",true,0f,10f);
                        helper.DrawVector4Componet("菲涅尔硬度","_FresnelUnit","w",true,0f,1f);
                        helper.DrawToggle("翻转菲涅尔","_InvertFresnel_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_FRESNEL_INVERT_ON);
                        matEditor.VectorProperty(helper.GetProperty("_FresnelRotation"),"菲涅尔方向偏移");
                   
                    // }
                });
            if (!_uieffectEnabled)
            {
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutDepthOutline,3,"深度描边", "_DepthOutline_Toggle",
                    flagBitsName: W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_DEPTH_OUTLINE,fontStyle:FontStyle.Bold, flagIndex: 1,
                    isIndentBlock: true, drawBlock:
                    (isToggle) =>
                    {
                        // if (isToggle)
                        // {
                            matEditor.ColorProperty(helper.GetProperty("_DepthOutline_Color"), "深度描边颜色");
                            helper.DrawVector4In2Line("_DepthOutline_Vec", "深度描边距离");
                        // }
                    });

                helper.DrawToggle("深度贴花", "_DepthDecal_Toggle", shaderKeyword: "_DEPTH_DECAL",fontStyle:FontStyle.Bold, drawBlock: (isToggle) =>
                {
                    if (isToggle)
                    {
                        for (int i = 0; i < mats.Count; i++)
                        {
                            StencilTestHelper.SetMaterialStencil(mats[i], "ParticleBaseDecal", _stencilValuesConfig,
                                out int ignore);
                        }

                        isCustomedStencil = true;
                        helper.GetProperty("_Cull").floatValue = (float)RenderFace.Back;
                        helper.GetProperty("_ZTest").floatValue = (float)CompareFunction.GreaterEqual;
                    }
                },drawEndChangeCheck: (isToggle) =>
                    {
                        if (!isToggle)
                        {
                            if (!isToggle)
                            {
                                helper.GetProperty("_CustomStencilTest").floatValue = 0f;
                                helper.GetProperty("_Cull").floatValue = (float)RenderFace.Front;
                                helper.GetProperty("_ZTest").floatValue = (float)CompareFunction.LessEqual;
                            }
                        }
                        
                    }
                );
            }

            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutVertexOffset,3,"顶点偏移","_VertexOffset_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_VERTEX_OFFSET_ON,isIndentBlock:true,fontStyle:FontStyle.Bold,drawBlock:
                isToggle =>
                {
                    // if (isToggle)
                    // {
                        helper.DrawTexture("顶点偏移贴图","_VertexOffset_Map",drawScaleOffset:true,drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_VERTEXOFFSETMAP,flagIndex:2);
                        DrawCustomDataSelect("顶点扰动X轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_X,1);
                        DrawCustomDataSelect("顶点扰动Y轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_Y,1);
                        DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeVertexOffsetMap,4,"顶点偏移贴图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_VERTEX_OFFSET_MAP,0);

                        helper.DrawVector4In2Line("_VertexOffset_Vec","顶点偏移动画");
                        helper.DrawVector4Componet("顶点偏移强度","_VertexOffset_Vec","z",false);
                        DrawCustomDataSelect("顶点扰动强度自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEXOFFSET_INTENSITY,1);
                        helper.DrawToggle("顶点偏移从零开始","_VertexOffset_StartFromZero",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_VERTEXOFFSET_START_FROM_ZERO,1);
                        helper.DrawToggle("顶点偏移使用法线方向","_VertexOffset_NormalDir_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_VERTEX_OFFSET_NORMAL_DIR,isIndentBlock:false,drawBlock:
                            isToggle =>
                            {
                                if (!isToggle)
                                {
                                    matEditor.ShaderProperty(helper.GetProperty("_VertexOffset_CustomDir"),"顶点偏移本地方向");
                                }
                            });
                        DrawToggleFoldOut(W9ParticleShaderFlags.foldOutVertexOffsetMask,4,"顶点偏移遮罩","_VertexOffset_Mask_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_VERTEXOFFSET_MASKMAP,1,
                            drawBlock:isMaskToggle =>
                            {
                                helper.DrawTexture("顶点偏移遮罩图","_VertexOffset_MaskMap",drawScaleOffset:true,drawWrapMode:true,flagBitsName:W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_VERTEXOFFSET_MASKMAP,flagIndex:2);
                                DrawCustomDataSelect("顶点扰动遮罩X轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_3_CUSTOMDATA_VERTEX_OFFSET_MASK_X,3);
                                DrawCustomDataSelect("顶点扰动遮罩Y轴偏移自定义曲线",W9ParticleShaderFlags.FLAGBIT_POS_3_CUSTOMDATA_VERTEX_OFFSET_MASK_Y,3);
                                DrawUVModeSelect(W9ParticleShaderFlags.foldOutBit2UVModeVertexOffsetMaskMap,4,"顶点偏移遮罩图UV来源",W9ParticleShaderFlags.FLAG_BIT_UVMODE_POS_0_VERTEX_OFFSET_MASKMAP,0);
                                helper.DrawVector4In2Line("_VertexOffset_MaskMap_Vec","顶点偏移遮罩动画");
                                helper.DrawVector4Componet("顶点偏移遮罩强度","_VertexOffset_MaskMap_Vec","z",true);
                            });
                    // }
                    
                });
            
            if (!_uieffectEnabled)
            {
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutParallexMapping,3,"遮蔽视差", "_ParallaxMapping_Toggle", shaderKeyword: "_PARALLAX_MAPPING",
                    isIndentBlock: true,fontStyle:FontStyle.Bold, drawBlock:
                    isTogggle =>
                    {
                        // if (isTogggle)
                        // {
                            helper.DrawTexture("视差贴图", "_ParallaxMapping_Map", drawWrapMode: true,
                                flagBitsName: W9ParticleShaderFlags.FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP, flagIndex: 2);
                            helper.DrawSlider("视差", "_ParallaxMapping_Intensity", 0, 0.1f);
                            
                            helper.DrawVector4Componet("遮蔽视差最小层数","_ParallaxMapping_Vec","x",true,0f,100f);
                            helper.DrawVector4Componet("遮蔽视差最大层数","_ParallaxMapping_Vec","y",true,0f,100f);
                            Vector4 parallexMappingVecValue = helper.GetProperty("_ParallaxMapping_Vec").vectorValue;
                            if (parallexMappingVecValue.y < parallexMappingVecValue.x+1)
                            {
                                parallexMappingVecValue.y = parallexMappingVecValue.x+1;
                            }
                            helper.GetProperty("_ParallaxMapping_Vec").vectorValue = parallexMappingVecValue;
                            if (parallexMappingVecValue.y > 20f)
                            {
                                EditorGUILayout.HelpBox("遮蔽视差层数过高将影响性能",MessageType.Warning);
                            }



                            // }
                    });
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutPortal,4,"模板视差", "_Portal_Toggle", fontStyle:FontStyle.Bold,drawBlock: isPortalToggle =>
                {
                    // if (isPortalToggle)
                    // {
                        if (isPortalToggle)
                        {
                            isCustomedStencil = true;
                        }
                        helper.DrawToggle("模板视差蒙版", "_Portal_MaskToggle", drawBlock: isPortalMaskToggle =>
                        {

                            if (isPortalMaskToggle)
                            {
                                for (int i = 0; i < mats.Count; i++)
                                {
                                    StencilTestHelper.SetMaterialStencil(mats[i], "ParticalBasePortalMask",
                                        _stencilValuesConfig, out int Ignore);
                                }

                                if (helper.GetProperty("_TransparentMode").floatValue == (float)TransparentMode.Transparent)
                                {
                                    helper.GetProperty("_TransparentMode").floatValue = (float)TransparentMode.CutOff;
                                }

                                helper.GetProperty("_ZTest").floatValue = (float)CompareFunction.LessEqual;
                            }
                            else if(isPortalToggle)
                            {
                                for (int i = 0; i < mats.Count; i++)
                                {
                                    StencilTestHelper.SetMaterialStencil(mats[i], "ParticalBasePortal",
                                        _stencilValuesConfig, out int Ignore);
                                }

                                helper.GetProperty("_ZTest").floatValue = (float)CompareFunction.GreaterEqual;
                            }

                        });
                    // }
                },drawEndChangeCheck: (isToggle) =>
                    {
                        if (!isToggle)
                        {
                            helper.GetProperty("_CustomStencilTest").floatValue = 0f;
                            helper.GetProperty("_TransparentMode").floatValue = (float)TransparentMode.Transparent;
                            helper.GetProperty("_ZTest").floatValue = (float)CompareFunction.LessEqual;
                        }
                    }
                );
            }
            
            

            //粒子序列帧融帧的逻辑，是将UV0为第一格，UV1234推到第二格，中间用AnimBlend融合）。所以多UV是必然和这个矛盾的。
            if (mats.Count == 1)
            {
                helper.DrawToggle("序列帧融帧(丝滑)", "_FlipbookBlending", shaderKeyword: "_FLIPBOOKBLENDING_ON",
                    fontStyle: FontStyle.Bold, drawBlock: (isToggle) =>
                    {
                        if (isToggle)
                        {
                            if (_meshSourceMode == MeshSourceMode.Particle)
                            {
                                if (shaderFlags[0].CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.SpecialUVChannel))
                                {
                                    EditorGUILayout.HelpBox("序列帧融帧和特殊UV通道同时开启，粒子序列帧应该影响UV0和UV1两个通道，特殊通道只能使用UV3（原始UV）",
                                        MessageType.Warning);
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("AnimationSheet的AffectUVChannel需要有UV0和UV1",
                                        MessageType.Info);
                                }
                            }
                            else if (_meshSourceMode == MeshSourceMode.Mesh)
                            {
                                EditorGUILayout.HelpBox("需要添加AnimationSheetHelper脚本", MessageType.Info);
                            }
                        }
                    });
            }


        }

        public void DrawTaOptions()
        {
            if (!_uieffectEnabled)
            {
                DrawToggleFoldOut(W9ParticleShaderFlags.foldOutZOffset,4,"深度偏移", "_ZOffset_Toggle", fontStyle:FontStyle.Bold,drawBlock: (isToggle) =>
                {
                    // if (isToggle)
                    // {
                        matEditor.ShaderProperty(helper.GetProperty("_offsetFactor"), "OffsetFactor");
                        matEditor.ShaderProperty(helper.GetProperty("_offsetUnits"), "Offset单位");
                    // }
                    if(!isToggle)
                    {
                        helper.GetProperty("_offsetFactor").floatValue = 0;
                        helper.GetProperty("_offsetUnits").floatValue = 0;
                    }
                });
            }

            if (!_uieffectEnabled||_uiParticleEnabled)
            {
                #region CustomData旧版本
/*
                


                {
                    
                    EditorGUILayout.Space();

                    bool isCustomedData1X = false,
                        isCustomedData1Y = false,
                        isCustomedData1Z = false,
                        isCustomedData1W = false,
                        isCustomedData2X = false,
                        isCustomedData2Y = false,
                        isCustomedData2Z = false,
                        isCustomedData2W = false;
                
               
                // helper.DrawToggle("CustomData1X主贴图X轴偏移","_CustomData1X_MainTexOffsetX_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_MAINTEXOFFSETX,drawBlock:
                //     (isToggle) => { isCustomedData1X = isToggle;});
                // helper.DrawToggle("CustomData1Y主贴图Y轴偏移","_CustomData1Y_MainTexOffsetY_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_MAINTEXOFFSETY,drawBlock:
                //     (isToggle) => { isCustomedData1Y = isToggle;});
                // helper.DrawToggle("CustomData1Z溶解强度","_CustomData1Z_Dissolve_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_DISSOLVE_ON,drawBlock:
                //     (isToggle) => { isCustomedData1Z = isToggle;});
                // helper.DrawToggle("CustomData1W色相偏移","_CustomData1W_HueShift_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_HUESHIFT,drawBlock:
                //     (isToggle) => { isCustomedData1W = isToggle;});
                // helper.DrawToggle("CustomData2XMask图X轴偏移","_CustomData2X_MaskMapOffsetX_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_MASKMAPOFFSETX,drawBlock:
                //     (isToggle) => { isCustomedData2X = isToggle;});
                // helper.DrawToggle("CustomData2YMask图Y轴偏移","_CustomData2Y_MaskMapOffsetY_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_MASKMAPOFFSETY,drawBlock:
                //     (isToggle) => { isCustomedData2Y = isToggle;});
                // helper.DrawToggle("CustomData2Z菲涅尔范围","_CustomData2Z_FresnelOffset_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Z_FRESNELOFFSET,drawBlock:
                //     (isToggle) => { isCustomedData2Z = isToggle;});

                    helper.DrawPopUp("CustomData1X", "_CustomData1X_MainTexOffsetX_Toggle", CustomData1XModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData1XMode)f)
                            {
                                case CustomData1XMode.none:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_MAINTEXOFFSETX, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_DISSOLVETEXOFFSETX, index: 1);
                                    isCustomedData1X = false;
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_X,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_X,1);
                                    break;
                                case CustomData1XMode.MainTexOffsetX:
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_MAINTEXOFFSETX, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_DISSOLVETEXOFFSETX, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1X,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_X,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_X,1);
                                    isCustomedData1X = true;
                                    break;
                                case CustomData1XMode.DissolveTexOffseX:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_MAINTEXOFFSETX, index: 0);
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1X_DISSOLVETEXOFFSETX, index: 1);
                                    isCustomedData1X = true;
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_X,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1X,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_X,1);
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData1Y", "_CustomData1Y_MainTexOffsetY_Toggle", CustomData1YModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData1YMode)f)
                            {
                                case CustomData1YMode.none:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_MAINTEXOFFSETY, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_DISSOLVETEXOFFSETY, index: 1);
                                    isCustomedData1Y = false;
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_Y,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_Y,1);
                                    break;
                                case CustomData1YMode.MainTexOffsetY:
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_MAINTEXOFFSETY, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_DISSOLVETEXOFFSETY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1Y,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_Y,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_Y,1);
                                    isCustomedData1Y = true;
                                    break;
                                case CustomData1YMode.DissolveOffsexY:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_MAINTEXOFFSETY, index: 0);
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Y_DISSOLVETEXOFFSETY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_Y,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1Y,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_Y,1);
                                    isCustomedData1Y = true;
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData1Z", "_CustomData1Z_Dissolve_Toggle", CustomData1ZModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData1ZMode)f)
                            {
                                case CustomData1ZMode.none:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_DISSOLVE_ON, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_NOISE_INTENSITY, index: 1);
                                    isCustomedData1Z = false;
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_DISSOLVE_INTENSITY,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_NOISE_INTENSITY,1);
                                    break;
                                case CustomData1ZMode.DissolveIntensity:
                                    shaderFlag.SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_DISSOLVE_ON,
                                        index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_NOISE_INTENSITY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1Z,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_DISSOLVE_INTENSITY,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_NOISE_INTENSITY,1);
                                    isCustomedData1Z = true;
                                    break;
                                case CustomData1ZMode.NoiseIntensity:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_DISSOLVE_ON, index: 0);
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1Z_NOISE_INTENSITY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_DISSOLVE_INTENSITY,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1Z,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_NOISE_INTENSITY,1);
                                    isCustomedData1Z = true;
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData1W", "_CustomData1W_HueShift_Toggle", CustomData1WModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData1WMode)f)
                            {
                                case CustomData1WMode.none:
                                    shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_HUESHIFT,
                                        index: 0);
                                    shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_SATURATE,
                                        index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_HUESHIFT,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_SATURATE,1);
                                    isCustomedData1W = false;
                                    break;
                                case CustomData1WMode.HueShift:
                                    shaderFlag.SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_HUESHIFT,
                                        index: 0);
                                    shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_SATURATE,
                                        index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1W,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_HUESHIFT,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_SATURATE,1);
                                    isCustomedData1W = true;
                                    break;
                                case CustomData1WMode.Saturate:
                                    shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_HUESHIFT,
                                        index: 0);
                                    shaderFlag.SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1W_SATURATE,
                                        index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_HUESHIFT,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData1W,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_SATURATE,1);
                                    isCustomedData1W = true;
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData2X", "_CustomData2X_MaskMapOffsetX_Toggle", CustomData2XModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData2XMode)f)
                            {
                                case CustomData2XMode.none:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_MASKMAPOFFSETX, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_VERTEXOFFSETX, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_X,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_X,1);
                                    isCustomedData2X = false;
                                    break;
                                case CustomData2XMode.MaskOffsetX:
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_MASKMAPOFFSETX, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_VERTEXOFFSETX, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2X,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_X,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_X,1);
                                    isCustomedData2X = true;
                                    break;
                                case CustomData2XMode.VertexOffsetX:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_MASKMAPOFFSETX, index: 0);
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2X_VERTEXOFFSETX, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_X,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2X,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_X,1);
                                    isCustomedData2X = true;
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData2Y", "_CustomData2Y_MaskMapOffsetY_Toggle", CustomData2YModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData2YMode)f)
                            {
                                case CustomData2YMode.none:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_MASKMAPOFFSETY, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_VERTEXOFFSETY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_Y,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_Y,1);
                                    isCustomedData2Y = false;
                                    break;
                                case CustomData2YMode.MaskOffsetY:
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_MASKMAPOFFSETY, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_VERTEXOFFSETY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2Y,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_Y,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_Y,1);
                                    isCustomedData2Y = true;
                                    break;
                                case CustomData2YMode.VertexOffsetY:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_MASKMAPOFFSETY, index: 0);
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Y_VERTEXOFFSETY, index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_Y,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2Y,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_Y,1);
                                    isCustomedData2Y = true;
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData2Z", "_CustomData2Z_FresnelOffset_Toggle", CustomData2ZModeName,
                        drawBlock: (f) =>
                        {
                            switch ((CustomData2ZMode)f)
                            {
                                case CustomData2ZMode.none:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Z_FRESNELOFFSET, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_CUSTOMDATA2Z_VERTEXOFFSET_INTENSITY,
                                        index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_FRESNEL_OFFSET,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEXOFFSET_INTENSITY,1);
                                    isCustomedData2Z = false;
                                    break;
                                case CustomData2ZMode.FresnelOffset:
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Z_FRESNELOFFSET, index: 0);
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_CUSTOMDATA2Z_VERTEXOFFSET_INTENSITY,
                                        index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2Z,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_FRESNEL_OFFSET,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEXOFFSET_INTENSITY,1);
                                    isCustomedData2Z = true;
                                    break;
                                case CustomData2ZMode.VertexOffsetIntensity:
                                    shaderFlag.ClearFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2Z_FRESNELOFFSET, index: 0);
                                    shaderFlag.SetFlagBits(
                                        W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_CUSTOMDATA2Z_VERTEXOFFSET_INTENSITY,
                                        index: 1);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_FRESNEL_OFFSET,0);
                                    shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2Z,W9ParticleShaderFlags.FLAGBIT_POS_1_CUSTOMDATA_VERTEXOFFSET_INTENSITY,1);
                                    isCustomedData2Z = true;
                                    break;
                            }
                        });

                    helper.DrawPopUp("CustomData2W", "_CustomData2W_Toggle", CustomData2WModeName, drawBlock: (f) =>
                    {
                        switch ((CustomData2WMode)f)
                        {
                            case CustomData2WMode.none:
                                shaderFlag.ClearFlagBits(
                                    W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2W_CHORATICABERRAT_INTENSITY,
                                    index: 1);
                                shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.Off,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_CHORATICABERRAT_INTENSITY,0);

                                isCustomedData2W = false;
                                break;
                            case CustomData2WMode.ChoraticaberratIntensity:
                                shaderFlag.SetFlagBits(
                                    W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2W_CHORATICABERRAT_INTENSITY,
                                    index: 1);
                                shaderFlag.SetCustomDataFlag(W9ParticleShaderFlags.CutomDataComponent.CustomData2W,W9ParticleShaderFlags.FLAGBIT_POS_0_CUSTOMDATA_CHORATICABERRAT_INTENSITY,0);
                                isCustomedData2W = true;
                                break;
                        }
                    });


                    bool isCustomedData1 = isCustomedData1X || isCustomedData1Y || isCustomedData1Z || isCustomedData1W;
                    isCustomedData1 = isCustomedData1 || shaderFlag.IsCustomData1On();
                  
                    if (isCustomedData1)
                    {
                        shaderFlag.SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1_ON);
                    }
                    else
                    {
                        shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1_ON);
                    } 
                    
                    bool isCustomedData2 = isCustomedData2X || isCustomedData2Y || isCustomedData2Z ||isCustomedData2W;
                    isCustomedData2 = isCustomedData2 || shaderFlag.IsCustomData2On();
                    if (isCustomedData2)
                    {
                        shaderFlag.SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2_ON);
                    }
                    else
                    {
                        shaderFlag.ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2_ON);
                    }
                    //旧版本结束。
                }
                */
                #endregion

                for (int i = 0; i < mats.Count; i++)
                {
                    if (shaderFlags[i].IsCustomData1On())
                    {
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1_ON);
                    }
                    else
                    {
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1_ON);
                    }

                    if (shaderFlags[i].IsCustomData2On())
                    {
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2_ON);
                    }
                    else
                    {
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2_ON);
                    }
                }

            }
            // matEditor.RenderQueueField();
            helper.DrawRenderQueue(helper.GetProperty("_QueueBias"));

            if (isCustomedStencil)
            {
                helper.GetProperty("_CustomStencilTest").floatValue = 1f;
            }
           
            DrawToggleFoldOut(W9ParticleShaderFlags.foldOutCustomStencilTest,4,"模板调试开关","_CustomStencilTest",drawBlock: isTogle =>
            {
                    matEditor.ShaderProperty(helper.GetProperty("_Stencil"),"模板值");
                    matEditor.ShaderProperty(helper.GetProperty("_StencilComp"),"模板比较方式");
                    matEditor.ShaderProperty(helper.GetProperty("_StencilOp"),"模板处理方式");
                    isCustomedStencil = isTogle;
                
            });

            if (!isCustomedStencil && !_uieffectEnabled)
            {
                for (int i = 0; i < mats.Count; i++)
                {
                    StencilTestHelper.SetMaterialStencil(mats[i], "ParticleBaseDefault", _stencilValuesConfig, out int ignore);
                }
            }
        }
        void DrawNoiseAffectBlock(Action drawBlock)
        {
            EditorGUI.BeginDisabledGroup(!_noiseEnabled);
            drawBlock();
            EditorGUI.EndDisabledGroup();
        }

        public string[] blendModeNames =
        {
            "透明度混合AlphaBlend",
            "预乘PreMultiply",
            "叠加Additive",
            "正片叠底Multiply"
        };
        
        public enum BlendMode
        {
            Alpha, // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Premultiply, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive,
            Multiply,
            Opaque
        }
        
        public enum TimeMode
        {
            Default,
            UnScaleTime,
            ScriptableTime
        }
        
        public enum RenderFace
        {
            Front = 2,
            Back = 1,
            Both = 0
        }

        public enum FresnelMode
        {
            Color = 0,
            Fade = 1
        }

        public string[] transparentModeNames =
        {
            "不透明Opaque",
            "半透明Transparent",
            "不透明裁剪CutOff"
        };
        
        public enum TransparentMode
        {
            Opaque = 0,
            Transparent = 1,
            CutOff = 2
        }
        
        public enum CustomData1XMode
        {
            none = 0,
            MainTexOffsetX = 1,
            DissolveTexOffseX = 2
        }

        private string[] CustomData1XModeName = new string[]
        {
            "无",
            "主贴图X轴偏移",
            "溶解贴图X轴偏移"
        };
        
        
        public enum CustomData1YMode
        {
            none = 0,
            MainTexOffsetY = 1,
            DissolveOffsexY = 2
        }
        
        private string[] CustomData1YModeName = new string[]
        {
            "无",
            "主贴图Y轴偏移",
            "溶解贴图Y轴偏移"
        };
        public enum CustomData1ZMode
        {
            none = 0,
            DissolveIntensity = 1,
            NoiseIntensity = 2
        }
        
        private string[] CustomData1ZModeName = new string[]
        {
            "无",
            "溶解强度",
            "扭曲强度"
        };
        
        public enum CustomData1WMode
        {
            none = 0,
            HueShift = 1,
            Saturate = 2
        }
        private string[] CustomData1WModeName = new string[]
        {
            "无",
            "色相偏移",
            "饱和度强度"
        };
        
        public enum CustomData2XMode
        {
            none = 0,
            MaskOffsetX = 1,
            VertexOffsetX = 2
        }
        
        private string[] CustomData2XModeName = new string[]
        {
            "无",
            "Mask图X轴偏移",
            "顶点扰动X轴偏移"
        };
        
        public enum CustomData2YMode
        {
            none = 0,
            MaskOffsetY = 1,
            VertexOffsetY = 2
        }
        
        private string[] CustomData2YModeName = new string[]
        {
            "无",
            "Mask图Y轴偏移",
            "顶点扰动Y轴偏移"
        };
        public enum CustomData2ZMode
        {
            none = 0,
            FresnelOffset = 1,
            VertexOffsetIntensity = 2
        }
        private string[] CustomData2ZModeName = new string[]
        {
            "无",
            "菲涅尔范围",
            "顶点扰动强度"
        };
        public enum CustomData2WMode
        {
            none = 0,
            ChoraticaberratIntensity = 1
        }
        private string[] CustomData2WModeName = new string[]
        {
            "无",
            "通道偏移强度"
        };
        
        void DoAfterDraw()
        {
            for (int i = 0; i < mats.Count; i++)
            {
                switch (_meshSourceMode)
                {
                    case MeshSourceMode.Particle:
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM, index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UV_FROM_MESH, index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UIEFFECT_ON, index: 0);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE,
                            index: 1);

                        //如果是粒子系统，则不需要走AnimationSheetHelper
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_ANIMATION_SHEET_HELPER);
                        break;
                    case MeshSourceMode.Mesh:
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM,
                            index: 1);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UV_FROM_MESH, index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UIEFFECT_ON, index: 0);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE,
                            index: 1);
                        break;
                    case MeshSourceMode.UIEffectRawImage:
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UV_FROM_MESH, index: 1);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UIEFFECT_ON, index: 0);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE,
                            index: 1);
                        break;
                    case MeshSourceMode.UIEffectSprite:
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UV_FROM_MESH, index: 1);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UIEFFECT_ON, index: 0);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE,
                            index: 1);
                        break;
                    case MeshSourceMode.UIEffectBaseMap:
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM,
                            index: 1);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UV_FROM_MESH, index: 1);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UIEFFECT_ON, index: 0);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE,
                            index: 1);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE,
                            index: 1);
                        break;
                }

                if (!shaderFlags[i].CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.SpecialUVChannel))
                {
                    shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD1, index: 1);
                    shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD2, index: 1);
                }

                if (!shaderFlags[i].CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.Cylinder))
                {
                    shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_CYLINDER_CORDINATE, index: 1);
                }


                TransparentMode transparentMode = (TransparentMode)mats[i].GetFloat("_TransparentMode");
                switch (_transparentMode)
                {
                    case TransparentMode.Opaque:
                        mats[i].SetInt("_ZWrite", (int)1);
                        mats[i].renderQueue = 2100 + (int)helper.GetProperty("_QueueBias").floatValue; //3D粒子永远最前显示
                        mats[i].SetInt("_Blend", (int)BlendMode.Opaque);
                        break;
                    case TransparentMode.Transparent:
                        if (helper.GetProperty("_ForceZWriteToggle").floatValue > 0.5f)
                        {
                            mats[i].SetInt("_ZWrite", (int)1);
                        }
                        else
                        {
                            mats[i].SetInt("_ZWrite", (int)0);
                        }

                        int defaultQueue = 3100;
                        if (_uieffectEnabled||_uiParticleEnabled)
                        {
                            defaultQueue = 3000;
                        }

                        mats[i].renderQueue = defaultQueue + (int)helper.GetProperty("_QueueBias").floatValue; //3D粒子永远最前显示

                        BlendMode bm = (BlendMode)mats[i].GetFloat("_Blend");
                        if (bm == BlendMode.Opaque)
                        {
                            mats[i].SetFloat("_Blend", (float)BlendMode.Alpha); //如果设置错误则强制设置。
                        }

                        break;
                    case TransparentMode.CutOff:
                        mats[i].SetInt("_ZWrite", (int)1);
                        mats[i].renderQueue = 2450 + (int)helper.GetProperty("_QueueBias").floatValue; //3D粒子永远最前显示
                        mats[i].SetInt("_Blend", (int)BlendMode.Opaque);
                        break;
                }

                if (_transparentMode == TransparentMode.CutOff)
                {
                    mats[i].EnableKeyword("_ALPHATEST_ON");
                }
                else
                {
                    mats[i].DisableKeyword("_ALPHATEST_ON");
                }



                // blendMode
                BlendMode blendMode = (BlendMode)mats[i].GetFloat("_Blend");

                switch (blendMode)
                {
                    case BlendMode.Alpha:
                        mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        // mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Premultiply:
                        mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        // mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Additive:
                        mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        //  mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Multiply:
                        mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                        mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        // mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        // mat.EnableKeyword("_ALPHAMODULATE_ON");
                        break;
                    case BlendMode.Opaque:
                        mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        break;
                }

                TimeMode timeMode = (TimeMode)mats[i].GetFloat("_TimeMode");

                switch (timeMode)
                {
                    case TimeMode.Default:
                        // setMaterialFlags(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UNSCALETIME_ON, false);
                        // setMaterialFlags(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON, false);
                        shaderFlags[i].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UNSCALETIME_ON);
                        shaderFlags[i].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON);
                        break;
                    case TimeMode.UnScaleTime:
                        // setMaterialFlags(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UNSCALETIME_ON, true);
                        // setMaterialFlags(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON, false);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UNSCALETIME_ON);
                        shaderFlags[i].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON);
                        break;
                    case TimeMode.ScriptableTime:
                        // setMaterialFlags(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UNSCALETIME_ON, false);
                        // setMaterialFlags(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON, true);
                        shaderFlags[i].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UNSCALETIME_ON);
                        shaderFlags[i].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON);
                        break;
                }
            }
        }
        
        public static GUIContent VertexStreams = new GUIContent("顶点流统计",
            "The vertex streams needed for this Material to function properly.");
  
        public static string streamPositionText = "Position (POSITION.xyz)";
        public static string streamNormalText = "Normal (NORMAL.xyz)";
        public static string streamColorText = "Color (COLOR.xyzw)";
        public static string streamUVText = "UV (TEXCOORD0.xy)";
        public static string streamUV3Text = "UV3 (TEXCOORD0.zw)";
        public static string streamUV2Text = "UV2 (TEXCOORD0.zw)";
        public static string streamUV2AndAnimBlendText = "UV2 (TEXCOORD3.zw)";
        public static string streamUV3AndAnimBlendText = "UV3 (TEXCOORD3.zw)";
        public static string streamAnimBlendText = "AnimBlend (TEXCOORD3.x)";
        public static string streamTangentText = "Tangent (TANGENT.xyzw)";
        public static string streamCustom1Text = "Custom1.xyzw(TEXCOORD1.xyzw)";
        public static string streamCustom2Text = "Custom2.xyzw(TEXCOORD2.xyzw)";


        public static GUIContent streamApplyToAllSystemsText = new GUIContent("使粒子与材质顶点流相同",
            "Apply the vertex stream layout to all Particle Systems using this material");

        public static string undoApplyCustomVertexStreams = L10n.Tr("Apply custom vertex streams from material");
        
        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();

        private bool checkIsParicleSystem = false;
        void CacheRenderersUsingThisMaterial(Material material, int matID)
        {
            checkIsParicleSystem = false;
            m_RenderersUsingThisMaterial.Clear();
            #if UNITY_2022_1_OR_NEWER
            ParticleSystemRenderer[] renderers =
                UnityEngine.Object.FindObjectsByType(typeof(ParticleSystemRenderer),FindObjectsSortMode.None) as ParticleSystemRenderer[];
            #else
            ParticleSystemRenderer[] renderers =
                UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            #endif
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material || renderer.trailMaterial == material)
                {
                    checkIsParicleSystem = true;
                    shaderFlags[matID].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_ANIMATION_SHEET_HELPER,index:1);//如果是粒子系统用，就主动关掉Helper的类型。
                    m_RenderersUsingThisMaterial.Add(renderer);
                }
            }
        }
        
        //雨轩：UnityEditorInternal命名空间下提供 一个类ReorderableList可以实现通过拖曳来达到列表元素的重新排序。
        private static ReorderableList vertexStreamList;
        //构建粒子系统顶点流界面
        public void DoVertexStreamsArea(Material material, List<ParticleSystemRenderer> renderers,
            int matID, bool useLighting = false)
        {
            EditorGUILayout.Space();
     
            // bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
            bool useFlipbookBlending = material.IsKeywordEnabled("_FLIPBOOKBLENDING_ON");
            bool useSpecialUVChannel = shaderFlags[matID].CheckIsUVModeOn(W9ParticleShaderFlags.UVMode.SpecialUVChannel);
            bool isUseUV3ForSpecialUV =
                shaderFlags[matID].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD2, index:1);
            // bool CustomDataEnabled = (material.GetFloat("_CustomData") > 0.0f);
            bool isCustomData1 = shaderFlags[matID].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA1_ON);
            bool isCustomData2 = shaderFlags[matID].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_CUSTOMDATA2_ON);
            

            // Build the list of expected vertex streams
            List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
            List<string> streamList = new List<string>();
            
            streams.Add(ParticleSystemVertexStream.Position); //必然会传递有顶点位置信息
            streamList.Add(streamPositionText); //记录顶点位置信息，给GUI面板用

            bool needTangent = false;
            bool needNormal = false;

            needNormal = (material.GetFloat("_VertexOffset_NormalDir_Toggle") > 0.5f);
            
            //如果有灯光，必有法线信息。如果有法线贴图，必有顶点切线法线信息。
            //菲涅尔效果需要用到法线内容。
            if (material.GetFloat("_fresnelEnabled") > 0.5f)
            {
                needNormal = true;
                needTangent = true;
            }

            if (material.GetFloat("_ParallaxMapping_Toggle") > 0.5f)
            {
                needTangent = true;
            }

            bool useUV3AsMainUV = shaderFlags[matID].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_USETEXCOORD2);

            if (needTangent)
            {
                streams.Add(ParticleSystemVertexStream.Tangent);
                streamList.Add(streamTangentText);
            }

            if (needNormal)
            {
                streams.Add(ParticleSystemVertexStream.Normal);
                streamList.Add(streamNormalText);
            }

            //粒子着色器，必有顶点颜色信息。
            streams.Add(ParticleSystemVertexStream.Color);
            streamList.Add(streamColorText);

            //TEXCOORD0填充
            //必有顶点第一套UV信息。
            streams.Add(ParticleSystemVertexStream.UV);
            streamList.Add(streamUVText);
            //在做动画序列帧时，需要:TEXCOORD1(xy为正常uv，zw为Blend用的第二套uv)，:TEXCOORD2(x为Blend混合值)
            if (useFlipbookBlending && useSpecialUVChannel)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streamList.Add(streamUV2Text);
            }
            else if (useSpecialUVChannel & !useFlipbookBlending)
            {
                 
                if (isUseUV3ForSpecialUV)
                {
                    streams.Add(ParticleSystemVertexStream.UV3);
                    streamList.Add(streamUV3Text);
                }
                else
                {
                    streams.Add(ParticleSystemVertexStream.UV2);
                    streamList.Add(streamUV2Text);
                }
                
            }
            else if (useFlipbookBlending & !useSpecialUVChannel)
            {
                if (!streams.Contains(ParticleSystemVertexStream.UV2))
                {
                    streams.Add(ParticleSystemVertexStream.UV2);
                    streamList.Add(streamUV2Text);
                }
            }
            else if(isCustomData1 || isCustomData2)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streamList.Add(streamUV2Text);
            }


            //填充TEXCOORD1
            bool isFillSkipUV2 = false;//因为如果要使用UV3，粒子系统必须填充UV2才能激活
            if (isCustomData1 || isCustomData2 || useFlipbookBlending)
            {
                streams.Add(ParticleSystemVertexStream.Custom1XYZW);
                streamList.Add(streamCustom1Text);
            }
            else if(useSpecialUVChannel & isUseUV3ForSpecialUV)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streamList.Add("TEXCOORD1.xy");
                isFillSkipUV2 = true;
            }

            //填充TEXCOORD2
            if (isCustomData2 || useFlipbookBlending)
            {
                streams.Add(ParticleSystemVertexStream.Custom2XYZW);
                streamList.Add(streamCustom2Text);
            }
            else if(useSpecialUVChannel & isUseUV3ForSpecialUV & !isFillSkipUV2)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streamList.Add("TEXCOORD2.xy");
                isFillSkipUV2 = true;
            }

            //填充TEXCOORD3
            if (useFlipbookBlending)
            {
                streams.Add(ParticleSystemVertexStream.AnimBlend);
                streamList.Add(streamAnimBlendText);
                if (useSpecialUVChannel)
                {
                    if (isUseUV3ForSpecialUV)
                    {
                        streams.Add(ParticleSystemVertexStream.UV3);
                        streamList.Add(streamUV3AndAnimBlendText);
                    }
                }
            }
            else if(useSpecialUVChannel & isUseUV3ForSpecialUV & !isFillSkipUV2)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streamList.Add("TEXCOORD3.xy");
            }
            
            // //如果是融合序列帧，则要跨越到Texcoord3
            // //如果是使用UV3，则需要用一个UV2开启来让粒子系统输出UV3
            // if (useFlipbookBlending || isUseUV3ForSpecialUV)
            // {
            //     
            //     //利用Custom1XYZW跨过TEXCOORD1;
            //     if (!streams.Contains(ParticleSystemVertexStream.Custom1XYZW))
            //     {
            //      
            //     }
            //     
            //     //利用Custom1XYZW跨过TEXCOORD2;
            //     if (!streams.Contains(ParticleSystemVertexStream.Custom2XYZW))
            //     {
            //         streams.Add(ParticleSystemVertexStream.Custom2XYZW);
            //         streamList.Add(streamCustom2Text);
            //     }
            //
            //     
            // }
            //
            //
            // if (isCustomData1 || isCustomData2) //是否在使用后可以自定义开启，不需要另外写开关
            // {
            //     if (!streams.Contains(ParticleSystemVertexStream.UV2))
            //     {
            //          streams.Add(ParticleSystemVertexStream.UV2); //需要跨过UV2,所以加入UV2
            //          streamList.Add(streamUV2Text);
            //     }
            //
            //     if (!streams.Contains(ParticleSystemVertexStream.Custom1XYZW))
            //     {
            //          streams.Add(ParticleSystemVertexStream.Custom1XYZW);
            //          streamList.Add(streamCustom1Text);
            //     }
            //
            //     if (isCustomData2)
            //     {
            //          if (!streams.Contains(ParticleSystemVertexStream.Custom2XYZW))
            //          {
            //              streams.Add(ParticleSystemVertexStream.Custom2XYZW);
            //              streamList.Add(streamCustom2Text);
            //          }
            //     }
            // }


            //可排序列表绘制。
            //创建一个可排序列表
            vertexStreamList = new ReorderableList(streamList, typeof(string), false, true, false, false);

            //创建表头。ReorderableList下面还有很多回调。可以按需选择。
            vertexStreamList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Vertex Streams"); };

            vertexStreamList.DoLayoutList(); //执行表格绘制。

            // Display a warning if any renderers have incorrect vertex streams
            string Warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in renderers) //每个使用该材质的粒子系统都会进行比较
            {
                renderer.GetActiveVertexStreams(rendererStreams); //获得ParticleSystemRenderer的顶点流
                if (!rendererStreams.SequenceEqual(streams)) //重点！是否和我们拼装的顶点流一致。
                    Warnings += "-" + renderer.name + "\n";
            }

            //
            if (!string.IsNullOrEmpty(Warnings))
            {
                //如果有Warning
                EditorGUILayout.HelpBox(
                    "下面的粒子系统Renderer顶点流不正确:\n" +
                    Warnings, MessageType.Error, true);
                // Set the streams on all systems using this materialz
                if (GUILayout.Button("使粒子与材质顶点流相同", EditorStyles.miniButton,
                        GUILayout.ExpandWidth(true)))
                {
                    //做一个撤回记录。
                    Undo.RecordObjects(renderers.Where(r => r != null).ToArray(), "Apply custom vertex streams from material");

                    //重点！直接赋值我们拼装好的顶点流。
                    foreach (ParticleSystemRenderer renderer in renderers)
                    {
                        renderer.SetActiveVertexStreams(streams);
                        
                    }
                }
            }
            
            //从2022.3.11开始添加这个功能。
            #if UNITY_2022_3_OR_NEWER && !(UNITY_2022_3_0 ||UNITY_2022_3_1||UNITY_2022_3_2||UNITY_2022_3_3||UNITY_2022_3_4||UNITY_2022_3_5||UNITY_2022_3_6||UNITY_2022_3_7||UNITY_2022_3_8||UNITY_2022_3_9||UNITY_2022_3_10)
            // Display a warning if any renderers have incorrect vertex streams
            string trailWarnings = "";
            List<ParticleSystemVertexStream> trailRendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in renderers) //每个使用该材质的粒子系统都会进行比较
            {
                renderer.GetActiveTrailVertexStreams(trailRendererStreams); //获得ParticleSystemRenderer的顶点流
                if (!trailRendererStreams.SequenceEqual(streams)) //重点！是否和我们拼装的顶点流一致。
                    trailWarnings += "-" + renderer.name + "\n";
            }
            
            if (!string.IsNullOrEmpty(trailWarnings))
            {
                //如果有Warning
                EditorGUILayout.HelpBox(
                    "下面的粒子系统Renderer拖尾顶点流不正确:\n" +
                    trailWarnings, MessageType.Error, true);
                // Set the streams on all systems using this material
                if (GUILayout.Button("使粒子拖尾与材质顶点流相同", EditorStyles.miniButton,
                        GUILayout.ExpandWidth(true)))
                {
                    //做一个撤回记录。
                    Undo.RecordObjects(renderers.Where(r => r != null).ToArray(), "Apply custom vertex streams from material");

                    //重点！直接赋值我们拼装好的顶点流。
                    foreach (ParticleSystemRenderer renderer in renderers)
                    {
                        renderer.SetActiveTrailVertexStreams(streams);
                    }
                }
            }
            #endif
            
            /*
            */
        }

        private string[] _customDataOptions =
        {
            "**不使用**",
            "CustomData1_X",
            "CustomData1_Y",
            "CustomData1_Z",
            "CustomData1_W",
            "CustomData2_X",
            "CustomData2_Y",
            "CustomData2_Z",
            "CustomData2_W"
        };

        public void DrawCustomDataSelect(string label, int dataBitPos, int dataIndex)
        {
            // if(!_isUseParticleSystem)return;//只有粒子系统才会处理相关内容。
            if (mats.Count != 1) return; //仅单选触发
            
            if(!(_meshSourceMode == MeshSourceMode.Particle || _uiParticleEnabled) ) return;
            EditorGUI.showMixedValue =
                helper.GetProperty(shaderFlags[0].GetCustomDataFlagPropertyName(dataIndex)).hasMixedValue; 
            W9ParticleShaderFlags.CutomDataComponent component = shaderFlags[0].GetCustomDataFlag(dataBitPos, dataIndex);
            EditorGUI.BeginChangeCheck();
            GUIContent[] optionGUIContents = new GUIContent[_customDataOptions.Length];
            for (int i = 0; i < optionGUIContents.Length; i++)
            {
                optionGUIContents[i] = new GUIContent(_customDataOptions[i]);
            }
            component = (W9ParticleShaderFlags.CutomDataComponent)EditorGUILayout.Popup(new GUIContent(label), (int)component, optionGUIContents);
            if (EditorGUI.EndChangeCheck())
            {
                shaderFlags[0].SetCustomDataFlag(component,dataBitPos,dataIndex);
            }
            EditorGUI.showMixedValue = false;
        }

    
        private string[] _uvModeNames =
        {
            "默认UV通道",
            "特殊UV通道",
            "极坐标|旋转",
            "圆柱无缝"
        };
        
        enum SpecialUVChannelMode
        {
            UV2_Texcoord1,
            UV3_Texcoord2
        }
        public void DrawUVModeSelect(int foldOutFlagBit, int foldOutFlagIndex,string label, int dataBitPos, int dataIndex,bool hasMap = true)
        {
            if (mats.Count != 1) return; //仅单选触发
            EditorGUI.BeginDisabledGroup(!hasMap);
            EditorGUI.indentLevel++;
            // EditorGUILayout.BeginHorizontal();
            EditorGUI.showMixedValue = helper.GetProperty(shaderFlags[0].GetUVModePropName(dataIndex)).hasMixedValue;
            W9ParticleShaderFlags.UVMode uvMode = shaderFlags[0].GetUVMode(dataBitPos, dataIndex);
            EditorGUI.BeginChangeCheck();
            GUIContent[] optinGUIContents = new GUIContent[_uvModeNames.Length];
            for (int i = 0; i < _uvModeNames.Length; i++)
            {
                optinGUIContents[i] = new GUIContent(_uvModeNames[i]);
            }

            // uvMode = (W9ParticleShaderFlags.UVMode)EditorGUILayout.Popup(new GUIContent(label), (int)uvMode,optinGUIContents);
            
            EditorGUILayout.BeginHorizontal();
            

            Rect rect = EditorGUILayout.GetControlRect();
            var labelRect = new Rect(rect.x + 2f, rect.y, rect.width - 2f, rect.height);
            var popUpRect = helper.GetRectAfterLabelWidth(rect,true);
            uvMode = (W9ParticleShaderFlags.UVMode) EditorGUI.Popup(popUpRect, (int)uvMode, optinGUIContents);
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, foldOutFlagIndex-3, foldOutFlagIndex);
            animBool.target = foldOutState;
            animBool.target =  EditorGUI.Foldout(rect, animBool.target, string.Empty, true);
            foldOutState = animBool.target;
            if (foldOutState)
            {
                shaderFlags[0].SetFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
            else
            {
                shaderFlags[0].ClearFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
            EditorGUI.LabelField(labelRect,label);
            

            
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                shaderFlags[0].SetUVMode(uvMode, dataBitPos, dataIndex);
            }

            EditorGUI.showMixedValue = false;

            EditorGUI.indentLevel++;
            // if (foldOutState)
            // {
            float faded = animBool.faded;
            if (faded == 0) faded = 0.0001f;
            EditorGUILayout.BeginFadeGroup(faded);
                if (uvMode != W9ParticleShaderFlags.UVMode.DefaultUVChannel)
                {
                    EditorGUILayout.LabelField("以下设置材质内通用:",EditorStyles.boldLabel);
                }
                
                switch (uvMode)
                {
                    case W9ParticleShaderFlags.UVMode.SpecialUVChannel:
                        helper.DrawPopUp("特殊UV通道选择","_SpecialUVChannelMode",  Enum.GetNames(typeof(SpecialUVChannelMode)),
                            drawBlock:
                            specialUVChannelMode =>
                            {
                                //这个设置就是全局的。
                                SpecialUVChannelMode spUVMode = (SpecialUVChannelMode)specialUVChannelMode;
                                switch (spUVMode)
                                {
                                    case SpecialUVChannelMode.UV2_Texcoord1:
                                        shaderFlags[0].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD1,index:1);
                                        shaderFlags[0].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD2,index:1);
                                        break;
                                    case SpecialUVChannelMode.UV3_Texcoord2:
                                        shaderFlags[0].ClearFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD1,index:1);
                                        shaderFlags[0].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_USE_TEXCOORD2,index:1);
                                        break;
                                }
                            });
                        break;
                    case W9ParticleShaderFlags.UVMode.PolarOrTwirl:
                        DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitTwril,3,"旋转扭曲","_UTwirlEnabled",flagBitsName:W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UTWIRL_ON,drawBlock:(isToggle) =>{
                                helper.DrawVector4In2Line("_TWParameter","旋转扭曲中心");
                                helper.DrawFloat("旋转扭曲强度","_TWStrength");
                        });

                        DrawToggleFoldOut(W9ParticleShaderFlags.foldOutBitPolar,3,"极坐标", "_PolarCoordinatesEnabled",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_POLARCOORDINATES_ON,drawBlock:(isToggle) =>{
                                // helper.DrawToggle("极坐标只影响特殊功能","_PolarCordinateOnlySpecialFunciton_Toggle",W9ParticleShaderFlags.FLAG_BIT_PARTICLE_PC_ONLYSPECIALFUNC);
                                helper.DrawVector4In2Line("_PCCenter","极坐标中心");
                                helper.DrawVector4Componet("极坐标强度","_PCCenter","z",true,0f,1f);
                        });
                        break;
                    case W9ParticleShaderFlags.UVMode.Cylinder:
                        EditorGUILayout.LabelField("圆柱模式消耗比较大，慎用");
                        helper.DrawVector4XYZComponet("圆柱坐标旋转","_CylinderUVRotate");
                        helper.DrawVector4XYZComponet("圆柱坐标偏移","_CylinderUVPosOffset");
                        Matrix4x4 cylinderMatrix =
                            Matrix4x4.Translate(helper.GetProperty("_CylinderUVPosOffset").vectorValue) *
                            Matrix4x4.Rotate(Quaternion.Euler(helper.GetProperty("_CylinderUVRotate").vectorValue));
                        helper.GetProperty("_CylinderMatrix0").vectorValue =cylinderMatrix.GetRow(0);
                        helper.GetProperty("_CylinderMatrix1").vectorValue =cylinderMatrix.GetRow(1);
                        helper.GetProperty("_CylinderMatrix2").vectorValue =cylinderMatrix.GetRow(2);
                        helper.GetProperty("_CylinderMatrix3").vectorValue =cylinderMatrix.GetRow(3);
                        shaderFlags[0].SetFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_1_CYLINDER_CORDINATE,index:1);
                        break;
                }

            // }
            EditorGUILayout.EndFadeGroup();
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

        }

        private string[] _meshSourceModeNames =
        {
            "粒子系统",
            "模型（非粒子发射）",
            "2D RawImage",
            "2D 精灵",
            "2D 材质贴图",
            "2D UIParticle"
        };

        enum MeshSourceMode
        {
            Particle,
            Mesh,
            UIEffectRawImage,
            UIEffectSprite,
            UIEffectBaseMap,
            UIParticle
        }
        void SetUVModeByOldSettings()
        {
            bool isTwril = shaderFlags[0].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_UTWIRL_ON);
            bool isPolar = shaderFlags[0].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_POLARCOORDINATES_ON);
            bool baseMapNoPolar =
                shaderFlags[0].CheckFlagBits(W9ParticleShaderFlags.FLAG_BIT_PARTICLE_PC_ONLYSPECIALFUNC);

            for (int j = 0; j < 16; j++)
            {
                if (isTwril || isPolar)
                {
                    if (j == 0)
                    {
                        if (baseMapNoPolar)
                        {
                            shaderFlags[0].SetUVMode(W9ParticleShaderFlags.UVMode.DefaultUVChannel, j * 2);
                            continue;
                        }
                    }

                    shaderFlags[0].SetUVMode(W9ParticleShaderFlags.UVMode.PolarOrTwirl, j * 2);
                }
            }
        }

        public void DrawTextureFoldOut(int foldOutFlagBit,int foldOutFlagIndex,string label, string texturePropertyName,
            string colorPropertyName = null, bool drawScaleOffset = true, bool drawWrapMode = false,
            int flagBitsName = 0, int flagIndex = 2, Action<Texture> drawBlock = null)
        {
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, foldOutFlagIndex - 3, foldOutFlagIndex);
            animBool.target = foldOutState;
            helper.DrawTextureFoldOut(ref animBool, label, texturePropertyName, colorPropertyName, drawScaleOffset,
                drawWrapMode, flagBitsName, flagIndex, drawBlock);
            foldOutState = animBool.target;
            if (foldOutState)
            {
                shaderFlags[0].SetFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
            else
            {
                shaderFlags[0].ClearFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
        }

        public void DrawToggleFoldOut(int foldOutFlagBit,int foldOutFlagIndex, string label, string propertyName = null,
            int flagBitsName = 0,
            int flagIndex = 0, string shaderKeyword = null, string shaderPassName = null, bool isIndentBlock = true, FontStyle fontStyle = FontStyle.Normal,
            Action<bool> drawBlock = null,Action<bool> drawEndChangeCheck = null)
        {
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, foldOutFlagIndex - 3, foldOutFlagIndex); //foldOut里的第一组。
            animBool.target = foldOutState;
            helper.DrawToggleFoldOut(ref animBool, label, propertyName, flagBitsName, flagIndex, shaderKeyword,
                shaderPassName, isIndentBlock, fontStyle, drawBlock, drawEndChangeCheck);
            foldOutState = animBool.target;
            if (foldOutState)
            {
                shaderFlags[0].SetFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
            else
            {
                shaderFlags[0].ClearFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
        }

        public void DrawBigBlockFoldOut(int foldOutFlagBit,int foldOutFlagIndex ,string label, Action drawBlock)
        {
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, foldOutFlagIndex - 3, foldOutFlagIndex);
            animBool.target = foldOutState;
            helper.DrawBigBlockFoldOut(ref animBool, label, drawBlock);
            foldOutState = animBool.target;
            if (foldOutState)
            {
                shaderFlags[0].SetFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
            else
            {
                shaderFlags[0].ClearFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            }
        }

        private AnimBool[] animBoolArr = new AnimBool[96];//先假定有3组。和存好的bit一一对应。

        //不是
        AnimBool GetAnimBool(int flagBit, int AnimBoolIndex,int flagIndex)
        {
            int bitPos = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((flagBit & (1 << i)) > 0)
                {
                    bitPos = i;
                    break;
                }
            }
            int arrIndex = AnimBoolIndex * 32 + bitPos;
            // Debug.Log(arrIndex.ToString() +"---"+ animBoolArr[arrIndex]);
            if (animBoolArr[arrIndex] == null)
            {
                animBoolArr[arrIndex] = new AnimBool(shaderFlags[0].CheckFlagBits(flagBit,index:flagIndex));
            }
            
            return animBoolArr[arrIndex];
        }
    }
}