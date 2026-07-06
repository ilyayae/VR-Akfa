using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(FacadeScene))]
public class FacadeSceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        if (GUILayout.Button("Load lighting"))
        {
            FacadeScene facadeScene = (FacadeScene)target;
            facadeScene.LoadLighting();
        }
    }
}

#endif

public class FacadeScene : MonoBehaviour
{
    public ProbeVolumeBakingSet bakingSet;
    [HideInInspector] public string sceneName;
    private void Awake()
    {
        sceneName = gameObject.scene.name;
        FacadeManager.main.OnLoadFacade(this);
    }

    public void LoadLighting()
    {
        ProbeReferenceVolume.instance.SetActiveBakingSet(bakingSet);
    }
}
