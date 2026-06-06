using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NBShader;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public sealed class NBShaderBundleSampleEntry : MonoBehaviour
{
    [SerializeField] private string m_BundleRoot = "NBShaderBundleSample";
    [SerializeField] private string m_SampleSceneName = "NBShaderSamples";
    [SerializeField] private string m_SampleSceneBundleName = "sample_scene.ab";
    [SerializeField] private string m_LowShaderBundleName = "nbshader_low.ab";
    [SerializeField] private string m_MediumShaderBundleName = "nbshader_medium.ab";
    [SerializeField] private string m_HighShaderBundleName = "nbshader_high.ab";
    [SerializeField] private string m_UltraShaderBundleName = "nbshader_ultra.ab";

    private bool m_IsLoading;
    private string m_Status = "Select a tier.";
    private string m_Error;
    private NBShaderBundleSampleTier? m_RequestedTier;
    private NBShaderBundleSampleTier? m_LoadedTier;
    private AssetBundle m_CurrentSceneBundle;
    private AssetBundle m_CurrentShaderBundle;
    private ShaderVariantCollection m_CurrentVariantCollection;
    private NBShaderFeatureRuntimeSettings m_CurrentRuntimeSettings;
    private Scene m_EntryScene;
    private bool m_IsRegisteredForPointerBlock;
    private static int s_ActivePointerBlockCount;

    private void Awake()
    {
        m_EntryScene = gameObject.scene;
        ForceLandscapeOrientation();
    }

    private void OnEnable()
    {
        RegisterPointerBlock();
        ForceLandscapeOrientation();
    }

    private void OnDisable()
    {
        UnregisterPointerBlock();
    }

    private void OnDestroy()
    {
        UnregisterPointerBlock();
    }

    private static void ForceLandscapeOrientation()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void OnGUI()
    {
        var area = GetPanelRect();
        GUILayout.BeginArea(area, GUI.skin.box);
        GUILayout.Label("NBShader Bundle Sample", GUI.skin.label);
        GUILayout.Space(8f);

        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                DrawTierButton("Low", NBShaderBundleSampleTier.Low);
                DrawTierButton("Medium", NBShaderBundleSampleTier.Medium);
            }

            using (new GUILayout.HorizontalScope())
            {
                DrawTierButton("High", NBShaderBundleSampleTier.High);
                DrawTierButton("Ultra", NBShaderBundleSampleTier.Ultra);
            }
        }

        GUILayout.Space(12f);
        GUILayout.Label("Status: " + m_Status);
        if (m_RequestedTier.HasValue)
            GUILayout.Label("Requested tier: " + m_RequestedTier.Value);
        if (m_LoadedTier.HasValue)
            GUILayout.Label("Loaded tier: " + m_LoadedTier.Value);
        if (!string.IsNullOrEmpty(m_Error))
            GUILayout.Label("Error: " + m_Error);

        GUILayout.Space(12f);
        GUILayout.Label("Scene bundle: " + GetBundlePath(m_SampleSceneBundleName));
#if UNITY_EDITOR
        var editorBundleTargetWarning = GetEditorBundleTargetWarning();
        if (!string.IsNullOrEmpty(editorBundleTargetWarning))
            GUILayout.Label(editorBundleTargetWarning);
#endif
        GUILayout.EndArea();
    }

    private void DrawTierButton(string label, NBShaderBundleSampleTier tier)
    {
        using (new GuiDisabledScope(m_IsLoading))
        {
            if (GUILayout.Button(label, GUILayout.Height(32f)))
                StartCoroutine(SelectTierRoutine(tier));
        }
    }

    private IEnumerator SelectTierRoutine(NBShaderBundleSampleTier tier)
    {
        m_IsLoading = true;
        m_Error = null;
        m_RequestedTier = tier;
        m_Status = "Unloading previous content...";

        yield return UnloadCurrentContentRoutine();

        m_Status = "Loading shader bundle " + GetShaderBundleName(tier) + "...";
        yield return LoadBundleRoutine(GetShaderBundleName(tier), delegate(AssetBundle bundle)
        {
            m_CurrentShaderBundle = bundle;
        });
        if (HasError())
        {
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        if (!LoadTierAssets(tier))
        {
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        m_Status = "Loading scene bundle " + m_SampleSceneBundleName + "...";
        yield return LoadBundleRoutine(m_SampleSceneBundleName, delegate(AssetBundle bundle)
        {
            m_CurrentSceneBundle = bundle;
        });
        if (HasError())
        {
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        m_Status = "Loading scene " + m_SampleSceneName + "...";
        AsyncOperation loadSceneOperation = null;
        try
        {
            loadSceneOperation = SceneManager.LoadSceneAsync(m_SampleSceneName, LoadSceneMode.Additive);
        }
        catch (Exception exception)
        {
            SetExceptionError("Failed to start loading scene " + m_SampleSceneName, exception);
        }

        if (HasError())
        {
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        if (loadSceneOperation == null)
        {
            m_Error = "Failed to start loading scene: " + m_SampleSceneName;
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        yield return loadSceneOperation;

        var scene = SceneManager.GetSceneByName(m_SampleSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            m_Error = "Scene was not loaded from bundle: " + m_SampleSceneName;
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        ActivateSampleScene(scene);
        if (HasError())
        {
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        var materialCount = 0;
        try
        {
            materialCount = ApplyTierToScene(scene, tier);
        }
        catch (Exception exception)
        {
            SetExceptionError("Failed to apply NBShader tier " + tier, exception);
        }

        if (HasError())
        {
            yield return UnloadCurrentContentRoutine();
            FinishLoading();
            yield break;
        }

        m_LoadedTier = tier;
        m_Status = "Loaded " + tier + ". Applied tier to " + materialCount + " material(s).";
        FinishLoading();
    }

    private IEnumerator UnloadCurrentContentRoutine()
    {
        var loadedScene = SceneManager.GetSceneByName(m_SampleSceneName);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            RestoreEntrySceneActiveIfNeeded(loadedScene);
            var unloadSceneOperation = SceneManager.UnloadSceneAsync(loadedScene);
            if (unloadSceneOperation != null)
                yield return unloadSceneOperation;
        }

        if (m_CurrentSceneBundle != null)
        {
            m_CurrentSceneBundle.Unload(true);
            m_CurrentSceneBundle = null;
        }

        m_CurrentVariantCollection = null;
        m_CurrentRuntimeSettings = null;
        if (m_CurrentShaderBundle != null)
        {
            m_CurrentShaderBundle.Unload(true);
            m_CurrentShaderBundle = null;
        }

        m_LoadedTier = null;
        yield return Resources.UnloadUnusedAssets();
    }

    private void ActivateSampleScene(Scene scene)
    {
        if (!SceneManager.SetActiveScene(scene))
        {
            m_Error = "Failed to activate loaded scene: " + scene.name;
            return;
        }

        DynamicGI.UpdateEnvironment();
    }

    private void RestoreEntrySceneActiveIfNeeded(Scene sceneToUnload)
    {
        if (SceneManager.GetActiveScene() != sceneToUnload)
            return;

        var entryScene = m_EntryScene;
        if (!entryScene.IsValid() || !entryScene.isLoaded)
            entryScene = gameObject.scene;

        if (entryScene.IsValid() && entryScene.isLoaded)
            SceneManager.SetActiveScene(entryScene);
    }

    private IEnumerator LoadBundleRoutine(string bundleName, Action<AssetBundle> onLoaded)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            m_Error = "Bundle name is empty.";
            yield break;
        }

        var path = GetBundlePath(bundleName);
        if (ShouldUseUnityWebRequest(path))
        {
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(path))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    m_Error =
                        "Failed to load bundle " + bundleName +
                        " from " + path +
                        " (result: " + request.result +
                        ", response: " + request.responseCode +
                        "): " + request.error;
                    yield break;
                }

                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (bundle == null)
                {
                    m_Error = "Loaded bundle is null: " + bundleName;
                    yield break;
                }

                onLoaded(bundle);
                yield break;
            }
        }

        if (!File.Exists(path))
        {
            m_Error = "Bundle file is missing: " + path;
            yield break;
        }

        AssetBundleCreateRequest loadRequest;
        try
        {
            loadRequest = AssetBundle.LoadFromFileAsync(path);
        }
        catch (Exception exception)
        {
            SetExceptionError("Failed to start loading bundle " + bundleName + " from " + path, exception);
            yield break;
        }

        yield return loadRequest;
        if (loadRequest.assetBundle == null)
        {
            m_Error = "Failed to load bundle: " + path;
            yield break;
        }

        onLoaded(loadRequest.assetBundle);
    }

    private bool LoadTierAssets(NBShaderBundleSampleTier tier)
    {
        try
        {
            if (m_CurrentShaderBundle == null)
            {
                m_Error = "Shader bundle is not loaded.";
                return false;
            }

            m_CurrentVariantCollection = LoadFirstAsset<ShaderVariantCollection>(m_CurrentShaderBundle);
            if (m_CurrentVariantCollection == null)
            {
                m_Error = "ShaderVariantCollection is missing from shader bundle.";
                return false;
            }

            m_Status = "Warming up shader variants for " + tier + "...";
            m_CurrentVariantCollection.WarmUp();

            m_CurrentRuntimeSettings = LoadFirstAsset<NBShaderFeatureRuntimeSettings>(m_CurrentShaderBundle);
            if (m_CurrentRuntimeSettings == null)
            {
                m_Error = "Runtime settings asset is missing from shader bundle.";
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            SetExceptionError("Failed to load shader tier assets for " + tier, exception);
            return false;
        }
    }

    private static T LoadFirstAsset<T>(AssetBundle bundle) where T : UnityEngine.Object
    {
        if (bundle == null)
            return null;

        var assets = bundle.LoadAllAssets<T>();
        return assets != null && assets.Length > 0 ? assets[0] : null;
    }

    private int ApplyTierToScene(Scene scene, NBShaderBundleSampleTier tier)
    {
        var materials = CollectSceneMaterials(scene);
        NBShaderFeatureRuntime.ApplyTier(materials, m_CurrentRuntimeSettings, ToRuntimeTier(tier));
        return materials.Count;
    }

    private static List<Material> CollectSceneMaterials(Scene scene)
    {
        var result = new List<Material>();
        var unique = new HashSet<Material>();
        if (!scene.IsValid() || !scene.isLoaded)
            return result;

        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            var renderers = roots[i].GetComponentsInChildren<Renderer>(true);
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var sharedMaterials = renderers[rendererIndex].sharedMaterials;
                for (var materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                {
                    var material = sharedMaterials[materialIndex];
                    if (material != null && unique.Add(material))
                        result.Add(material);
                }
            }
        }

        return result;
    }

    private static NBShaderFeatureTier ToRuntimeTier(NBShaderBundleSampleTier tier)
    {
        switch (tier)
        {
            case NBShaderBundleSampleTier.Low:
                return NBShaderFeatureTier.Low;
            case NBShaderBundleSampleTier.Medium:
                return NBShaderFeatureTier.Medium;
            case NBShaderBundleSampleTier.High:
                return NBShaderFeatureTier.High;
            case NBShaderBundleSampleTier.Ultra:
                return NBShaderFeatureTier.Ultra;
            default:
                return NBShaderFeatureTier.Ultra;
        }
    }

    private static bool ShouldUseUnityWebRequest(string path)
    {
        return !string.IsNullOrEmpty(path) && path.Contains("://");
    }

    private bool HasError()
    {
        return !string.IsNullOrEmpty(m_Error);
    }

    private void SetExceptionError(string context, Exception exception)
    {
        m_Error = context + ": " + exception.GetType().Name + " - " + exception.Message;
    }

    private void FinishLoading()
    {
        if (HasError())
            m_Status = "Failed.";
        m_IsLoading = false;
    }

    public string GetShaderBundleName(NBShaderBundleSampleTier tier)
    {
        switch (tier)
        {
            case NBShaderBundleSampleTier.Low:
                return m_LowShaderBundleName;
            case NBShaderBundleSampleTier.Medium:
                return m_MediumShaderBundleName;
            case NBShaderBundleSampleTier.High:
                return m_HighShaderBundleName;
            case NBShaderBundleSampleTier.Ultra:
                return m_UltraShaderBundleName;
            default:
                return string.Empty;
        }
    }

    public string GetBundlePath(string bundleName)
    {
        return Path.Combine(Application.streamingAssetsPath, m_BundleRoot, bundleName).Replace('\\', '/');
    }

#if UNITY_EDITOR
    private static string GetEditorBundleTargetWarning()
    {
        var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        if (buildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
            buildTarget == UnityEditor.BuildTarget.StandaloneWindows64 ||
            buildTarget == UnityEditor.BuildTarget.StandaloneOSX ||
            buildTarget == UnityEditor.BuildTarget.StandaloneLinux64)
        {
            return null;
        }

        return "Editor preview reads Windows64 AssetBundles from StreamingAssets. Active Build Target is " +
               buildTarget +
               "; use Build PC AssetBundles before Play if another platform build changed StreamingAssets.";
    }
#endif

    public string sampleSceneName { get { return m_SampleSceneName; } }
    public string sampleSceneBundleName { get { return m_SampleSceneBundleName; } }

    private readonly struct GuiDisabledScope : System.IDisposable
    {
        private readonly bool m_PreviousEnabled;

        public GuiDisabledScope(bool disabled)
        {
            m_PreviousEnabled = GUI.enabled;
            GUI.enabled = m_PreviousEnabled && !disabled;
        }

        public void Dispose()
        {
            GUI.enabled = m_PreviousEnabled;
        }
    }

    public static bool IsPointerOverVisiblePanel(Vector2 screenPosition)
    {
        if (s_ActivePointerBlockCount <= 0)
            return false;

        var guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        return GetPanelRect().Contains(guiPosition);
    }

    private static Rect GetPanelRect()
    {
        var margin = Mathf.Min(24f, Mathf.Max(8f, Screen.width * 0.04f));
        var width = Mathf.Min(420f, Mathf.Max(240f, Screen.width - margin * 2f));
        return new Rect(margin, margin, width, Screen.height - margin * 2f);
    }

    private void RegisterPointerBlock()
    {
        if (m_IsRegisteredForPointerBlock)
            return;

        m_IsRegisteredForPointerBlock = true;
        s_ActivePointerBlockCount++;
    }

    private void UnregisterPointerBlock()
    {
        if (!m_IsRegisteredForPointerBlock)
            return;

        m_IsRegisteredForPointerBlock = false;
        s_ActivePointerBlockCount = Mathf.Max(0, s_ActivePointerBlockCount - 1);
    }
}

public enum NBShaderBundleSampleTier
{
    Low = 0,
    Medium = 1,
    High = 2,
    Ultra = 3
}
