using System;
using System.Collections.Generic;
using System.IO;
using NBShader;
using NBShaders2.Editor.FeatureLevel;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class NBShaderBundleSampleBuilder
{
    private const string SampleMaterialSearchRoot = "Assets/NBShaderSamples";
    private const string EntryScenePath = "Assets/NBShaderBundleSample/Scenes/NBShaderBundleSampleEntry.unity";
    private const string SampleScenePath = "Assets/NBShaderSamples/NBShaderSamples.unity";
    private const string NBShaderPath = "Packages/com.xuanxuan.nb.fx/NBShaders2/Shader/NBShader.shader";

    private const string GeneratedRoot = "Assets/NBShaderBundleSample/Generated";
    private const string GeneratedSvcFolder = GeneratedRoot + "/ShaderVariantCollections";
    private const string GeneratedRuntimeSettingsFolder = GeneratedRoot + "/RuntimeSettings";
    private const string BundleBuildRoot = GeneratedRoot + "/BundleBuild";
    private const string BundleStagingOutputFolder = GeneratedRoot + "/BundleOutputStaging";
    private const string BundleOutputBackupFolder = GeneratedRoot + "/BundleOutputBackup";
    private const string BundleOutputFolder = "Assets/StreamingAssets/NBShaderBundleSample";
    private const string ApkOutputFolder = "Builds/NBShaderBundleSample";
    private const string ApkOutputPath = ApkOutputFolder + "/NBShaderBundleSample.apk";

    private const string SampleSceneBundleName = "sample_scene.ab";
    // Scene dependencies are authored against this stable bundle name; final tier files are renamed copies.
    private const string InternalShaderDependencyBundleName = "nbshader.ab";

    private static readonly TierBuildSpec[] TierBuilds =
    {
        new TierBuildSpec(NBShaderFeatureTier.Low, "nbshader_low.ab"),
        new TierBuildSpec(NBShaderFeatureTier.Medium, "nbshader_medium.ab"),
        new TierBuildSpec(NBShaderFeatureTier.High, "nbshader_high.ab"),
        new TierBuildSpec(NBShaderFeatureTier.Ultra, "nbshader_ultra.ab")
    };

    [MenuItem("Tools/NBShader/Bundle Sample/Build AssetBundles")]
    public static void BuildAssetBundles()
    {
        BuildAssetBundles(true);
    }

    private static void BuildAssetBundles(bool strictContentValidation)
    {
        EnsureRequiredAsset(EntryScenePath);
        EnsureRequiredAsset(SampleScenePath);
        EnsureRequiredAssetOfType<Shader>(NBShaderPath);
        EnsureAssetFolder(GeneratedSvcFolder);
        EnsureAssetFolder(GeneratedRuntimeSettingsFolder);
        RecreateAssetFolder(BundleBuildRoot);
        RecreateAssetFolder(BundleStagingOutputFolder);
        DeleteControlledAssetFolder(BundleOutputBackupFolder);

        try
        {
            var svcResult = NBShaderVariantCollectionBuilder.Generate(
                new[] { SampleMaterialSearchRoot },
                GeneratedSvcFolder);
            if (!svcResult.succeeded)
                throw new BuildFailedException("NBShader SVC generation failed: " + svcResult.firstErrorMessage);

            var runtimeSettingsByTier = CreateRuntimeSettingsAssets();
            BuildTierBundleSets(runtimeSettingsByTier, BundleStagingOutputFolder);
            AssetDatabase.Refresh();
            ValidateBundleOutputs(BundleStagingOutputFolder);
            ValidateBundleContents(BundleStagingOutputFolder, strictContentValidation);
            CommitBundleOutputs();
            ValidateBundleOutputs(BundleOutputFolder);
        }
        finally
        {
            if (AssetDatabase.IsValidFolder(BundleBuildRoot))
                AssetDatabase.DeleteAsset(BundleBuildRoot);
            if (AssetDatabase.IsValidFolder(BundleStagingOutputFolder))
                AssetDatabase.DeleteAsset(BundleStagingOutputFolder);
        }

        AssetDatabase.Refresh();
        Debug.Log("NBShader bundle sample AssetBundles built to " + BundleOutputFolder);
    }

    [MenuItem("Tools/NBShader/Bundle Sample/Validate Build Outputs")]
    public static void ValidateBuildOutputs()
    {
        ValidateEntrySceneBuildConfiguration(true);
        ValidatePublishedBundleOutputs(true);
        Debug.Log("NBShader bundle sample build outputs are valid.");
    }

    [MenuItem("Tools/NBShader/Bundle Sample/Build Android APK")]
    public static void BuildAndroidApk()
    {
        if (!EnsureAndroidBuildTarget())
            throw new BuildFailedException("Failed to switch active build target to Android.");

        BuildAssetBundles(false);
        ValidateEntrySceneBuildConfiguration(false);

        var outputPath = GetProjectAbsolutePath(ApkOutputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { EntryScenePath },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.Development | BuildOptions.AllowDebugging
        });

        if (report == null || report.summary.result != BuildResult.Succeeded)
        {
            var result = report != null ? report.summary.result.ToString() : "No report";
            throw new BuildFailedException("NBShader bundle sample Android APK build failed: " + result);
        }

        Debug.Log("NBShader bundle sample Android development APK built to " + ApkOutputPath);
    }

    private static Dictionary<NBShaderFeatureTier, string> CreateRuntimeSettingsAssets()
    {
        var result = new Dictionary<NBShaderFeatureTier, string>();
        for (var i = 0; i < TierBuilds.Length; i++)
        {
            var tier = TierBuilds[i].tier;
            var path = GetRuntimeSettingsPath(tier);
            var asset = LoadOrCreateRuntimeSettingsAsset(path);
            if (!NBShaderFeatureLevelEditorAPI.WriteRuntimeSettingsAssetNoSave(asset))
                throw new BuildFailedException("Failed to write NBShader runtime settings asset: " + path);

            result[tier] = path;
        }

        return result;
    }

    private static string GetRuntimeSettingsPath(NBShaderFeatureTier tier)
    {
        return GeneratedRuntimeSettingsFolder + "/NBShaderRuntimeSettings_" + tier + ".asset";
    }

    private static NBShaderFeatureRuntimeSettings LoadOrCreateRuntimeSettingsAsset(string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (existing == null)
        {
            var asset = ScriptableObject.CreateInstance<NBShaderFeatureRuntimeSettings>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        var settings = existing as NBShaderFeatureRuntimeSettings;
        if (settings == null)
            throw new BuildFailedException("Runtime settings output path is occupied by another asset type: " + path);

        return settings;
    }

    private static void BuildTierBundleSets(Dictionary<NBShaderFeatureTier, string> runtimeSettingsByTier, string outputFolder)
    {
        var copiedSceneBundle = false;
        for (var i = 0; i < TierBuilds.Length; i++)
        {
            var spec = TierBuilds[i];
            var svcPath = NBShaderVariantCollectionBuilder.GetOutputPath(GeneratedSvcFolder, spec.tier);
            EnsureRequiredAssetOfType<ShaderVariantCollection>(svcPath);

            string runtimeSettingsPath;
            if (!runtimeSettingsByTier.TryGetValue(spec.tier, out runtimeSettingsPath))
                throw new BuildFailedException("Missing runtime settings asset for tier: " + spec.tier);
            EnsureRequiredAssetOfType<NBShaderFeatureRuntimeSettings>(runtimeSettingsPath);

            using (NBShaderFeatureLevelEditorAPI.OverrideBuildStripExplicitTier(spec.tier))
            {
                var tierOutputFolder = BundleBuildRoot + "/" + spec.tier;
                var manifest = BuildBundles(
                    tierOutputFolder,
                    new[]
                    {
                        new AssetBundleBuild
                        {
                            assetBundleName = SampleSceneBundleName,
                            assetNames = new[] { SampleScenePath }
                        },
                        new AssetBundleBuild
                        {
                            assetBundleName = InternalShaderDependencyBundleName,
                            assetNames = new[] { NBShaderPath, svcPath, runtimeSettingsPath }
                        }
                    },
                    spec.bundleName);
                ValidateTierBuildManifest(manifest, spec.bundleName);

                // The scene bundle is built with each tier so Unity records the stable internal shader dependency name.
                if (!copiedSceneBundle)
                {
                    CopyGeneratedFile(tierOutputFolder + "/" + SampleSceneBundleName, outputFolder + "/" + SampleSceneBundleName);
                    copiedSceneBundle = true;
                }

                CopyGeneratedFile(tierOutputFolder + "/" + InternalShaderDependencyBundleName, outputFolder + "/" + spec.bundleName);
            }
        }
    }

    private static AssetBundleManifest BuildBundles(string outputFolder, AssetBundleBuild[] builds, string label)
    {
        EnsureAssetFolder(outputFolder);
        var manifest = BuildPipeline.BuildAssetBundles(
            outputFolder,
            builds,
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
            throw new BuildFailedException("Failed to build NBShader bundle sample " + label + ".");

        return manifest;
    }

    private static void ValidateTierBuildManifest(AssetBundleManifest manifest, string label)
    {
        if (manifest == null)
            throw new BuildFailedException("Missing AssetBundle manifest for NBShader bundle sample " + label + ".");

        var bundles = manifest.GetAllAssetBundles();
        if (!Contains(bundles, SampleSceneBundleName))
            throw new BuildFailedException("Sample scene bundle is missing from build manifest for " + label + ".");
        if (!Contains(bundles, InternalShaderDependencyBundleName))
            throw new BuildFailedException("NBShader dependency bundle is missing from build manifest for " + label + ".");

        var sceneDependencies = manifest.GetDirectDependencies(SampleSceneBundleName);
        if (!Contains(sceneDependencies, InternalShaderDependencyBundleName))
        {
            throw new BuildFailedException(
                "Sample scene bundle does not depend on the expected NBShader dependency bundle for " + label + ".");
        }

        var shaderDependencies = manifest.GetDirectDependencies(InternalShaderDependencyBundleName);
        if (Contains(shaderDependencies, SampleSceneBundleName))
        {
            throw new BuildFailedException(
                "NBShader dependency bundle unexpectedly depends on the sample scene bundle for " + label + ".");
        }
    }

    private static bool Contains(string[] values, string target)
    {
        if (values == null)
            return false;

        for (var i = 0; i < values.Length; i++)
        {
            if (string.Equals(values[i], target, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static void CopyGeneratedFile(string sourceAssetPath, string targetAssetPath)
    {
        var sourcePath = GetProjectAbsolutePath(sourceAssetPath);
        var targetPath = GetProjectAbsolutePath(targetAssetPath);
        if (!File.Exists(sourcePath))
            throw new BuildFailedException("Expected bundle output is missing: " + sourceAssetPath);

        File.Copy(sourcePath, targetPath, true);
    }

    private static void ValidateBundleOutputs(string outputFolder)
    {
        EnsureGeneratedFile(outputFolder + "/" + SampleSceneBundleName);
        for (var i = 0; i < TierBuilds.Length; i++)
            EnsureGeneratedFile(outputFolder + "/" + TierBuilds[i].bundleName);
    }

    private static void ValidatePublishedBundleOutputs(bool strictContentValidation)
    {
        ValidateBundleOutputs(BundleOutputFolder);
        ValidateBundleContents(BundleOutputFolder, strictContentValidation);
    }

    private static void ValidateBundleContents(string outputFolder, bool strict)
    {
        var sampleBundle = LoadBundleForValidation(outputFolder, SampleSceneBundleName, strict);
        if (sampleBundle == null)
            return;

        try
        {
            ValidateSampleSceneBundleContent(sampleBundle);
        }
        finally
        {
            sampleBundle.Unload(false);
        }

        for (var i = 0; i < TierBuilds.Length; i++)
        {
            var spec = TierBuilds[i];
            var shaderBundle = LoadBundleForValidation(outputFolder, spec.bundleName, strict);
            if (shaderBundle == null)
                return;

            try
            {
                ValidateTierShaderBundleContent(shaderBundle, spec);
            }
            finally
            {
                shaderBundle.Unload(false);
            }
        }
    }

    private static AssetBundle LoadBundleForValidation(string outputFolder, string bundleName, bool strict)
    {
        var path = GetProjectAbsolutePath(outputFolder + "/" + bundleName);
        try
        {
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle != null)
                return bundle;
        }
        catch (Exception exception)
        {
            HandleSkippedBundleContentValidation(bundleName, exception.Message, strict);
            return null;
        }

        HandleSkippedBundleContentValidation(
            bundleName,
            "The bundle could not be loaded in the current Editor target.",
            strict);
        return null;
    }

    private static void HandleSkippedBundleContentValidation(string bundleName, string reason, bool strict)
    {
        var message = "Skipped NBShader bundle content validation for " + bundleName + ": " + reason;
        if (strict)
            throw new BuildFailedException(message);

        Debug.LogWarning(message);
    }

    private static void ValidateSampleSceneBundleContent(AssetBundle bundle)
    {
        var scenePaths = bundle.GetAllScenePaths();
        if (scenePaths == null || scenePaths.Length != 1 || !ContainsPath(scenePaths, SampleScenePath))
            throw new BuildFailedException("Sample scene bundle must contain only " + SampleScenePath + ".");

        var assetNames = bundle.GetAllAssetNames();
        if (ContainsPath(assetNames, NBShaderPath))
            throw new BuildFailedException("Sample scene bundle must not contain NBShader.shader.");
        if (ContainsAnySuffix(assetNames, ".shadervariants"))
            throw new BuildFailedException("Sample scene bundle must not contain ShaderVariantCollection assets.");
        for (var i = 0; i < TierBuilds.Length; i++)
        {
            var runtimeSettingsPath = GetRuntimeSettingsPath(TierBuilds[i].tier);
            if (ContainsPath(assetNames, runtimeSettingsPath))
                throw new BuildFailedException("Sample scene bundle must not contain runtime settings asset " + runtimeSettingsPath + ".");
        }
    }

    private static void ValidateTierShaderBundleContent(AssetBundle bundle, TierBuildSpec spec)
    {
        var scenePaths = bundle.GetAllScenePaths();
        if (scenePaths != null && scenePaths.Length > 0)
            throw new BuildFailedException("NBShader tier bundle must not contain scenes: " + spec.bundleName);

        var assetNames = bundle.GetAllAssetNames();
        var svcPath = NBShaderVariantCollectionBuilder.GetOutputPath(GeneratedSvcFolder, spec.tier);
        var runtimeSettingsPath = GetRuntimeSettingsPath(spec.tier);
        if (!ContainsPath(assetNames, NBShaderPath))
            throw new BuildFailedException("NBShader tier bundle is missing NBShader.shader: " + spec.bundleName);
        if (!ContainsPath(assetNames, svcPath))
            throw new BuildFailedException("NBShader tier bundle is missing SVC asset " + svcPath + ": " + spec.bundleName);
        if (!ContainsPath(assetNames, runtimeSettingsPath))
            throw new BuildFailedException("NBShader tier bundle is missing runtime settings asset " + runtimeSettingsPath + ": " + spec.bundleName);
    }

    private static bool ContainsPath(string[] values, string target)
    {
        if (values == null || string.IsNullOrEmpty(target))
            return false;

        var normalizedTarget = NormalizeAssetPath(target);
        for (var i = 0; i < values.Length; i++)
        {
            if (string.Equals(NormalizeAssetPath(values[i]), normalizedTarget, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ContainsAnySuffix(string[] values, string suffix)
    {
        if (values == null || string.IsNullOrEmpty(suffix))
            return false;

        for (var i = 0; i < values.Length; i++)
        {
            var value = NormalizeAssetPath(values[i]);
            if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string NormalizeAssetPath(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
    }

    private static void EnsureGeneratedFile(string assetPath)
    {
        if (!File.Exists(GetProjectAbsolutePath(assetPath)))
            throw new BuildFailedException("Expected bundle output is missing: " + assetPath);
    }

    private static void CommitBundleOutputs()
    {
        EnsureAssetFolder(GetParentAssetFolder(BundleOutputFolder));

        var backupCreated = false;
        if (AssetDatabase.IsValidFolder(BundleOutputBackupFolder))
            AssetDatabase.DeleteAsset(BundleOutputBackupFolder);

        if (AssetDatabase.IsValidFolder(BundleOutputFolder))
        {
            var backupError = AssetDatabase.MoveAsset(BundleOutputFolder, BundleOutputBackupFolder);
            if (!string.IsNullOrEmpty(backupError))
                throw new BuildFailedException("Failed to back up existing NBShader bundle sample output: " + backupError);
            backupCreated = true;
        }

        var moveError = AssetDatabase.MoveAsset(BundleStagingOutputFolder, BundleOutputFolder);
        if (string.IsNullOrEmpty(moveError))
        {
            if (backupCreated)
                AssetDatabase.DeleteAsset(BundleOutputBackupFolder);
            return;
        }

        if (backupCreated && AssetDatabase.IsValidFolder(BundleOutputBackupFolder) && !AssetDatabase.IsValidFolder(BundleOutputFolder))
        {
            var restoreError = AssetDatabase.MoveAsset(BundleOutputBackupFolder, BundleOutputFolder);
            if (!string.IsNullOrEmpty(restoreError))
            {
                throw new BuildFailedException(
                    "Failed to publish NBShader bundle sample output: " + moveError +
                    "\nFailed to restore previous output from backup: " + restoreError +
                    "\nPrevious output remains at " + BundleOutputBackupFolder + ".");
            }
        }

        throw new BuildFailedException("Failed to publish NBShader bundle sample output: " + moveError);
    }

    private static string GetProjectAbsolutePath(string assetPath)
    {
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, assetPath).Replace('\\', '/');
    }

    private static void EnsureRequiredAsset(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null)
            throw new BuildFailedException("Required asset is missing: " + path);
    }

    private static void EnsureRequiredAssetOfType<T>(string path) where T : UnityEngine.Object
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
            throw new BuildFailedException("Required " + typeof(T).Name + " asset is missing: " + path);
    }

    private static void ValidateEntrySceneBuildConfiguration(bool strict)
    {
        var enabledSceneCount = 0;
        var entrySceneEnabled = false;
        var scenes = EditorBuildSettings.scenes;
        for (var i = 0; i < scenes.Length; i++)
        {
            var scene = scenes[i];
            if (scene == null || !scene.enabled)
                continue;

            enabledSceneCount++;
            if (string.Equals(scene.path, EntryScenePath, StringComparison.Ordinal))
                entrySceneEnabled = true;
        }

        if (enabledSceneCount == 1 && entrySceneEnabled)
            return;

        var message =
            "Global Build Settings do not contain only the NBShader bundle sample entry scene. " +
            "The Android APK builder uses an explicit scene list and does not modify ProjectSettings.";
        if (strict)
            throw new BuildFailedException(message);

        Debug.LogWarning(message);
    }

    private static bool EnsureAndroidBuildTarget()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            return true;

        return EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        if (string.IsNullOrEmpty(folder) || !folder.StartsWith("Assets/", StringComparison.Ordinal))
            throw new BuildFailedException("Output folder must be under Assets: " + folder);

        var parts = folder.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void RecreateAssetFolder(string folder)
    {
        if (!folder.StartsWith("Assets/", StringComparison.Ordinal))
            throw new BuildFailedException("Output folder must be under Assets: " + folder);

        DeleteControlledAssetFolder(folder);

        EnsureAssetFolder(folder);
    }

    private static void DeleteControlledAssetFolder(string folder)
    {
        if (!folder.StartsWith(GeneratedRoot + "/", StringComparison.Ordinal))
            throw new BuildFailedException("Only generated sample folders can be deleted: " + folder);

        if (AssetDatabase.IsValidFolder(folder))
            AssetDatabase.DeleteAsset(folder);
    }

    private static string GetParentAssetFolder(string folder)
    {
        var index = folder.LastIndexOf('/');
        if (index <= 0)
            throw new BuildFailedException("Invalid asset folder path: " + folder);

        return folder.Substring(0, index);
    }

    private readonly struct TierBuildSpec
    {
        public readonly NBShaderFeatureTier tier;
        public readonly string bundleName;

        public TierBuildSpec(NBShaderFeatureTier tier, string bundleName)
        {
            this.tier = tier;
            this.bundleName = bundleName;
        }
    }
}
