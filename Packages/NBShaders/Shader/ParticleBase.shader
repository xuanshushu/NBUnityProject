Shader "Effects/NBShader"
{
    Properties
    {
    	_MeshSourceMode("Mesh来源模式",Float) = 0
        _UIEffect_Toggle("UI模式_Toggle",Float) = 0
        _DistortionBothDirection_Toggle("__DistortionBothDirection_Toggle",Float) = 0
        _DistanceFade_Toggle("_DistanceFade_Toggle",Float) = 0
        _ChangeSaturability_Toggle("__ChangeSaturability_Toggle",Float) = 0
        _Mask_Toggle("__Mask_Toggle",Float) = 0
        _Mask_RotationToggle("__Mask_RotationToggle",Float) = 0
        _Mask2_Toggle("__Mask2_Toggle",Float) = 0
        _Mask3_Toggle("__Mask3_Toggle",Float) = 0
        _BaseBackColor_Toggle("__BaseBackColor_Toggle",Float) = 0
        _UseUV1_Toggle("__UseUV1_Toggle",Float) = 0
        _TransparentMode("_TransparentMode",Float) = 1
        _ForceZWriteToggle("_ForceZWriteToggle",Float) = 0
        
        _Dissolve_Toggle("__Dissolve_Toggle",Float) = 0
        _DissolveMask_Toggle("__DissolveMask_Toggle",Float) = 0
        _DissolveVoronoi_Toggle("__DissolveVoronoi_Toggle",Float) = 0
        _Dissolve_useRampMap_Toggle("__Dissolve_useRampMap_Toggle",Float) = 0
        _Dissolve_Test_Toggle("__Dissolve_Test_Toggle",Float) = 0

        
        _ColorBlendMap_Toggle("__ColorBlendMap_Toggle",Float) = 0
        _FresnelMode("__FresnelMode",Float) = 0
        _InvertFresnel_Toggle("__InvertFresnel_Toggle",Float) = 0
        _HueShift_Toggle("__HueShift_Toggle",Float) = 0
        _BackFaceColor_Toggle("_BackFaceColor_Toggle",Float) = 0
        _BackFristPassToggle("_BackFirstPassToggle",Float) = 0
        
        _PolarCordinateOnlySpecialFunciton_Toggle("极坐标仅对特殊功能生效_Toggle",Float) = 0
        
        _CustomData1X_MainTexOffsetX_Toggle("_CustomData1X_MainTexOffsetX_Toggle",Float) = 0
        _CustomData1Y_MainTexOffsetY_Toggle("_CustomData1Y_MainTexOffsetY_Toggle",Float) = 0
        _CustomData1Z_Dissolve_Toggle("_CustomData1Z_Dissolve_Toggle",Float) = 0
        _CustomData1W_HueShift_Toggle("_CustomData1W_HueShift_Toggle",Float) = 0
        _CustomData2X_MaskMapOffsetX_Toggle("_CustomData2X_MaskMapOffsetX_Toggle",Float) = 0
        _CustomData2Y_MaskMapOffsetY_Toggle("_CustomData2Y_MaskMapOffsetY_Toggle",Float) = 0
        _CustomData2Z_FresnelOffset_Toggle("_CustomData2Z_FresnelOffset_Toggle",Float) = 0
        _CustomData2W_Toggle("_CustomData2W_Toggle",Float) = 0
        
//        [PerRendererData] [MainTexture] _MainTex("Sprite Texture-ignore", 2D) = "white" {}
        [PerRendererData] _MainTex ("Sprite Texture-ignore", 2D) = "white" {}
        _Color("颜色贴图叠加", Color) = (1,1,1,1)
        _UI_MainTex_ST("UI模式主贴图 xy:UV缩放 zw:UV偏移",vector) = (1,1,0,0)
        _MainTex_Reverse_ST("MainTex_Reverse_ST-ignore",Vector) = (1,1,0,0)
        
        _BaseMap ("主贴图 xy:UV缩放 zw:UV偏移", 2D) = "white" { }
        _BaseMapMaskMapOffset ("xy主贴图偏移速度", vector) = (0, 0, 0, 0)

        _BaseMapUVRotation ("主贴图旋转", Range(0, 360)) = 0
        [HDR]_BaseColor ("主贴图颜色_hdr", Color) = (1, 1, 1, 1)//HDR颜色不需要做Gamma Linear转换，Unity默认用Linear颜色
//        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)//HDR颜色不需要做Gamma Linear转换，Unity默认用Linear颜色
        [HDR]_BaseBackColor ("背面颜色_hdr", Color) = (1, 1, 1, 1)//HDR颜色不需要做Gamma Linear转换，Unity默认用Linear颜色
        _BaseColorIntensityForTimeline("整体颜色强度", Range(0,10)) = 1 //独立出来是以为Timeline  K颜色的时候会很奇怪的影响色相
        _Saturability("饱和度", range(0,1)) = 0
        _HueShift("色相",Range(0,1)) = 0
        _AlphaAll("整体透明度",Range(0,1)) = 1
        _IgnoreVetexColor_Toggle("_IgnoreVetexColor_Toggle",Float) = 0
    	
    	_SpecialUVChannelMode("特殊UV通道选择",Float) = 0
    	
    	_CylinderUVRotate("圆柱UV旋转",Vector) = (0,0,90,0)
    	_CylinderUVPosOffset("圆柱UV位置偏移",Vector) = (0,0,0,0)
    	_CylinderMatrix0("圆柱偏移矩阵0",Vector) = (0,0,0,0)
    	_CylinderMatrix1("圆柱偏移矩阵1",Vector) = (0,0,0,0)
    	_CylinderMatrix2("圆柱偏移矩阵2",Vector) = (0,0,0,0)
    	_CylinderMatrix3("圆柱偏移矩阵3",Vector) = (0,0,0,0)
        
        _Cutoff ("裁剪位置", float) = 0

        //时间缩放影响开关----------
        [HideInInspector] _TimeMode("__TimeMode",float) = 0.0
        
        
        // MaskMap------------
        _MaskMap ("遮罩贴图 xy:UV缩放 zw:UV偏移", 2D) = "white" { }
        _MaskMap2 ("遮罩2贴图 xy:UV缩放 zw:UV偏移", 2D) = "white"{}
        _MaskMap3 ("遮罩3贴图 xy:UV缩放 zw:UV偏移", 2D) = "white"{}
        _MaskMapOffsetAnition("xy:遮罩偏移速度，zw:遮罩2偏移速度", vector) = (0,0,0,0)
        _MaskMap3OffsetAnition("xy:遮罩3偏移速度", vector) = (0,0,0,0)
        _MaskMapUVRotation ("遮罩旋转", Range(0, 360)) = 0.0
        _MaskDistortion_intensity ("遮罩扭曲强度", float) = 0.0
        _MaskMapRotationSpeed("遮罩旋转速度", float) = 0.0
        _MaskMapVec("x整体遮罩强度",Vector) = (1,0,0,0)
        
        // 擦除----------------
        //[Header(ChaChu(Anima For CustomData.z).......)]
        //[KeywordEnum(NoChange,XianXing, JingXiang, Self)] _ch ("ChaChu mode", Float) = 0
        [HideInInspector] _Chachu ("__Chachu_ignore", Float) = 0.0
        _EdgeFade ("EdgeFade_ignore", Range(0, 1)) = 0.05
        _XianXingCH_UVRota ("XianXingCH_UVRota_ignore", float) = 0
        _jingxiangCH_dire ("Direction_ignore", Range(0, 1)) = 0
        
        // 漩涡 -------------------
        //[Toggle(_JIZUOBIAO)] _N121 ("JIZUOBIAO?", float) = 0
        [HideInInspector] _UTwirlEnabled ("__UTwirlEnabled", Float) = 0.0
        _TWParameter ("xy:旋转扭曲中心", vector) = (0.5, 0.5, 0, 0)
        _TWStrength ("旋转扭曲强度", float) = 0
        
        
        // 极坐标 -------------------
        //[Toggle(_JIZUOBIAO)] _N121 ("JIZUOBIAO?", float) = 0
        [HideInInspector]_PolarCoordinatesEnabled ("__PolarCoordinatesEnabled", Float) = 0.0
        _PCCenter ("xy:极坐标中心 z:极坐标强度", vector) = (0.5, 0.5, 1, 0)//位置坐标用的前两个分量，z分量给强度。

        
        // 噪波 --------------
        //[Toggle(_NOISEMAP)]_N ("NOISEMAP?", float) =0
        [HideInInspector] _noisemapEnabled ("__noisemapEnabled", Float) = 0.0
        [HideInInspector] _noiseMaskMap_Toggle ("__noiseMaskMap_Toggle", Float) = 0.0
        _NoiseMap ("扭曲贴图 xy:UV缩放 zw:UV偏移", 2D) = "white" { }
        _NoiseMaskMap ("扭曲遮罩贴图 xy:UV缩放 zw:UV偏移", 2D) = "white" { }
        _NoiseMapUVRotation("扭曲旋转",Range(0,360)) = 0
        _NoiseOffset ("xy:扭曲偏移速度 ", vector) = (0, 0, 0, 0)//w分量为本地uv坐标到世界坐标的变化
    	_TexDistortion_intensity ("扭曲强度", float) = 0.5
        _DistortionDirection ("扭曲方向xy, 色散强度z", vector) = (1,1,0,0)
        _Distortion_Choraticaberrat_Toggle("扭曲色散开关_Toggle",Float) = 0 
        _Distortion_Choraticaberrat_WithNoise_Toggle("色散受扭曲影响_Toggle",Float) = 1
        
        
        
        // 流光 ----------
        //[Header(LiuGuang(Anima For CustomData.w).......)]
        //[Toggle(_EMISSION)]_N1 ("EMISSION?", float) = 0
        [HideInInspector] _EmissionEnabled ("__EmissionEnabled", Float) = 0.0
        _EmissionMap ("流光贴图 xy:UV缩放 zw:UV偏移", 2D) = "white" { }
        _EmissionMapUVRotation ("流光贴图旋转", Range(0, 360)) = 0
        _Emi_Distortion_intensity ("流光贴图扭转强度", float) = 0
        _EmissionMapUVOffset ("xy:流光贴图偏移速度", vector) = (0, 0, 0, 0)
        _EmissionSelfAlphaWeight ("__EmissionSelfAlphaWeight_ignore", float) = 0
        _uvRapSoft ("LiuuvRapSoft-ignore", Range(0, 1)) = 0
        [HDR]_EmissionMapColor ("流光贴图颜色_hdr", Color) = (1, 1, 1, 1)
        _EmissionMapColorIntensity("流光颜色强度", float) = 1
        
        // Rongjie ------------------
        // [Header(RongJie(Anima For CustomData.y).......)]
        // [Toggle(_DISSOLVE)]_RJ ("RONGJIE?", float) = 0
        _Dissolve ("x:溶解强度 y:溶解描边范围 z:局部控制强度 w:溶解硬度, _DissolveWidth", vector) = (0.5, 0.1, 1, 0.1)
        _DissolveMap("溶解贴图 xy:UV缩放 zw:UV偏移",2D) = "grey"{}
        _DissolveMaskMap("局部溶解蒙版 xy:UV缩放 zw:UV偏移",2D) = "white"{}
        _DissolveOffsetRotateDistort("xy:溶解贴图偏移速度 z:溶解贴图旋转",Vector) = (0,0,0,0)
        [HDR]_DissolveLineColor("溶解描边颜色_hdr",Color) = (0,0,0,1)
        _DissolveVoronoi_Vec("xy:噪波1缩放,zw:噪波2缩放",Vector) = (1,1,2,2)
        _DissolveVoronoi_Vec2("x:噪波1和噪波2混合系数(圆尖),y:噪波整体和溶解贴图混合系数,z:噪波1速度,w:噪波2速度",Vector) = (1,1,2,2)
        _DissolveVoronoi_Vec3("xy:噪波1偏移速度,zw:噪波2偏移速度",Vector) = (0,0,0,0)
    	_DissolveVoronoi_Vec4("xy:噪波1偏移,zw:噪波2偏移",Vector) = (0,0,0,0)
        _Dissolve_Vec2("溶解丝滑度（溶解值黑白调整）黑色X,白色Y",Vector) = (0,1,0,0)
        _DissolveRampMap("溶解Ramp图",2D) = "white"{}
        [HDR]_DissolveRampColor("溶解Ramp颜色_hdr",Color) = (1,1,1,1)
        
        //颜色渐变贴图--------
        _ColorBlendMap("颜色渐变贴图 xy:UV缩放 zw:UV偏移",2D) = "white"{}
        [HDR]_ColorBlendColor("颜色渐变叠加_hdr",Color) = (1,1,1,1)
        _ColorBlendMapOffset("xy:颜色渐变贴图偏移动画",Vector) = (0,0,0,0)

        _CustomData1X ("ignore", float) = 0
        _CustomData1Y ("ignore", float) = 0
        //因为希望本Shader兼容CanvasRender，由于Canvas渲染只会传递TEXCOORD通道的xy分量（zw分量忽略）的特性,所以强制让CustomedData只传递xy分量。
        _CustomData2X ("ignore", float) = 0
        
        // -------------------------------------
        // Particle specific   属于粒子特殊的属性
        [ToggleOff] _CustomData ("__CustomData_Toggle", Float) = 0.0   //Toggleoff 和 Toggle 的区别
        [ToggleOff] _FlipbookBlending ("__flipbookblending_Toggle", Float) = 0.0   //Toggleoff 和 Toggle 的区别
//        _SoftParticlesNearFadeDistance ("Soft Particles Near Fade", Float) = 0.0
//        _SoftParticlesFarFadeDistance ("Soft Particles Far Fade", Float) = 1.0
        //[Toggle] _Fading ("Fading?     =Default(1,2,0,0)", Float) = 0.0
        _CameraNearFadeDistance ("Camera Near Fade-ignore", Float) = 1.0
        _CameraFarFadeDistance ("Camera Far Fade-ignore", Float) = 2.0
        //临时
        _fogintensity ("雾影响强度", Range(0, 1)) = 1
        // -------------------------------------
        // Hidden properties - Generic  通用的隐藏属性
        [HideInInspector] _Blend ("__mode-ignore", Float) = 0.0
        [HideInInspector] _AlphaClip ("__clip-ignore", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src-ignore", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst-ignore", Float) = 0.0
        [HideInInspector] _Cull ("__cull-ignore", Float) = 2.0
        [HideInInspector] _ZTest ("__ztest-ignore", Float) = 4.0 //默认值LEqual
        [HideInInspector] _ZWrite("__ZWrite-ignore", Float) = 0 //默认值LEqual
        // [HideInInspector] _ZTestt ("__ztestt", Float) = 4.0//雨轩：注释掉了。。。这是个什么鬼。。。



        _CustomStencilTest ("__CustomStencilTest-ignore", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("__StencilComp-ignore", Float) = 8
        _Stencil("Stencil ID-ignore", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation-ignore", Float) = 0
        _StencilWriteMask ("Stencil Write Mask-ignore", Float) = 255
        _StencilReadMask ("Stencil Read Mask-ignore", Float) = 255
        _ColorMask("Color Mask-ignore", Float) = 15

        //[HideInInspector] _ZTestO ("__ztest0", Float) = 1.0
        _FresnelFadeDistance ("菲涅尔透明乘数", float) = 1
        _FresnelUnit("菲涅尔通用", Vector) = (0,0.5,1,0.5)
        
        _DepthOutline_Toggle("深度描边",Float) = 0
        [HDR]_DepthOutline_Color("深度描边颜色_hdr",Color) = (1,1,1,1)
        _DepthOutline_Vec("菲涅尔深度描边参数",Vector) = (0,0.5,0,0)
//        _DepthOutline_withoutFresnel_Toggle("深度描边关闭菲涅尔",Float) = 0
//        _FresnelUnit2("菲涅尔通用2", Vector) = (0,1,0,0)
        
        _DepthDecal_Toggle("深度贴花",Float) = 0
        
        _VertexOffset_Toggle("顶点偏移",Float) = 0
        _VertexOffset_Map("顶点偏移贴图",2D) = "white"{}
        _VertexOffset_Vec("顶点偏移动画",Vector) = (0,0,1,0)
        _VertexOffset_NormalDir_Toggle("顶点偏移自定义方向开关",Float) = 0
        _VertexOffset_CustomDir("顶点偏移自定义方向",Vector) = (1,1,1,0)
        
        _ParallaxMapping_Toggle("视差",Float) = 0
        _ParallaxMapping_Map("视差贴图",2D) = "white"{}
        _ParallaxMapping_Intensity("视差强度",Float) = 1
        
        
        // Particle specific   粒子特殊的隐藏属性
        [HideInInspector] _ColorMode ("_ColorMode", Float) = 0.0
        [HideInInspector] _BaseColorAddSubDiff ("_ColorMode", Vector) = (0, 0, 0, 0)
        [HideInInspector] _SoftParticlesEnabled ("__softparticlesenabled", Float) = 0.0
        [HideInInspector] _CameraFadingEnabled ("__camerafadingenabled", Float) = 0.0
        [HideInInspector] _SoftParticleFadeParams ("xy:软粒子远近裁剪面", Vector) = (0, 0, 0, 0)
        [HideInInspector] _CameraFadeParams ("__camerafadeparams_ignore", Vector) = (0, 0, 0, 0)
        [HideInInspector] _IntersectEnabled("__IntersectEnabled_ignore",Float) = 0.0
        [HideInInspector] _IntersectRadius("__IntersectRadius_ignore",Float) = 0.3
        [HideInInspector] _IntersectColor("__IntersectColor_ignore",Color) = (1,1,1,1)
        
        [HideInInspector] _ScreenDistortModeToggle("_ScreenDistortModeToggle",Float) = 0
        
        
        
        // Editmode props  编辑模式下的PropFlags？
        [HideInInspector] _QueueBias ("Queue偏移_QueueBias", Float) =0
        
        // ObsoleteProperties   弃用的属性？？？？
        [HideInInspector] _FlipbookMode ("flipbook mode", Float) = 0
        [HideInInspector] _Mode ("mode", Float) = 0
        // [HideInInspector] _Color ("color", Color) = (1, 1, 1, 1)
        
        [HideInInspector]_fresnelEnabled ("__fresnelEnabled", Float) = 0.0
        [NoScaleOffset]_FresnelHDRITex("__FresnelHDRITex_ignore",Cube) = "white"{} 
        [HDR]_FresnelColor ("菲涅尔颜色_hdr", COLOR) = (1, 1, 1, 1)
        _FresnelRotation("菲涅尔方向偏移",vector) = (0,0,0,0.5)
        _FresnelInOutSlider ("direction-ignore", Range(0, 1)) = 1
        _FrePower ("FrePower-ignore", Range(0,1)) = 0.5
        _FresnelSelfAlphaWeight("__FresnelSelfAlphaWeight-ignore",float) = 0
        //用于程序控制透明属性
        [HideInInspector] _ColorA ("ColorA-ignore", Color) = (1,1,1,1)
        
        _Portal_Toggle("模板视差开关",Float) = 0
        _Portal_MaskToggle("模板视差蒙版开关",Float) = 0

        //基于深度的a通道控制
        [HideInInspector] _Fade("xy:近距离透明过度范围", Vector) = (2,4,0,0)

        [HideInInspector] _InspectorData("__InspectorData-ignore",vector) = (1,1,0,0)
    
        [Header(ZOffset)]
        _ZOffset_Toggle("深度偏移_Toggle",Float) = 0
        _offsetFactor("深度偏移Sacle-ignore", range(-2000,2000)) = 0
        _offsetUnits("深度偏移单位距离-ignore", range(-2000,2000)) = 0
        
        [HideInInspector] _W9ParticleShaderFlags("_W9ParticleShaderFlags", Integer) = 0
        [HideInInspector] _W9ParticleShaderFlags1("_W9ParticleShaderFlags1", Integer) = 0
        [HideInInspector] _W9ParticleShaderWrapFlags("_W9ParticleShaderWrapFlags", Integer) = 0
        [HideInInspector] _W9ParticleCustomDataFlag0("_W9ParticleCustomDataFlag0", Integer) = 0
        [HideInInspector] _W9ParticleCustomDataFlag1("_W9ParticleCustomDataFlag1", Integer) = 0
        [HideInInspector] _W9ParticleCustomDataFlag2("_W9ParticleCustomDataFlag2", Integer) = 0
        [HideInInspector] _UVModeFlag0("_UVModeFlag0", Integer) = 0
        [HideInInspector] _W9ParticleShaderGUIFoldToggle("_W9ParticleShaderGUIFoldToggle", Integer) = 31//前5个开关默认打开
        [HideInInspector] _W9ParticleShaderGUIFoldToggle1("_W9ParticleShaderGUIFoldToggle1", Integer) = 255//这边默认全开
        
//        _offsetUnits("深度偏移单位距离-ignore", range(-2000,2000)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        
        Stencil
        {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
        }

        //BlendOp[_BlendOp]   //考虑注释~
        Blend[_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]//粒子不写入深度缓冲
        ZTest[_ZTest]

        ColorMask [_ColorMask]
        
        // ------------------------------------------------------------------
        //  预渲染反面Pass，基本理念。先剔除正面棉片，渲染反面面片（一般在后），然后常规Pass再渲染正面（一般在前）
        //  这样可以应对大部分的特效半透明渲染的次序问题。
        //  URP下的多Pass渲染，需要使用特定的LightModeTag：
        //  https://zhuanlan.zhihu.com/p/469589277#:~:text=%E6%B3%A8%E6%84%8F%E7%AC%AC%E4%BA%8C%E4%B8%AA%E5%85%89%E7%85%A7pass%E7%9A%84%E5%85%89%E7%85%A7tag%E4%B8%BA%20%22LightMode%22%20%3D%20%22SRPDefaultUnlit%22%EF%BC%8C%E8%BF%99%E6%98%AF%E5%9B%A0%E4%B8%BA%E5%BF%85%E9%A1%BB%E8%AE%BE%E7%BD%AE%E4%B8%BA%E7%89%B9%E5%AE%9A%E7%9A%84%E5%87%A0%E4%B8%AALightMode%20%E6%89%8D%E8%83%BD%E6%AD%A3%E7%A1%AE%E8%A2%AB%E6%B8%B2%E6%9F%93%E3%80%82%E5%85%B7%E4%BD%93%E5%93%AA%E5%87%A0%E4%B8%AA%20LightMode%20%E8%A7%81URP%E5%8C%85%E9%87%8C%E7%9A%84%20Runtime/Passes%20%E9%87%8C%E7%9A%84%20DrawObjectsPass.cs%E8%84%9A%E6%9C%AC%EF%BC%8C%E9%87%8C%E9%9D%A2%E6%9C%89%E5%A6%82%E4%B8%8B%E4%BB%A3%E7%A0%81%EF%BC%9A
        //  开关Pass，同样也需要用到LightTag
        //  https://blog.csdn.net/shaoy1234567/article/details/106494878
        
        Pass
        {
             Tags
            {
                "LightMode" = "SRPDefaultUnlit" "Queue"="Opaque"
            }
            offset [_offsetFactor], [_offsetUnits]
            Name "SRPDefaultUnlit"
            Cull Front
            
            HLSLPROGRAM
            #define PARTICLE
            #pragma target 4.5
            #pragma exclude_renderers d3d11_9x
            #pragma exclude_renderers d3d9
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ _SCREEN_DISTORT_MODE
            #pragma shader_feature_local _ _MASKMAP_ON
            // #pragma shader_feature_local _MASKMAP
            // #pragma shader_feature_local _MASKMAP2
            //#pragma shader_feature_local _NOISEMAP
            #pragma shader_feature_local _NOISEMAP
            //#pragma shader_feature_local _EMISSION   //流光
            #pragma shader_feature_local _EMISSION
            //#pragma shader_feature_local _ _DISSOLVE    //溶解
            #pragma shader_feature_local _DISSOLVE
            //后续Test类的关键字要找机会排除
            #pragma shader_feature_local _DISSOLVE_EDITOR_TEST
            #pragma  shader_feature_local  _COLORMAPBLEND//颜色渐变
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT//UI 2D遮罩
            // #pragma shader_feature_local _ _CH_XIANXING _CH_JINGXIANG  _CH_SELF   //线性擦除  径向擦除  mask擦除
            #pragma shader_feature_local _PARCUSTOMDATA_ON

            //用于特效层关键字
            // #pragma shader_feature_local  _UIEFFECT_ON
            
            #pragma shader_feature_local _ FRESNEL_CUBEMAP FRESNEL_REFLECTIONPROBE
           
            
            // #pragma multi_compile _ _UIPARTICLE_ON//用于UIParticle组件动态更改参数//暂时注释掉，觉得没什么意义
            #pragma multi_compile _ SOFT_UI_FRAME//用于UI软蒙版
            // -------------------------------------
            // Particle Keywords
            //#pragma shader_feature_local _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON     //设置alpah Add 。。组合
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma shader_feature_local _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON  //粒子颜色和材质颜色的混合运算  暂时先不要了
            #pragma shader_feature_local _FLIPBOOKBLENDING_ON
            #pragma shader_feature_local _SOFTPARTICLES_ON
            // #pragma shader_feature_local _OCCLUDEOPACITY_ON
            // #pragma shader_feature_local _ _SATURABILITY_ON

            //UnscaleTime用于接收项目传的公开不受缩放影响的Time值
            #pragma shader_feature_local _UNSCALETIME
            //scriptableTime用于程序每帧传值
            #pragma shader_feature_local _SCRIPTABLETIME
            //#pragma shader_feature_local _DISTORTION_ON
            #pragma shader_feature_local _NOISEMAP_NORMALIZEED

            #pragma shader_feature_local _DEPTH_DECAL
            #pragma shader_feature_local _PARALLAX_MAPPING

            #pragma shader_feature_local _STENCIL_WITHOUT_PLAYER
            
            // -------------------------------------
            // Unity defined keywords
            // 之后进行优化时再说。
            #pragma multi_compile_fog
            // #define FOG_EXP2 1 
            

            // #if defined(_SOFTPARTICLES_ON)
            //     #define NEED_EYE_DEPTH
            // #endif

            #define PARTICLE_BACKFACE_PASS
                    
            #pragma vertex vertParticleUnlit
            #pragma fragment fragParticleUnlit
            
            // #include "UnityCG.cginc"
            // #include "AutoLight.cginc"
            // #include "UnityUI.cginc"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            
            #include "Packages/com.xuanxuan.render.utility/Shader/HLSL/XuanXuan_Utility.hlsl"
            #include "HLSL/ParticlesUnlitInputNew.hlsl"
            #include "HLSL/ParticlesUnlitForwardPassNew.hlsl"
            
            ENDHLSL
            
        }
        
        // ------------------------------------------------------------------
        //  Forward pass.
        Pass
        {
            Tags
        {
              "LightMode" = "UniversalForward" 
        } //Queue设置是希望特效渲染在场景透明物体前面
            offset [_offsetFactor], [_offsetUnits]
            Cull[_Cull]

            HLSLPROGRAM
            #define PARTICLE
            //20240228 target3.0 顶点着色器限制16个输出。提高版本
            #pragma target 4.5
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ _SCREEN_DISTORT_MODE
            #pragma shader_feature_local _ _MASKMAP_ON
            // #pragma shader_feature_local _MASKMAP
            // #pragma shader_feature_local _MASKMAP2
            //#pragma shader_feature_local _NOISEMAP
            #pragma shader_feature_local _NOISEMAP
            //#pragma shader_feature_local _EMISSION   //流光
            #pragma shader_feature_local _EMISSION
            //#pragma shader_feature_local _ _DISSOLVE    //溶解
            #pragma shader_feature_local _DISSOLVE
                        //后续Test类的关键字要找机会排除
            #pragma shader_feature_local _DISSOLVE_EDITOR_TEST
            #pragma  shader_feature_local  _COLORMAPBLEND//颜色渐变
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT//UI 2D遮罩
            // #pragma shader_feature_local _ _CH_XIANXING _CH_JINGXIANG  _CH_SELF   //线性擦除  径向擦除  mask擦除
            #pragma shader_feature_local _PARCUSTOMDATA_ON

            //用于特效层关键字
            // #pragma shader_feature_local  _UIEFFECT_ON
            
            #pragma shader_feature_local _ FRESNEL_CUBEMAP FRESNEL_REFLECTIONPROBE

            
            // #pragma multi_compile _ _UIPARTICLE_ON//用于UIParticle组件动态更改参数//暂时注释掉，觉得没什么意义
            #pragma multi_compile _ SOFT_UI_FRAME//用于UI软蒙版
            // -------------------------------------
            // Particle Keywords
            //#pragma shader_feature_local _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON     //设置alpah Add 。。组合
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma shader_feature_local _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON  //粒子颜色和材质颜色的混合运算  暂时先不要了
            #pragma shader_feature_local _FLIPBOOKBLENDING_ON
            #pragma shader_feature_local _SOFTPARTICLES_ON
            // #pragma shader_feature_local _OCCLUDEOPACITY_ON
            // #pragma shader_feature_local _ _SATURABILITY_ON

            //UnscaleTime用于接收项目传的公开不受缩放影响的Time值
            #pragma shader_feature_local _UNSCALETIME
            //scriptableTime用于程序每帧传值
            #pragma shader_feature_local _SCRIPTABLETIME
            //#pragma shader_feature_local _DISTORTION_ON
            #pragma shader_feature_local _NOISEMAP_NORMALIZEED

            #pragma shader_feature_local _DEPTH_DECAL
            #pragma shader_feature_local _PARALLAX_MAPPING

            #pragma shader_feature_local _STENCIL_WITHOUT_PLAYER


            
            // -------------------------------------
            // Unity defined keywords
            // 之后进行优化时再说。
            #pragma multi_compile_fog
            // #define FOG_EXP2 1 
            
            #pragma vertex vertParticleUnlit
            #pragma fragment fragParticleUnlit
            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The DeclareDepthTexture.hlsl file contains utilities for sampling the Camera
            // depth texture.

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            
            #include "Packages/com.xuanxuan.render.utility/Shader/HLSL/XuanXuan_Utility.hlsl"

            #include "HLSL/ParticlesUnlitInputNew.hlsl"
            #include "HLSL/ParticlesUnlitForwardPassNew.hlsl"
            
            
            ENDHLSL
            
        }
    }
    
    CustomEditor "ParticleBaseGUI"
}