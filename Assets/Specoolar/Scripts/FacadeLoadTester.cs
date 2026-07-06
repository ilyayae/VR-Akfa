using UnityEngine;

public class FacadeLoadTester : MonoBehaviour
{
    public FacadeManager manager;
    public string FacadeName;
    public bool onstart;
    void Start()
    {
        if (onstart)
            manager.SetActiveFacade(FacadeName);
    }

    [ContextMenu("Set")]
    public void Set()
    {
        manager.SetActiveFacade(FacadeName);
    }
}
