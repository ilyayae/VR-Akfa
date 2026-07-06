using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBrain : MonoBehaviour
{
    public enum WindowMechanism { SWING, SWING_TILT, SLIDE }
    public WindowMechanism currentMechanism = WindowMechanism.SWING;

    public GameObject myFrame;
    public GameObject myHandle;
    public WindowDoor myDoor;
    public GameObject myGrab;
    public WindowHandle handleScript;
    public List<GameObject> transforms;
    public List<TextureChanger> changers = new();
    public List<HingeChanger> hinges = new();

    private WindowHandle.HandleState lastHandleState = WindowHandle.HandleState.CLOSED;
    private bool isCurrentlyBlocked = false;
    private bool wasPracticallyClosed = true;

    // Protection flags for safe mechanism transitioning
    private bool isTransitioningMechanism = false;
    private Coroutine mechanismTransitionRoutine;

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject go = parent.GetChild(i).gameObject;
            if (go == myFrame || go == myHandle || go == myDoor.gameObject || go == myGrab || transforms.Contains(go))
                continue;
            Destroy(go);
        }
    }

    public void SetMechanism(WindowMechanism mech)
    {
        if (currentMechanism == mech) return;

        if (mechanismTransitionRoutine != null)
        {
            StopCoroutine(mechanismTransitionRoutine);
        }

        mechanismTransitionRoutine = StartCoroutine(DoSetMechanism(mech));
    }

    private IEnumerator DoSetMechanism(WindowMechanism newMech)
    {
        isTransitioningMechanism = true;
        if (myDoor != null && myDoor.GetOpenedDegreeOfWindow() > 0.01f)
        {
            yield return StartCoroutine(myDoor.SetOpenedDegreeOfWindow(0f));
        }

        if (handleScript != null && handleScript.GetDegreeOfHandle() > 0.01f)
        {
            yield return StartCoroutine(handleScript.SetDegreeOfHandle(0f));
        }

        if (myDoor != null)
        {
            myDoor.SetHandleLockedState(true);
            myDoor.SetJointLocked();
        }

        yield return new WaitForFixedUpdate();

        currentMechanism = newMech;
        ApplyHandleLimit();
        if (myDoor != null)
        {
            doorState newDoorState = doorState.SWING;
            if (newMech == WindowMechanism.SWING) newDoorState = doorState.SWING;
            else if (newMech == WindowMechanism.SLIDE) newDoorState = doorState.SLIDE;
            else if (newMech == WindowMechanism.SWING_TILT) newDoorState = doorState.SWING;

            myDoor.SwitchState(newDoorState);
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        isTransitioningMechanism = false; // Unlock evaluation
        EvaluateWindowState();
        mechanismTransitionRoutine = null;
    }

    private void ApplyHandleLimit()
    {
        if (handleScript != null)
        {
            if (currentMechanism == WindowMechanism.SWING_TILT)
                handleScript.SetMaxAngle(180f);
            else
                handleScript.SetMaxAngle(90f);
        }
    }

    public void MoveArticulationRoot(Vector3 newPosition)
    {
        ArticulationBody ab = GetComponentInChildren<ArticulationBody>();
        if (ab != null) ab.TeleportRoot(newPosition, transform.rotation);
        else transform.position = newPosition;
    }

    public void SetHandlePosition(Vector3 worldPosition)
    {
        if (handleScript != null)
        {
            handleScript.ResetToClosedPosition();
        }

        myHandle.gameObject.SetActive(false);
        myHandle.transform.position = worldPosition;
        myHandle.gameObject.SetActive(true);
    }

    public void NewHandle(GameObject newHandle)
    {
        if (handleScript != null)
        {
            handleScript.onHandleStateChanged -= OnHandleStateChanged;
        }

        ClearChildren(myHandle.transform);
        GameObject handleInstance = Instantiate(newHandle, myHandle.transform);

        HandlePointer pointer = handleInstance.GetComponent<HandlePointer>();
        if (pointer != null && pointer.handle != null)
        {
            handleScript = pointer.handle;
            ApplyHandleLimit();
            handleScript.onHandleStateChanged += OnHandleStateChanged;
            lastHandleState = handleScript.currentHandleState;
            EvaluateWindowState();
        }
    }

    public void NewWindow(GameObject newWindowDoor, bool lefty)
    {
        RemoveOldChangers(myFrame.transform);
        RemoveOldChangers(myDoor.transform);

        ClearChildren(myFrame.transform);
        ClearChildren(myDoor.transform);

        GameObject Door = Instantiate(newWindowDoor, myDoor.transform);

        SetHandlePosition(Door.GetComponent<HingeChanger>().handle.position);

        TextureChanger[] newChangers = Door.GetComponentsInChildren<TextureChanger>();
        changers.AddRange(newChangers);

        HingeChanger[] newHinges = Door.GetComponentsInChildren<HingeChanger>();
        hinges.AddRange(newHinges);
    }
    public void NewGlass(GameObject newGlassPrefab) { }

    private void RemoveOldChangers(Transform parent)
    {
        foreach (Transform child in parent)
        {
            TextureChanger oldChanger = child.GetComponent<TextureChanger>();
            if (oldChanger != null && changers.Contains(oldChanger)) changers.Remove(oldChanger);

            HingeChanger oldHinge = child.GetComponent<HingeChanger>();
            if (oldHinge != null && hinges.Contains(oldHinge)) hinges.Remove(oldHinge);
        }
    }

    private void Update()
    {
        if (myDoor == null || isTransitioningMechanism) return;

        bool isPracticallyClosed = myDoor.GetOpenedDegreeOfWindow() < (myDoor.openDegree * 0.9f);
        if (isPracticallyClosed != wasPracticallyClosed)
        {
            wasPracticallyClosed = isPracticallyClosed;
        }
        EvaluateWindowState();
    }

    public void OnHandleStateChanged(WindowHandle.HandleState newState)
    {
        lastHandleState = newState;
        EvaluateWindowState();
    }

    public void EvaluateWindowState()
    {
        if (myDoor == null || handleScript == null || isTransitioningMechanism) return;

        float doorOpenDeg = myDoor.GetOpenedDegreeOfWindow();
        bool isPracticallyClosed = doorOpenDeg < (myDoor.openDegree * 0.9f);

        if (isPracticallyClosed)
        {
            if (currentMechanism == WindowMechanism.SWING_TILT)
            {
                if (lastHandleState == WindowHandle.HandleState.TILT && myDoor.state != doorState.TILT)
                    myDoor.SwitchState(doorState.TILT);
                else if (lastHandleState == WindowHandle.HandleState.SWING && myDoor.state != doorState.SWING)
                    myDoor.SwitchState(doorState.SWING);
            }
        }

        bool handleMatchesState = false;
        if (currentMechanism == WindowMechanism.SWING_TILT)
        {
            if (myDoor.state == doorState.SWING && lastHandleState == WindowHandle.HandleState.SWING) handleMatchesState = true;
            if (myDoor.state == doorState.TILT && lastHandleState == WindowHandle.HandleState.TILT) handleMatchesState = true;
        }
        else
        {
            if (lastHandleState != WindowHandle.HandleState.CLOSED) handleMatchesState = true;
        }

        if (lastHandleState == WindowHandle.HandleState.CLOSED) handleMatchesState = false;
        if (isPracticallyClosed)
        {
            if (lastHandleState == WindowHandle.HandleState.CLOSED)
            {
                if (!myDoor.isHandleLocked) StartCoroutine(closeSequence());
            }
            else
            {
                if (myDoor.isHandleLocked)
                {
                    myDoor.SetHandleLockedState(false);
                    myDoor.SetJointUnlocked();
                }
            }
        }
        else
        {
            if (!handleMatchesState)
            {
                if (!isCurrentlyBlocked)
                {
                    myDoor.BlockWindowFromClosing();
                    isCurrentlyBlocked = true;
                }
            }
            else
            {
                if (isCurrentlyBlocked)
                {
                    myDoor.UnblockWindowFromClosing();
                    isCurrentlyBlocked = false;
                }
            }
        }
    }

    public IEnumerator closeSequence()
    {
        myDoor.SetHandleLockedState(true);
        yield return StartCoroutine(myDoor.SetOpenedDegreeOfWindow(0));
        myDoor.SetJointLocked();
        isCurrentlyBlocked = false;
    }
}