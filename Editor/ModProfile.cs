using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "New ModProfile", menuName = "Data/Mod Profile", order = 36)]
public class ModProfile : ScriptableObject {
    [SerializeField]
    private string title;
    
    [SerializeField,Multiline]
    private string description;
    
    [SerializeField]
    private Sprite icon;
    
	[SerializeField] private SteamWorkshopItemTag tags;
    
    [System.Serializable]
    private class LevelReference : AssetReferenceT<Level> {
        public LevelReference(string guid) : base(guid) { }
    }

    [System.Serializable]
    private class CharacterReplacement {
        [SerializeField] private CivilianReference existingCharacter;
        [SerializeField] private CivilianReference replacementCharacter;
        public CivilianReference GetReplacementCharacter() => replacementCharacter;
    }
    
    [SerializeField] private List<LevelReference> levels;
    [SerializeField] private List<CharacterReplacement> replacementCharacters;
    
    [Flags]
    private enum SteamWorkshopItemTag {
        Characters = 1,
        Maps = 2,
    }
    
    [ContextMenu("Build")]
    private void Build() {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.DefaultGroup;
        // Clear the group
        Undo.RecordObject(group, "Set default group to modded items.");
        var entries = new List<AddressableAssetEntry>(group.entries);
        foreach (var entry in entries) {
            group.RemoveAssetEntry(entry);
        }
        // Add our stuff to the group
        foreach (var level in levels) {
            settings.CreateOrMoveEntry(level.AssetGUID, group, false, false);
        }

        PlayerSettings.companyName = "ArchivalEugeneNaelstrof";
        PlayerSettings.productName = "ChurnVector";

        var churnVectorCharacters = settings.FindGroup("ChurnVectorCharacters");
        var characterEntries = churnVectorCharacters.entries;
        foreach (var replacement in replacementCharacters) {
            var replacementCharacterID = replacement.GetReplacementCharacter().AssetGUID;
            var found = characterEntries.FirstOrDefault((entry) => entry.guid == replacementCharacterID);
            // Double check that we aren't replacing assets with existing assets before adding it to our bundle.
            if (found == null) {
                settings.CreateOrMoveEntry(replacementCharacterID, group, false, false);
            }
        }
        var defaultBuildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";
        var defaultLoadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]";
        
        settings.profileSettings.SetValue(settings.activeProfileId, "Local.BuildPath", defaultBuildPath);
        settings.profileSettings.SetValue(settings.activeProfileId, "Local.LoadPath", defaultLoadPath);
        
        var modBuildPath = $"[UnityEngine.Application.persistentDataPath]/mods/{name}/[BuildTarget]";
        var modLoadPath = $"{Modding.uniquePathReplacementID}/[BuildTarget]";

        var buildPathInfo = settings.profileSettings.GetProfileDataByName("ChurnVectorMod.BuildPath");
        if (buildPathInfo == null) {
            settings.profileSettings.CreateValue("ChurnVectorMod.BuildPath", modBuildPath);
            settings.profileSettings.CreateValue("ChurnVectorMod.LoadPath", modLoadPath);
            settings.profileSettings.SetValue(settings.activeProfileId, "ChurnVectorMod.BuildPath", modBuildPath);
            settings.profileSettings.SetValue(settings.activeProfileId, "ChurnVectorMod.LoadPath", modLoadPath);
        } else {
            settings.profileSettings.SetValue(settings.activeProfileId, "ChurnVectorMod.BuildPath", modBuildPath);
            settings.profileSettings.SetValue(settings.activeProfileId, "ChurnVectorMod.LoadPath", modLoadPath);
        }

        ExternalCatalogSetup externalCatalog = ScriptableObject.CreateInstance<ExternalCatalogSetup>();
        externalCatalog.AssetGroups = new List<AddressableAssetGroup> { group };
        buildPathInfo = settings.profileSettings.GetProfileDataByName("ChurnVectorMod.BuildPath");
        externalCatalog.BuildPath.SetVariableById(settings, buildPathInfo.Id);
        var runtimeLoadPathInfo = settings.profileSettings.GetProfileDataByName("ChurnVectorMod.LoadPath");
        externalCatalog.RuntimeLoadPath.SetVariableById(settings, runtimeLoadPathInfo.Id);
        externalCatalog.CatalogName = name;
        AssetDatabase.CreateAsset(externalCatalog, Path.Combine(settings.DataBuilderFolder, "ExternalCatalog.asset"));

        var schema = group.GetSchema<BundledAssetGroupSchema>();
        schema.BuildPath.SetVariableById(settings, buildPathInfo.Id);
        schema.LoadPath.SetVariableById(settings, runtimeLoadPathInfo.Id);
        
        BuildScriptPackedMultiCatalogMode multicatalog = (BuildScriptPackedMultiCatalogMode)settings.DataBuilders.FirstOrDefault((builder) => builder is BuildScriptPackedMultiCatalogMode);
        if (multicatalog == null) {
            multicatalog = CreateInstance<BuildScriptPackedMultiCatalogMode>();
            multicatalog.ExternalCatalogs = new List<ExternalCatalogSetup> { externalCatalog };
            AssetDatabase.CreateAsset(multicatalog, Path.Combine(settings.DataBuilderFolder, "BuildScriptPackedMultiCatalog.asset"));
            Undo.RecordObject(settings, "Added builder to settings.");
            settings.DataBuilders.Add(multicatalog);
        }
        Undo.RecordObject(multicatalog, "Set externalCatalogs");
        multicatalog.ExternalCatalogs = new List<ExternalCatalogSetup> { externalCatalog };
        Undo.RecordObject(settings, "Set active build index");
        settings.ActivePlayerDataBuilderIndex = settings.DataBuilders.IndexOf(multicatalog);

        BuildForPlatform(BuildTarget.StandaloneWindows64);
        //BuildForPlatform(BuildTarget.StandaloneLinux64);
        //BuildForPlatform(BuildTarget.StandaloneOSX);

        JSONNode infoJson = JSONNode.Parse("{}");
        infoJson["title"] = title;
        infoJson["description"] = description;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out string iconGUID, out long localid);
        infoJson["spriteGUID"] = iconGUID;
        var arrayNode = new JSONArray();
        foreach (SteamWorkshopItemTag tag in (SteamWorkshopItemTag[])Enum.GetValues(typeof(SteamWorkshopItemTag))) {
            if ((tag & tags) != 0) {
                arrayNode.Add(tag.ToString());
            }
        }
        infoJson["tags"] = arrayNode;
        StreamWriter infoWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, "mods", name, "info.json"));
        infoWriter.Write(infoJson.ToString(2));
        infoWriter.Close();
        WriteOutSpriteToPath(Path.Combine(Application.persistentDataPath, "mods", name, "preview.png"), icon);
    }

    private void WriteOutSpriteToPath(string path, Sprite sprite) {
        // Write out the sprite to a render texture, so that we can ignore its "readOnly" state.
        var rect = sprite.textureRect;
        RenderTexture targetTexture = new RenderTexture((int)rect.width, (int)rect.height, 0);
        CommandBuffer buffer = new CommandBuffer();
        buffer.Blit(sprite.texture, targetTexture, Vector2.one, sprite.textureRectOffset);
        Graphics.ExecuteCommandBuffer(buffer);

        // Then read-back the render texture to a regular texture2D.
        Texture2D previewTextureWrite = new Texture2D((int)rect.width, (int)rect.height);
        RenderTexture oldTex = RenderTexture.active;
        RenderTexture.active = targetTexture;
        previewTextureWrite.ReadPixels(new Rect(0, 0, rect.width, rect.height), 0, 0);
        File.WriteAllBytes(path, previewTextureWrite.EncodeToPNG());
        RenderTexture.active = oldTex;
    }

    private void BuildForPlatform(BuildTarget target) {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, target);
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        if (result == null) {
            throw new Exception("Something went very wrong!");
        }

        if (!string.IsNullOrEmpty(result.Error)) {
            throw new UnityException(result.Error);
        }
    }
}
