using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBrain : MonoBehaviour
{
    public GameObject myFrame;
    public WindowDoor myDoor;
    public GameObject myHandle;
    public WindowHandle handleScript;
    public List<TextureChanger> changers = new();
    float originalLow;

    private void Start()
    {
    }

    public void NewHandle(GameObject newHandle)
    {
        if(handleScript  != null)
        {
            handleScript.onClose -= OnHandleClosed;
            handleScript.onOpen -= OnHandleOpened;
        }
        handleScript = Instantiate(newHandle, myHandle.transform).GetComponent<HandlePointer>().handle;
        handleScript.onClose += OnHandleClosed;
        handleScript.onOpen += OnHandleOpened;
        originalLow = myDoor.myBody.xDrive.lowerLimit;
    }

    public void NewWindow(GameObject newDoor, GameObject newFrame)
    {
        GameObject frame = Instantiate(newFrame,myFrame.transform);
        GameObject door = Instantiate(newDoor, myDoor.transform);
        changers.Add(frame.GetComponent<TextureChanger>());
        changers.Add(door.GetComponent<TextureChanger>());
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