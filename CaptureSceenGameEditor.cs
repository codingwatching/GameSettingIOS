#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaptureSceenGame))]
[CanEditMultipleObjects]
public class CaptureSceenGameEditor : Editor
{
    private CaptureSceenGame _captureSceenGame;

    private void OnEnable()
    {
        _captureSceenGame = FindObjectOfType<CaptureSceenGame>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Capture Icon"))
        {
            _captureSceenGame.CaptureIcon();
        }
        if (GUILayout.Button("Capture Single"))
        {
            _captureSceenGame.CapScreen();
        }
        
        if (GUILayout.Button("Capture Double"))
        {
            _captureSceenGame.CaptureScreenshots();
        }
        
        if(GUILayout.Button("SetCanvasCamera"))
        {
            _captureSceenGame.SetCanvasCamera();
        }
             
    }
}
#endif