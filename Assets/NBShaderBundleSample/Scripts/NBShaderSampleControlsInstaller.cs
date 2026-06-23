using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NBShader;

public static class NBShaderSampleControlsInstaller
{
    private const string SampleSceneName = "NBShaderSamples";
    private const string ControlsRootName = "NBShaderSampleControls";
    private const string MainCameraName = "Main Camera";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        for (var i = 0; i < SceneManager.sceneCount; i++)
            TryInstall(SceneManager.GetSceneAt(i));
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    private static void TryInstall(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded || scene.name != SampleSceneName)
            return;

        var cameraObject = FindRootChild(scene, MainCameraName);
        if (cameraObject == null)
            return;

        EnsureEventSystem(scene);

        var controlsRoot = FindRootChild(scene, ControlsRootName);
        if (controlsRoot == null)
            controlsRoot = CreateControlsRoot(scene);

        var controller = cameraObject.GetComponent<NBShaderSampleFirstPersonController>();
        if (controller == null)
            controller = cameraObject.AddComponent<NBShaderSampleFirstPersonController>();

        controller.ConfigureMobileControls(controlsRoot);
    }

    private static GameObject CreateControlsRoot(Scene scene)
    {
        var root = new GameObject(
            ControlsRootName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(root, scene);

        var rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.localScale = Vector3.one;

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateStick(root.transform, "Move Stick", Vector2.zero, new Vector2(128f, 112f), "<Gamepad>/leftStick");
        CreateStick(root.transform, "Look Stick", new Vector2(1f, 0f), new Vector2(-128f, 112f), "<Gamepad>/rightStick");
        return root;
    }

    private static OnScreenStick CreateStick(
        Transform parent,
        string name,
        Vector2 anchor,
        Vector2 anchoredPosition,
        string controlPath)
    {
        var stickObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        stickObject.transform.SetParent(parent, false);

        var stickRect = (RectTransform)stickObject.transform;
        stickRect.anchorMin = anchor;
        stickRect.anchorMax = anchor;
        stickRect.pivot = new Vector2(0.5f, 0.5f);
        stickRect.anchoredPosition = anchoredPosition;
        stickRect.sizeDelta = new Vector2(170f, 170f);

        var stickImage = stickObject.GetComponent<Image>();
        stickImage.color = new Color(0f, 0f, 0f, 0.28f);
        stickImage.raycastTarget = false;

        var handleObject = new GameObject(
            "Handle",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(OnScreenStick));
        handleObject.transform.SetParent(stickObject.transform, false);

        var handleRect = (RectTransform)handleObject.transform;
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(74f, 74f);

        var handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 0.55f);
        handleImage.raycastTarget = true;

        var stick = handleObject.GetComponent<OnScreenStick>();
        stick.controlPath = controlPath;
        stick.movementRange = 64f;
        stick.behaviour = OnScreenStick.Behaviour.RelativePositionWithStaticOrigin;
        return stick;
    }

    private static void EnsureEventSystem(Scene scene)
    {
        if (EventSystem.current != null || FindExistingEventSystem() != null)
            return;

        var eventSystem = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
    }

    private static EventSystem FindExistingEventSystem()
    {
        return UnityObjectFindCompat.FindAny<EventSystem>();
    }

    private static GameObject FindRootChild(Scene scene, string objectName)
    {
        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == objectName)
                return roots[i];
        }

        return null;
    }
}
