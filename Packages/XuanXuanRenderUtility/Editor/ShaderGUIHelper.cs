using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
// using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor
{
    public class ShaderGUIHelper
    {
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

        AnimBool trueAnimBool = new AnimBool(true);

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

        public void DrawBigBlockFoldOut(ref AnimBool foldOutAnimBool, String label, Action drawBlock)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var foldoutRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            // EditorGUI.DrawRect(foldoutRect,Color.red);
            var labelRect = new Rect(rect.x + 2f, rect.y, rect.width - 2f, rect.height);
            // EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);
            foldOutAnimBool.target = EditorGUI.Foldout(foldoutRect, foldOutAnimBool.target, string.Empty, true);
            FontStyle origFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUI.LabelField(labelRect, label);
            EditorStyles.label.fontStyle = origFontStyle;
            EditorGUILayout.EndHorizontal();
            float faded = foldOutAnimBool.faded;
            if (faded == 0) faded = 0.00001f;
            EditorGUILayout.BeginFadeGroup(faded);
            EditorGUI.indentLevel++;
            drawBlock();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFadeGroup();
            GuiLine();
        }

        public void DrawBigBlockWithToggle(String label, string propertyName, int flagBitsName = 0, int flagIndex = 0,
            string shaderKeyword = null, string shaderPassName = null, string shaderPassName2 = null,
            Action<bool> drawBlock = null)
        {

            DrawToggle(label, propertyName, flagBitsName, flagIndex, shaderKeyword, shaderPassName, shaderPassName2,
                isIndentBlock: true, FontStyle.Bold, drawBlock: drawBlock);
            GuiLine();

        }

        public void DrawToggleFoldOut(ref AnimBool foldOutAnimBool, String label, string propertyName = null,
            int flagBitsName = 0,
            int flagIndex = 0, string shaderKeyword = null, string shaderPassName = null, bool isIndentBlock = true,
            FontStyle fontStyle = FontStyle.Normal,
            Action<bool> drawBlock = null, Action<bool> drawEndChangeCheck = null)
        {
            if (fontStyle == FontStyle.Bold)
            {
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var toggleRect = GetRectAfterLabelWidth(rect);

            var foldoutRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            foldoutRect.width = toggleRect.x - foldoutRect.x;
            var labelRect = new Rect(rect.x + 2f, rect.y, rect.width - 2f, rect.height);

            bool isToggle = false;
            // 必须先画Toggle，不然按钮会被FoldOut和Label覆盖。
            DrawToggle("", propertyName, flagBitsName, flagIndex, shaderKeyword, shaderPassName, isIndentBlock: false,
                fontStyle: FontStyle.Normal, rect: toggleRect, drawBlock:
                toggle => { isToggle = toggle; }, drawEndChangeCheck: isEndChangeToggle =>
                {
                    if (drawEndChangeCheck != null)
                    {
                        drawEndChangeCheck(isEndChangeToggle);
                    }
                });

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
                EditorGUI.BeginDisabledGroup(!isToggle);
                drawBlock?.Invoke(isToggle);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFadeGroup();
            if (isIndentBlock) EditorGUI.indentLevel--;
        }

        public void DrawToggle(String label, string propertyName = null, int flagBitsName = 0, int flagIndex = 0,
            string shaderKeyword = null, string shaderPassName = null, string shaderPassName2 = null,
            bool isIndentBlock = true, FontStyle fontStyle = FontStyle.Normal, Rect rect = new Rect(),
            Action<bool> drawBlock = null, Action<bool> drawEndChangeCheck = null)
        {
            if (propertyName != null && GetProperty(propertyName) == null)
                return;

            if (fontStyle == FontStyle.Bold)
            {
                EditorGUILayout.Space();
            }

            bool isToggle = false;

            if (propertyName != null)
            {
                isToggle = GetProperty(propertyName).floatValue > 0.5f ? true : false;
            }
            else if (flagBitsName != 0 && shaderFlags[0] != null)
            {
                isToggle = shaderFlags[0].CheckFlagBits(flagBitsName, index: flagIndex);
            }
            else if (shaderKeyword != null)
            {
                isToggle = mats[0].IsKeywordEnabled(shaderKeyword);
            }
            else if (shaderPassName != null)
            {
                isToggle = mats[0].GetShaderPassEnabled(shaderPassName);
            }
            else if (shaderPassName2 != null)
            {
                isToggle = mats[0].GetShaderPassEnabled(shaderPassName2);
            }

            if (propertyName != null)
            {
                EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
            }

            for (int i = 0; i < mats.Count; i++)
            {
                if (isToggle)
                {
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

            EditorGUI.BeginChangeCheck();
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
                for (int i = 0; i < mats.Count; i++)
                {
                    if (isToggle)
                    {
                        if (propertyName != null)
                        {
                            GetProperty(propertyName).floatValue = 1;
                        }

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
                        if (propertyName != null)
                        {
                            GetProperty(propertyName).floatValue = 0;
                        }

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

                drawEndChangeCheck?.Invoke(isToggle);
            }

            if (isIndentBlock) EditorGUI.indentLevel++;
            drawBlock?.Invoke(isToggle);
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
            Action<float> drawBlock = null)
        {
            EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
            float f = GetProperty(propertyName).floatValue;
            if (isReciprocal) f = 1 / f;
            EditorGUI.BeginChangeCheck();
            f = EditorGUILayout.FloatField(label, f);
            if (isReciprocal) f = 1 / f;
            if (EditorGUI.EndChangeCheck())
            {
                GetProperty(propertyName).floatValue = f;
            }

            drawBlock?.Invoke(f);
            EditorGUI.showMixedValue = false;
        }

        public void DrawVector4In2Line(string propertyName, string firstLineLabel = null, string secondLineLabel = null,
            Action drawBlock = null)
        {
            MaterialProperty property = GetProperty(propertyName);
            EditorGUI.showMixedValue = property.hasMixedValue;


            Vector2 xy = new Vector2(property.vectorValue.x, property.vectorValue.y);
            Vector2 zw = new Vector2(property.vectorValue.z, property.vectorValue.w);

            EditorGUI.BeginChangeCheck();
            if (firstLineLabel != null)
            {
                xy = EditorGUILayout.Vector2Field(firstLineLabel, xy);
            }

            if (secondLineLabel != null)
            {
                zw = EditorGUILayout.Vector2Field(secondLineLabel, zw);
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.vectorValue = new Vector4(xy.x, xy.y, zw.x, zw.y);
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

        public void DrawVector4MinMaxSlider(string propertyName, string Lable, string minChannel, string maxChanel,
            float minValue = 0f, float maxValue = 1f, Action<float> drawBlock = null)
        {
            EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
            Vector4 vec = GetProperty(propertyName).vectorValue;
            float minChannelVal = GetCompInVec4(vec, minChannel);
            float maxChanelVal = GetCompInVec4(vec, maxChanel);

            EditorGUI.BeginChangeCheck();
            using (EditorGUILayout.HorizontalScope scope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(Lable);
                minChannelVal =
                    EditorGUILayout.FloatField(minChannelVal, new GUILayoutOption[] { GUILayout.Width(80) });
                EditorGUILayout.MinMaxSlider(ref minChannelVal, ref maxChanelVal, minValue, maxValue);
                maxChanelVal = EditorGUILayout.FloatField(maxChanelVal, new GUILayoutOption[] { GUILayout.Width(80) });

            }

            if (EditorGUI.EndChangeCheck())
            {
                vec = SetCompInVec4(vec, minChannel, minChannelVal);
                vec = SetCompInVec4(vec, maxChanel, maxChanelVal);
                GetProperty(propertyName).vectorValue = vec;
            }

            EditorGUI.showMixedValue = false;

        }

        public void DrawVector4Componet(string label, string propertyName, string channel, bool isSlider,
            float minValue = 0, float maxValue = 1, float powerSlider = 1, float multiplier = 1,
            bool isReciprocal = false, Action<float> drawBlock = null)
        {
            EditorGUI.showMixedValue = GetProperty(propertyName).hasMixedValue;
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
                GetProperty(propertyName).vectorValue = SetCompInVec4(vec, channel, f);
            }

            drawBlock?.Invoke(f);
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

            // // EditorGUI.DrawRect(leftRect,Color.red);
            // float biasWidth = EditorGUI.indentLevel * 15f - 2f;
            // leftRect.x -= biasWidth;
            // leftRect.width += biasWidth;
            // EditorGUI.DrawRect(leftRect,Color.green);
            return newRec;
        }

        public void DrawTextureFoldOut(ref AnimBool foldOutAnimBool, string label, string texturePropertyName,
            string colorPropertyName = null, bool drawScaleOffset = true, bool drawWrapMode = false,
            int flagBitsName = 0, int flagIndex = 2, Action<Texture> drawBlock = null)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var foldoutRect = new Rect(rect.x, rect.y, rect.width - MaterialEditor.GetRectAfterLabelWidth(rect).width,
                rect.height);
            var textureThumbnialRect = new Rect(rect.x + 2f, rect.y, 14f, rect.height);
            var labelRect = new Rect(rect.x + 35f, rect.y, rect.width - 18f, rect.height);
            Texture texture =
                matEditor.TexturePropertyMiniThumbnail(textureThumbnialRect, GetProperty(texturePropertyName), "", "");
            EditorGUI.LabelField(labelRect, label);

            foldOutAnimBool.target = EditorGUI.Foldout(foldoutRect, foldOutAnimBool.target, string.Empty, true);
            if (colorPropertyName != null)
            {
                Rect colorPropRect = GetRectAfterLabelWidth(rect, true);
                // colorPropRect.x -= EditorGUI.indentLevel
                Color color = matEditor.ColorProperty(colorPropRect, GetProperty(colorPropertyName), "");
            }

            EditorGUILayout.EndHorizontal();
            float faded = foldOutAnimBool.faded;
            if (faded == 0) faded = 0.00001f;
            EditorGUILayout.BeginFadeGroup(faded);
            EditorGUI.BeginDisabledGroup(texture == null);

            DrawAfterTexture(true, label, texturePropertyName, drawScaleOffset, drawWrapMode, flagBitsName, flagIndex,
                drawBlock);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndFadeGroup();
        }

        public void DrawTexture(string label, string texturePropertyName, string colorPropertyName = null,
            bool drawScaleOffset = true, bool drawWrapMode = false, int flagBitsName = 0, int flagIndex = 2,
            Action<Texture> drawBlock = null)
        {
            bool hasTexture = mats[0].GetTexture(texturePropertyName) != null;
            matEditor.TexturePropertySingleLine(new GUIContent(label), GetProperty(texturePropertyName),
                GetProperty(colorPropertyName));
            DrawAfterTexture(hasTexture, label, texturePropertyName, drawScaleOffset, drawWrapMode, flagBitsName,
                flagIndex, drawBlock);
        }

        public void DrawAfterTexture(bool hasTexture, string label, string texturePropertyName,
            bool drawScaleOffset = true, bool drawWrapMode = false, int flagBitsName = 0, int flagIndex = 2,
            Action<Texture> drawBlock = null)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(!hasTexture);
            if (drawWrapMode)
            {
                //这个多选材质就不出现了，不好处理
                if (mats.Count == 1)
                {
                    int tmpWrapMode = shaderFlags[0].CheckFlagBits(flagBitsName, index: flagIndex) ? 1 : 0;
                    tmpWrapMode = shaderFlags[0].CheckFlagBits(flagBitsName << 16, index: flagIndex)
                        ? tmpWrapMode + 2
                        : tmpWrapMode;
                    tmpWrapMode = EditorGUILayout.Popup(new GUIContent(label + "循环模式"), tmpWrapMode,
                        Enum.GetNames(typeof(SamplerWarpMode)));
                    switch (tmpWrapMode)
                    {
                        case 0:
                            shaderFlags[0].ClearFlagBits(flagBitsName, index: flagIndex);
                            shaderFlags[0].ClearFlagBits(flagBitsName << 16, index: flagIndex);
                            break;
                        case 1:
                            shaderFlags[0].SetFlagBits(flagBitsName, index: flagIndex);
                            shaderFlags[0].ClearFlagBits(flagBitsName << 16, index: flagIndex);
                            break;
                        case 2:
                            shaderFlags[0].ClearFlagBits(flagBitsName, index: flagIndex);
                            shaderFlags[0].SetFlagBits(flagBitsName << 16, index: flagIndex);
                            break;
                        case 3:
                            shaderFlags[0].SetFlagBits(flagBitsName, index: flagIndex);
                            shaderFlags[0].SetFlagBits(flagBitsName << 16, index: flagIndex);
                            break;
                    }
                }
            }

            if (drawScaleOffset)
            {
                matEditor.TextureScaleOffsetProperty(GetProperty(texturePropertyName));
            }

            drawBlock?.Invoke(GetProperty(texturePropertyName).textureValue);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
        }


        public void DrawPopUp(string label, string propertyName, string[] options, string[] toolTips = null,
            Action<float> drawBlock = null)
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
                property.floatValue = mode;
            }

            drawBlock?.Invoke(mode);

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

        public void DrawRenderQueue(MaterialProperty queueBiasProp)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Queue：" + mats[0].renderQueue);
            int QueueBias = (int)queueBiasProp.floatValue;
            QueueBias = EditorGUILayout.IntField("QueueBias:", QueueBias);
            queueBiasProp.floatValue = QueueBias;
            EditorGUILayout.EndHorizontal();
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


    }
}