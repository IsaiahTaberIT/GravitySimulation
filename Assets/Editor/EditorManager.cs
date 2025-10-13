using UnityEditor;
using UnityEngine;


[ExecuteAlways]
public class EditorManager : Editor
{
    public UnityEngine.Rendering.Universal.FullScreenPassRendererFeature myFullScreenFeature;
    private void OnSceneGUI()
    {
        myFullScreenFeature.SetActive(false); // Disables the feature
        Debug.Log("ran");
    }
}
