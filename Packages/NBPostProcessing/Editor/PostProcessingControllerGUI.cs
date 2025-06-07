using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Reflection;
#if CINIMACHINE_3_0
using Unity.Cinemachine;
#endif

using System;
// using Unity.Properties;

[CustomEditor(typeof(PostProcessingController))]
public class PostProcessingControllerGUI : Editor
{
    private SerializedProperty _managerProperty;
    private SerializedProperty _indexProperty;

    public override void OnInspectorGUI()
    {
        PostProcessingController ppController = (PostProcessingController)target;
        serializedObject.Update();

        _managerProperty = serializedObject.FindProperty("_manager");
        _indexProperty = serializedObject.FindProperty("_index");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_managerProperty);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginChangeCheck();
        ppController.customScreenCenterPos =
            EditorGUILayout.Vector2Field("自定义屏幕中心", ppController.customScreenCenterPos);
        if (EditorGUI.EndChangeCheck())
        {
            ReflectMethod("SetScreenCenterPos", ppController);
        }

        DrawToggleFoldOut(ppController.AnimBools[0], "色散", ref ppController.chromaticAberrationToggle,
            drawEndChangeCheck: isChangeToggle => { ReflectMethod("InitAllSettings", ppController); }
            , drawBlock: isToggle =>
            {
                EditorGUI.BeginChangeCheck();
                ppController.caFromDistort = EditorGUILayout.Toggle("色散UV跟随后处理扭曲", ppController.caFromDistort);
                if (EditorGUI.EndChangeCheck())
                {
                    ReflectMethod("SetUVFromDistort", ppController);
                }

                ppController.chromaticAberrationIntensity =
                    EditorGUILayout.FloatField("色散强度", ppController.chromaticAberrationIntensity);

                if (!ppController.caFromDistort)
                {
                    ppController.chromaticAberrationPos =
                        EditorGUILayout.FloatField("色散位置", ppController.chromaticAberrationPos);

                    ppController.chromaticAberrationRange =
                        EditorGUILayout.FloatField("色散过渡范围", ppController.chromaticAberrationRange);
                }
            });

        DrawToggleFoldOut(ppController.AnimBools[1], "扭曲", ref ppController.distortSpeedToggle, drawEndChangeCheck:
            isChangeToggle => { ReflectMethod("InitAllSettings", ppController); },
            drawBlock: isToggle =>
            {
                EditorGUI.BeginChangeCheck();
                ppController.distortScreenUVMode =
                    EditorGUILayout.Toggle("后处理走常规屏幕坐标", ppController.distortScreenUVMode);
                if (EditorGUI.EndChangeCheck())
                {
                    ReflectMethod("SetUVFromDistort", ppController);
                }

                EditorGUI.BeginChangeCheck();
                ppController.distortSpeedTexture =
                    (Texture2D)EditorGUILayout.ObjectField("后处理扭曲贴图", ppController.distortSpeedTexture,
                        typeof(Texture2D));
                if (EditorGUI.EndChangeCheck())
                {
                    ReflectMethod("InitAllSettings", ppController);
                }

                if (ppController.distortScreenUVMode)
                {
                    EditorGUI.BeginChangeCheck();
                    ppController.distortTextureMidValue =
                        EditorGUILayout.FloatField("扭曲贴图中间值", ppController.distortTextureMidValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ReflectMethod("SetTexture", ppController);
                    }
                }


                ppController.distortSpeedTexSt = EditorGUILayout.Vector4Field("扭曲贴图ST", ppController.distortSpeedTexSt);

                ppController.distortSpeedIntensity =
                    EditorGUILayout.FloatField("扭曲强度", ppController.distortSpeedIntensity);

                if (!ppController.distortScreenUVMode)
                {
                    ppController.distortSpeedPosition =
                        EditorGUILayout.FloatField("扭曲位置", ppController.distortSpeedPosition);
                    ppController.distortSpeedRange = EditorGUILayout.FloatField("扭曲过渡范围", ppController.distortSpeedRange);
                }

                ppController.distortSpeedMoveSpeedX =
                    EditorGUILayout.FloatField("扭曲纹理流动X", ppController.distortSpeedMoveSpeedX);
                ppController.distortSpeedMoveSpeed =
                    EditorGUILayout.FloatField("扭曲纹理流动Y", ppController.distortSpeedMoveSpeed);
            });


        DrawToggleFoldOut(ppController.AnimBools[2], "径向模糊", ref ppController.radialBlurToggle, drawEndChangeCheck:
            isChangeToggle => { ReflectMethod("InitAllSettings", ppController); },
            drawBlock: isToggle =>
            {
                EditorGUI.BeginChangeCheck();
                ppController.radialBlurFromDistort =
                    EditorGUILayout.Toggle("径向模糊跟随后处理扭曲", ppController.radialBlurFromDistort);
                if (EditorGUI.EndChangeCheck())
                {
                    ReflectMethod("SetUVFromDistort", ppController);
                }

                ppController.radialBlurSampleCount =
                    EditorGUILayout.IntSlider("采样次数", ppController.radialBlurSampleCount, 1, 12);
                ppController.radialBlurIntensity = EditorGUILayout.FloatField("强度", ppController.radialBlurIntensity);
                if (!ppController.radialBlurFromDistort)
                {
                    ppController.radialBlurPos = EditorGUILayout.FloatField("位置", ppController.radialBlurPos);
                    ppController.radialBlurRange = EditorGUILayout.FloatField("过渡范围", ppController.radialBlurRange);
                }
            });

        #if CINIMACHINE_3_0
        DrawToggleFoldOut(ppController.AnimBools[3], "震屏", ref ppController.cameraShakeToggle, drawEndChangeCheck:
            isChangeToggle =>
            {
                ReflectMethod("InitAllSettings",ppController);
            },
            drawBlock: isToggle =>
            {

                EditorGUI.BeginChangeCheck();
                ppController.cinemachineCamera = (CinemachineCamera)EditorGUILayout.ObjectField("绑定Cinemachine相机", ppController.cinemachineCamera, typeof(CinemachineCamera));
                if (EditorGUI.EndChangeCheck())
                {
                    // ReflectMethod("InitCinemachineCamera",ppController);
                    ppController.InitCinemachineCamera();
                }

                ppController.cameraShakeIntensity =
                    EditorGUILayout.FloatField("相机震动强度", ppController.cameraShakeIntensity);
            });
        #endif

        DrawToggleFoldOut(ppController.AnimBools[4], "肌理叠加图", ref ppController.overlayTextureToggle, drawEndChangeCheck:isChangeToggle =>
            {
                ReflectMethod("InitAllSettings", ppController);
            }, 
            drawBlock: isToggle =>
            {
                EditorGUI.BeginChangeCheck();
                ppController.overlayTexturePolarCoordMode =
                    EditorGUILayout.Toggle("肌理图极坐标模式", ppController.overlayTexturePolarCoordMode);
                ppController.overlayTexture = (Texture2D)EditorGUILayout.ObjectField("肌理图", ppController.overlayTexture, typeof(Texture2D));
                if (EditorGUI.EndChangeCheck())
                {
                    ReflectMethod("SetTexture",ppController);
                }

                ppController.overlayTextureSt = EditorGUILayout.Vector4Field("肌理图缩放平移", ppController.overlayTextureSt);
                ppController.overlayTextureAnim =
                    EditorGUILayout.Vector2Field("肌理图偏移动画", ppController.overlayTextureAnim);
                ppController.overlayTextureIntensity =
                    EditorGUILayout.FloatField("肌理图强度", ppController.overlayTextureIntensity);
                
                EditorGUI.BeginChangeCheck();
                ppController.overlayMaskTexture = (Texture2D)EditorGUILayout.ObjectField("肌理蒙版图", ppController.overlayMaskTexture, typeof(Texture2D));
                if (EditorGUI.EndChangeCheck())
                {
                    ReflectMethod("SetTexture",ppController);
                }

                ppController.overlayMaskTextureSt =
                    EditorGUILayout.Vector4Field("肌理图蒙版缩放平移", ppController.overlayMaskTextureSt);

            });

        DrawToggleFoldOut(ppController.AnimBools[5], "反闪", ref ppController.flashToggle, drawEndChangeCheck:
            isChangeToggle =>
            {
                ReflectMethod("InitAllSettings",ppController);
            },
            drawBlock: isToggle =>
            {
                ppController.flashInvertIntensity =
                    EditorGUILayout.FloatField("反转度", ppController.flashInvertIntensity);
                ppController.flashDeSaturateIntensity =
                    EditorGUILayout.FloatField("饱和度", ppController.flashDeSaturateIntensity);
                ppController.flashContrast = EditorGUILayout.FloatField("对比度", ppController.flashContrast);
                ppController.flashColor = EditorGUILayout.ColorField("闪颜色", ppController.flashColor);
            });

        DrawToggleFoldOut(ppController.AnimBools[6], "暗角", ref ppController.vignetteToggle, drawEndChangeCheck:
            isChangeToggle =>
            {
                ReflectMethod("InitAllSettings", ppController);
            },
            drawBlock: isToggle =>
            {
                ppController.vignetteColor = EditorGUILayout.ColorField("暗角颜色", ppController.vignetteColor);
                ppController.vignetteIntensity = EditorGUILayout.FloatField("暗角强度", ppController.vignetteIntensity);
                ppController.vignetteRoundness = EditorGUILayout.FloatField("暗角圆度", ppController.vignetteRoundness);
                ppController.vignetteSmothness = EditorGUILayout.FloatField("暗角平滑度", ppController.vignetteSmothness);
            });

        if (GUILayout.Button("选择当前Manager"))
        {
            ReflectMethod("FindManager",ppController);
        }
        #if CINIMACHINE_3_0
        if (GUILayout.Button("选择当前CinemachineCamera"))
        {
            ppController.FindVirtualCamera();
        }
        #endif

    }
    
    public void DrawToggleFoldOut(AnimBool foldOutAnimBool,string label, ref bool isToggle,
           bool isIndentBlock = true,
            FontStyle fontStyle = FontStyle.Bold,
            Action<bool> drawBlock = null, Action<bool> drawEndChangeCheck = null)
        {
            if (fontStyle == FontStyle.Bold)
            {
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();

            var foldoutRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            // foldoutRect.width = toggleRect.x - foldoutRect.x;
            var labelRect = new Rect(rect.x + 18f, rect.y, rect.width - 18f, rect.height);

            // bool isToggle = false;
            // 必须先画Toggle，不然按钮会被FoldOut和Label覆盖。
            EditorGUI.BeginChangeCheck();
            isToggle = EditorGUI.Toggle(rect, isToggle, EditorStyles.toggle);
            if (EditorGUI.EndChangeCheck())
            {
                drawEndChangeCheck?.Invoke(isToggle);
            }

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

    void ReflectMethod(string methodName,PostProcessingController controller)
    {
        MethodInfo privateMethod = typeof(PostProcessingController).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (privateMethod != null)
        {
            privateMethod.Invoke(controller, null);
        }
        else
        {
            Debug.LogError("Private method " + methodName + " not found!");
        }
    }
}