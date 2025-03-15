using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CaptureSceenGame : MonoBehaviour
{
    public bool isCapturePortrait = false;


    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     { // capture screen shot on space key down
    //         CaptureScreenshots();
    //     }
    //     else
    //     {
    //         if (Input.GetKeyDown(KeyCode.M))
    //         {
    //             CaptureIcon();
    //         }
    //     }
    // }

    [ContextMenu("CaptureIcon")]
    public void CaptureIcon()
    {
        string folderPath = "Assets/InfoGame/"; // the path of your project folder

        if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
            System.IO.Directory.CreateDirectory(folderPath); // it will get created
        CaptureScreenshot(folderPath, "Icon_512", 512, 512);
    }

    [ContextMenu("Capture Single")]
    public void CapScreen()
    {
        string folderPath = "Assets/InfoGame/"; // the path of your project folder

        if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
            System.IO.Directory.CreateDirectory(folderPath); // it will get created

        var screenshotName =
            "Screenshot_" +
            System.DateTime.Now.ToString(
                "dd-MM-yyyy-HH-mm-ss") + // puts the current time right into the screenshot name
            ".png"; // put youre favorite data format here
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName),
            1); // takes the sceenshot, the "2" is for the scaled resolution, you can put this to 600 but it will take really long to scale the image up
        AssetDatabase.Refresh();
        Debug.Log("Capture Screenshot Name " + screenshotName);
    }


    [ContextMenu("Capture Double")]
    public void CaptureScreenshots()
    {
        string folderPath = "Assets/InfoGame/"; // the path of your project folder

        if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
            System.IO.Directory.CreateDirectory(folderPath); // it will get created

#if UNITY_IOS
        if (isCapturePortrait)
        {
            CaptureScreenshot(folderPath, "Portrait", 1242, 2688);
            CaptureScreenshot(folderPath, "Portrait", 1242, 2208);
        }
        else
        {
            CaptureScreenshot(folderPath, "Landscape", 2688, 1242);
            CaptureScreenshot(folderPath, "Landscape", 2208, 1242);
        }
#else
        if (isCapturePortrait)
        {
            CaptureScreenshot(folderPath, "Portrait", 1080, 1920);
        }
        else
        {
            CaptureScreenshot(folderPath, "Landscape", 1920, 1080);
        }
#endif
        // else
        // {
        //     var screenshotName =
        //                         "Screenshot_" +
        //                         System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + // puts the current time right into the screenshot name
        //                         ".png"; // put your favorite data format here
        //
        //     ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 1); // takes the screenshot
        //     AssetDatabase.Refresh();
        //     Debug.Log("Capture Screenshot Name " + screenshotName);
        // }
    }

    void CaptureScreenshot(string folderPath, string resolutionName, int width, int height)
    {
        var renderTexture = new RenderTexture(width, height, 24);
        var screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        Camera.main.targetTexture = renderTexture;
        Camera.main.Render();

        RenderTexture.active = renderTexture;
        screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotTexture.Apply();

        Camera.main.targetTexture = null;
        RenderTexture.active = null;

        Destroy(renderTexture);

        var screenshotName =
            resolutionName + "_" +
            System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") +
            ".png";

        var screenshotPath = System.IO.Path.Combine(folderPath, screenshotName);
        System.IO.File.WriteAllBytes(screenshotPath, screenshotTexture.EncodeToPNG());
        AssetDatabase.Refresh();
        Debug.Log("Captured Screenshot: " + screenshotPath);
    }

    public void SetCanvasCamera()
    {
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
}

#endif