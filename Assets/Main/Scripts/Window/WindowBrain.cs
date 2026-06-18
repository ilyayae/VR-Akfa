using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WindowBrain : MonoBehaviour
{
    public GameObject myFrame;
    public GameObject myHandle;
    public WindowDoor myDoor;
    public WindowHandle handleScript;
    public List<GameObject> transforms;
    public List<TextureChanger> changers = new();
    public List<HingeChanger> hinges = new();

    public Transform twoWindowHandleTransform;
    public Transform oneWindowHandleTransform;
    public Vector3 twoWindowHandlePosition;
    public Vector3 oneWindowHandlePosition;

    private void Start()
    {
        twoWindowHandlePosition = twoWindowHandleTransform.localPosition;
        oneWindowHandlePosition = oneWindowHandleTransform.localPosition;
    }
    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject go = parent.GetChild(i).gameObject;
            if (go == myFrame || go == myHandle || go == myDoor.gameObject || transforms.Contains(go))
                continue;
            Destroy(go);
        }
    }

    public void MoveArticulationRoot(Vector3 newPosition)
    {
        ArticulationBody ab = GetComponentInChildren<ArticulationBody>();

        if (ab != null)
        {
            ab.TeleportRoot(newPosition, transform.rotation);
        }
        else
        {
            transform.position = newPosition;
        }
    }
    public void SetHandleOneOrTwo(bool one)
    {
        if (one)
        {
            myHandle.gameObject.SetActive(false);
            myHandle.transform.localPosition = oneWindowHandlePosition;
            myHandle.gameObject.SetActive(true);
        }
        else
        {
            myHandle.gameObject.SetActive(false);
            myHandle.transform.localPosition = twoWindowHandlePosition;
            myHandle.gameObject.SetActive(true);
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
    }

    public void NewWindow(GameObject newFrame, bool lefty)
    {
        RemoveOldChangers(myFrame.transform);
        RemoveOldChangers(myDoor.transform);

        ClearChildren(myFrame.transform);
        ClearChildren(myDoor.transform);

        GameObject frame = Instantiate(newFrame, myDoor.transform);


        TextureChanger[] newChangers = frame.GetComponentsInChildren<TextureChanger>();
        changers.AddRange(newChangers);

        HingeChanger[] newHinges = frame.GetComponentsInChildren<HingeChanger>();
        hinges.AddRange(newHinges);
    }

    public void NewGlass(GameObject newGlassPrefab)
    {
        /*
        ClearChildren(myGlass.transform);

        GameObject glass = Instantiate(newGlassPrefab, myGlass.transform);
        */
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
            HingeChanger oldHinge = child.GetComponent<HingeChanger>();
            if (oldHinge != null && hinges.Contains(oldHinge))
            {
                hinges.Remove(oldHinge);
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