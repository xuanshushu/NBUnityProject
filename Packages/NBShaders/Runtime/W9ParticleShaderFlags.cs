using System;
using UnityEngine;

public class W9ParticleShaderFlags: ShaderFlagsBase
{
    public const string FlagsName = "_W9ParticleShaderFlags";
    public static int FlagsId = Shader.PropertyToID(FlagsName);
    
    
    public const string Flags1Name = "_W9ParticleShaderFlags1";
    public static int Flags1Id = Shader.PropertyToID(Flags1Name);
    
    public const string WrapFlagsName = "_W9ParticleShaderWrapFlags";
    public static int WrapFlagsId = Shader.PropertyToID(WrapFlagsName);
    
    public const string foldOutFlagName = "_W9ParticleShaderGUIFoldToggle";
    public static int foldOutFlagId = Shader.PropertyToID(foldOutFlagName);
    
    public const string foldOutFlagName1 = "_W9ParticleShaderGUIFoldToggle1";
    public static int foldOutFlagId1 = Shader.PropertyToID(foldOutFlagName1);
    
    public const string foldOutFlagName2 = "_W9ParticleShaderGUIFoldToggle2";
    public static int foldOutFlagId2 = Shader.PropertyToID(foldOutFlagName2);

    public const string colorChannelFlagName = "_W9ParticleShaderColorChannelFlag";
    public static int colorChannelFlagId = Shader.PropertyToID(colorChannelFlagName);
    protected override int GetShaderFlagsId(int index = 0)
    {
        switch (index)
        {
            case 0:
                return FlagsId;
      
            case 1:
                return Flags1Id;
      
            case 2:
                return WrapFlagsId;
            
            //FoldOut必须要紧挨着，因为按照Index去拿AnimBool
            case 3:
                return foldOutFlagId;
            
            case 4:
                return foldOutFlagId1;
            
            case 5:
                return foldOutFlagId2;
            
            case 6:
                return colorChannelFlagId;
        
            default:
                return FlagsId;
        }
    }

    protected override string GetShaderFlagsName(int index = 0)
    {
        switch (index)
        {
            case 0:
                return FlagsName;
           
            case 1:
                return Flags1Name;
              
            case 2:
                return WrapFlagsName;
            
            case 3:
                return foldOutFlagName;
            
            case 4:
                return foldOutFlagName1;
            
            case 5:
                return colorChannelFlagName;
                
            default:
                return FlagsName;
        }
    }
    
    
    
    

    public W9ParticleShaderFlags(Material material = null): base(material)
    {
    }
    public const int FLAG_BIT_SATURABILITY_ON = 1 << 0;
    public const int FLAG_BIT_PARTICLE_NOISE_CHORATICABERRAT_WITH_NOISE = 1 << 1;
    public const int FLAG_BIT_PARTICLE_FRESNEL_FADE_ON = 1 << 2;
    public const int FLAG_BIT_PARTICLE_FRESNEL_COLOR_ON = 1 << 3;
    public const int FLAG_BIT_PARTICLE_USETEXCOORD2 = 1 << 4;
    public const int FLAG_BIT_PARTICLE_DISTANCEFADE_ON = 1 << 5;
    public const int FLAG_BIT_PARTICLE_CHORATICABERRAT= 1 << 6;
    public const int FLAG_BIT_PARTILCE_MASKMAPROTATIONANIMATION_ON = 1 << 7;
    public const int FLAG_BIT_PARTICLE_POLARCOORDINATES_ON = 1 << 8;
    public const int FLAG_BIT_PARTICLE_UTWIRL_ON = 1 << 9;
    public const int FLAG_BIT_PARTICLE_LINEARTOGAMMA_ON = 1 << 10;
    public const int FLAG_BIT_PARTICLE_FRESNEL_ON = 1 << 11;
    public const int FLAG_BIT_PARTICLE_NOISEMAP_NORMALIZEED_ON = 1 << 12;
    public const int FLAG_BIT_PARTICLE_FRESNEL_COLOR_AFFETCT_BY_ALPHA = 1 << 13;
    public const int FLAG_BIT_PARTICLE_UIEFFECT_ON = 1 << 14;
    public const int FLAG_BIT_PARTICLE_UNSCALETIME_ON = 1 << 15;
    public const int FLAG_BIT_PARTICLE_SCRIPTABLETIME_ON = 1 << 16;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1_ON = 1 << 17;
    public const int FLAG_BIT_PARTICLE_FRESNEL_INVERT_ON = 1 << 18;
    public const int FLAG_BIT_HUESHIFT_ON = 1 << 19;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2_ON = 1 << 20;
    public const int FLAG_BIT_PARTICLE_NORMALMAP_MASK_MODE = 1 << 21;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1Y_MAINTEXOFFSETY = 1 << 22;
 
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1W_HUESHIFT = 1 << 23;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2X_MASKMAPOFFSETX = 1 << 24;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2Y_MASKMAPOFFSETY = 1 << 25;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2Z_FRESNELOFFSET = 1 << 26;
    public const int FLAG_BIT_PARTICLE_DISSOLVE_MASK = 1 << 27;
    public const int FLAG_BIT_PARTICLE_BACKCOLOR = 1 << 28;
    public const int FLAG_BIT_PARTICLE_PC_ONLYSPECIALFUNC = 1 << 29;
    public const int FLAG_BIT_PARTICLE_VERTEX_OFFSET_ON = 1 << 30;
    public const int FLAG_BIT_PARTICLE_VERTEX_OFFSET_NORMAL_DIR= 1 << 31;
    
    public const int FLAG_BIT_PARTICLE_1_DEPTH_OUTLINE= 1 << 0;
    public const int FLAG_BIT_PARTICLE_1_PARALLAX_MAPPING= 1 << 1;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1X_DISSOLVETEXOFFSETX= 1 << 2;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1Y_DISSOLVETEXOFFSETY = 1 << 3;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1Z_NOISE_INTENSITY = 1 << 4;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA1W_SATURATE = 1 << 5;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2X_VERTEXOFFSETX = 1 << 6;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2Y_VERTEXOFFSETY = 1 << 7;
    public const int FLAG_BIT_PARTICLE_CUSTOMDATA2W_CHORATICABERRAT_INTENSITY = 1 << 8;
    public const int FLAG_BIT_PARTICLE_1_IGNORE_VERTEX_COLOR = 1 << 9;
    public const int FLAG_BIT_PARTICLE_1_DISSOVLE_VORONOI = 1 << 10;
    public const int FLAG_BIT_PARTICLE_1_DISSOVLE_USE_RAMP = 1 << 11;
    public const int FLAG_BIT_PARTICLE_1_MASK_MAP2 = 1 << 12;
    public const int FLAG_BIT_PARTICLE_1_MASK_MAP3 = 1 << 13;
    public const int FLAG_BIT_PARTICLE_1_NOISE_MASKMAP = 1 << 14;
    public const int FLAG_BIT_PARTICLE_1_ANIMATION_SHEET_HELPER = 1 << 15;
    public const int FLAG_BIT_PARTICLE_1_CUSTOMDATA2Z_VERTEXOFFSET_INTENSITY= 1 << 16;
    public const int FLAG_BIT_PARTICLE_1_UIEFFECT_SPRITE_MODE= 1 << 17;
    public const int FLAG_BIT_PARTICLE_1_USE_TEXCOORD1= 1 << 18;
    public const int FLAG_BIT_PARTICLE_1_USE_TEXCOORD2= 1 << 19;
    public const int FLAG_BIT_PARTICLE_1_CYLINDER_CORDINATE= 1 << 20;
    public const int FLAG_BIT_PARTICLE_1_UV_FROM_MESH= 1 << 21;//3D条件下，如果不是来源于Mesh，就默认来源于粒子。
    public const int FLAG_BIT_PARTICLE_1_UIEFFECT_BASEMAP_MODE= 1 << 22;//3D条件下，如果不是来源于Mesh，就默认来源于粒子。
    public const int FLAG_BIT_PARTICLE_1_IS_PARTICLE_SYSTEM= 1 << 23;//3D条件下，如果不是来源于Mesh，就默认来源于粒子。
    public const int FLAG_BIT_PARTICLE_1_MAINTEX_CONTRAST= 1 << 24;
    public const int FLAG_BIT_PARTICLE_1_VERTEXOFFSET_START_FROM_ZERO= 1 << 25;
    public const int FLAG_BIT_PARTICLE_1_VERTEXOFFSET_MASKMAP= 1 << 26;
    public const int FLAG_BIT_PARTICLE_1_MAINTEX_COLOR_REFINE= 1 << 27;
    public const int FLAG_BIT_PARTICLE_1_BUMP_TEX_UV_FOLLOW_MAINTEX= 1 << 28;
    public const int FLAG_BIT_PARTICLE_1_SIXWAY_RAMPMAP= 1 << 29;
    public const int FLAG_BIT_PARTICLE_1_MATCAP_MULTY_MODE= 1 << 30;
    
    
    public const int FLAG_BIT_WRAPMODE_BASEMAP= 1 << 0;
    public const int FLAG_BIT_WRAPMODE_MASKMAP= 1 << 1;
    public const int FLAG_BIT_WRAPMODE_MASKMAP2= 1 << 2;
    public const int FLAG_BIT_WRAPMODE_NOISEMAP= 1 << 3;
    public const int FLAG_BIT_WRAPMODE_EMISSIONMAP= 1 << 4;
    public const int FLAG_BIT_WRAPMODE_DISSOLVE_MAP= 1 << 5;
    public const int FLAG_BIT_WRAPMODE_DISSOLVE_MASKMAP= 1 << 6;
    public const int FLAG_BIT_WRAPMODE_DISSOLVE_RAMPMAP= 1 << 7;
    public const int FLAG_BIT_WRAPMODE_COLORBLENDMAP= 1 << 8;
    public const int FLAG_BIT_WRAPMODE_VERTEXOFFSETMAP= 1 << 9;
    public const int FLAG_BIT_WRAPMODE_PARALLAXMAPPINGMAP = 1 << 10;
    public const int FLAG_BIT_WRAPMODE_MASKMAP3= 1 << 11;
    public const int FLAG_BIT_WRAPMODE_NOISE_MASKMAP= 1 << 12;
    public const int FLAG_BIT_WRAPMODE_VERTEXOFFSET_MASKMAP= 1 << 13;
    public const int FLAG_BIT_WRAPMODE_BUMPTEX= 1 << 14;

    public const int foldOutBitMeshOption = 1 << 0;
    public const int foldOutBitMainTexOption = 1 << 1;
    public const int foldOutBitBaseOption = 1 << 2;
    public const int foldOutBitFeatureOption = 1 << 3;
    public const int foldOutBitBaseMap = 1 << 4;
    public const int foldOutBitMask = 1 << 5;
    public const int foldOutBitMaskMap = 1 << 6;
    public const int foldOutBitMask2 = 1 << 7;
    public const int foldOutBitMask3 = 1 << 8;
    public const int foldOutBitTwril = 1 << 9;
    public const int foldOutBitPolar = 1 << 10;
    public const int foldOutBitHueShift = 1 << 11;
    public const int foldOutBitSaturability = 1 << 12;
    public const int foldOutBitDistanceFade = 1 << 13;
    public const int foldOutBitSoftParticles = 1 << 14;
    public const int foldOutBitMaskRotate = 1 << 15;
    public const int foldOutBitNoise = 1 << 16;
    public const int foldOutBitNoiseMap = 1 << 17;
    public const int foldOutBitNoiseMaskToggle = 1 << 18;
    public const int foldOutBitEmission= 1 << 19;
    public const int foldOutBitDistortionChoraticaberrat= 1 << 20;
    public const int foldOutDissolve= 1 << 21;
    public const int foldOutDissolveMap= 1 << 22;
    public const int foldOutDissolveVoronoi= 1 << 23;
    public const int foldOutDissolveRampMap= 1 << 24;
    public const int foldOutDissolveMask= 1 << 25;
    public const int foldOutColorBlend= 1 << 26;
    public const int foldOutFresnel= 1 << 27;
    public const int foldOutDepthOutline= 1 << 28;
    public const int foldOutVertexOffset= 1 << 29;
    public const int foldOutParallexMapping= 1 << 30;
    // public const int foldOutBit1Portal= 1 << 31;
    
    


    public const int foldOutBit1UVModeMainTex = 1 << 0;
    public const int foldOutBit1UVModeMaskMap= 1 << 1;
    public const int foldOutBit1UVModeMaskMap2= 1 << 2;
    public const int foldOutBit1UVModeMaskMap3= 1 << 3;
    public const int foldOutBit1UVModeNoiseMap = 1 << 4;
    public const int foldOutBit1UVModeNoiseMaskMap = 1 << 5;
    public const int foldOutBit1UVModeEmissionMap = 1 << 6;
    public const int foldOutBit1UVModeDissolveMap = 1 << 7;
    public const int foldOutBit1UVModeDissolveMaskMap = 1 << 8;
    public const int foldOutBit1UVModeColorBlendMap = 1 << 9;
    public const int foldOutBit1UVModeVertexOffsetMap = 1 << 10;
    public const int foldOutBit1UVModeVertexOffsetMaskMap = 1 << 11;
    public const int foldOutBit1UVModeBumpTex = 1 << 12;
    
    //留一些位置给以后可能会增加的贴图。
    public const int foldOutBit1Portal= 1 << 20;
    public const int foldOutBit1ZOffset= 1 << 21;
    public const int foldOutBit1CustomStencilTest= 1 << 22;
    public const int foldOutBit1TaOption = 1 << 23;
    public const int foldOutBit1MianTexContrast= 1 << 24;
    public const int foldOutBit1VertexOffsetMask= 1 << 25;
    public const int foldOutBit1MainTexColorRefine= 1 << 26;
    public const int foldOutBit1LightOption= 1 << 27;
    public const int foldOutBit1ShaderKeyword= 1 << 28;
    public const int foldOutBit1BumpTex= 1 << 29;
    
    public const int foldOutBit2BumpTexToggle= 1 << 0;
    public const int foldOutBit2MatCapToggle= 1 << 1;


    #region CustomDataCodes

    public const string CustomDataFlag0Name = "_W9ParticleCustomDataFlag0";
    public const string CustomDataFlag1Name = "_W9ParticleCustomDataFlag1";
    public const string CustomDataFlag2Name = "_W9ParticleCustomDataFlag2";
    public const string CustomDataFlag3Name = "_W9ParticleCustomDataFlag3";
    public static int CustomDataFlag0Id = Shader.PropertyToID(CustomDataFlag0Name);
    public static int CustomDataFlag1Id = Shader.PropertyToID(CustomDataFlag1Name);
    public static int CustomDataFlag2Id = Shader.PropertyToID(CustomDataFlag2Name);
    public static int CustomDataFlag3Id = Shader.PropertyToID(CustomDataFlag3Name);

    public enum CutomDataComponent
    {
        Off,
        CustomData1X,
        CustomData1Y,
        CustomData1Z,
        CustomData1W,
        CustomData2X,
        CustomData2Y,
        CustomData2Z,
        CustomData2W,
        UnKnownOrMixed = -1
    }

    public const int FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_X = 0 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_MAINTEX_OFFSET_Y = 1 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_DISSOLVE_INTENSITY = 2 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_HUESHIFT = 3 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_X = 4 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_MASK_OFFSET_Y = 5 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_FRESNEL_OFFSET = 6 * 4;
    public const int FLAGBIT_POS_0_CUSTOMDATA_CHORATICABERRAT_INTENSITY = 7 * 4;
    
    public const int FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_X = 0 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_OFFSET_Y = 1 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_NOISE_INTENSITY = 2 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_SATURATE = 3 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_X = 4 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_VERTEX_OFFSET_Y = 5 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_VERTEXOFFSET_INTENSITY = 6 * 4;
    public const int FLAGBIT_POS_1_CUSTOMDATA_DISSOLVE_MASK_INTENSITY = 7 * 4;
    
    public const int FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE1_OFFSET_X = 0*4;
    public const int FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE1_OFFSET_Y = 1*4;
    public const int FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE2_OFFSET_X = 2*4;
    public const int FLAGBIT_POS_2_CUSTOMDATA_DISSOLVE_NOISE2_OFFSET_Y = 3*4;
    public const int FLAGBIT_POS_2_CUSTOMDATA_NOISE_DIRECTION_X= 4*4;
    public const int FLAGBIT_POS_2_CUSTOMDATA_NOISE_DIRECTION_Y= 5*4;
    public const int FLAGBIT_POS_2_CUSTOMDATA_MAINTEX_CONTRAST= 6*4;
    //---->这里还有一个坑可以用哦
    
    public const int FLAGBIT_POS_3_CUSTOMDATA_VERTEX_OFFSET_MASK_X= 0*4;
    public const int FLAGBIT_POS_3_CUSTOMDATA_VERTEX_OFFSET_MASK_Y= 1*4;
    

    public const int isCustomDataBit = 1 << 3;
    public const int Data12Bit = 1 << 2;//true CustomData1 / false CustomData2
    public const int DataXYorZWBit = 1 << 1;//true xy / false zw
    public const int DataXZorYWBit = 1 << 0;//true xz / false yw
    public const int CustomData1XBit = isCustomDataBit | Data12Bit | DataXYorZWBit | DataXZorYWBit;
    public const int CustomData1YBit = isCustomDataBit | Data12Bit | DataXYorZWBit ;
    public const int CustomData1ZBit = isCustomDataBit | Data12Bit                 | DataXZorYWBit;
    public const int CustomData1WBit = isCustomDataBit | Data12Bit;
    public const int CustomData2XBit = isCustomDataBit             | DataXYorZWBit | DataXZorYWBit;
    public const int CustomData2YBit = isCustomDataBit             | DataXYorZWBit;
    public const int CustomData2ZBit = isCustomDataBit                             | DataXZorYWBit;
    public const int CustomData2WBit = isCustomDataBit;

    private int GetCustomDataFlagID(int dataIndex)
    {
        switch (dataIndex)
        {
            case 0 :
                return CustomDataFlag0Id;
               
            case 1 :
                return CustomDataFlag1Id;
            
            case 2 :
                return CustomDataFlag2Id;
            case 3 :
                return CustomDataFlag3Id;
               
        }

        return 0;
    }

    public string GetCustomDataFlagPropertyName(int dataIndex)
    {
        switch (dataIndex)
        {
            case 0 :
                return CustomDataFlag0Name;
               
            case 1 :
                return CustomDataFlag1Name;
            
            case 2:
                return CustomDataFlag2Name;
            
            case 3:
                return CustomDataFlag3Name;
                
        }

        return null;
    }

    public CutomDataComponent GetCustomDataFlag(int dataBitPos, int dataIndex)
    {
        int bit = material.GetInteger(GetCustomDataFlagID(dataIndex));

        bit = bit >> dataBitPos;
        bit &= 15; // binary 1111

        if ((bit & isCustomDataBit) == 0)
        {
            return CutomDataComponent.Off;
        }
        else if (bit == CustomData1XBit)
        {
            return CutomDataComponent.CustomData1X;
        }
        else if (bit == CustomData1YBit)
        {
            return CutomDataComponent.CustomData1Y;
        }
        else if (bit == CustomData1ZBit)
        {
            return CutomDataComponent.CustomData1Z;
        }
        else if (bit == CustomData1WBit)
        {
            return CutomDataComponent.CustomData1W;
        }
        else if (bit == CustomData2XBit)
        {
            return CutomDataComponent.CustomData2X;
        }
        else if (bit == CustomData2YBit)
        {
            return CutomDataComponent.CustomData2Y;
        }
        else if (bit == CustomData2ZBit)
        {
            return CutomDataComponent.CustomData2Z;
        }
        else if (bit == CustomData2WBit)
        {
            return CutomDataComponent.CustomData2W;
        }
        else
        {
        }
        Debug.Log("不可能存在的情况");
        return CutomDataComponent.Off;

    }

    public void SetCustomDataFlag(CutomDataComponent cutomDataComponent,int dataBitPos, int dataIndex)
    {
        int bit = 0;
        switch (cutomDataComponent)
        {
            case CutomDataComponent.Off:
                bit = 0;
                break;
            case CutomDataComponent.CustomData1X:
                bit = CustomData1XBit;
                break;
            case CutomDataComponent.CustomData1Y:
                bit = CustomData1YBit;
                break;
            case CutomDataComponent.CustomData1Z:
                bit = CustomData1ZBit;
                break;
            case CutomDataComponent.CustomData1W:
                bit = CustomData1WBit;
                break;
            case CutomDataComponent.CustomData2X:
                bit = CustomData2XBit;
                break;
            case CutomDataComponent.CustomData2Y:
                bit = CustomData2YBit;
                break;
            case CutomDataComponent.CustomData2Z:
                bit = CustomData2ZBit;
                break;
            case CutomDataComponent.CustomData2W:
                bit = CustomData2WBit;
                break;
        }
        bit = bit << dataBitPos;
        int clearBit = ~(15 << dataBitPos);//~ (1111 << dataBitPos)

        int materialBit = material.GetInteger(GetCustomDataFlagID(dataIndex));
        materialBit = materialBit & clearBit;
        materialBit = materialBit | bit;
        material.SetInteger(GetCustomDataFlagID(dataIndex),materialBit);
    }

    public bool IsCustomDataOn()
    {
        int prop0Flag = material.GetInteger(CustomDataFlag0Id);
        int prop1Flag = material.GetInteger(CustomDataFlag1Id);
        int prop2Flag = material.GetInteger(CustomDataFlag2Id);
        int prop3Flag = material.GetInteger(CustomDataFlag3Id);
        uint dataOnBit = 0b_1000_1000_1000_1000_1000_1000_1000_1000;//10001000100010001000100010001000;

        return ((prop0Flag & dataOnBit) >0) || ((prop1Flag & dataOnBit)>0) || ((prop2Flag & dataOnBit)>0)||((prop3Flag & dataOnBit)>0);
    }

    bool CheckCustomData(int dataIndex, int flagIndex)
    {
        int flagID = 0;
        switch (flagIndex)
        {
            case 0:
                flagID = CustomDataFlag0Id;
                break;
            case 1:
                flagID = CustomDataFlag1Id;
                break;
            case 2:
                flagID = CustomDataFlag2Id;
                break;
            case 3:
                flagID = CustomDataFlag3Id;
                break;
        }

        int flag = material.GetInteger(flagID);
        int i = 0;
        while (i<8)
        {
            int bit = flag >> (4 * i);
            if (dataIndex == 1)
            {
                if ((bit & 0b_1000) > 0 && (bit & 0b_0100) > 0)
                {
                    return true;
                }
            }
            else if(dataIndex == 2)
            {
                if ((bit & 0b_1000) > 0 && (bit & 0b_0100) == 0)
                {
                    return true;
                }
            }

            i += 1;
        }
        return false;
    }

    public bool IsCustomData1On()
    {
        if (!IsCustomDataOn())
        {
            return false;
        }

        bool isCustomData1On = false;
        isCustomData1On |= CheckCustomData(1, 0);
        isCustomData1On |= CheckCustomData(1, 1);
        isCustomData1On |= CheckCustomData(1, 2);
        isCustomData1On |= CheckCustomData(1, 3);
        return isCustomData1On;
    }
    
    public bool IsCustomData2On()
    {
        if (!IsCustomDataOn())
        {
            return false;
        }
        bool isCustomData1On = false;
        isCustomData1On |= CheckCustomData(2, 0);
        isCustomData1On |= CheckCustomData(2, 1);
        isCustomData1On |= CheckCustomData(2, 2);
        isCustomData1On |= CheckCustomData(2, 3);
        return isCustomData1On;
   
    }
    #endregion

    public const string UVModeFlag0Name = "_UVModeFlag0";
    public static int UVModeFlag0PropID = Shader.PropertyToID(UVModeFlag0Name);

    public string GetUVModePropName(int dataIndex)
    {
        switch (dataIndex)
        {
            case 0:
                return UVModeFlag0Name;
        }

        return null;
    }

    public enum UVMode
    {
        DefaultUVChannel,   //0 0b_00
        SpecialUVChannel,   //1 0b_01
        PolarOrTwirl,       //2 0b_10
        Cylinder, //3 0b_11
        UnknownOrMixed = -1
    }
    
    public const int FLAG_BIT_UVMODE_POS_0_MAINTEX = 0 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_MASKMAP = 1 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_MASKMAP_2 = 2 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_MASKMAP_3 = 3 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_NOISE_MAP = 4 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_NOISE_MASK_MAP = 5 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_EMISSION_MAP = 6 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_DISSOLVE_MAP = 7 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_DISSOLVE_MASK_MAP = 8 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_COLOR_BLEND_MAP = 9 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_VERTEX_OFFSET_MAP = 10 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_VERTEX_OFFSET_MASKMAP = 11 * 2;
    public const int FLAG_BIT_UVMODE_POS_0_BUMPMAP = 12 * 2;

    public int GetUVModeFlagPropID(int flagIndex)
    {
        switch (flagIndex)
        {
            case 0:
                return UVModeFlag0PropID;
        }

        return 0;
    }

    public void SetUVMode(UVMode mode, int uvModePos, int flagIndex = 0)
    {
        int uvModeFlagPropId = GetUVModeFlagPropID(flagIndex);
        int uvModeFlag = material.GetInteger(uvModeFlagPropId);


        int clearFlag = 0b_11 << uvModePos;
        clearFlag = ~ clearFlag;

        uvModeFlag &= clearFlag;
        int modeBit = (int)mode << uvModePos;
        uvModeFlag |= modeBit;
        
        material.SetInteger(uvModeFlagPropId,uvModeFlag);
    }

    public UVMode GetUVMode(int uvModePos, int flagIndex = 0)
    {
        int uvModeFlagPropId = GetUVModeFlagPropID(flagIndex);
        int uvModeFlag = material.GetInteger(uvModeFlagPropId);
        uvModeFlag = uvModeFlag >> uvModePos;
        uvModeFlag &= 0b_11;
        return (UVMode)uvModeFlag;
    }

    public bool CheckIsUVModeOn(UVMode mode)
    {
        uint uvModeFlag0 = (uint)material.GetInteger(UVModeFlag0PropID);

        uint uvModeBit = (uint)mode;

        bool isUvMode = false;
        for (int i = 0; i < 16; i++)
        {
            uint checkBit = uvModeFlag0 >> (i * 2);
            checkBit = checkBit & 0b_11;
            if (checkBit == uvModeBit)
            {
                isUvMode = true;
                break;
            }
        }
        return isUvMode;
    }
    
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_MAINTEX_ALPHA = 0 * 2;
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_MASKMAP1 = 1 * 2;
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_MASKMAP2 = 2 * 2;
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_MASKMAP3 = 3 * 2;
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_NOISE_MASK = 4 * 2;
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_DISSOLVE_MAP = 5 * 2;
    public const int FLAG_BIT_COLOR_CHANNEL_POS_0_DISSOLVE_MASK_MAP = 6 * 2;
    

    public enum ColorChannel
    {
        X,
        Y,
        Z,
        W,
        UnKnownOrMixedValue
    }

    public void SetColorChanel(ColorChannel channel, int colorChannelFlagPos)
    {
        int colorChannelFlag = material.GetInteger(colorChannelFlagId);
        
        int clearFlag = 0b_11 << colorChannelFlagPos;
        clearFlag = ~ clearFlag;
        
        colorChannelFlag &= clearFlag;
        int channelBit = (int)channel << colorChannelFlagPos;
        colorChannelFlag |= channelBit;
        
        material.SetInteger(colorChannelFlagId,colorChannelFlag);
    }

    public ColorChannel GetColorChanel(int colorChannelFlagPos)
    {
        int colorChannelFlag = material.GetInteger(colorChannelFlagId);
        colorChannelFlag = colorChannelFlag >> colorChannelFlagPos;
        colorChannelFlag &= 0b_11;
        return (ColorChannel)colorChannelFlag;
    }

}
