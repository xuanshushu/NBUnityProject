#ifndef PARTICLESUNLITINPUT
    #define PARTICLESUNLITINPUT


    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   

  //---------------particleInput-------------------
    CBUFFER_START(UnityPerMaterial)
    float4 _SoftParticleFadeParams;
    float4 _CameraFadeParams;
    // #ifdef _INTERSECT_ON
        float _IntersectRadius;
        half4 _IntersectColor;
    // #endif
    half _Saturability;
    half _HueShift;
    half _Contrast;
    half3 _ContrastMidColor;
    half _AlphaAll;
    float4 _BaseMap_ST;
    float4 _BaseMap_AnimationSheetBlend_ST;//20240826 暂时只是给AnimationSheetHelper用。
    half _AnimationSheetHelperBlendIntensity;
    float4 _MaskMap_ST;
    half4 _BaseColor;
    half4 _BaseBackColor;
    half _BaseColorIntensityForTimeline;
    half4 _EmissionMap_ST;
    half4 _NoiseMap_ST;
    half4 _NoiseMaskMap_ST;
    half4 _DistortionDirection;
    half4 _BaseColorAddSubDiff;
    half _fogintensity;
    half _Emi_Distortion_intensity;
    half _BaseMapUVRotation;
    float _MaskMapUVRotation;
    float _NoiseMapUVRotation;
    half _uvRapSoft;
    half4 _EmissionMapColor;
    half _EmissionMapColorIntensity;

    half _EdgeFade;
    half4 _NoiseOffset;
    half4 _EmissionMapUVOffset;
    half _EmissionMapUVRotation;
    half _EmissionSelfAlphaWeight;
    half _TexDistortion_intensity;
    // //half _RJ_Distortion_intensity;
    // half _XianXingCH_UVRota;
    // half _jingxiangCH_dire;

    half _MaskDistortion_intensity;
    half4 _BaseMapMaskMapOffset;
    half4 _MaskMapOffsetAnition;
    half4 _MaskMap3OffsetAnition;
    half4 _MaskMapVec;
    float4 _PCCenter;
    float4 _TWParameter;
    float _TWStrength;
    float4 _Fade;
    float _MaskMapRotationSpeed;
    

    half _FrePower;
    half _FresnelInOutSlider;
    half4 _FresnelRotation;
    half _FresnelSelfAlphaWeight;
    half4 _FresnelUnit;
    half4 _FresnelUnit2;
    half4 _DepthOutline_Vec;
    half4 _DepthOutline_Color;
    half4 _FresnelColor;
    half4 _ColorA;
    float4 _ClipRect;

    float4 _CylinderMatrix0;
    float4 _CylinderMatrix1;
    float4 _CylinderMatrix2;
    float4 _CylinderMatrix3;
   
    half4 _Color;
    float4 _UI_MainTex_ST;//在UI中，RawImage组件的功能和正常的TexST不一致，所以这里使用另外传的方式。
    float4 _MainTex_Reverse_ST;

    half _Cutoff;

    float4 _MaskMap2_ST;
    float4 _MaskMap3_ST;   

    float time;
    half _FresnelFadeDistance;

    half4 LB_RT;

    half4 _Dissolve;
    half4 _DissolveMap_ST;
    half4 _DissolveOffsetRotateDistort;
    half4 _DissolveMaskMap_ST;

    half4 _DissolveLineColor;
    half4 _DissolveRampColor;
    float4 _DissolveVoronoi_Vec;
    half4 _DissolveVoronoi_Vec2;
    float4 _DissolveVoronoi_Vec3;
    float4 _DissolveVoronoi_Vec4;
    half4 _DissolveRampMap_ST;
    half4 _Dissolve_Vec2;

    half4 _ColorBlendMap_ST;
    half2 _ColorBlendMapOffset;
    half4 _ColorBlendColor;

    half3  _VertexOffset_Vec;
    half3 _VertexOffset_CustomDir;
    half4 _VertexOffset_Map_ST;

    half4 _VertexOffset_MaskMap_ST;
    half3 _VertexOffset_MaskMap_Vec;

    half _ParallaxMapping_Intensity;
    half4 _ParallaxMapping_Map_ST;
    half4 _ParallaxMapping_Vec;

    uint _W9ParticleShaderFlags;

    uint _W9ParticleShaderFlags1;

    uint _W9ParticleShaderWrapFlags;

    uint _W9ParticleCustomDataFlag0;
    uint _W9ParticleCustomDataFlag1;
    uint _W9ParticleCustomDataFlag2;
    uint _W9ParticleCustomDataFlag3;

    uint _UVModeFlag0;

    CBUFFER_END


    bool CheckLocalFlags(uint bits)
    {
        return (_W9ParticleShaderFlags&bits) != 0;
    }
    bool CheckLocalFlags1(uint bits)
    {
        return (_W9ParticleShaderFlags1&bits) != 0;
    }
    int CheckLocalWrapFlags(uint bits)
    {
        bool bit0 = (_W9ParticleShaderWrapFlags&bits) != 0;
        bool bit1 = (_W9ParticleShaderWrapFlags&(bits<<16)) != 0;
        if(!bit0 && !bit1)
        {
            return 0;
        }
        else if(bit0 && !bit1)
        {
            return 1;
        }
        else if(!bit0 && bit1)
        {
             return 2;
        }
        else if(bit0 && bit1)
        {
            return 3;
        }
        else
        {
            return -1;
        }
    }

    SamplerState sampler_linear_repeat;
    SamplerState sampler_linear_clamp;
    SamplerState sampler_linear_RepeatU_ClampV;
    SamplerState sampler_linear_ClampU_RepeatV;

    half4 SampleTexture2DWithWrapFlags(Texture2D tex,float2 uv,uint bits,bool sampleLOD = false,int lod = 0)
    {
        const int wrapMode = CheckLocalWrapFlags(bits);
        switch (wrapMode)
        {
            case 0:
                if (sampleLOD)
                {
                    return SAMPLE_TEXTURE2D_LOD(tex,sampler_linear_repeat,uv,lod);
                    
                }
                else
                {
                    return tex.Sample(sampler_linear_repeat,uv);
                }
                break;
            case 1:
                if (sampleLOD)
                {
                    return SAMPLE_TEXTURE2D_LOD(tex,sampler_linear_clamp,uv,lod);
                    
                }
                else
                {
                    return tex.Sample(sampler_linear_clamp,uv);
                }
                break;
            case 2:
                if (sampleLOD)
                {
                    return SAMPLE_TEXTURE2D_LOD(tex,sampler_linear_RepeatU_ClampV,uv,lod);
                    
                }
                else
                {
                    return tex.Sample(sampler_linear_RepeatU_ClampV,uv);
                }
                break;
            case 3:
                if (sampleLOD)
                {
                    return SAMPLE_TEXTURE2D_LOD(tex,sampler_linear_ClampU_RepeatV,uv,lod);
                    
                }
                else
                {
                    return tex.Sample(sampler_linear_ClampU_RepeatV,uv);
                }
                break;
            default:
                if (sampleLOD)
                {
                    return SAMPLE_TEXTURE2D_LOD(tex,sampler_linear_repeat,uv,lod);
                    
                }
                else
                {
                    return tex.Sample(sampler_linear_repeat,uv);
                }
                break;
        }
    }

 

    #include "../HLSL/EffectFlags.hlsl"


    samplerCUBE _FresnelHDRITex;
    sampler2D _MainTex;
    
    
    
    #define SOFT_PARTICLE_NEAR_FADE _SoftParticleFadeParams.x    //�궨��SOFT_PARTICLE_NEAR_FADE ��Ϊ_SoftParticleFadeParams���Ե�x����~
    #define SOFT_PARTICLE_INV_FADE_DISTANCE _SoftParticleFadeParams.y
    
    #define CAMERA_NEAR_FADE  _CameraFadeParams.x
    #define CAMERA_INV_FADE_DISTANCE  _CameraFadeParams.y

    Texture2D _BaseMap;
    Texture2D _NoiseMap;
    Texture2D _NoiseMaskMap;
    Texture2D _EmissionMap;
    Texture2D _MaskMap;
    Texture2D _MaskMap2;
    Texture2D _MaskMap3;

    #ifdef _SCREEN_DISTORT_MODE
        Texture2D _ScreenColorCopy1;
    #endif

    // Pre-multiplied alpha helper
    #if defined(_ALPHAPREMULTIPLY_ON)  //if( blend: One OneMinusSrcAlpha)
        #define ALBEDO_MUL albedo
    #else
        #define ALBEDO_MUL albedo.a
    #endif

    
    #ifdef SOFT_UI_FRAME
        sampler2D _SoftUIFrameMask;
        // half4 LB_RT;
    #endif

    #ifdef _DISSOLVE
        Texture2D _DissolveMap;
        Texture2D _DissolveMaskMap;
        Texture2D _DissolveRampMap;
    #endif

    # ifdef  _COLORMAPBLEND
        Texture2D _ColorBlendMap;
    #endif

    

    half4 tex2D_TryLinearizeWithoutAlphaFX(sampler2D tex, float2 uv)
    {
        half4 outColor = 0;
            
        #if defined(PARTICLE)//UI下使用ParticleBase不需要做 Gamma2Linear 转换
        UNITY_FLATTEN
        if(CheckLocalFlags(FLAG_BIT_PARTICLE_UIEFFECT_ON))
        {
            outColor = tex2D(tex, uv);
        }
        else
        {
            outColor = TryLinearizeWithoutAlpha(tex2D(tex, uv));
        }
        #endif
        return outColor;
    }
    //
    // Color blending fragment function
    float4 MixParticleColor(float4 baseColor, float4 particleColor, float4 colorAddSubDiff)
    {
        #if defined(_COLOROVERLAY_ON) // Overlay blend
            float4 output = baseColor;
            output.rgb = lerp(1 - 2 * (1 - baseColor.rgb) * (1 - particleColor.rgb), 2 * baseColor.rgb * particleColor.rgb, step(baseColor.rgb, 0.5));
            output.a *= particleColor.a;
            return output;
        #elif defined(_COLORCOLOR_ON) // Color blend
            half3 aHSL = RgbToHsv(baseColor.rgb);
            half3 bHSL = RgbToHsv(particleColor.rgb);
            half3 rHSL = half3(bHSL.x, bHSL.y, aHSL.z);
            return half4(HsvToRgb(rHSL), baseColor.a * particleColor.a);
        #elif defined(_COLORADDSUBDIFF_ON) // Additive, Subtractive and Difference blends based on 'colorAddSubDiff'
            float4 output = baseColor;
            output.rgb = baseColor.rgb + particleColor.rgb * colorAddSubDiff.x;
            output.rgb = lerp(output.rgb, abs(output.rgb), colorAddSubDiff.y);
            output.a *= particleColor.a;
            return output;
        #else // Default to Multiply blend
        return baseColor * particleColor;
        #endif
    }

    // Soft particles - returns alpha value for fading particles based on the depth to the background pixel
    float SoftParticles(float near, float far, float sceneZ,float thisZ)
    {
        float fade = 1;
        if (near > 0.0 || far > 0.0)
        {
            // fade = saturate(far * ((sceneZ - near) - thisZ));
            float dist = sceneZ - thisZ;
            fade = NB_Remap(dist, near,far,0,1);
        }
        return fade;
    }

    
    // Camera fade - returns alpha value for fading particles based on camera distance
    half CameraFade(float near, float far, float thisZ)
    {
        return saturate((thisZ - near) * far);
    }
    
    //相交位置渐变功能
    half4 Intersect(float IntersectRadius,half4 IntersectColor,float sceneZ,float thisZ)
    {
        half fade = sceneZ - thisZ;
        fade =1- NB_Remap(fade,0,IntersectRadius,0,1);

        half4 c = 0;
        c.rgb = IntersectColor.rgb;
        c.a  = fade*IntersectColor.a;
        
        return c;        
    }

    //遮挡穿透显示功能。
    half OccludeOpacity(half preAlpha,half _OccludeOpacity,half sceneZ,half thisZ)
    {
        half fakeZtest =  step(thisZ,sceneZ);
        return lerp(preAlpha*_OccludeOpacity,preAlpha,fakeZtest);
    }

    
    // Sample a texture and do blending for texture sheet animation if needed
    half4 BlendTexture(sampler2D _Texture, float2 uv, float3 blendUv)
    {
        half4 color = tex2D_TryLinearizeWithoutAlphaFX(_Texture, uv);
    
        half4 color2;
        #ifdef _FLIPBOOKBLENDING_ON
            color2 = tex2D_TryLinearizeWithoutAlphaFX(_Texture, blendUv.xy);
            color = lerp(color, color2, blendUv.z);
        #endif
    
        // if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_ANIMATION_SHEET_HELPER))
        // {
        //     color2 = tex2D_TryLinearizeWithoutAlphaFX(_Texture, blendUv.xy);
        //     color = lerp(color, color2, blendUv.z);
        // }
        return color;
    }

    half4 BlendTexture(Texture2D _Texture, float2 uv, float3 blendUv,uint bits)
    {
        half4 color = SampleTexture2DWithWrapFlags(_Texture,uv,bits);
        half4 color2;
        #ifdef _FLIPBOOKBLENDING_ON
            color2 = SampleTexture2DWithWrapFlags(_Texture,blendUv.xy,bits);
            color = lerp(color, color2, blendUv.z);
        #endif

        // if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_ANIMATION_SHEET_HELPER))
        // {
        //     color2 = SampleTexture2DWithWrapFlags(_Texture, blendUv.xy,bits);
        //     color = lerp(color, color2, blendUv.z);
        // }
        return color;
    }

    float2 UVOffsetAnimaiton(float2 UV,half2 OffsetSpeed)
    {
        float2 newUV =  float2(OffsetSpeed.x*time+UV.x,OffsetSpeed.y*time+UV.y);
        return newUV;
    }
    
    // 采样噪波
    half4 SampleNoise(half4 NoiseOffset, Texture2D _Texture,float2 UV, half3 wordPos)
    {
        
        float2 UV2 = float2(NoiseOffset.x * time + UV.x, NoiseOffset.y * time + UV.y);    //_Time.y

        half4 color = SampleTexture2DWithWrapFlags(_Texture, UV2 ,FLAG_BIT_WRAPMODE_NOISEMAP);
        // color.xy *= color.a;
        
        return color;
    }
    
    
    // // 替换颜色
    // half3 ReplaceColor_float(float3 In, float3 From, float3 To, float Range, float Fuzziness)
    // {
    //     float Distance = distance(From, In);
    //     half3 Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 0.001)));  //e-f? 0.00001
    //     return Out;
    // }

    //漩涡 圆形区域内变形。圆圈中心处的像素会旋转指定角度；圆圈中其他像素的旋转会随着相对于中心距离的变化而减小，在圆圈边缘处减小为零
    float2 UTwirl(float2 UV, float2 Center, float Strength)
    {
        float2 delta = UV - Center;
        float angle = Strength * length(delta);
        float x = cos(angle) * delta.x - sin(angle) * delta.y;
        float y = sin(angle) * delta.x + cos(angle) * delta.y;
        return float2(x + Center.x  , y + Center.y );

    }
    
    //Fresnel
    half4 Unity_FresnelEffect(float3 Normal, float3 ViewDir, float Power, float Dire,half fresnelPos)
    {
        // half aa = saturate(dot(normalize(Normal), (ViewDir)));
        half aa = dot(normalize(Normal), (ViewDir));
        aa = (aa+1)*0.5;
        aa = NB_Remap(aa,fresnelPos,1,0,1);
        aa = lerp(aa, (1 - aa), Dire);
        half Out = pow( aa, Power);
        return Out;
    }

    half3 Rotation(half3 normalizedDirection,half3 rotation)
    {
        half4 Dir = half4(normalizedDirection,1);
        float4x4 M_RotationX = float4x4(
        
        1,0,0,0,
        0,cos(rotation.x),sin(rotation.x),0,
        0,-sin(rotation.x),cos(rotation.x),0,
        0,0,0,1

        );
        float4x4 M_RotationY = float4x4(

        cos(rotation.y),0,sin(rotation.y),0,
        0,1,0,0,
        -sin(rotation.y),0,cos(rotation.y),0,
        0,0,0,1
        
        );
        float4x4 M_RotationZ = float4x4(
        
        cos(rotation.z),sin(rotation.z),0,0,
        -sin(rotation.z),cos(rotation.z),0,0,
        0,0,1,0,
        0,0,0,1
        
        );

        return mul(M_RotationZ,mul(M_RotationY,mul(M_RotationX,Dir)));

    }

    //----------------公告板功能-----------------//
    half3 BillBoard(float3 camPos, float3 vertexPos, int billboardType, int _ReverseZ)
    {
        float3 Z = normalize(mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)));
        if(_ReverseZ == 1)
        {
            Z *= -1;   
        }
        Z.y *= billboardType;
        
        float3 Y = float3(0,1,0);
        float3 X = normalize(cross(Z,Y));
        Y = normalize(cross(X,Z));
        float4x4 M = float4x4(
            X.x, Y.x, Z.x, 0,
            X.y, Y.y, Z.y, 0,
            X.z, Y.z, Z.z, 0,
            0,0,0,1
            );
        float3 newPos = mul(M, vertexPos);
        return newPos;
    }

    half3 BillBoardNormal(float3 camPos, float3 vertexPos, int billboardType, int _ReverseZ)
    {
        float3 Z = normalize(mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)));
        if(_ReverseZ == 1)
        {
            Z *= -1;   
        }
        Z.y *= billboardType;
        
        float3 Y = float3(0,1,0);
        float3 X = normalize(cross(Y,Z));
        Y = normalize(cross(X,Z));
        float4x4 M = float4x4(
            X.x, Y.x, Z.x, 0,
            X.y, Y.y, Z.y, 0,
            X.z, Y.z, Z.z, 0,
            0,0,0,1
            );
        float3 newPos = -mul(M, vertexPos);
        return newPos;
    }


    float2 ParticleUVCommonProcess(float2 originUVAfterTwirlPolar,float4 scaleTilling,float2 offset = float2(0,0),float rotation = 0,float2 rotationCenter = float2(0.5,0.5))
    {
        float2 uv = originUVAfterTwirlPolar;
        uv = Rotate_Radians_float(uv,rotationCenter,rotation);
        uv = uv*scaleTilling.xy + scaleTilling.zw;
        
        uv = UVOffsetAnimaiton(uv,offset);
        
        return uv;
    }

    struct ParticleUVs
    {
        float2 mainTexUV;
        float2 specUV;
        float2 animBlendUV;
        float2 maskMapUV;
        float2 maskMap2UV;
        float2 maskMap3UV;
        float2 emissionUV;
        float2 dissolve_uv;
        float2 dissolve_mask_uv;
        float2 dissolve_noise1_UV;
        float2 dissolve_noise2_UV;
        float2 colorBlendUV;
        float2 noiseMapUV;
        float2 noiseMaskMapUV;
    };

    BaseUVs ProcessBaseUVs(float4 meshTexcoord0, float2 specialUVInTexcoord3,float4 VaryingsP_Custom1,float4 VaryingsP_Custom2,float3 postionOS)
    {
        //UV2的内容在外边就决定好。
            float2 defaultUVChannel = meshTexcoord0.xy;
            float2 specialUVChannel = meshTexcoord0.zw;
            #if _FLIPBOOKBLENDING_ON
                if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM) & CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD2))
                {
                    specialUVChannel = specialUVInTexcoord3;
                }
            #else
                if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_UV_FROM_MESH))
                {
                    //Mesh条件下开启使用特殊UV通道的情况
                    if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD1))
                    {
                        specialUVChannel = VaryingsP_Custom1.xy;
                    }
                    if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD2))
                    {
                        specialUVChannel = VaryingsP_Custom2.xy;
                    }
                }
                else
                {
                    //只有在粒子系统下开启特殊通道的情况，会在面板层引导合并相关内容。UI/Mesh不开启特殊通道没有意义。
                    specialUVChannel = meshTexcoord0.zw;
                }
            #endif
          
            
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE))
            {
                defaultUVChannel = defaultUVChannel*_MainTex_Reverse_ST.xy +_MainTex_Reverse_ST.zw;
            }
            //TODO：补写MeshUV的实现
            float2 cylinderUV = meshTexcoord0.xy;
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_CYLINDER_CORDINATE))
            {
                float4x4 _CylinderUVMatrix = float4x4(_CylinderMatrix0,_CylinderMatrix1,_CylinderMatrix2,_CylinderMatrix3);
                postionOS = mul(_CylinderUVMatrix,float4(postionOS,1));
                cylinderUV = CylinderCoordinate(postionOS);
            }
            
            float2 UVAfterTwirlPolar = defaultUVChannel;
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_UTWIRL_ON))
            {
               UVAfterTwirlPolar = UTwirl(defaultUVChannel,_TWParameter.xy, _TWStrength);
            }
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_POLARCOORDINATES_ON))
            {
                float2 UVAfterTwirl = UVAfterTwirlPolar;
                UVAfterTwirlPolar = PolarCoordinates(UVAfterTwirlPolar,_PCCenter.xy);
                UVAfterTwirlPolar = lerp(UVAfterTwirl,UVAfterTwirlPolar,_PCCenter.z);
            }
            BaseUVs baseUVs = (BaseUVs)0;
            baseUVs.defaultUVChannel = defaultUVChannel;
            baseUVs.specialUVChannel = specialUVChannel;
            baseUVs.uvAfterTwirlPolar = UVAfterTwirlPolar;
            baseUVs.cylinderUV = cylinderUV;
            return baseUVs;
    }

    void ParticleProcessUV(float4 meshTexcoord0, float2 specialUVInTexcoord3,inout ParticleUVs particleUVs,float4 VaryingsP_Custom1,float4 VaryingsP_Custom2,float2 screenUV,float3 postionOS)
    {
        BaseUVs baseUVs= ProcessBaseUVs(meshTexcoord0,specialUVInTexcoord3,VaryingsP_Custom1,VaryingsP_Custom2,postionOS);
        
        particleUVs.specUV = baseUVs.specialUVChannel;
        float2 baseMapUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_MAINTEX,baseUVs);

        #ifdef _FLIPBOOKBLENDING_ON //开启序列帧融合
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_ANIMATION_SHEET_HELPER))
            {
                //走AnimationSheetHelper脚本的情况，永远和baseMap同步。
                particleUVs.animBlendUV = baseMapUV*_BaseMap_AnimationSheetBlend_ST.xy+_BaseMap_AnimationSheetBlend_ST.zw;
            }
            else
            {
                //走粒子的情况
                particleUVs.animBlendUV = meshTexcoord0.zw;
            }
        #endif


        #ifdef _SCREEN_DISTORT_MODE
            particleUVs.mainTexUV = screenUV;
        #else
            baseMapUV = Rotate_Radians_float(baseMapUV, half2(0.5, 0.5), _BaseMapUVRotation);  //主贴图旋转
            UNITY_BRANCH
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_UIEFFECT_ON) & !CheckLocalFlags1(FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE))
            {
                if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE))
                {
                    float2 originUV = meshTexcoord0.xy;//精灵主贴图不调整。
                    particleUVs.mainTexUV = originUV*_UI_MainTex_ST.xy+_UI_MainTex_ST.zw;
                }
                else
                {
                    particleUVs.mainTexUV = baseMapUV*_UI_MainTex_ST.xy+_UI_MainTex_ST.zw;
                }
            }
            else
            {
                baseMapUV.x += GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_X,0,VaryingsP_Custom1,VaryingsP_Custom2);
                baseMapUV.y += GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_Y,0,VaryingsP_Custom1,VaryingsP_Custom2);
                particleUVs.mainTexUV = TRANSFORM_TEX(baseMapUV, _BaseMap);  //主帖图UV重复和偏移
            }
            particleUVs.mainTexUV = UVOffsetAnimaiton(particleUVs.mainTexUV,_BaseMapMaskMapOffset.xy);
            
        #endif
        

        #if defined(_MASKMAP_ON)
        
            float2 MaskMapuv = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_MASKMAP,baseUVs);

            UNITY_BRANCH
            if(CheckLocalFlags(FLAG_BIT_PARTILCE_MASKMAPROTATIONANIMATION_ON))
            {
                _MaskMapUVRotation += time * _MaskMapRotationSpeed;
            }

            MaskMapuv= Rotate_Radians_float(MaskMapuv, half2(0.5, 0.5), _MaskMapUVRotation);
        
            MaskMapuv = TRANSFORM_TEX(MaskMapuv, _MaskMap);
        
            MaskMapuv.x += GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_X,0,VaryingsP_Custom1,VaryingsP_Custom2);
            MaskMapuv.y += GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_Y,0,VaryingsP_Custom1,VaryingsP_Custom2);

            MaskMapuv = UVOffsetAnimaiton(MaskMapuv,_MaskMapOffsetAnition.xy);
            particleUVs.maskMapUV = MaskMapuv;

            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_MASK_MAP2))
            {
                float2 maskMap2UV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_MASKMAP_2,baseUVs);
                maskMap2UV = maskMap2UV * _MaskMap2_ST.xy + _MaskMap2_ST.zw;
                    
                maskMap2UV = UVOffsetAnimaiton(maskMap2UV,_MaskMapOffsetAnition.zw);
                particleUVs.maskMap2UV = maskMap2UV;
            }

            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_MASK_MAP3))
            {
                float2 maskMap3UV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_MASKMAP_3,baseUVs);
                maskMap3UV = maskMap3UV* _MaskMap3_ST.xy + _MaskMap3_ST.zw;
                
                maskMap3UV = UVOffsetAnimaiton(maskMap3UV,_MaskMap3OffsetAnition.xy);
                particleUVs.maskMap3UV = maskMap3UV;
            }
        
        #endif

        #if defined(_EMISSION)
            float2 emissionUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_EMISSION_MAP,baseUVs);
            particleUVs.emissionUV = ParticleUVCommonProcess(emissionUV,_EmissionMap_ST,_EmissionMapUVOffset.xy,_EmissionMapUVRotation);
        #endif

        #if defined(_DISSOLVE)
            // if(CheckLocalFlags1(FLAG_BIT_PARTICLE_CUSTOMDATA1X_DISSOLVETEXOFFSETX))
            // {
            //     _DissolveMap_ST.z += VaryingsP_Custom1.x;
            // }
            // if(CheckLocalFlags1(FLAG_BIT_PARTICLE_CUSTOMDATA1Y_DISSOLVETEXOFFSETY))
            // {
            //     _DissolveMap_ST.w += VaryingsP_Custom1.y;
            // }
            _DissolveMap_ST.z += GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_X,0,VaryingsP_Custom1,VaryingsP_Custom2);
            _DissolveMap_ST.w += GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_Y,0,VaryingsP_Custom1,VaryingsP_Custom2);
        
            float2 dissolveUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_DISSOLVE_MAP,baseUVs);
            particleUVs.dissolve_uv = ParticleUVCommonProcess(dissolveUV,_DissolveMap_ST,_DissolveOffsetRotateDistort.xy,_DissolveOffsetRotateDistort.z);
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_DISSOLVE_MASK))
            {
                float2 dissolveMaskUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_DISSOLVE_MASK_MAP,baseUVs);
                particleUVs.dissolve_mask_uv = ParticleUVCommonProcess(dissolveMaskUV,_DissolveMaskMap_ST,float2(0,0),_DissolveOffsetRotateDistort.z);
            }
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_DISSOVLE_VORONOI))
            {
                float2 halfUV = dissolveUV;
                // halfUV.x = abs(halfUV.x-0.5);//20240729不明白当时为什么要做这个ABS处理。先注销掉看看。
                _DissolveVoronoi_Vec4.x += GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE1_OFFSET_X,0,VaryingsP_Custom1,VaryingsP_Custom2);
                _DissolveVoronoi_Vec4.y += GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE1_OFFSET_Y,0,VaryingsP_Custom1,VaryingsP_Custom2);
                _DissolveVoronoi_Vec4.z += GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE2_OFFSET_X,0,VaryingsP_Custom1,VaryingsP_Custom2);
                _DissolveVoronoi_Vec4.w += GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE2_OFFSET_Y,0,VaryingsP_Custom1,VaryingsP_Custom2);
                particleUVs.dissolve_noise1_UV = halfUV * _DissolveVoronoi_Vec.xy + _DissolveVoronoi_Vec4.xy + time*_DissolveVoronoi_Vec3.xy;
                particleUVs.dissolve_noise2_UV = halfUV * _DissolveVoronoi_Vec.zw + _DissolveVoronoi_Vec4.zw + time*_DissolveVoronoi_Vec3.zw;
            }
        #endif
        
        #ifdef _COLORMAPBLEND
            float2 colorBlendUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_COLOR_BLEND_MAP,baseUVs);
            particleUVs.colorBlendUV = ParticleUVCommonProcess(colorBlendUV,_ColorBlendMap_ST,_ColorBlendMapOffset.xy);
        #endif

           half cum_noise = 0;

        //TODO
           
        #if defined(_NOISEMAP)
            
            //和ParticleUVCommonProcess相比，此处没有UV动画，NoiseMap的UV流动在最终的SampleNoise中进行
            float2 noiseMapUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_NOISE_MAP,baseUVs);
            particleUVs.noiseMapUV = ParticleUVCommonProcess(noiseMapUV,_NoiseMap_ST,half2(0,0),_NoiseMapUVRotation);

            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_NOISE_MASKMAP))
            {
                float2 noiseMaskMapUV = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_NOISE_MASK_MAP,baseUVs);
                particleUVs.noiseMaskMapUV = ParticleUVCommonProcess(noiseMaskMapUV,_NoiseMaskMap_ST,half2(0,0),0);
               
            }
        #endif
    }

    Texture2D _VertexOffset_Map;
    Texture2D _VertexOffset_MaskMap;
    
    // half3  _VertexOffset_Vec;
    // half3 _VertexOffset_CustomDir;
    // half4 _VertexOffset_Map_ST;

    half3 VetexOffset(half3 positionOS,half2 originUV,half2 originMaskUV,half3 normalOS)
    {
        half2 uv = TRANSFORM_TEX(originUV,_VertexOffset_Map);
        uv = UVOffsetAnimaiton(uv,_VertexOffset_Vec.xy);
        // half vertexOffsetSample = tex2Dlod(_VertexOffset_Map,half4(uv,0,0));
        half vertexOffsetSample = SampleTexture2DWithWrapFlags(_VertexOffset_Map,uv,FLAG_BIT_WRAPMODE_VERTEXOFFSET_MASKMAP,true,0);
        // UNITY_BRANCH
        // if(CheckLocalWrapFlags(FLAG_BIT_WRAPMODE_VERTEXOFFSETMAP))
        // {
        //     vertexOffsetSample = SAMPLE_TEXTURE2D_LOD(_VertexOffset_Map,sampler_linear_clamp,uv,0);
        // }
        // else
        // {
        //     vertexOffsetSample = SAMPLE_TEXTURE2D_LOD(_VertexOffset_Map,sampler_linear_repeat,uv,0);
        // }

        if (!CheckLocalFlags1(FLAG_BIT_PARTICLE_1_VERTEXOFFSET_START_FROM_ZERO))
        {
            vertexOffsetSample = vertexOffsetSample*2-1;
        }

        half3 finalPos;
        half vertexOffsetMask = 1;
        if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_VERTEXOFFSET_MASKMAP))
        {
            half2 maskUV = TRANSFORM_TEX(originMaskUV,_VertexOffset_MaskMap);
            maskUV = UVOffsetAnimaiton(maskUV,_VertexOffset_MaskMap_Vec.xy);
            half vertexOffsetMaskSample = SampleTexture2DWithWrapFlags(_VertexOffset_MaskMap,maskUV,FLAG_BIT_WRAPMODE_VERTEXOFFSET_MASKMAP,true,0);
            vertexOffsetMask = lerp(1,vertexOffsetMaskSample,_VertexOffset_MaskMap_Vec.z);
        }
     
        UNITY_BRANCH
        if(CheckLocalFlags(FLAG_BIT_PARTICLE_VERTEX_OFFSET_NORMAL_DIR))
        {
            finalPos = positionOS + normalOS*_VertexOffset_Vec.z*vertexOffsetSample*vertexOffsetMask;
        }
        else
        {
            finalPos = positionOS + _VertexOffset_CustomDir*_VertexOffset_Vec.z*vertexOffsetSample*vertexOffsetMask;
        }


        return finalPos;
        
    }

    //向UV横向两边的色散。
    half4 DistortionChoraticaberrat(Texture2D baseTexture,half2 originUV, half2 uvAfterNoise,half ChoraticaberratIntensity,uint bits)
    {
        half2 delta = half2(originUV.x *2-1,0);

        if(CheckLocalFlags(FLAG_BIT_PARTICLE_NOISE_CHORATICABERRAT_WITH_NOISE))
        {
            half2  NoiseIntensity = uvAfterNoise - originUV;
            half noiseXIntensity = abs(NoiseIntensity.x);
            delta *= ChoraticaberratIntensity*noiseXIntensity;
        }
        else
        {
            delta *= ChoraticaberratIntensity;
        }
        
        half2 ra = SampleTexture2DWithWrapFlags(baseTexture,uvAfterNoise,bits).xw;
        ra.r *= ra.y;
        half2 ga = SampleTexture2DWithWrapFlags(baseTexture,uvAfterNoise - delta,bits).yw;
        ga.r *= ga.y;
        half2 ba = SampleTexture2DWithWrapFlags(baseTexture,uvAfterNoise - delta*2,bits).zw;
        ba.r *= ba.y;
        return half4(ra.r,ga.r,ba.r,clamp(ra.y*0.5+ga.y*0.5+ba.y*0.5,0,1));
    }

    bool needSceneDepth()
    {
        #if defined(_DEPTH_DECAL) || defined(_SOFTPARTICLES_ON)
            return true;
        #endif

        if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_DEPTH_OUTLINE))
        {
            return true;
        }

        return  false;
    }

    bool needEyeDepth()
    {
        #if defined(_SOFTPARTICLES_ON)
            return true;
        #endif

        if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_DEPTH_OUTLINE)||CheckLocalFlags(FLAG_BIT_PARTICLE_DISTANCEFADE_ON))
        {
            return true;
        }
        
        return  false;
    }

    bool ignoreFresnel()
    {
        #if defined(PARTICLE_BACKFACE_PASS)
        return  true;
        #endif
        return false;
    }

    Texture2D _ParallaxMapping_Map;

    half2 ParallaxMappingSimple(half2 texCoords, half3 viewDir)
    {
         float height = SampleTexture2DWithWrapFlags(_ParallaxMapping_Map,texCoords,FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP).r;
         height *= _ParallaxMapping_Intensity;
         viewDir = normalize(viewDir);
         viewDir.xy /= (viewDir.z);
         texCoords -= viewDir.xy * height;
         return  texCoords;
    }

    half2 ParallaxMappingPeelDepth(half2 texCoords, half3 viewDir)
    {
        const float minLayers = 2;
        const float maxLayers = 32;
        float numLayers = lerp(maxLayers, minLayers, abs(dot(half3(0.0, 0.0, 1.0), viewDir)));//视线越垂直于表面，层数越少，反之越多。
        float layerDepth = 1/numLayers;
        float currentLayerDepth = 0;
        // viewDir.xy/=viewDir.z;
        half2 p = viewDir.xy*_ParallaxMapping_Intensity;
        half2 deltaTexcoord = p/numLayers;

        half2 currentTexcoords = texCoords;
        float currentMapDepthValue = SampleTexture2DWithWrapFlags(_ParallaxMapping_Map,currentTexcoords,FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP).r;

        [loop]
        while (currentLayerDepth < currentMapDepthValue )
        {
            currentTexcoords -= deltaTexcoord;
            currentMapDepthValue = SampleTexture2DWithWrapFlags(_ParallaxMapping_Map,currentTexcoords,FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP).r;
            currentLayerDepth += layerDepth;
        }

        return  currentTexcoords;
        
    }
    half2 ParallaxOcclusionMapping(half2 texCoords, half3 viewDir)
    { 
        // number of depth layers
        // const float minLayers = 10;
        // const float maxLayers = 10;
        const float minLayers = _ParallaxMapping_Vec.x;
        const float maxLayers = _ParallaxMapping_Vec.y;
        float numLayers = lerp(maxLayers, minLayers, abs(dot(half3(0.0, 0.0, 1.0), viewDir)));  
        // calculate the size of each layer
        float layerDepth = 1.0 / numLayers;
        // depth of current layer
        float currentLayerDepth = 0.0;
        // the amount to shift the texture coordinates per layer (from vector P)
        half2 P = viewDir.xy / viewDir.z * _ParallaxMapping_Intensity; 
        half2 deltaTexCoords = P / numLayers;
      
        // get initial values
        half2  currentTexCoords     = texCoords;
        float currentDepthMapValue = SampleTexture2DWithWrapFlags(_ParallaxMapping_Map, currentTexCoords,FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP).r;
        currentLayerDepth = clamp(currentLayerDepth,0,1);

        int i = 0;
        [loop]
        while(currentLayerDepth < currentDepthMapValue && i<numLayers)
        {
            // shift texture coordinates along direction of P
            currentTexCoords -= deltaTexCoords;
            // get depthmap value at current texture coordinates
            currentDepthMapValue = SampleTexture2DWithWrapFlags(_ParallaxMapping_Map, currentTexCoords,FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP).r;  
            // get depth of next layer
            currentLayerDepth += layerDepth;
            i++;
        }
        
        // -- parallax occlusion mapping interpolation from here on
        // get texture coordinates before collision (reverse operations)
        half2 prevTexCoords = currentTexCoords + deltaTexCoords;

        // get depth after and before collision for linear interpolation
        float afterDepth  = currentDepthMapValue - currentLayerDepth;
        float beforeDepth = SampleTexture2DWithWrapFlags(_ParallaxMapping_Map, prevTexCoords,FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP).r - currentLayerDepth + layerDepth;
     
        // interpolation of texture coordinates
        float weight = afterDepth / (afterDepth - beforeDepth);
        half2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

        return finalTexCoords;
    }
#endif