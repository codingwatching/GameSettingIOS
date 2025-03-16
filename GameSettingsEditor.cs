#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Build;

public class GameSettingsEditor : EditorWindow
{
    private Texture2D gameIcon;
    private string companyName = "";
    private string gameName = "";
    private string gameDescription = "";
    private bool isLandscape = true;
    private int maxTextureSize;
    private readonly int[] textureSizes = { 2048, 1024, 512, 256, 128, 64 };
    private string[] textureSizeLabels;
    private int selectedTextureSizeIndex = 0;
    private bool isCreateFolder = false;

    private OverrideTextureCompression textureCompression = OverrideTextureCompression.ForceFastCompressor;
    private ManagedStrippingLevel managedStrippingLevel = ManagedStrippingLevel.High;

    private const string infoGamePath = "Assets/InfoGame";
    private const string desInfoGamePath = infoGamePath + "/Description.txt";
    private const string privacyInfoPath = infoGamePath + "/PrivacyInfo.xcprivacy";

    private const string manifestPath = "Packages/manifest.json";


    [MenuItem("Tools/Game Settings")]
    public static void ShowWindow()
    {
        GetWindow<GameSettingsEditor>("Game Settings");
    }

    private void OnEnable()
    {
        // Initialize labels for the dropdown
        textureSizeLabels = new string[textureSizes.Length];
        for (int i = 0; i < textureSizes.Length; i++)
        {
            textureSizeLabels[i] = textureSizes[i].ToString();
        }
    }


    private void OnGUI()
    {
        GUILayout.Label("Game Settings", EditorStyles.boldLabel);
        if(GUILayout.Button("Create Folder And File"))
        {
            CreateFolderAndFile();
        }
        
        if(GUILayout.Button("Get Info"))
        {
           GetInfoGame();
        }
        
        GUILayout.Space(10);
        // Icon selection
        GUILayout.Label("Game Icon:");
        gameIcon = (Texture2D)EditorGUILayout.ObjectField(gameIcon, typeof(Texture2D), false);

        /*if (gameIcon != null)
        {
            GUILayout.Label("Preview:");
            GUILayout.Box(gameIcon, GUILayout.Width(100), GUILayout.Height(100));
        }*/

        // Company Name input
        GUILayout.Label("Company Name:");
        companyName = EditorGUILayout.TextField(companyName);

        // Game Name input
        GUILayout.Label("Game Name:");
        gameName = EditorGUILayout.TextField(gameName);

        string packageName = $"com.{companyName}.{gameName}".ToLower();
        GUILayout.Label("Package Name: " + packageName);

        GUILayout.Space(10);
        // Game Description input
        GUILayout.Label("Game Description:");
        gameDescription = EditorGUILayout.TextArea(gameDescription, GUILayout.Height(25));

        GUILayout.Space(10);
        // Orientation selection
        isLandscape = GUILayout.Toggle(isLandscape, "Landscape Mode");

        // Managed Stripping Level
        managedStrippingLevel =
            (ManagedStrippingLevel)EditorGUILayout.EnumPopup("Managed Stripping Level", managedStrippingLevel);

        // Texture settings
        selectedTextureSizeIndex =
            EditorGUILayout.Popup("Max Texture Size", selectedTextureSizeIndex, textureSizeLabels);
        maxTextureSize = textureSizes[selectedTextureSizeIndex]; // Get selected texture size
        textureCompression =
            (OverrideTextureCompression)EditorGUILayout.EnumPopup("Texture Compression", textureCompression);


        GUILayout.Space(20);
        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        if (GUILayout.Button("Apply Settings"))
        {
            ApplySettings(packageName);
            SaveGameInfo();
        }

        if (GUILayout.Button("Open manifest.json"))
        {
            OpenManifestFile();
        }

        GUILayout.Space(10);
        GUILayout.Label("Build iOS Settings", EditorStyles.boldLabel);
        if (GUILayout.Button("Build iOS"))
        {
            BuildIOS();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Move InfoGame to Build Folder"))
        {
            MoveInfoGameToBuildFolder();
        }

        if (GUILayout.Button("Open iOS Build Folder"))
        {
            OpenPathBuild();
        }
    }
    
    private void GetInfoGame()
    {
        gameIcon = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown)[0];
        companyName = PlayerSettings.companyName;
        gameName = PlayerSettings.productName;
        gameDescription = File.ReadAllText(desInfoGamePath);
        isLandscape = PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft;
        managedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.iOS);
        Debug.Log("Get Info Game Success");
    }

    
    private void CreateFolderAndFile()
    {
        if (!Directory.Exists(desInfoGamePath))
        {
            AssetDatabase.CreateFolder("Assets", "InfoGame");
            AssetDatabase.Refresh();
            Debug.Log("Created folder: " + desInfoGamePath);
        }

        if (!File.Exists(desInfoGamePath))
        {
            File.CreateText(desInfoGamePath);
            AssetDatabase.Refresh();
            Debug.Log("Created file: " + desInfoGamePath);
        }

        if (!File.Exists(privacyInfoPath))
        {
            File.WriteAllText(privacyInfoPath, GetPrivacyInfoContent());
            AssetDatabase.Refresh();
            Debug.Log("Created file: " + privacyInfoPath);
        }
    }
    
    private void ApplySettings(string packageName)
    {
        PlayerSettings.companyName = companyName;
        PlayerSettings.productName = gameName;
        
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, packageName);

        PlayerSettings.stripEngineCode = true;
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, managedStrippingLevel);


        if (gameIcon != null)
        {
            string path = AssetDatabase.GetAssetPath(gameIcon);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { gameIcon });
            Debug.Log("Icon Set Successfully");
        }

        PlayerSettings.defaultInterfaceOrientation = isLandscape ? UIOrientation.LandscapeLeft : UIOrientation.Portrait;
        Debug.Log("Screen Orientation Set to " + (isLandscape ? "Landscape" : "Portrait"));
    }

    private void SaveGameInfo()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(desInfoGamePath));
        File.WriteAllLines(desInfoGamePath, new[] { "Name Game: " + gameName, gameDescription });
        Debug.Log("Game info saved to " + desInfoGamePath);
    }

    private void OpenManifestFile()
    {
        if (File.Exists(manifestPath))
        {
            EditorUtility.RevealInFinder(manifestPath);
        }
        else
        {
            Debug.LogError("Manifest file not found: " + manifestPath);
        }
    }

    private void BuildIOS()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, $"{gameName}_iOS");

        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        if (!EditorBuildSettings.scenes.Any(scene => scene.enabled))
        {
            Debug.LogError("No scenes are added to Build Settings. Please add at least one scene.");
            return;
        }

        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, BuildTarget.iOS, BuildOptions.None);
        Debug.Log("iOS Build Completed: " + buildPath);
    }

    private void OpenPathBuild()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, $"{gameName}_iOS");
        if (Directory.Exists(buildPath))
        {
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError("iOS Build folder not found: " + buildPath);
        }
    }

    private void MoveInfoGameToBuildFolder()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, $"{gameName}_iOS");
        if (!Directory.Exists(buildPath))
        {
            Debug.LogError("Build folder does not exist. Please build the game first.");
            return;
        }

        string destinationFolderPath = Path.Combine(buildPath, "InfoGame");
        try
        {
            if (Directory.Exists(infoGamePath))
            {
                if (Directory.Exists(destinationFolderPath))
                {
                    Directory.Delete(destinationFolderPath, true);
                }

                DirectoryCopy(infoGamePath, destinationFolderPath);
                Debug.Log("InfoGame folder moved to build folder: " + destinationFolderPath);

                string privacyFile = Path.Combine(destinationFolderPath, "PrivacyInfo.xcprivacy");
                string unityFrameworkPath = Path.Combine(buildPath, "UnityFramework", "PrivacyInfo.xcprivacy");
                if (File.Exists(privacyFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(unityFrameworkPath));
                    File.Copy(privacyFile, unityFrameworkPath, true);
                    Debug.Log("PrivacyInfo.xcprivacy moved to UnityFramework folder.");
                }
            }
            else
            {
                Debug.LogError("InfoGame folder not found: " + infoGamePath);
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Error copying the folder: " + e.Message);
        }
    }

    private void DirectoryCopy(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            DirectoryCopy(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }

    private static string GetPrivacyInfoContent()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<!--
   PrivacyInfo.xcprivacy
   Unity-iPhone

   Created by Fap1 on 27/04/2024.
   Copyright (c) 2024 ___ORGANIZATIONNAME___. All rights reserved.
-->
<plist version=""1.0"">
    <dict>
        <key>NSPrivacyAccessedAPITypes</key>
        <array>
            <dict>
                <key>NSPrivacyAccessedAPIType</key>
                <string>NSPrivacyAccessedAPICategoryUserDefaults</string>
                <key>NSPrivacyAccessedAPITypeReasons</key>
                <array>
                    <string>CA92.1</string>
                    <string>C56D.1</string>
                </array>
            </dict>
            <dict>
                <key>NSPrivacyAccessedAPIType</key>
                <string>NSPrivacyAccessedAPICategorySystemBootTime</string>
                <key>NSPrivacyAccessedAPITypeReasons</key>
                <array>
                    <string>35F9.1</string>
                </array>
            </dict>
            <dict>
                <key>NSPrivacyAccessedAPIType</key>
                <string>NSPrivacyAccessedAPICategoryDiskSpace</string>
                <key>NSPrivacyAccessedAPITypeReasons</key>
                <array>
                    <string>E174.1</string>
                </array>
            </dict>
            <dict>
                <key>NSPrivacyAccessedAPIType</key>
                <string>NSPrivacyAccessedAPICategoryFileTimestamp</string>
                <key>NSPrivacyAccessedAPITypeReasons</key>
                <array>
                    <string>DDA9.1</string>
                    <string>0A2A.1</string>
                </array>
            </dict>
        </array>
    </dict>
</plist>";
    }
}

#endif