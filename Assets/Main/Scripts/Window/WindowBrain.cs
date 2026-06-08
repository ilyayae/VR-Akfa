using System.Collections;
using UnityEngine;

public class WindowBrain : MonoBehaviour
{
    public WindowDoor myDoor;
    public WindowHandle myHandle;

    float originalLow;

    private void Start()
    {
        myHandle.onClose += OnHandleClosed;
        myHandle.onOpen += OnHandleOpened;
        originalLow = myDoor.myBody.xDrive.lowerLimit;
    }

    public void NewHandle()
    {

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