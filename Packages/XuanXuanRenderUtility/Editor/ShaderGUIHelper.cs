using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AnimatedValues;
// using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Reflection;

namespace UnityEditor
{
    /*
        多选材质面板的原则记录：
        0、多选后 同时只会有1个matEditor。每个属性只会有一个MaterialProperty。但会有多个Mat以及Mat属性值。会有多个ShaderFlag以及ShaderFlag值。
        1、当多选后，属性处于Mixed状态时。对propety.Value进行设置是不合法的。只有非Mixed状态才会完全设置。包括这样对property.Value值进行判断也是非法的。如果在不确定Mixed的情况下，应该通过遍历Mats[i].Get或Set进行操作。Flag亦同理。
        2、drawBlock应该都传Property，让DrawBlock内能知道Mixed状态。然后在Block内需要判断状态是否明确才进行相关的操作。
        3、drawBlock一般放需要绘制的行为。对数值修改的行为统一放到OnValueChangeCheckBlock。
        4、所有在GUI上存储，并需要后续判断的状态，需要可以标记mixed状态。如果是枚举，需要有一个指定为-1的UnKnownOrMixed枚举。如果是bool，应该改为int值，并规定-1为UnKnownOrMixed状态。
        5、对于属性值的更改设定，都应该在OnValueChange的情况下进行。
        6、对于Toggle作用的方法，均应该有EditorOnly的Property进行标记Toggle储存。Keywords或者Flag应该是设定结果，而非Toggle标记。
        7、DrawVectorComponent这种多个GUI公用一个property属性的情况，需要有手动的提供各个Component是否是Mixed的方案。
    */
    public class ShaderGUIHelper
    {
        public ShaderGUIHelper()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }
        private void OnUndoRedoPerformed()//定义一个Undo回调
        {
            DrawGradientUndoPerformed();
        }
        public class ShaderPropertyPack
        {
            public MaterialProperty property;
            public string name;
        }

        private List<Material> mats;
        private MaterialEditor matEditor;
        public List<ShaderPropertyPack> ShaderPropertyPacks = new List<ShaderPropertyPack>();
        public ShaderFlagsBase[] shaderFlags = null;


        public void Init(MaterialEditor materialEditor, MaterialProperty[] properties,
            ShaderFlagsBase[] shaderFlags_in = null, List<Material> mats_in = null)
        {
            shaderFlags = shaderFlags_in;
            ShaderPropertyPacks.Clear();

            mats = mats_in;
            Shader shader = mats[0].shader;
            matEditor = materialEditor;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                ShaderPropertyPack pack = new ShaderPropertyPack();
                pack.name = ShaderUtil.GetPropertyName(shader, i);
                for (int index = 0; index < properties.Length; ++index)
                {
                    if (properties[index] != null && properties[index].name == pack.name)
                    {
                        pack.property = properties[index];
                        break;
                    }
                    else
                    {
                        if (index == properties.Length - 1)
                        {
                            Debug.LogError(pack.name + "找不到Properties");
                        }
                    }
                }

                ShaderPropertyPacks.Add(pack);
            }
        }
        
        
        public void DrawTextureFoldOut(int foldOutFlagBit,int foldOutFlagIndex,int animBoolIndex,string label, string texturePropertyName,
            string colorPropertyName = null, bool drawScaleOffset = true, bool drawWrapMode = false,
            int flagBitsName = 0, int flagIndex = 2, Action<MaterialProperty> drawBlock = null)
        {
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, animBoolIndex, foldOutFlagIndex);
            animBool.target = foldOutState;
            DrawTextureFoldOut(ref animBool, label, texturePropertyName, colorPropertyName, drawScaleOffset,
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

        public void DrawToggleFoldOut(int foldOutFlagBit,int foldOutFlagIndex, int animBoolIndex,string label, string propertyName,
            int flagBitsName = 0,
            int flagIndex = 0, string shaderKeyword = null, string shaderPassName = null, bool isIndentBlock = true, FontStyle fontStyle = FontStyle.Normal,
            Action<MaterialProperty> drawBlock = null,Action<MaterialProperty> drawEndChangeCheck = null)
        {
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, animBoolIndex, foldOutFlagIndex); //foldOut里的第一组。
            animBool.target = foldOutState;
            DrawToggleFoldOut(ref animBool, label, propertyName, flagBitsName, flagIndex, shaderKeyword,
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

        public void DrawBigBlockFoldOut(int foldOutFlagBit,int foldOutFlagIndex,int animBoolIndex ,string label, Action drawBlock)
        {
            EditorGUILayout.Space();
            DrawFoldOut(foldOutFlagBit,foldOutFlagIndex,animBoolIndex, label,FontStyle.Bold, drawBlock);
            GuiLine();
        }

        private AnimBool[] animBoolArr = new AnimBool[96];//先假定有3组。和存好的bit一一对应。
        
        public AnimBool GetAnimBool(int flagBit, int AnimBoolIndex,int flagIndex)
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

        public void DrawBigBlock(String label, Action drawBlock)
        {
            EditorGUILayout.Space();
            var origFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField(label);
            EditorStyles.label.fontStyle = origFontStyle;
            drawBlock();
            GuiLine();
        }

        public void DrawFoldOut(int foldOutFlagBit,int foldOutFlagIndex,int animBoolIndex, String label,FontStyle fontStyle = FontStyle.Normal, Action drawBlock = null)
        {
            bool foldOutState = shaderFlags[0].CheckFlagBits(foldOutFlagBit, index: foldOutFlagIndex);
            AnimBool animBool = GetAnimBool(foldOutFlagBit, animBoolIndex, foldOutFlagIndex);
            animBool.target = foldOutState;
            
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var foldoutRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            var labelRect = new Rect(rect.x + 2f, rect.y, rect.width - 2f, rect.height);
            
            animBool.target = EditorGUI.Foldout(foldoutRect, animBool.target, string.Empty, true);
            
            FontStyle origFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = fontStyle;
            EditorGUI.LabelField(labelRect, label);
            EditorStyles.label.fontStyle = origFontStyle;
            EditorGUILayout.EndHorizontal();
            
            float faded = animBool.faded;
            if (faded == 0) faded = 0.00001f;
            EditorGUILayout.BeginFadeGroup(faded);
            EditorGUI.indentLevel++;
            drawBlock?.Invoke();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFadeGroup();
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

        public void DrawBigBlockWithToggle(String label, string propertyName, int flagBitsName = 0, int flagIndex = 0,
            string shaderKeyword = null, string shaderPassName = null, string shaderPassName2 = null,
            Action<MaterialProperty> drawBlock = null)
        {

            DrawToggle(label, propertyName, flagBitsName, flagIndex, shaderKeyword, shaderPassName, shaderPassName2,
                isIndentBlock: true, FontStyle.Bold, drawBlock: drawBlock);
            GuiLine();

        }

        public void DrawToggleFoldOut(ref AnimBool foldOutAnimBool, String label, string propertyName,
            int flagBitsName = 0,
            int flagIndex = 0, string shaderKeyword = null, string shaderPassName = null, bool isIndentBlock = true,
            FontStyle fontStyle = FontStyle.Normal,
            Action<MaterialProperty> drawBlock = null, Action<MaterialProperty> drawEndChangeCheck = null)
        {
            MaterialProperty toggleProp = GetProperty(propertyName);
            
            if (fontStyle == FontStyle.Bold)
            {
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var toggleRect = GetRectAfterLabelWidth(rect);

            var foldoutRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            foldoutRect.width = toggleRect.x - foldoutRect.x;
            var labelRect = new Rect(rect.x , rect.y, rect.width , rect.height);

            // bool isToggle = false;
            // 必须先画Toggle，不然按钮会被FoldOut和Label覆盖。
            DrawToggle("", propertyName, flagBitsName, flagIndex, shaderKeyword, shaderPassName, isIndentBlock: false,
                fontStyle: FontStyle.Normal, rect: toggleRect, 
                // drawBlock: toggle => 
                // {
                //     if (drawBlock != null)
                //     {
                //         drawBlock(toggle);
                //     }
                // }, //这里面的内容应该是在FoldOut里面触发。
                drawEndChangeCheck: drawEndChangeCheck);

            // EditorGUI.DrawRect(foldoutRect,Color.red);
            foldOutAnimBool.target = EditorGUI.Foldout(foldoutRect, foldOutAnimBool.target, string.Empty, true);
            var origFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = fontStyle;
            // EditorGUI.DrawRect(labelRect,Color.blue);
            EditorGUI.LabelField(labelRect, label);
            EditorStyles.label.fontStyle = origFontStyle;
            EditorGUILayout.EndHorizontal();
            if (isIndentBlock) EditorGUI.indentLevel++;
            float faded = foldOutAnimBool.faded;
            if (faded == 0) faded = 0.00001f; //用于欺骗FadeGroup，不要让他真的关闭了。这样会藏不住相关的GUI。我们的目的是，GUI藏住，但是逻辑还是在跑。drawBlock要执行。
            EditorGUILayout.BeginFadeGroup(faded);
            {
                bool isDisabledGroup = toggleProp.hasMixedValue || toggleProp.floatValue < 0.5f;
                EditorGUI.BeginDisabledGroup(isDisabledGroup);
                drawBlock?.Invoke(toggleProp);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFadeGroup();
            if (isIndentBlock) EditorGUI.indentLevel--;
        }

        public void DrawToggle(String label, string propertyName, int flagBitsName = 0, int flagIndex = 0,
            string shaderKeyword = null, string shaderPassName = null, string shaderPassName2 = null,
            bool isIndentBlock = true, FontStyle fontStyle = FontStyle.Normal, Rect rect = new Rect(),
            Action<MaterialProperty> drawBlock = null, Action<MaterialProperty> drawEndChangeCheck = null)
        {
            if (GetProperty(propertyName) == null)
                return;

            if (fontStyle == FontStyle.Bold)
            {
                EditorGUILayout.Space();
            }

            MaterialProperty toggleProperty = GetProperty(propertyName);
            EditorGUI.showMixedValue = toggleProperty.hasMixedValue;

            EditorGUI.BeginChangeCheck();
            bool isToggle = toggleProperty.floatValue > 0.5f ? true : false;
            var origFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = fontStyle;
            if (rect.width > 0) //给FoldOut功能使用。
            {
                isToggle = EditorGUI.Toggle(rect, isToggle, EditorStyles.toggle);
            }
            else
            {
                isToggle = EditorGUILayout.Toggle(label, isToggle);
            }

            EditorStyles.label.fontStyle = origFontStyle;
            if (EditorGUI.EndChangeCheck())
            {
                toggleProperty.floatValue = isToggle ? 1.0f : 0.0f;
                if (!toggleProperty.hasMixedValue)
                {
                    for (int i = 0; i < mats.Count; i++)
                    {
                        if (isToggle)
                        {
                            mats[i].SetFloat(propertyName,1);
                            
                            if (flagBitsName != 0 && shaderFlags[i] != null)
                            {
                                shaderFlags[i].SetFlagBits(flagBitsName, index: flagIndex);
                            }

                            if (shaderKeyword != null)
                            {
                                mats[i].EnableKeyword(shaderKeyword);
                            }

                            if (shaderPassName != null)
                            {
                                mats[i].SetShaderPassEnabled(shaderPassName, true);
                            }

                            if (shaderPassName2 != null)
                            {
                                mats[i].SetShaderPassEnabled(shaderPassName2, true);
                            }
                        }
                        else
                        {
                            
                            mats[i].SetFloat(propertyName,0);
                            

                            if (flagBitsName != 0 && shaderFlags[i] != null)
                            {
                                shaderFlags[i].ClearFlagBits(flagBitsName, index: flagIndex);
                            }

                            if (shaderKeyword != null)
                            {
                                mats[i].DisableKeyword(shaderKeyword);
                            }

                            if (shaderPassName != null)
                            {
                                mats[i].SetShaderPassEnabled(shaderPassName, false);
                            }

                            if (shaderPassName2 != null)
                            {
                                mats[i].SetShaderPassEnabled(shaderPassName2, false);
                            }
                        }
                    }
                }
                drawEndChangeCheck?.Invoke(toggleProperty);
            }

            if (isIndentBlock) EditorGUI.indentLevel++;
            drawBlock?.Invoke(toggleProperty);
            if (isIndentBlock) EditorGUI.indentLevel--;

            EditorGUI.showMixedValue = false;
        }


        public void DrawSlider(string label, string propertyName, float minValue, float maxValue,
            Action<float> drawBlock = null)
        {
            EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
            float f = GetProperty(propertyName).floatValue;
            EditorGUI.BeginChangeCheck();
            f = EditorGUILayout.Slider(label, f, minValue, maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                GetProperty(propertyName).floatValue = f;
            }

            drawBlock?.Invoke(f);

            EditorGUI.showMixedValue = false;
        }


        public void DrawFloat(string label, string propertyName, bool isReciprocal = false,
            Action<MaterialProperty> drawBlock = null)
        {
            EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
            MaterialProperty floatProperty = GetProperty(propertyName);
            float f = floatProperty.floatValue;
            if (isReciprocal) f = 1 / f;
            EditorGUI.BeginChangeCheck();
            f = EditorGUILayout.FloatField(label, f);
            if (isReciprocal) f = 1 / f;
            if (EditorGUI.EndChangeCheck())
            {
                floatProperty.floatValue = f;
            }

            drawBlock?.Invoke(floatProperty);
            EditorGUI.showMixedValue = false;
        }

        Vector2 GetVec2InVec4(Vector4 vec4,bool isFirstLine)
        {
            if (isFirstLine)
            {
                return new Vector2(vec4.x, vec4.y);
            }
            else
            {
                return new Vector2(vec4.z, vec4.w);
            }
        }
        
        Vector4 SetVec2InVec4(Vector4 vec4,bool isFirstLine,Vector2 vec2Value)
        {
            if (isFirstLine)
            {
                vec4.x = vec2Value.x;
                vec4.y = vec2Value.y;
                return vec4;
            }
            else
            {
                 vec4.z = vec2Value.x;
                 vec4.w = vec2Value.y;
                 return vec4;
            }
        }
        

        bool Vector4In2LineHasMixedValue(string propertyName, bool isFirstLine)
        {
            MaterialProperty matProp = GetProperty(propertyName);
            if (matProp.hasMixedValue)
            {
                Vector2 val = Vector2.zero;
                for (int i = 0; i < mats.Count; i++)
                {
                    Vector2 matValue =  GetVec2InVec4(mats[i].GetVector(propertyName), isFirstLine);
                    if (i == 0)
                    {
                        val = matValue;
                    }
                    else
                    {
                        if (!val.Equals(matValue) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void DrawVector4In2Line(string propertyName, string label , bool isFirstLine,
            Action drawBlock = null)
        {
            EditorGUI.showMixedValue = Vector4In2LineHasMixedValue(propertyName, true);
            MaterialProperty property = GetProperty(propertyName);

            EditorGUI.BeginChangeCheck();

                Vector2 vec2 = GetVec2InVec4(property.vectorValue, isFirstLine);
                vec2 = EditorGUILayout.Vector2Field(label, vec2);
            

            if (EditorGUI.EndChangeCheck())
            {
                int shaderID = Shader.PropertyToID(propertyName);
                for (int i = 0; i < mats.Count; i++)
                {
                    Vector4 vec4 = mats[i].GetVector(shaderID);
                    vec4 = SetVec2InVec4(vec4, isFirstLine, vec2);
                    if (mats.Count == 1)
                    {
                        GetProperty(propertyName).vectorValue = vec4;//为了K动画，多选不能K动画。   
                    }
                    else
                    {
                        mats[i].SetVector(shaderID, vec4);
                    }
                }
            }

            drawBlock?.Invoke();
            EditorGUI.showMixedValue = false;

        }

        float GetCompInVec4(Vector4 vec, string comp)
        {
            float f = 0;
            switch (comp)
            {
                case "x":
                    f = vec.x;
                    break;
                case "y":
                    f = vec.y;
                    break;
                case "z":
                    f = vec.z;
                    break;
                case "w":
                    f = vec.w;
                    break;
            }

            return f;
        }

        Vector4 SetCompInVec4(Vector4 vec, string comp, float value)
        {
            switch (comp)
            {
                case "x":
                    vec.x = value;
                    break;
                case "y":
                    vec.y = value;
                    break;
                case "z":
                    vec.z = value;
                    break;
                case "w":
                    vec.w = value;
                    break;
            }

            return vec;
        }

        bool Vector4ComponentHasMixedValue(string propertyName, string channel)
        {
            MaterialProperty property = GetProperty(propertyName);
            if (property.hasMixedValue)
            {
                float val = 0;
                for (int i = 0; i < mats.Count; i++)
                {
                    if (i == 0)
                    {
                        val = GetCompInVec4(mats[i].GetVector(propertyName),channel) ;
                    }
                    else
                    {
                        if (!val.Equals(GetCompInVec4(mats[i].GetVector(propertyName),channel)) )
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
            
        }
        public void DrawVector4Component(string label, string propertyName, string channel, bool isSlider,
            float minValue = 0, float maxValue = 1, float powerSlider = 1, float multiplier = 1,
            bool isReciprocal = false, Action<float,bool> drawBlock = null, Action<float,bool> drawEndChangeCheckBlock = null)
        {
            bool hasMixedValue = Vector4ComponentHasMixedValue(propertyName, channel);
            EditorGUI.showMixedValue = hasMixedValue;
            Vector4 vec = GetProperty(propertyName).vectorValue;
            float f = GetCompInVec4(vec, channel);
            f *= multiplier;
            if (isReciprocal) f = 1 / f;
            EditorGUI.BeginChangeCheck();
            if (isSlider)
            {
                if (powerSlider > 1)
                {
                    f = PowerSlider(EditorGUILayout.GetControlRect(new GUILayoutOption[] { GUILayout.Height(18) }),
                        new GUIContent(label), f, minValue, maxValue, powerSlider);
                }
                else
                {
                    f = EditorGUILayout.Slider(label, f, minValue, maxValue);
                }
            }
            else
            {
                f = EditorGUILayout.FloatField(label, f);
            }

            if (isReciprocal) f = 1 / f;
            f /= multiplier;

            if (EditorGUI.EndChangeCheck())
            {
                int id= Shader.PropertyToID(propertyName);
                for (int i = 0; i < mats.Count; i++)
                {
                    Vector4 val = mats[i].GetVector(id);
                    val = SetCompInVec4(val, channel, f);
                    if (mats.Count == 1)
                    {
                        GetProperty(propertyName).vectorValue = val;//为了K动画，多选不能K动画。
                    }
                    else
                    {
                        mats[i].SetVector(id, val);
                    }
                }

                drawEndChangeCheckBlock?.Invoke(f, hasMixedValue);
            }

            drawBlock?.Invoke(f,hasMixedValue);
            EditorGUI.showMixedValue = false;
        }

        public void DrawVector4XYZComponet(string label, string propertyName, Action<Vector3> drawBlock = null)
        {
            EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
            Vector4 originVec = GetProperty(propertyName).vectorValue;
            Vector3 vec = originVec;
            EditorGUI.BeginChangeCheck();
            vec = EditorGUILayout.Vector3Field(label, vec);
            if (EditorGUI.EndChangeCheck())
            {
                GetProperty(propertyName).vectorValue = new Vector4(vec.x, vec.y, vec.z, originVec.w);
            }

            drawBlock?.Invoke(vec);
            EditorGUI.showMixedValue = false;
        }

        public enum SamplerWarpMode
        {
            Repeat,
            Clamp,
            RepeatX_ClampY,
            ClampX_RepeatY
        }

        public Rect GetRectAfterLabelWidth(Rect rect, bool ignoreIndent = false)
        {
            Rect rectAfterLabelWidth = MaterialEditor.GetRectAfterLabelWidth(rect); //右边缘是准的。
            Rect leftAlignedFieldRect = MaterialEditor.GetLeftAlignedFieldRect(rect); //左边缘是准的，实际有2f空隙。
            float x = leftAlignedFieldRect.x + 2f;
            float width = rectAfterLabelWidth.x + rectAfterLabelWidth.width - x;

            var newRec = new Rect(x, rectAfterLabelWidth.y, width, rectAfterLabelWidth.height);

            if (ignoreIndent)
            {
                float indent = (float)EditorGUI.indentLevel * 15f;
                newRec.x -= indent;
                newRec.width += indent;
            }
            return newRec;
        }

        public void DrawTextureFoldOut(ref AnimBool foldOutAnimBool, string label, string texturePropertyName,
            string colorPropertyName = null, bool drawScaleOffset = true, bool drawWrapMode = false,
            int wrapModeFlagBitsName = 0, int flagIndex = 2, Action<MaterialProperty> drawBlock = null)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect(false,68f);//MaterialEditor.GetTextureFieldHeight() => 64f;
            // var foldoutRect = new Rect(rect.x, rect.y, rect.width , rect.height);
            var textureThumbnialRect = new Rect(rect.x , rect.y, rect.width, rect.height);
            Texture texture = matEditor.TextureProperty(textureThumbnialRect,GetProperty(texturePropertyName), label, drawScaleOffset);
            EditorGUILayout.EndHorizontal();

            if (colorPropertyName != null)
            {
                // Rect colorPropRect = GetRectAfterLabelWidth(rect, true);
                // colorPropRect.x -= EditorGUI.indentLevel
                Rect colorPropRect = EditorGUILayout.GetControlRect(false);
                Color color = matEditor.ColorProperty(colorPropRect, GetProperty(colorPropertyName), "");
            }
            var foldoutRect = EditorGUILayout.GetControlRect(false);//MaterialEditor.GetTextureFieldHeight() => 64f;
            Rect labelRect = foldoutRect;
            labelRect.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, label+"相关功能", EditorStyles.boldLabel);
            foldOutAnimBool.target = EditorGUI.Foldout(foldoutRect, foldOutAnimBool.target, "", true);
            float faded = foldOutAnimBool.faded;
            if (faded == 0) faded = 0.00001f;
            EditorGUILayout.BeginFadeGroup(faded);
            EditorGUI.BeginDisabledGroup(texture == null);

            DrawAfterTexture(true, label, texturePropertyName, false, drawWrapMode, wrapModeFlagBitsName, flagIndex,
                drawBlock);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndFadeGroup();
        }

        public void DrawTexture(string label, string texturePropertyName, string colorPropertyName = null,
            bool drawScaleOffset = true, bool drawWrapMode = false, int wrapModeFlagBitsName = 0, int flagIndex = 2,
            Action<MaterialProperty> drawBlock = null)
        {
            bool hasTexture = mats[0].GetTexture(texturePropertyName) != null;
            matEditor.TextureProperty(GetProperty(texturePropertyName),label,drawScaleOffset);
            if (colorPropertyName != null)
            {
                Rect colorRect = EditorGUILayout.GetControlRect();
                matEditor.ColorProperty(colorRect,GetProperty(colorPropertyName), "");
            }
                
            DrawAfterTexture(hasTexture, label, texturePropertyName, false, drawWrapMode, wrapModeFlagBitsName,
                flagIndex, drawBlock);
        }

        bool WrapModeFlagHasMixedValue(int wrapModeFlagBitsName, int flagIndex)
        {
            int tmpWrapMode = 0;
            for (int i = 0; i < shaderFlags.Length; i++)
            {
                if (i == 0)
                {
                    tmpWrapMode = GetWrapModeFlagValue(wrapModeFlagBitsName,flagIndex,shaderFlags[i]);
                }
                else
                {
                    if (!tmpWrapMode.Equals(GetWrapModeFlagValue(wrapModeFlagBitsName, flagIndex, shaderFlags[i])))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        int GetWrapModeFlagValue(int wrapModeFlagBitsName, int flagIndex,ShaderFlagsBase shaderFlag)
        {
            int tmpWrapMode = shaderFlag.CheckFlagBits(wrapModeFlagBitsName, index: flagIndex) ? 1 : 0;
            tmpWrapMode = shaderFlag.CheckFlagBits(wrapModeFlagBitsName << 16, index: flagIndex)
                ? tmpWrapMode + 2
                : tmpWrapMode;
            return tmpWrapMode;
        }

        void SetWrapModeFlagValue(int wrapModeFlagBitsName, int flagIndex,int wrapModeValue)
        {
            for (int i = 0; i < shaderFlags.Length; i++)
            {
                switch (wrapModeValue)
                {
                    case 0:
                        shaderFlags[i].ClearFlagBits(wrapModeFlagBitsName, index: flagIndex);
                        shaderFlags[i].ClearFlagBits(wrapModeFlagBitsName << 16, index: flagIndex);
                        break;
                    case 1:
                        shaderFlags[i].SetFlagBits(wrapModeFlagBitsName, index: flagIndex);
                        shaderFlags[i].ClearFlagBits(wrapModeFlagBitsName << 16, index: flagIndex);
                        break;
                    case 2:
                        shaderFlags[i].ClearFlagBits(wrapModeFlagBitsName, index: flagIndex);
                        shaderFlags[i].SetFlagBits(wrapModeFlagBitsName << 16, index: flagIndex);
                        break;
                    case 3:
                        shaderFlags[i].SetFlagBits(wrapModeFlagBitsName, index: flagIndex);
                        shaderFlags[i].SetFlagBits(wrapModeFlagBitsName << 16, index: flagIndex);
                        break;
                }
            }
        }

        public void DrawWrapMode(string texturelabel,int wrapModeFlagBitsName = 0, int flagIndex = 2)
        {
            EditorGUI.showMixedValue = WrapModeFlagHasMixedValue(wrapModeFlagBitsName, flagIndex);
                
            int tmpWrapMode = GetWrapModeFlagValue(wrapModeFlagBitsName, flagIndex,shaderFlags[0]);
            EditorGUI.BeginChangeCheck();
            tmpWrapMode = EditorGUILayout.Popup(new GUIContent(texturelabel + "循环模式"), tmpWrapMode,
                Enum.GetNames(typeof(SamplerWarpMode)));
            if (EditorGUI.EndChangeCheck())
            {
                SetWrapModeFlagValue(wrapModeFlagBitsName, flagIndex,tmpWrapMode);
            }
            EditorGUI.showMixedValue = false;
        }
        public void DrawAfterTexture(bool hasTexture, string label, string texturePropertyName,
            bool drawScaleOffset = false, bool drawWrapMode = false, int wrapModeFlagBitsName = 0, int flagIndex = 2,
            Action<MaterialProperty> drawBlock = null)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(!hasTexture);
            if (drawWrapMode)
            {
                DrawWrapMode(label, wrapModeFlagBitsName,flagIndex);
                // EditorGUI.showMixedValue = WrapModeFlagHasMixedValue(wrapModeFlagBitsName, flagIndex);
                //
                // int tmpWrapMode = GetWrapModeFlagValue(wrapModeFlagBitsName, flagIndex,shaderFlags[0]);
                // EditorGUI.BeginChangeCheck();
                // tmpWrapMode = EditorGUILayout.Popup(new GUIContent(label + "循环模式"), tmpWrapMode,
                //     Enum.GetNames(typeof(SamplerWarpMode)));
                // if (EditorGUI.EndChangeCheck())
                // {
                //     SetWrapModeFlagValue(wrapModeFlagBitsName, flagIndex,tmpWrapMode);
                // }
                // EditorGUI.showMixedValue = false;
            }

            if (drawScaleOffset)
            {
                matEditor.TextureScaleOffsetProperty(GetProperty(texturePropertyName));
            }

            drawBlock?.Invoke(GetProperty(texturePropertyName));
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
        }


        public void DrawPopUp(string label, string propertyName, string[] options, string[] toolTips = null,
            Action<MaterialProperty> drawBlock = null,Action<MaterialProperty> drawOnValueChangedBlock = null)
        {
            MaterialProperty property = GetProperty(propertyName);
            if (property == null) return;
            EditorGUI.showMixedValue = property.hasMixedValue;

            float mode = property.floatValue;
            EditorGUI.BeginChangeCheck();
            GUIContent[] optionGUIContents = new GUIContent[options.Length];
            for (int i = 0; i < optionGUIContents.Length; i++)
            {
                if (toolTips != null && toolTips.Length == options.Length)
                {
                    optionGUIContents[i] = new GUIContent(options[i], toolTips[i]);
                }
                else
                {
                    optionGUIContents[i] = new GUIContent(options[i]);
                }
            }

            mode = EditorGUILayout.Popup(new GUIContent(label), (int)mode, optionGUIContents);
            if (EditorGUI.EndChangeCheck())
            {
                int propID = Shader.PropertyToID(propertyName);
                for (int i = 0; i < mats.Count; i++)
                {
                    mats[i].SetFloat(propID,mode);   
                }

                property.floatValue = mode;
                drawOnValueChangedBlock?.Invoke(property);
            }

            drawBlock?.Invoke(property);

            EditorGUI.showMixedValue = false;
        }

        public MaterialProperty GetProperty(string propertyName)
        {
            foreach (ShaderPropertyPack pack in ShaderPropertyPacks)
            {
                if (pack.name == propertyName)
                {
                    return pack.property;
                }
            }

            // Debug.LogError("材质球" + mat.name + "找不到属性" + propertyName, mat);
            return null;
        }

        bool RenderQueueHasMixedValue()
        {
            int queue = 0;
            for (int i = 0; i < mats.Count; i++)
            {
                if (i == 0)
                {
                    queue = mats[i].renderQueue;
                }
                else
                {
                    if (queue != mats[i].renderQueue)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void DrawRenderQueue(MaterialProperty queueBiasProp)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
            int queueLabelWidth = 55;
            Rect queueLabelRect = new Rect(labelRect.x , labelRect.y, queueLabelWidth, rect.height);
            Rect queueNumberRect = new Rect(queueLabelRect.x + queueLabelWidth , queueLabelRect.y,EditorGUIUtility.labelWidth - queueLabelWidth, rect.height);
            EditorGUI.LabelField(queueLabelRect, "Queue:" );
            EditorGUI.showMixedValue = RenderQueueHasMixedValue();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(queueNumberRect, mats[0].renderQueue.ToString());
            EditorGUI.EndDisabledGroup();
            EditorGUI.showMixedValue = false;
            Rect afterLabelRect = GetRectAfterLabelWidth(rect);
            int QueueBias = (int)queueBiasProp.floatValue;
            EditorGUI.showMixedValue = queueBiasProp.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            QueueBias = EditorGUI.IntField(afterLabelRect, "QueueBias:", QueueBias);
            if (EditorGUI.EndChangeCheck())
            {
                queueBiasProp.floatValue = QueueBias;
            }
            EditorGUI.showMixedValue = false;
        }

        void GuiLine(int i_height = 1)
        {

            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

        }

        public static float PowerSlider(Rect position, GUIContent label, float value, float leftValue, float rightValue,
            float power)
        {
            var editorGuiType = typeof(EditorGUI);
            var methodInfo = editorGuiType.GetMethod(
                "PowerSlider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                null,
                new[] { typeof(Rect), typeof(GUIContent), typeof(float), typeof(float), typeof(float), typeof(float) },
                null);
            if (methodInfo != null)
            {
                return (float)methodInfo.Invoke(null,
                    new object[] { position, label, value, leftValue, rightValue, power });
            }

            return leftValue;
        }


        private GradientAlphaKey[] defaultGradientAlphaKey = new GradientAlphaKey[]
            { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) };


        float RGBLuminance(Color color)
        {
            return 0.2126f* color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        private bool isUpateGradientPickerCache = false;

        public void DrawGradientUndoPerformed()
        {
            isUpateGradientPickerCache = true;
        }
        Dictionary<Gradient,bool> gradientsUpdateDic = new Dictionary<Gradient,bool>();
        //如果是黑白Gradient，则取Gradient的颜色的黑白值（这样在面板上可视化比较好）
        //如果既有颜色，也有Alpha。则在CountProperty上采取前16位和后16位编码。
        //原则：gradient对象只是一个操作中介。取值应该直接在MatProperty上去，Set值也应该在验证合法后才能Set，不合法应该提出警告。
        public void DrawGradient(ref Gradient gradient,bool hdr,ColorSpace colorSpace,string label,int maxCount,string countPropertyName,MaterialProperty[] colorProperties = null,MaterialProperty[] alphaProperties = null)
        {
            if (isUpateGradientPickerCache)
            {
                foreach (var keys in gradientsUpdateDic.Keys.ToList())
                {
                    gradientsUpdateDic[keys] = true;
                }

                isUpateGradientPickerCache = false;
            }
            MaterialProperty countProperty = GetProperty(countPropertyName);
            Rect rect = EditorGUILayout.GetControlRect();

            // var labelRect = new Rect(rect.x + 2f, rect.y, rect.width - 2f, rect.height);
            // EditorGUI.LabelField(labelRect,label);
            var gradientRect = rect;
            
            bool hasMixedValue = false;
            hasMixedValue |= countProperty.hasMixedValue;

            bool isBlackAndWhiteGradient = colorProperties == null && alphaProperties != null;
            bool isNoAlphaColorGradient =  colorProperties != null && alphaProperties == null;
            
            int colorKeysCount;
            int alphaKeysCount;

            int countPropertyIntValue = countProperty.intValue;
            if (colorProperties != null && alphaProperties != null)
            {
                colorKeysCount = countPropertyIntValue & 0xFFFF;
                alphaKeysCount = countPropertyIntValue >> 16;
            }
            else
            {
                colorKeysCount = countPropertyIntValue;
                alphaKeysCount = 2;
            }
            
            
            if (colorProperties != null)
            {
                for(int i = 0; i < colorKeysCount; i++)
                {
                    hasMixedValue |= colorProperties[i].hasMixedValue;
                }
            }
            if (alphaProperties != null)
            {
                for (int i = 0; i < Mathf.CeilToInt(alphaKeysCount/2f); i++)
                {
                    hasMixedValue |= alphaProperties[i].hasMixedValue;
                }
            }

            
            if (gradient == null||gradientsUpdateDic[gradient])
            {
                if (gradient == null)
                {
                    gradient = new Gradient();
                    // gradient.colorSpace = ColorSpace.Gamma;
                    gradientsUpdateDic.Add(gradient,false);
                }

                gradientsUpdateDic[gradient] = false;
                
                if (colorProperties != null || isBlackAndWhiteGradient)
                {
                    GradientColorKey[] colorKeys;
                    if (gradient.colorKeys.Length != colorKeysCount)
                    {
                        colorKeys = new GradientColorKey[colorKeysCount];
                    }
                    else
                    {
                        colorKeys = gradient.colorKeys;
                    }
                    for (int i = 0; i < colorKeysCount; i++)
                    {
                        if (isBlackAndWhiteGradient)
                        {
                            Vector4 vec = alphaProperties[i / 2].vectorValue;
                            Color c = Color.white;
                            if (i % 2 == 0)
                            {
                                c.r = vec.x;
                                c.g = vec.x;
                                c.b = vec.x;
                            }
                            else
                            {
                                c.r = vec.z;
                                c.g = vec.z;
                                c.b = vec.z;
                            }
                            // Debug.Log(i);
                            // Debug.Log(c);
                            colorKeys[i].color = c;
                            colorKeys[i].time = i % 2 == 0 ? vec.y : vec.w;
                        }
                        else
                        {
                            Color c = colorProperties[i].colorValue; 
                            colorKeys[i].color = c;
                            colorKeys[i].time = c.a;
                        }
                    }
                    gradient.colorKeys = colorKeys;
                }
          
                if (isBlackAndWhiteGradient || isNoAlphaColorGradient || alphaProperties == null)
                {
                    gradient.alphaKeys = defaultGradientAlphaKey;
                }
                else
                {

                    GradientAlphaKey[] alphaKeys;
                    if (gradient.alphaKeys.Length != alphaKeysCount)
                    {
                        alphaKeys = new GradientAlphaKey[alphaKeysCount];
                    }
                    else
                    {
                        alphaKeys = gradient.alphaKeys;
                    }

                    for (int i = 0; i < alphaKeysCount ; i++)
                    {
                        Vector4 vec = alphaProperties[i / 2].vectorValue;
                        if (i % 2 == 0)
                        {
                            alphaKeys[i].alpha = vec.x;
                            alphaKeys[i].time = vec.y;
                        }
                        else
                        {
                            alphaKeys[i].alpha = vec.z;
                            alphaKeys[i].time = vec.w;
                        }
                    }
                    gradient.alphaKeys = alphaKeys;
                    
                }
                GradientReflectionHelper.RefreshGradientData();
                // Debug.Log("----------------SetCurrentGradient------------------");
            }

            
           
            

            EditorGUI.showMixedValue = hasMixedValue;
            
            EditorGUI.BeginChangeCheck();
            gradient = EditorGUI.GradientField(gradientRect, new GUIContent(label),gradient,hdr,colorSpace);
            if (EditorGUI.EndChangeCheck())
            {
                gradientsUpdateDic[gradient] = true;
                int countPropertyValue = countPropertyIntValue;

                if (colorProperties != null || isBlackAndWhiteGradient)
                {
                    int finalColorKeysCount = gradient.colorKeys.Length;
                    if (finalColorKeysCount <= maxCount)
                    {
                        if (isBlackAndWhiteGradient)
                        {
                            int floatPropertyCount = Mathf.CeilToInt((float)finalColorKeysCount / (2f));
                            for (int i = 0; i < floatPropertyCount; i++)
                            {
                                Vector4 vec = Vector4.zero;
                                vec.x = gradient.colorKeys[i*2].color.r;
                                vec.y = gradient.colorKeys[i*2].time;
                                if (i * 2 + 1 < gradient.colorKeys.Length)
                                {
                                    vec.z = gradient.colorKeys[i * 2 + 1].color.r;
                                    vec.w = gradient.colorKeys[i * 2 + 1].time;
                                }

                                alphaProperties[i].vectorValue = vec;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < finalColorKeysCount; i++)
                            {
                                
                                Color c = gradient.colorKeys[i].color;
                                c.a = gradient.colorKeys[i].time;
                                colorProperties[i].colorValue = c;
                            }
                        }

                        countPropertyValue &= (0xFFFF <<16);
                        countPropertyValue |= finalColorKeysCount;
                    }
                }
                
                if (!(isBlackAndWhiteGradient || isNoAlphaColorGradient || alphaProperties == null))
                {
                    int finalAlphaKeysCount = gradient.alphaKeys.Length;
                    if (finalAlphaKeysCount <= maxCount)
                    {
                        int floatPropertyCount = Mathf.CeilToInt((float)finalAlphaKeysCount / (2f));
                        for (int i = 0; i < floatPropertyCount; i++)
                        {
                            Vector4 vec = Vector4.zero;
                            vec.x = gradient.alphaKeys[i*2].alpha;
                            vec.y = gradient.alphaKeys[i*2].time;
                            if (i * 2 + 1 < gradient.alphaKeys.Length)
                            {
                                vec.z = gradient.alphaKeys[i * 2 + 1].alpha;
                                vec.w = gradient.alphaKeys[i * 2 + 1].time;
                            }
                            alphaProperties[i].vectorValue = vec;
                        }

                        countPropertyValue &= (0xFFFF);
                        int alphaCount = finalAlphaKeysCount << 16;
                        countPropertyValue |= alphaCount;
                    }
                }

                countProperty.intValue = countPropertyValue;

            }
        }

        
       

    }
}