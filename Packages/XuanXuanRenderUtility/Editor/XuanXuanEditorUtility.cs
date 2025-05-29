using System;
using UnityEngine;
using UnityEditor;
// using System.Reflection;

public class XuanXuanEditorUtility
{
#if UNITY_EDITOR
    
    private static Type gameViewType = null;
    public static Action EditorUpdater = () => { };
    private bool isInitializeEditorUpdate = false;
    // [InitializeOnLoadMethod]
    // 初始化时注册更新
    public static void InitializeEditorUpdate()
    {
        EditorApplication.update += OnEditorUpdate;
        Debug.Log("常规类：已启动编辑器帧更新");
    }

    // 清理时注销更新
    public static void ShutdownEditorUpdate()
    {
        EditorApplication.update -= OnEditorUpdate;
        Debug.Log("常规类：已停止编辑器帧更新");
    }                                                           

    // 更新逻辑
    private static void OnEditorUpdate()
    {
        if (!Application.isPlaying)
        {
            // 在此编写每帧逻辑
            // Debug.Log("常规类：编辑器帧更新");
            EditorUpdater();
            // RepaintGameView();
        }
    }
    
    //尝试Repaint，但是放弃了。
    // static void RepaintGameView()
    // {
    //     
    //     // 通过反射获取 GameView 类型
    //     if (gameViewType == null)
    //     {
    //         gameViewType = System.Type.GetType("UnityEditor.GameView, UnityEditor");
    //     }
    //     if (gameViewType == null)
    //     {
    //         Debug.LogError("GameView 类型获取失败！");
    //         return;
    //     }
    //
    //     // 获取当前激活的 GameView 窗口
    //     var gameView = EditorWindow.GetWindow(gameViewType,false);
    //     if (gameView != null)
    //     {
    //         // 调用 Repaint 方法刷新界面
    //         gameView.Repaint();
    //     }
    // }
#endif
}
