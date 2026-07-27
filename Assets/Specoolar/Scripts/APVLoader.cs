using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(APVLoader))]
public class APVLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        if (GUILayout.Button("Load lighting"))
        {
            APVLoader loader = (APVLoader)target;
            loader.LoadLighting();
        }
    }
}

#endif

public class APVLoader : MonoBehaviour
{
    public ProbeVolumeBakingSet bakingSet;

    private void Start()
    {
        LoadLighting();
    }
    public void LoadLighting()
    {
        ProbeReferenceVolume.instance.SetActiveBakingSet(bakingSet);
    }
}
