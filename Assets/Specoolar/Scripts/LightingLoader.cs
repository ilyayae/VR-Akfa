using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LightingLoader : MonoBehaviour
{
    public ProbeVolumeBakingSet bakingSet;

    void Start()
    {
        Load();
    }
    private void OnEnable()
    {
        Load();
    }

    public void Load()
    {
        ProbeReferenceVolume.instance.SetActiveBakingSet(bakingSet);
    }
}
