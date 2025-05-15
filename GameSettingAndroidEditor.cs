#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using System.IO.Compression;
using UnityEditor.Build.Reporting;
using CompressionLevel = System.IO.Compression.CompressionLevel;


public class GameSettingAndroidEditor : EditorWindow
{
    
    private Texture2D gameIcon;
    private string companyName = "";
    private string gameName = "";
    private string gameDescription = "";
    private bool isLandscape = true;
    private bool isCreateFolder = false;
    private int version = 1;

    private const string infoGamePath = "Assets/InfoGame";
    private const string desInfoGamePath = infoGamePath + "/Description.txt";
    private const string manifestPath = "Packages/manifest.json";


    [MenuItem("Tools/Game Settings Android")]
    public static void ShowWindow()
    {
        GetWindow<GameSettingAndroidEditor>("Game Settings Android");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Settings Android", EditorStyles.boldLabel);
        if(GUILayout.Button("Create Folder And File"))  CreateFolderAndFile();
        if(GUILayout.Button("Get Info")) GetInfoGame();
        if(GUILayout.Button("Open Player Settings")) OpenPlayerSettings();
        
        GUILayout.Space(10);
        // Icon selection
        GUILayout.Label("Game Icon:");
        gameIcon = (Texture2D)EditorGUILayout.ObjectField(gameIcon, typeof(Texture2D), false);
        if (GUILayout.Button("Clone and Resize Icon (512 & 144)"))
        {
            if (gameIcon != null)
            {

                string path = AssetDatabase.GetAssetPath(gameIcon);
                string dir = Path.GetDirectoryName(path);

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.filterMode = FilterMode.Point; // hoặc Bilinear
                    importer.SaveAndReimport();
                }
                
                // Resize and save 512x512
                Texture2D icon512 = ResizeTexturePixelPerfect(gameIcon, 512, 512);
                SaveTextureAsPNG(icon512, Path.Combine(dir, "icon_512x512.png"));

                // Resize and save 144x144
                Texture2D icon144 = ResizeTexturePixelPerfect(gameIcon, 144, 144);
                SaveTextureAsPNG(icon144, Path.Combine(dir, "icon_144x144.png"));

                AssetDatabase.Refresh();
                Debug.Log("Icons resized and saved.");
            }
            else
            {
                Debug.LogWarning("Please select a texture.");
            }
        }

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
        version = EditorGUILayout.IntField("Version", version);

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
        GUILayout.Label("Build APK Settings", EditorStyles.boldLabel);
        
        if(GUILayout.Button("Switch to Android"))
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        GUILayout.Space(5);
        if (GUILayout.Button("Build APK"))
        {
            BuildAPK();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Move InfoGame to Build Folder"))
        {
            MoveInfoGameToBuildFolder();
        }
         

        if (GUILayout.Button("Open APK Build Folder"))
        {
            OpenPathBuild();
        }
    }
    
    private Texture2D ResizeTexturePixelPerfect(Texture2D source, int width, int height)
    {
        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[width * height];
        float ratioX = (float)source.width / width;
        float ratioY = (float)source.height / height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcX = Mathf.FloorToInt(x * ratioX);
                int srcY = Mathf.FloorToInt(y * ratioY);
                pixels[y * width + x] = source.GetPixel(srcX, srcY);
            }
        }

        result.SetPixels(pixels);
        result.Apply();
        return result;
    }
    private void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log("Saved: " + path);
    }
    
    private void GetInfoGame()
    {
        if(PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown).Length > 0)
            gameIcon = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown)[0];
        if(PlayerSettings.companyName != "")
            companyName = PlayerSettings.companyName;
        if(PlayerSettings.productName != "")
            gameName = PlayerSettings.productName;
        
        if(File.ReadAllText(desInfoGamePath) != "")
            gameDescription = File.ReadAllText(desInfoGamePath);
        isLandscape = PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft;
        Debug.Log("Get Info Game Success");
    }

    private void OpenPlayerSettings()
    {
       // ExecuteMenuItem failed because there is no menu named 'Edit/Project Settings/Player'
       SettingsService.OpenProjectSettings("Project/Player");
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
    }
    
    
    
    private void ApplySettings(string packageName)
    {
        PlayerSettings.companyName = companyName;
        PlayerSettings.productName = gameName;
        
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);
 
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.bundleVersion = version.ToString(); 

        // set minimum api level
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetArchitectures =  AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;        // set armv7
        //PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 1);
        //PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        

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

    private void BuildAPK()
    {
        string buildFolder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, $"{gameName}_Android");

        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        if (!EditorBuildSettings.scenes.Any(scene => scene.enabled))
        {
            Debug.LogError("No scenes are added to Build Settings. Please add at least one scene.");
            return;
        }

        // Đường dẫn file apk
        string apkPath = Path.Combine(buildFolder, $"{gameName}.apk");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path).ToArray(),
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("APK Build succeeded: " + apkPath);
            EditorUtility.RevealInFinder(apkPath);
        }
        else
        {
            Debug.LogError("APK Build failed.");
        }
    }

    private void OpenPathBuild()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, $"{gameName}_Android");
        if (Directory.Exists(buildPath))
        {
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError("APK Build folder not found: " + buildPath);
        }
    }

    private void MoveInfoGameToBuildFolder()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, $"{gameName}_Android");
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
}

#endif