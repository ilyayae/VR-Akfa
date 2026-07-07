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
    public OtherWindowChanger changer;
    private void Awake()
    {
        sceneName = gameObject.scene.name;
        FacadeManager.Instance.OnLoadFacade(this);
    }

    public void LoadLighting()
    {
        ProbeReferenceVolume.instance.SetActiveBakingSet(bakingSet);
    }
}
