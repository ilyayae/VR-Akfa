using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBrain : MonoBehaviour
{
    public GameObject myGlass;
    public GameObject myFrame;
    public GameObject myHandle;
    public WindowDoor myDoor;
    public WindowHandle handleScript;
    public List<GameObject> transforms;
    public List<TextureChanger> changers = new();
    float originalLow;

    private void Start()
    {
    }
    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject go = parent.GetChild(i).gameObject;
            if (go == myGlass || go == myFrame || go == myHandle || go == myDoor.gameObject || transforms.Contains(go))
                return;
            Destroy(go);
        }
    }

    public void NewHandle(GameObject newHandle)
    {
        if (handleScript != null)
        {
            handleScript.onClose -= OnHandleClosed;
            handleScript.onOpen -= OnHandleOpened;
        }

        ClearChildren(myHandle.transform);

        GameObject handleInstance = Instantiate(newHandle, myHandle.transform);

        HandlePointer pointer = handleInstance.GetComponent<HandlePointer>();
        if (pointer != null && pointer.handle != null)
        {
            handleScript = pointer.handle;
            handleScript.onClose += OnHandleClosed;
            handleScript.onOpen += OnHandleOpened;
        }

        originalLow = myDoor.myBody.xDrive.lowerLimit;
    }

    public void NewWindow(GameObject newDoor, GameObject newFrame, bool lefty)
    {
        RemoveOldChangers(myFrame.transform);
        RemoveOldChangers(myDoor.transform);

        ClearChildren(myFrame.transform);
        ClearChildren(myDoor.transform);

        GameObject frame = Instantiate(newFrame, myFrame.transform);
        GameObject door = Instantiate(newDoor, myDoor.transform);

        if(lefty)
        {
            frame.transform.localScale = new Vector3(-1, 1, 1);
            door.transform.localScale = new Vector3(-1, 1, 1);
        }

        TextureChanger frameChanger = frame.GetComponent<TextureChanger>();
        if (frameChanger != null) changers.Add(frameChanger);

        TextureChanger doorChanger = door.GetComponent<TextureChanger>();
        if (doorChanger != null) changers.Add(doorChanger);
    }

    public void NewGlass(GameObject newGlassPrefab)
    {
        ClearChildren(myGlass.transform);

        GameObject glass = Instantiate(newGlassPrefab, myGlass.transform);
    }

    private void RemoveOldChangers(Transform parent)
    {
        foreach (Transform child in parent)
        {
            TextureChanger oldChanger = child.GetComponent<TextureChanger>();
            if (oldChanger != null && changers.Contains(oldChanger))
            {
                changers.Remove(oldChanger);
            }
        }
    }

    public void OnHandleClosed()
    {
        if (myDoor.GetOpenedDegreeOfWindow() > myDoor.openDegree)
        {
            myDoor.BlockWindowFromClosing();
        }
        else
        {
            StartCoroutine(closeSequence());
        }
    }

    public IEnumerator closeSequence()
    {
        yield return StartCoroutine(myDoor.SetOpenedDegreeOfWindow(0));
        myDoor.SetJointLocked();
    }

    public void OnHandleOpened()
    {
        if (myDoor.GetOpenedDegreeOfWindow() > myDoor.openDegree)
        {
            myDoor.UnblockWindowFromClosing();
        }
        else
        {
            myDoor.SetJointUnlocked();
        }
    }
}