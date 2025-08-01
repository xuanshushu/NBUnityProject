#ifndef PARTICLESUNLITFORWARDPASS
    #define PARTICLESUNLITFORWARDPASS
    #include "HLSL/ParticlesUnlitInputNew.hlsl"
    #include "HLSL/SixWaySmokeLit.hlsl"

    
    ///////////////////////////////////////////////////////////////////////////////
    //                  Vertex and Fragment functions                            //

    
    VaryingsParticle vertParticleUnlit(AttributesParticle input)
    {
        VaryingsParticle output = (VaryingsParticle)0;

        output.VaryingsP_Custom1 = input.Custom1; 
        output.VaryingsP_Custom2 = input.Custom2; 
        
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        
        time = _Time.y;

        float4 positionOS = input.vertex;

        if(CheckLocalFlags(FLAG_BIT_PARTICLE_VERTEX_OFFSET_ON))
        {
            //因为极坐标和旋转会强制到Frag计算，所以顶点在这边特殊处理一遍。
            BaseUVs baseUVsForVertexOffset = ProcessBaseUVs(input.texcoords,0,output.VaryingsP_Custom1,output.VaryingsP_Custom2,positionOS);
            
            _VertexOffset_Map_ST.z += GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_X,0,input.Custom1,input.Custom2);
            _VertexOffset_Map_ST.w += GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_Y,0,input.Custom1,input.Custom2);
            _VertexOffset_Vec.z = GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_VERTEXOFFSET_INTENSITY,_VertexOffset_Vec.z,input.Custom1,input.Custom2);

            if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_VERTEXOFFSET_MASKMAP))
            {
                _VertexOffset_MaskMap_ST.z += GetCustomData(_W9ParticleCustomDataFlag3,FLAGBIT_POS_3_CUSTOMDATA_VERTEX_OFFSET_MASK_X,0,input.Custom1,input.Custom2);
                _VertexOffset_MaskMap_ST.w += GetCustomData(_W9ParticleCustomDataFlag3,FLAGBIT_POS_3_CUSTOMDATA_VERTEX_OFFSET_MASK_Y,0,input.Custom1,input.Custom2);
            }
            
            float2 vertexOffsetUVs = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_VERTEX_OFFSET_MAP,baseUVsForVertexOffset);
            float2 vertexOffsetMaskUVs = GetUVByUVMode(_UVModeFlag0,FLAG_BIT_UVMODE_POS_0_VERTEX_OFFSET_MASKMAP,baseUVsForVertexOffset);

            positionOS.xyz = VetexOffset(positionOS,vertexOffsetUVs,vertexOffsetMaskUVs,input.normalOS);
        }
        
        
        // position ws is used to compute eye depth in vertFading
        output.positionWS.xyz = mul(unity_ObjectToWorld, positionOS).xyz;
        output.positionOS.xyz = positionOS;

        output.clipPos = TransformObjectToHClip(positionOS);
        
        #ifdef _PARALLAX_MAPPING
            //视差贴图，需要在Tangent空间下计算。
            float3x3 objectToTangent =
                float3x3(
                    input.tangentOS.xyz,
                    cross(input.normalOS,input.tangentOS.xyz)  * input.tangentOS.w,//Bitangent
                    input.normalOS
                );
            output.tangentViewDir = mul(objectToTangent,GetObjectSpaceNormalizeViewDir(positionOS));
        #endif
        
        float unityFogFactor = ComputeFogFactor(output.clipPos.z);

        output.positionWS.w = unityFogFactor;
        
        output.color = TryLinearize(input.color);

        output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS.xyz);
        output.normalWSAndAnimBlend.xyz = TransformObjectToWorldNormal(input.normalOS.xyz);

        #if defined(_NORMALMAP)||defined(_FX_LIGHT_MODE_SIX_WAY) 
            real sign = input.tangentOS.w * GetOddNegativeScale();
            half3 tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
            output.tangentWS = half4(tangentWS,sign);
        #endif


        #ifndef _FX_LIGHT_MODE_UNLIT
            #ifdef _FX_LIGHT_MODE_SIX_WAY
                float3 bitangent = sign * cross(output.normalWSAndAnimBlend.xyz, output.tangentWS.xyz);
                // GetSixWayBakeDiffuseLight(output.normalWSAndAnimBlend.xyz,output.tangentWS,bitangent,
                //     output.bakeDiffuseLighting0,output.bakeDiffuseLighting1,output.bakeDiffuseLighting2,
                //     output.backBakeDiffuseLighting0,output.backBakeDiffuseLighting1,output.backBakeDiffuseLighting2);

                GetSixWayBakeDiffuseLight(output.normalWSAndAnimBlend.xyz,output.tangentWS,bitangent,
                    output.bakeDiffuseLighting0,output.bakeDiffuseLighting1,output.bakeDiffuseLighting2,
                    output.backBakeDiffuseLighting0,output.backBakeDiffuseLighting1,output.backBakeDiffuseLighting2);

            #else
                OUTPUT_SH(output.normalWSAndAnimBlend.xyz, output.vertexSH);
                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                output.vertexLight = VertexLighting(output.positionWS.xyz, output.normalWSAndAnimBlend.xyz);
                #endif
            #endif
        #endif
        
        
        
        UNITY_FLATTEN
        if(CheckLocalFlags(FLAG_BIT_PARTICLE_FRESNEL_ON))
        {
            output.fresnelViewDir = output.viewDirWS; 
        }

        output.texcoord.xy = input.texcoords.xy;

     
  
        
        
        //顶点处理的原则：
        //Twirl和极坐标,贴花处理，在片段着色器层处理UV。
        //BaseMap，遮罩Mask，Noise，高光（自发光） 和极坐标处理相关。
        if(!isProcessUVInFrag())
        {

            float2 specialUVInTexcoord3 = 0;
            //如果同时在粒子系统里开启序列帧融帧和特殊UV通道模式。
            #if _FLIPBOOKBLENDING_ON
                if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM) & (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD1)|CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD2)))
                {
                    specialUVInTexcoord3 = input.texcoordBlend.yz;
                    output.texcoord2AndSpecialUV.zw = specialUVInTexcoord3;
                }
            #endif
            ParticleUVs particleUVs = (ParticleUVs)0;
            float2 screenUV = 0;
            
            ParticleProcessUV(input.texcoords, specialUVInTexcoord3,particleUVs,output.VaryingsP_Custom1,output.VaryingsP_Custom2,screenUV,output.positionOS.xyz);
            output.texcoord2AndSpecialUV.xy = particleUVs.animBlendUV;
            output.texcoord2AndSpecialUV.zw= particleUVs.specUV;
            output.texcoord.xy = particleUVs.mainTexUV;
            output.texcoord.zw = particleUVs.maskMapUV;
           
            output.texcoordMaskMap2.xy = particleUVs.maskMap2UV;
            output.texcoordMaskMap2.zw = particleUVs.maskMap3UV;
            #ifdef _NORMALMAP
                output.bumpTexTexcoord.xy = particleUVs.bumpTexUV;

            #endif
            
            #if defined (_EMISSION)   || defined(_COLORMAPBLEND)
                output.emissionColorBlendTexcoord.xy = particleUVs.emissionUV;
                output.emissionColorBlendTexcoord.zw = particleUVs.colorBlendUV;
            #endif

            #ifdef _NOISEMAP
                output.noisemapTexcoord.xy = particleUVs.noiseMapUV;
                output.noisemapTexcoord.zw = particleUVs.noiseMaskMapUV;
            #endif
            #if defined(_DISSOLVE) 
                output.dissolveTexcoord.xy = particleUVs.dissolve_uv;
                output.dissolveTexcoord.zw = particleUVs.dissolve_mask_uv;
                output.dissolveNoiseTexcoord.xy = particleUVs.dissolve_noise1_UV;
                output.dissolveNoiseTexcoord.zw = particleUVs.dissolve_noise2_UV;
            #endif
        }
        else
        {
            output.texcoord = input.texcoords;
            #if _FLIPBOOKBLENDING_ON
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM) & CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD2))
            {
                output.texcoord2AndSpecialUV.zw = input.texcoordBlend.yz;
            }
            #endif
        }
        #ifdef _FLIPBOOKBLENDING_ON
            //粒子帧融合的情况，兼容一下。
            output.normalWSAndAnimBlend.w = input.texcoordBlend.x;
        #endif

        


        
        UNITY_BRANCH
        if(needEyeDepth())
        {
            float4 ndc = output.clipPos*0.5f;
            output.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
            output.positionNDC.zw = output.clipPos.zw;
        }
 
        return output;
    }


    ///////////////////////Fragment functions  ////////////////////////
    
    half4 fragParticleUnlit(VaryingsParticle input, half facing : VFACE): SV_Target
    {
        
        input.viewDirWS = normalize(input.viewDirWS );
        
        
        UNITY_SETUP_INSTANCE_ID(input);

        time = _Time.y;

        float2 screenUV = input.clipPos.xy / _ScaledScreenParams.xy;
        
        real sceneZBufferDepth = 0;
        real sceneZ = 0;
        
        UNITY_BRANCH
        if(needSceneDepth())
        {
            #if UNITY_REVERSED_Z
            sceneZBufferDepth = SampleSceneDepth(screenUV);
            #else
            // Adjust z to match NDC for OpenGL
            sceneZBufferDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
            #endif
            sceneZ = (unity_OrthoParams.w == 0) ? LinearEyeDepth(sceneZBufferDepth, _ZBufferParams) : LinearDepthToEyeDepth(sceneZBufferDepth);//场景当前深度
        }
    
        real thisZ = 0;
        if(needEyeDepth())
        {
            thisZ = LinearEyeDepth(input.positionNDC.z / input.positionNDC.w, _ZBufferParams);//当前Frag深度。
        }
        

        #ifdef _DEPTH_DECAL
            float3 fragWorldPos = ComputeWorldSpacePosition(screenUV, sceneZBufferDepth, UNITY_MATRIX_I_VP);
            float3 fragobjectPos = TransformWorldToObject(fragWorldPos);
        
            float3 absFragObjectPos = abs(fragobjectPos);
            half clipValue = step(absFragObjectPos.x,0.5);
            clipValue *= step(absFragObjectPos.y,0.5);
            clipValue *= step(absFragObjectPos.z,0.5);
            half decalAlpha = NB_Remap (abs(fragobjectPos.y),0.1,0.5,1,0);
            decalAlpha *= clipValue;
            float2 decalUV = fragobjectPos.xz + 0.5;

        #endif

        float4 uv = input.texcoord;
        #ifdef _DEPTH_DECAL
            uv.xy = decalUV;
        #endif

        float3 blendUv;
        blendUv.xy = input.texcoord2AndSpecialUV.xy;
        blendUv.z = input.normalWSAndAnimBlend.w;
        float2 MaskMapuv;
        float2 MaskMapuv2;
        float2 MaskMapuv3;
        float2 noiseMap_uv;
        float2 noiseMaskMap_uv;
        float2 colorBlendMap_uv;
        float2 emission_uv;
        float2 dissolve_uv;
        float2 dissolve_mask_uv;
        float4 dissolve_noise_uv;
        float2 BumpTex_uv;

        //如果同时在粒子系统里开启序列帧融帧和特殊UV通道模式。
        
        if(isProcessUVInFrag())
        {
            float2 specialUVInTexcoord3 = 0;
            #if _FLIPBOOKBLENDING_ON
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM) & (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_USE_TEXCOORD2)))
            {
                specialUVInTexcoord3 = input.texcoord2AndSpecialUV.zw;
            }
            
            #endif
            ParticleUVs particleUVs = (ParticleUVs)0;
            ParticleProcessUV(uv,specialUVInTexcoord3,particleUVs,input.VaryingsP_Custom1,input.VaryingsP_Custom2,screenUV,input.positionOS.xyz);
            uv.xy = particleUVs.mainTexUV;
            blendUv.xy = particleUVs.animBlendUV;
            MaskMapuv = particleUVs.maskMapUV;
            MaskMapuv2 = particleUVs.maskMap2UV;
            MaskMapuv3 = particleUVs.maskMap3UV;
            emission_uv = particleUVs.emissionUV;
            dissolve_uv = particleUVs.dissolve_uv;
            dissolve_mask_uv = particleUVs.dissolve_mask_uv;
            colorBlendMap_uv = particleUVs.colorBlendUV;
            noiseMap_uv = particleUVs.noiseMapUV;
            noiseMaskMap_uv = particleUVs.noiseMaskMapUV;
            dissolve_noise_uv = float4(particleUVs.dissolve_noise1_UV,particleUVs.dissolve_noise2_UV);
            BumpTex_uv = particleUVs.bumpTexUV;
            
        }
        else
        {
            MaskMapuv = input.texcoord.zw;
            MaskMapuv2 = input.texcoordMaskMap2.xy;
            MaskMapuv3 = input.texcoordMaskMap2.zw;
            
            #ifdef _NORMALMAP
            BumpTex_uv = input.bumpTexTexcoord.xy;
            #endif
            
            #ifdef _NOISEMAP
                noiseMap_uv = input.noisemapTexcoord.xy;
                noiseMaskMap_uv = input.noisemapTexcoord.zw;
            #endif
            
            #if defined (_EMISSION)   || defined(_COLORMAPBLEND)
                emission_uv = input.emissionColorBlendTexcoord.xy;
                colorBlendMap_uv = input.emissionColorBlendTexcoord.zw;
            #endif
            
            #ifdef _DISSOLVE
                dissolve_uv = input.dissolveTexcoord.xy;
                dissolve_mask_uv = input.dissolveTexcoord.zw;
                dissolve_noise_uv = input.dissolveNoiseTexcoord;
            #endif
        }
        half2 originUV = uv;

        #ifdef _PARALLAX_MAPPING
            uv.xy = ParallaxOcclusionMapping(uv,input.tangentViewDir);
        #endif
        
        half2 cum_noise = 0;
        half2 cum_noise_xy = 0.5;
        half noiseMask = 1;
        #if defined(_NOISEMAP)
            half4 noiseSample = SampleNoise(_NoiseOffset, _NoiseMap, noiseMap_uv, input.positionWS.xyz);
            cum_noise = noiseSample.xy;
            UNITY_FLATTEN
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_NOISEMAP_NORMALIZEED_ON))
            {
                cum_noise = cum_noise * 2 - 1;
            }
            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_NOISE_MASKMAP))
            {
                half4 noiseMaskSample = SampleTexture2DWithWrapFlags(_NoiseMaskMap,noiseMaskMap_uv,FLAG_BIT_WRAPMODE_NOISE_MASKMAP);
                noiseMask= GetColorChannel(noiseMaskSample,FLAG_BIT_COLOR_CHANNEL_POS_0_NOISE_MASK);
                noiseMask *= noiseSample.a;
            }
            _TexDistortion_intensity = GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_NOISE_INTENSITY,_TexDistortion_intensity,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
    
            _DistortionDirection.x += GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_NOISE_DIRECTION_X,0,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
            _DistortionDirection.y += GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_NOISE_DIRECTION_Y,0,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
            // 将扭曲放到post去做
            #if defined(_SCREEN_DISTORT_MODE)
                cum_noise_xy = cum_noise * _TexDistortion_intensity * _DistortionDirection.xy;
                cum_noise_xy = cum_noise_xy * 1.25 + 0.5;
            #endif

            float2 mainTexNoise =  cum_noise * noiseMask * _TexDistortion_intensity * _DistortionDirection.xy;
            uv.xy += mainTexNoise;//主贴图纹理扭曲
            blendUv.xy += mainTexNoise;
        #endif

        // SampleAlbedo--------------------
        half4 albedo = 0;
        #if defined(_SCREEN_DISTORT_MODE)
            albedo = half4(cum_noise_xy, 1.0, noiseMask);
        #else
        
            UNITY_FLATTEN
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_BACKCOLOR))
            {
                _BaseColor = facing > 0 ? _BaseColor : _BaseBackColor;
            }


            Texture2D baseMap;
            
            #ifdef _SCREEN_DISTORT_MODE
                baseMap = _ScreenColorCopy1;
            #else
                baseMap = _BaseMap;
            #endif
        
            UNITY_BRANCH
            if (CheckLocalFlags(FLAG_BIT_PARTICLE_UIEFFECT_ON) & !CheckLocalFlags1(FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE))
            {
                albedo = BlendTexture(_MainTex, uv, blendUv) * _Color;
            }
            else if (CheckLocalFlags(FLAG_BIT_PARTICLE_CHORATICABERRAT))
            {
               
                _DistortionDirection.z = GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_CHORATICABERRAT_INTENSITY,_DistortionDirection.z,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
                _DistortionDirection.z *= 0.1;
                albedo = DistortionChoraticaberrat(baseMap,originUV,uv,_DistortionDirection.z,FLAG_BIT_WRAPMODE_BASEMAP);
            }
            else
            {
                
                 albedo = BlendTexture(baseMap, uv, blendUv,FLAG_BIT_WRAPMODE_BASEMAP);
                
            }

            albedo.a = GetColorChannel(albedo,FLAG_BIT_COLOR_CHANNEL_POS_0_MAINTEX_ALPHA);
        
            albedo *= _BaseColor ;
            albedo.rgb *= _BaseColorIntensityForTimeline;

        #endif
        
        half alpha = albedo.a;
        half3 result = albedo.rgb;
        
        #ifdef _FX_LIGHT_MODE_SIX_WAY
        float4 rigRTBkSample  = BlendTexture(_RigRTBk, uv, blendUv,FLAG_BIT_WRAPMODE_BASEMAP);
        float4 rigLBtFSample  = BlendTexture(_RigLBtF, uv, blendUv,FLAG_BIT_WRAPMODE_BASEMAP);
        #endif
      
        UNITY_BRANCH
        if(CheckLocalFlags(FLAG_BIT_HUESHIFT_ON))
        {
            half3 hsv = RgbToHsv(result);
            _HueShift = GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_HUESHIFT,_HueShift,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
            hsv.r += _HueShift;
            result = HsvToRgb(hsv);
        }
        
        if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_MAINTEX_CONTRAST))
        {
            _Contrast = GetCustomData(_W9ParticleCustomDataFlag2,FLAGBIT_POS_2_CUSTOMDATA_MAINTEX_CONTRAST,_Contrast,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
            result.rgb = lerp(_ContrastMidColor,result.rgb,_Contrast);
        }

        
        if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_MAINTEX_COLOR_REFINE))
        {
            half3 colorA = result.rgb*_BaseMapColorRefine.x;
            half3 colorB = pow(result.rgb,_BaseMapColorRefine.y)*_BaseMapColorRefine.z;
            result.rgb = lerp(colorA,colorB,_BaseMapColorRefine.w);
        }
        
        half3 normalTS = half3(0, 0, 1);//TODO

        if (CheckLocalFlags1(FLAG_BIT_PARTICLE_1_BUMP_TEX_UV_FOLLOW_MAINTEX))
        {
            BumpTex_uv = uv;
        }

        half metallic = 1;
        half smoothness = 1;
        #ifdef _NORMALMAP
        
        half4 normalMapSample = SampleTexture2DWithWrapFlags(_BumpTex,BumpTex_uv,FLAG_BIT_WRAPMODE_BUMPTEX);
        if (CheckLocalFlags(FLAG_BIT_PARTICLE_NORMALMAP_MASK_MODE))
        {
            normalTS = UnpackNormalRGB(half4(normalMapSample.xy,0,1),_BumpScale);
            metallic *= normalMapSample.z;
            smoothness *= normalMapSample.w;
        }
        else
        {
            normalTS = UnpackNormalScale(half4(normalMapSample),_BumpScale);
        }

        #endif
        //光照模式
        #ifndef _FX_LIGHT_MODE_UNLIT
    
            InputData inputData;
            InitializeInputData(input, normalTS, inputData);
            metallic *= _MaterialInfo.x;
            half3 specular = 0;
            smoothness *= _MaterialInfo.y;
            half occlusion = 1;
            half3 pbrEmission = 0;
           // return half4(inputData.bakedGI,1);
            #if defined (_FX_LIGHT_MODE_BLINN_PHONG) || defined(_FX_LIGHT_MODE_HALF_LAMBERT) 
            half4 specularGloss = _SpecularColor;
            #ifdef _FX_LIGHT_MODE_BLINN_PHONG
            half4 blinnPhong = UniversalFragmentBlinnPhong(inputData,result.rgb, specularGloss, smoothness, pbrEmission, alpha,normalTS);
            #else //_FX_LIGHT_MODE_HALF_LAMBERT
            half4 blinnPhong = UniversalFragmentHalfLambert(inputData,result.rgb, specularGloss, smoothness, pbrEmission, alpha,normalTS);
            #endif
            result = blinnPhong.rgb;
            alpha = blinnPhong.a;
            #elif _FX_LIGHT_MODE_PBR
            half4 pbr = UniversalFragmentPBR(inputData,result.rgb,  metallic,  specular, smoothness,  occlusion,  pbrEmission, alpha);
            result = pbr.rgb;
            alpha = pbr.a;
            #elif _FX_LIGHT_MODE_SIX_WAY
            BSDFData bsdfData = (BSDFData)0;
            bsdfData.absorptionRange = GetAbsorptionRange(_SixWayInfo.x);
            bsdfData.diffuseColor = albedo;
            bsdfData.normalWS = inputData.normalWS;
            bsdfData.tangentWS = input.tangentWS;//Check this
            bsdfData.rigRTBk = rigRTBkSample.xyz * INV_PI;//AccordingTo SixWayForwardPass
            bsdfData.rigLBtF = rigLBtFSample.xyz * INV_PI;//AccordingTo SixWayForwardPass
            bsdfData.bakeDiffuseLighting0 = input.bakeDiffuseLighting0;
            bsdfData.bakeDiffuseLighting1 = input.bakeDiffuseLighting1;
            bsdfData.bakeDiffuseLighting2 = input.bakeDiffuseLighting2;
            bsdfData.backBakeDiffuseLighting0 = input.backBakeDiffuseLighting0;
            bsdfData.backBakeDiffuseLighting1 = input.backBakeDiffuseLighting1;
            bsdfData.backBakeDiffuseLighting2 = input.backBakeDiffuseLighting2;
            bsdfData.emissionInput = rigLBtFSample.a;
            GetSixWayEmission(bsdfData,_SixWayEmissionRamp,_SixWayEmissionColor,CheckLocalFlags1(FLAG_BIT_PARTICLE_1_SIXWAY_RAMPMAP));//Init Emission
            bsdfData.alpha = rigRTBkSample.a * _BaseColor.a;

            ModifyBakedDiffuseLighting(bsdfData,inputData.bakedGI);

            half4 sixWay = UniversalFragmentSixWay(inputData,bsdfData);
            
            
        // half3 dir = _MainLightPosition.xyz;
        // dir = TransformToLocalFrame(dir, bsdfData);
        // return half4(dir,1);
        
            result = sixWay.rgb;
            alpha = sixWay.a;
        
            #endif
            
            #ifdef _ADDITIONAL_LIGHTS_VERTEX
            result.rgb *= input.vertexLight;
            #endif

            input.normalWSAndAnimBlend.xyz = inputData.normalWS;
        
        #else
            #ifdef _NORMALMAP
            float sgn = input.tangentWS.w;      // should be either +1 or -1
            float3 bitangent = sgn * cross(input.normalWSAndAnimBlend.xyz, input.tangentWS.xyz);
            half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWSAndAnimBlend.xyz);
            input.normalWSAndAnimBlend.xyz = TransformTangentToWorld(normalTS, tangentToWorld);
            #endif
        #endif
       
        #ifdef _MATCAP
        // URP 
        half3 normalVS = mul(input.normalWSAndAnimBlend.xyz, (float3x3)UNITY_MATRIX_I_V); // 逆转置矩阵 
        half3 positionVS = TransformWorldToView(input.positionWS);

        
        float3 r = reflect(positionVS, normalVS); 
        r = normalize(r); 
        float m = 2.828427f * sqrt(r.z + 1.0);  
        float2 matCapUV = r.xy / m + 0.5;
        half3 matCapSample = SAMPLE_TEXTURE2D(_MatCapTex,sampler_linear_clamp,matCapUV);

        matCapSample *= _MatCapColor.rgb;

        half3 matCapMutilResult = result * matCapSample;
        half3 matAddResult = result + matCapSample;
        half3 matCapResult = lerp(matAddResult,matCapMutilResult,_MatCapInfo.x);
       
        result = lerp(result,matCapResult,_MatCapColor.a);
        #endif
        
        
        
        //流光部分
        half4 emission = half4(0, 0, 0,1);
        #if defined(_EMISSION)
            #ifdef _NOISEMAP
                emission_uv += cum_noise * _Emi_Distortion_intensity;
            #endif
            // emission = tex2D_TryLinearizeWithoutAlphaFX(_EmissionMap,emission_uv);
            emission = SampleTexture2DWithWrapFlags(_EmissionMap,emission_uv,FLAG_BIT_WRAPMODE_EMISSIONMAP);
            emission.xyz *= emission.a;
            _EmissionMapColor *=  _EmissionMapColorIntensity;
            emission.xyz *= _EmissionMapColor;
        
        #endif
        
        result += emission;

        //溶解部分
        #if defined(_DISSOLVE)
            #ifdef _NOISEMAP
                dissolve_uv += cum_noise * _DissolveOffsetRotateDistort.w;

                UNITY_FLATTEN
                if(CheckLocalFlags(FLAG_BIT_PARTICLE_DISSOLVE_MASK))
                {
                    dissolve_mask_uv += cum_noise * _DissolveOffsetRotateDistort.w;
                }
            #endif
        
            half4 dissolveMapSample  = SampleTexture2DWithWrapFlags(_DissolveMap,dissolve_uv,FLAG_BIT_WRAPMODE_DISSOLVE_MAP);
            
            half dissolveValue = GetColorChannel(dissolveMapSample,FLAG_BIT_COLOR_CHANNEL_POS_0_DISSOLVE_MAP);

            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_DISSOVLE_VORONOI))
            {
                half cell;
                half noise1;
                noise1 = SimplexNoise(dissolve_noise_uv.xy,_Time.y*_DissolveVoronoi_Vec2.z);
            
                half noise2;
                Unity_Voronoi_float(dissolve_noise_uv.zw,_Time.y*_DissolveVoronoi_Vec2.w,_DissolveVoronoi_Vec.zw,noise2,cell);
                half overlayVoroni;
          
                half dissolveSample = dissolveValue;
                Unity_Blend_HardLight_half(noise1,noise2,_DissolveVoronoi_Vec2.x,overlayVoroni);
                
                Unity_Blend_HardLight_half(overlayVoroni,dissolveSample,_DissolveVoronoi_Vec2.y,dissolveValue);

                
            }

            dissolveValue = SimpleSmoothstep(_Dissolve_Vec2.x,_Dissolve_Vec2.y,dissolveValue);

            #ifdef _DISSOLVE_EDITOR_TEST      //后续Test类的关键字要找机会排除
                return half4(dissolveValue.rrr,1);
            #endif
               

            half dissolveMaskValue = 0;
            UNITY_BRANCH
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_DISSOLVE_MASK))
            {
                half4 dissolveMaskSample = SampleTexture2DWithWrapFlags(_DissolveMaskMap,dissolve_mask_uv,FLAG_BIT_WRAPMODE_DISSOLVE_MASKMAP);
                dissolveMaskValue = GetColorChannel(dissolveMaskSample,FLAG_BIT_COLOR_CHANNEL_POS_0_DISSOLVE_MASK_MAP);
                _Dissolve.z += GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_MASK_INTENSITY,0,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
                dissolveMaskValue *= _Dissolve.z;
                dissolveValue = lerp(dissolveValue,1.01,dissolveMaskValue);
            }
            half originDissolve = dissolveValue;
        
            _Dissolve.x += GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_DISSOLVE_INTENSITY,0,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
        
            dissolveValue = dissolveValue-_Dissolve.x;
            half dissolveValueBeforeSoftStep = dissolveValue;
            half softStep = _Dissolve.w;
            dissolveValue = SimpleSmoothstep(0,softStep,(dissolveValue));

            alpha  *= dissolveValue;
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_DISSOVLE_USE_RAMP))
            {
                half rampRange = 1-dissolveValueBeforeSoftStep ;
                rampRange = rampRange * _DissolveRampMap_ST.x +_DissolveRampMap_ST.z;
                
                half4 rampSample = SampleTexture2DWithWrapFlags(_DissolveRampMap,half2(rampRange,0.5),FLAG_BIT_WRAPMODE_DISSOLVE_RAMPMAP);
                result = lerp(result,rampSample.rgb*_DissolveRampColor.rgb,rampSample.a*_DissolveRampColor.a);
            }
           
            half lineMask = 1 - smoothstep(0,softStep,alpha * (dissolveValueBeforeSoftStep - _Dissolve.y));
            result = lerp(result,_DissolveLineColor.rgb,lineMask*_DissolveLineColor.a);
            
            
        
        #endif
     
        //颜色渐变
        #ifdef _COLORMAPBLEND
            half4 colorBlend = SampleTexture2DWithWrapFlags(_ColorBlendMap,colorBlendMap_uv,FLAG_BIT_WRAPMODE_COLORBLENDMAP);
            colorBlend.rgb = colorBlend.rgb * _ColorBlendColor.rgb;
            result.rgb  = lerp(result.rgb,result.rgb * colorBlend.rgb,_ColorBlendColor.a);
        #endif

        //菲涅
        
            UNITY_BRANCH
            if(CheckLocalFlags(FLAG_BIT_PARTICLE_FRESNEL_ON))
            {
                half fresnelValue = 0;
                if(!ignoreFresnel())
                {
                    half3 fresnelDir = normalize(input.fresnelViewDir+_FresnelRotation.rgb);

                    half dotNV = dot(fresnelDir,input.normalWSAndAnimBlend.xyz) ;
                    fresnelValue =  dotNV;

           
                    _FresnelUnit.x += GetCustomData(_W9ParticleCustomDataFlag0,FLAGBIT_POS_0_CUSTOMDATA_FRESNEL_OFFSET,0,input.VaryingsP_Custom1,input.VaryingsP_Custom2);;
                            
                    // half fresnelHardness =  - _FresnelUnit.w*0.5 +0.5;
                    fresnelValue = NB_Remap(fresnelValue,_FresnelUnit.x,_FresnelUnit.x + 1.01 - _FresnelUnit.w,0,1);
                    UNITY_BRANCH
                    if(!CheckLocalFlags(FLAG_BIT_PARTICLE_FRESNEL_INVERT_ON))
                    {
                        fresnelValue = 1- fresnelValue;
                    }
                    fresnelValue = pow(fresnelValue,_FresnelUnit.y);

                    
                    // fresnelValue = smoothstep(0.5-fresnelHardness,0.5+fresnelHardness,fresnelValue);
                }

                UNITY_BRANCH
                if(CheckLocalFlags(FLAG_BIT_PARTICLE_FRESNEL_COLOR_ON))
                {
                    float fresnelColorIntensity = fresnelValue*_FresnelColor.a*_FresnelUnit.z;
                    
                    result = lerp(result,_FresnelColor.rgb,fresnelColorIntensity);
                    if (!CheckLocalFlags(FLAG_BIT_PARTICLE_FRESNEL_COLOR_AFFETCT_BY_ALPHA))
                    {
                        alpha = max(alpha,fresnelColorIntensity);//颜色要不要不被主贴图Alpha影响呢？
                    }
                }

                UNITY_BRANCH
                if(CheckLocalFlags(FLAG_BIT_PARTICLE_FRESNEL_FADE_ON))
                {
                    fresnelValue *= alpha;
                    alpha = lerp(alpha,fresnelValue,_FresnelUnit.z);
                }
                
            }
        
            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_DEPTH_OUTLINE))
            {
                half depthOutlineValue = 1- SoftParticles(_DepthOutline_Vec.x, _DepthOutline_Vec.y, sceneZ,thisZ);
                depthOutlineValue *= _DepthOutline_Color.a;
                half3 originResult = result;
                //如何在一个pass里，完美的给出两个颜色的Fade。这个问题，没有想清楚。 
                result = lerp(result,_DepthOutline_Color.rgb,clamp(depthOutlineValue*3,0,1));
                result = lerp(result,originResult,clamp(alpha-depthOutlineValue,0,1));
                alpha = max(alpha,depthOutlineValue);
                
            }
        
        
        
        //遮罩部分
        #if defined(_MASKMAP_ON)

            #if defined(_NOISEMAP)
                MaskMapuv += cum_noise * _MaskDistortion_intensity; //加入扭曲效果
            #endif
            half4 maskmap1Sample = SampleTexture2DWithWrapFlags(_MaskMap, MaskMapuv,FLAG_BIT_WRAPMODE_MASKMAP);
            half mask1 = GetColorChannel(maskmap1Sample,FLAG_BIT_COLOR_CHANNEL_POS_0_MASKMAP1);
        
            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_MASK_MAP2))
            {
                half maskMap2Sample = SampleTexture2DWithWrapFlags(_MaskMap2, MaskMapuv2,FLAG_BIT_WRAPMODE_MASKMAP2).r;
                half mask2 = GetColorChannel(maskMap2Sample,FLAG_BIT_COLOR_CHANNEL_POS_0_MASKMAP2);
                mask1 *= mask2;
            }

            UNITY_BRANCH
            if(CheckLocalFlags1(FLAG_BIT_PARTICLE_1_MASK_MAP3))
            {
                half maskMap3Sample = SampleTexture2DWithWrapFlags(_MaskMap3, MaskMapuv3,FLAG_BIT_WRAPMODE_MASKMAP3).r;
                half mask3 = GetColorChannel(maskMap3Sample,FLAG_BIT_COLOR_CHANNEL_POS_0_MASKMAP3);
                mask1 *= mask3;
            }

            mask1 = lerp(1,mask1,_MaskMapVec.x);
            mask1 = saturate(mask1);
        
            // maskmap1.rgb *= maskmap1.a;//预乘
        
            alpha *= mask1;  //mask边缘
        #endif
        

        //可以看https://www.cyanilux.com/tutorials/depth/
        // float4 projectedPosition = input.positionNDC;
        // float thisZ1 = LinearEyeDepth(projectedPosition.z / projectedPosition.w, _ZBufferParams);

        
        UNITY_BRANCH
        if(CheckLocalFlags(FLAG_BIT_PARTICLE_DISTANCEFADE_ON))
        {
            half fade = DepthFactor(thisZ, _Fade.x, _Fade.y);
            alpha *= fade; 
        }
        
        
        #if defined(_SOFTPARTICLES_ON)
  
        half softAlpha = SoftParticles(SOFT_PARTICLE_NEAR_FADE, SOFT_PARTICLE_INV_FADE_DISTANCE, sceneZ,thisZ);
        alpha *= softAlpha;
        
        #endif
        
        
        

        
        UNITY_BRANCH
        if(CheckLocalFlags(FLAG_BIT_SATURABILITY_ON))
        {
            half3 resultWB = luminance(result);
            _Saturability = GetCustomData(_W9ParticleCustomDataFlag1,FLAGBIT_POS_1_CUSTOMDATA_SATURATE,_Saturability,input.VaryingsP_Custom1,input.VaryingsP_Custom2);
            result.rgb = lerp(resultWB.rgb, result.rgb, _Saturability);
        }
        

        //和粒子颜色信息运算。雨轩：乘顶点色。
        if(!CheckLocalFlags1(FLAG_BIT_PARTICLE_1_IGNORE_VERTEX_COLOR))
        {
            result *= input.color.rgb;
            alpha *= input.color.a;
        }
        // 程序额外的颜色
        result *= _ColorA.rgb;
        alpha *= _ColorA.a;
        // // alpha *= _ColorA * 0.8;


        #ifdef _DEPTH_DECAL
        alpha *= decalAlpha;
        #endif
        
    
        
        half3 beforeFogResult = result;
        result = MixFog(result,input.positionWS.w);
        result = lerp(beforeFogResult, result, _fogintensity);

        // #ifndef _SCREEN_DISTORT_MODE
        //     result.rgb = result.rgb * alpha;
        // #endif
        
        UNITY_FLATTEN
        if(CheckLocalFlags(FLAG_BIT_PARTICLE_LINEARTOGAMMA_ON))
        {
            result.rgb = LinearToGammaSpace(result.rgb);
        }
        

        alpha *= _AlphaAll;

        #if defined  (_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
            result *= alpha;
            #ifdef _ALPHAPREMULTIPLY_ON
                alpha *= _AdditiveToPreMultiplyAlphaLerp;
            #endif
        #endif
        half4 color = half4(result, alpha);

        

        #ifdef _ALPHATEST_ON
        clip(color.a - _Cutoff);

        #endif

        color = min(color,1000);
        
        return color;
    }
    
#endif
