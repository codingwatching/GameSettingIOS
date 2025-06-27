#if UNITY_EDITOR_64

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Tool_SetupAndroid
{
    public static void GetCurrentKeystorePath()
    {
        // Get the current keystore path
        string keystorePath = PlayerSettings.Android.keystoreName;

        // Display the keystore path in the Console
        Debug.Log("Current Keystore Path: " + keystorePath);
    }

    public static void SetKeystoreAndAlias()
    {
        // Switch build target to Android if not already set
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        // Set the keystore name (path to the keystore file)


        Debug.Log("Keystore and alias settings have been successfully applied.");
        var productName = PlayerSettings.productName;
        string customBundleIdentifier = GenerateCustomBundleIdentifier(productName);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, customBundleIdentifier);
        //--------------

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;


        PlayerSettings.Android.useCustomKeystore = true;
        var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string keystorePath = System.IO.Path.Combine("D:\\", "user.keystore");
        if (keystorePath != string.Empty)
        {
            if (!System.IO.File.Exists(keystorePath))
            {
                // Create a new keystore file if it doesn't exist
                System.IO.File.Create(keystorePath).Dispose();
                Debug.Log("Created new keystore at: " + keystorePath);
            }
        }
        else
        {
            Debug.LogError("Keystore path is empty. Please specify a valid path.");
            return;
            
        }
        PlayerSettings.Android.keystoreName ="D:/user.keystore"; // Ensure this path is correct
        Debug.Log("Keystore Path: " + PlayerSettings.Android.keystoreName);
        PlayerSettings.Android.minifyWithR8 = true;
        PlayerSettings.Android.minifyRelease = true;
        ApplyKeystoreSettings();
    }

    private static void ApplyKeystoreSettings()
    {
        Debug.Log("Setting up Keystore and Alias...");
        // Set the keystore password
        PlayerSettings.Android.keystorePass = "123123";

        // Set the key alias name
        PlayerSettings.Android.keyaliasName = "a2";

        // Set the key alias password
        PlayerSettings.Android.keyaliasPass = "123123";

        PlayerSettings.Android.keystorePass = "123123";
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Setting up Keystore and Alias Done");
    }

    private static string GenerateCustomBundleIdentifier(string productName)
    {
        // Replace invalid characters with hyphens
        string sanitizedProductName = Regex.Replace(productName, @"[^a-zA-Z0-9]+", "-").Trim('-');
        return $"com.{sanitizedProductName}";
    }
}
#endif