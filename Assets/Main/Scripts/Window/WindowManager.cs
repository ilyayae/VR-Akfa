using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public enum numberMode
{
    Two,
    One,
    Three
}

public class WindowManager : MonoBehaviour
{
    [SerializeField] private List<WindowSet> sets = new();
    [SerializeField] private List<GameObject> handlesR = new();
    [SerializeField] private List<GameObject> handlesL = new();
    [SerializeField] private List<GameObject> glass = new();
    [SerializeField] private List<WindowMaterialsBundle> materials = new();


    [SerializeField] private WindowBrain LeftWindow;
    [SerializeField] private WindowBrain RightWindow;

    [SerializeField] private Renderer windowsill;

    private numberMode WindowMode = numberMode.Two;
    private int currentSetId = 0;
    private GameObject currentInstantiatedFrame;

    private void Start()
    {
        SetHandleToId(0);
        SetFrameToId(0);
        SetTextureToId(0);
        SetGlassToId(0);
    }
    int currentTextureINSIDE = 0;
    int currentTextureOUTSIDE = 0;
    int currSetting = 0;
    public void SetLaminationSettingState(int set)
    {
        currSetting = set;
    }
    private bool xray = false;
    public void OnOffXRay()
    {
        if(xray)
        {
            xray = false;
            currentInstantiatedFrame.GetComponentInChildren<TextureChanger>().UnmakeXRayed();
            foreach (TextureChanger tchange in RightWindow.changers) tchange.UnmakeXRayed();
            foreach (TextureChanger tchange in LeftWindow.changers) tchange.UnmakeXRayed();
        }
        else
        {
            xray = true;
            currentInstantiatedFrame.GetComponentInChildren<TextureChanger>().MakeXRayed();
            foreach (TextureChanger tchange in RightWindow.changers) tchange.MakeXRayed();
            foreach (TextureChanger tchange in LeftWindow.changers) tchange.MakeXRayed();
        }
    }
    public void SetTextureToId(int id)
    {
        Material hingeMat = materials[id].windowsill;
        if (currentInstantiatedFrame != null && currentInstantiatedFrame.gameObject.activeSelf)
        {
            ApplyToTextureChanger(currentInstantiatedFrame.GetComponentInChildren<TextureChanger>(), id);
            ApplyToHingeChanger(currentInstantiatedFrame.GetComponentInChildren<HingeChanger>(), hingeMat);
        }
        if (windowsill != null && (currSetting == 0 || currSetting == 2))
        {
            windowsill.material = materials[id].windowsill;
        }

        if (RightWindow != null && RightWindow.gameObject.activeSelf)
        {
            foreach (TextureChanger tchange in RightWindow.changers) ApplyToTextureChanger(tchange, id);
            foreach (HingeChanger hchange in RightWindow.hinges) ApplyToHingeChanger(hchange, hingeMat);
        }

        if (LeftWindow != null && LeftWindow.gameObject.activeSelf)
        {
            foreach (TextureChanger tchange in LeftWindow.changers) ApplyToTextureChanger(tchange, id);
            foreach (HingeChanger hchange in LeftWindow.hinges) ApplyToHingeChanger(hchange, hingeMat);
        }

        if(currSetting == 0 || currSetting == 1)
        {
            FacadeManager.Instance.setMaterials(materials[id].window);
        }
    }

    public void SetLaminationForWindow(int idInside, int idOutside)
    {
        int savedState = currSetting; 
        currSetting = 1;
        SetTextureToId(idOutside);
        currSetting = 2;
        SetTextureToId(idInside);
        currSetting = savedState;
    }
    private void ApplyToTextureChanger(TextureChanger changer, int id)
    {
        if (changer == null) return;

        switch (currSetting)
        {
            case 0:
                currentTextureINSIDE = id;
                currentTextureOUTSIDE = id;
                changer.ChangeTextureBoth(materials[id].window);
                break;
            case 1:
                changer.ChangeTextureOutdoors(materials[id].window);
                currentTextureOUTSIDE = id;
                break;
            case 2:
                changer.ChangeTextureRoom(materials[id].window);
                currentTextureINSIDE = id;
                break;
        }
    }

    private void ApplyToHingeChanger(HingeChanger changer, Material mat)
    {
        if (changer == null) return;

        switch (currSetting)
        {
            case 0:
                changer.ChangeTextureBoth(mat);
                break;
            case 1:
                break;
            case 2:
                changer.ChangeTextureBoth(mat);
                break;
        }
    }
    int currentHandle = 0;
    public void SetHandleToId(int id)
    {
        GameManager.Instance.UngrabLeft();
        GameManager.Instance.UngrabRight();
        currentHandle = id;
        if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
        {
            RightWindow.NewHandle(handlesR[id]);
        }
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
        {
            LeftWindow.NewHandle(handlesL[id]);
        }
    }
    public void SetFrameToId(int id)
    {
        GameManager.Instance.UngrabLeft();
        GameManager.Instance.UngrabRight();
        StartCoroutine(FrameChangeSequence(id));
    }
    IEnumerator CloseRightWindow()
    {
        GameManager.Instance.UngrabRight();
        if (RightWindow != null && RightWindow.gameObject.activeInHierarchy)
        {
            yield return StartCoroutine(RightWindow.myDoor.SetOpenedDegreeOfWindow(0f));
            if (RightWindow.handleScript != null)
            {
                yield return StartCoroutine(RightWindow.handleScript.SetDegreeOfHandle(0f));
            }
            RightWindow.myDoor.ResetToClosedPosition();
            if (RightWindow.handleScript != null) RightWindow.handleScript.ResetToClosedPosition();
        }
    }

    IEnumerator CloseLeftWindow()
    {
        GameManager.Instance.UngrabLeft();
        if (LeftWindow != null && LeftWindow.gameObject.activeInHierarchy)
        {
            yield return StartCoroutine(LeftWindow.myDoor.SetOpenedDegreeOfWindow(0f));
            if (LeftWindow.handleScript != null)
            {
                yield return StartCoroutine(LeftWindow.handleScript.SetDegreeOfHandle(0f));
            }
            LeftWindow.myDoor.ResetToClosedPosition();
            if (LeftWindow.handleScript != null) LeftWindow.handleScript.ResetToClosedPosition();
        }
    }

    private IEnumerator AlignToHingeKeepY(WindowDoor door, Vector3 targetHingeWorldPos)
    {
        ArticulationBody body = door.myBody;
        Transform t = door.transform;

        // Force snap before doing any articulation hierarchy rebuilds
        door.ResetToClosedPosition();
        WindowBrain brain = door.GetComponentInParent<WindowBrain>();
        if (brain != null && brain.handleScript != null)
        {
            brain.handleScript.ResetToClosedPosition();
        }

        yield return new WaitForFixedUpdate();

        Vector3 offset = targetHingeWorldPos - t.position;
        offset -= Vector3.Project(offset, t.up);

        Vector3 newPosition = t.position + offset;

        if (body != null)
        {
            if (body.isRoot)
            {
                body.TeleportRoot(newPosition, t.rotation);
            }
            else
            {
                body.enabled = false;
                t.position = newPosition;
                body.enabled = true;
            }
        }
        else
        {
            t.position = newPosition;
        }

        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();

        door.UpdateInitialTransform();
        door.ReapplyStateDirectly();
    }

    private Coroutine activeModeRoutine;

    public void SetSingleWindowMode()
    {
        if (WindowMode == numberMode.One) return;
        if (activeModeRoutine != null) StopCoroutine(activeModeRoutine);
        activeModeRoutine = StartCoroutine(SwitchModeSequence(numberMode.One));
    }

    public void SetDualWindowMode()
    {
        if (WindowMode == numberMode.Two) return;
        if (activeModeRoutine != null) StopCoroutine(activeModeRoutine);
        activeModeRoutine = StartCoroutine(SwitchModeSequence(numberMode.Two));
    }

    public void SetTripleWindowMode()
    {
        if (WindowMode == numberMode.Three) return;
        if (activeModeRoutine != null) StopCoroutine(activeModeRoutine);
        activeModeRoutine = StartCoroutine(SwitchModeSequence(numberMode.Three));
    }

    private IEnumerator SwitchModeSequence(numberMode targetMode)
    {
        GameManager.Instance.UngrabLeft();
        GameManager.Instance.UngrabRight();
        WindowMode = targetMode;
        yield return StartCoroutine(CloseRightWindow());
        if (LeftWindow != null && LeftWindow.gameObject.activeInHierarchy)
        {
            yield return StartCoroutine(CloseLeftWindow());
        }
        if (targetMode == numberMode.One || targetMode == numberMode.Three)
        {
            LeftWindow.gameObject.SetActive(false);
            RightWindow.gameObject.SetActive(true);
        }
        else if (targetMode == numberMode.Two)
        {
            LeftWindow.gameObject.SetActive(true);
            RightWindow.gameObject.SetActive(true);
            LeftWindow.myDoor.ResetToClosedPosition();
            if (LeftWindow.handleScript != null) LeftWindow.handleScript.ResetToClosedPosition();
        }
        yield return new WaitForFixedUpdate();
        yield return StartCoroutine(FrameChangeSequence(currentSetId));
        SetHandleToId(currentHandle);
        if (targetMode == numberMode.Two)
        {
            LeftWindow.SetMechanism(RightWindow.currentMechanism);
        }

        activeModeRoutine = null;
    }
    private IEnumerator FrameChangeSequence(int id)
    {
        Coroutine rightWindowCoroutine = StartCoroutine(CloseRightWindow());
        Coroutine leftWindowCoroutine = StartCoroutine(CloseLeftWindow());

        yield return rightWindowCoroutine;
        yield return leftWindowCoroutine;
        currentSetId = id;
        if (currentInstantiatedFrame != null)
        {
            Destroy(currentInstantiatedFrame);
        }

        if (WindowMode == numberMode.One)
        {
            WindowShaderBinder.instance.size.x = 150;
            Vector3 newScale = windowsill.transform.localScale;
            newScale.x = 100;
            windowsill.transform.localScale = newScale;
            currentInstantiatedFrame = Instantiate(sets[id].frameModelSingular, transform);
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;

            HingeChanger hc = currentInstantiatedFrame.GetComponent<HingeChanger>();
            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelSingular, false, hc.SmallHandTargetR);
            }
            SetLaminationForWindow(currentTextureINSIDE, currentTextureOUTSIDE);

            if (hc != null && hc.RightHingeTransform != null)
            {
                yield return StartCoroutine(AlignToHingeKeepY(RightWindow.myDoor, hc.RightHingeTransform.position));
            }

            if (RightWindow.hinges[0].BigHandTarget != null && RightWindow.hinges[0].HingeTarget)
            {
                ConstraintSource hingeSource = new ConstraintSource();
                hingeSource.sourceTransform = RightWindow.hinges[0].HingeTarget;
                hingeSource.weight = 1f;
                hc.HingeR.AddSource(hingeSource);
                ConstraintSource handSource = new ConstraintSource();
                handSource.sourceTransform = RightWindow.hinges[0].BigHandTarget;
                handSource.weight = 1f;
                hc.BigHandR.AddSource(handSource);
                hc.HingeR.weight = 1f;
                hc.HingeR.constraintActive = true;

                hc.BigHandR.weight = 1f;
                hc.BigHandR.constraintActive = true;
            }
        }
        else if (WindowMode == numberMode.Three)
        {
            WindowShaderBinder.instance.size.x = 220;
            Vector3 newScale = windowsill.transform.localScale;
            newScale.x = 147;
            windowsill.transform.localScale = newScale;
            currentInstantiatedFrame = Instantiate(sets[id].frameModelTriple, transform);
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;

            HingeChanger hc = currentInstantiatedFrame.GetComponent<HingeChanger>();
            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelTriple, false, hc.SmallHandTargetR);
            }
            SetLaminationForWindow(currentTextureINSIDE, currentTextureOUTSIDE);

            if (hc != null && hc.RightHingeTransform != null)
            {
                yield return StartCoroutine(AlignToHingeKeepY(RightWindow.myDoor, hc.RightHingeTransform.position));
            }

            if (RightWindow.hinges[0].BigHandTarget != null && RightWindow.hinges[0].HingeTarget)
            {
                ConstraintSource hingeSource = new ConstraintSource();
                hingeSource.sourceTransform = RightWindow.hinges[0].HingeTarget;
                hingeSource.weight = 1f;
                hc.HingeR.AddSource(hingeSource);
                ConstraintSource handSource = new ConstraintSource();
                handSource.sourceTransform = RightWindow.hinges[0].BigHandTarget;
                handSource.weight = 1f;
                hc.BigHandR.AddSource(handSource);
                hc.HingeR.weight = 1f;
                hc.HingeR.constraintActive = true;

                hc.BigHandR.weight = 1f;
                hc.BigHandR.constraintActive = true;
            }
        }
        else if (WindowMode == numberMode.Two)
        {
            WindowShaderBinder.instance.size.x = 150;
            Vector3 newScale = windowsill.transform.localScale;
            newScale.x = 100;
            windowsill.transform.localScale = newScale;
            currentInstantiatedFrame = Instantiate(sets[id].frameModelDual, transform);
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;

            HingeChanger hc = currentInstantiatedFrame.GetComponent<HingeChanger>();
            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelR, false, hc.SmallHandTargetR);
            }
            if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
            {
                LeftWindow.NewWindow(sets[id].doorModelL, true, hc.SmallHandTargetL);
            }
            SetLaminationForWindow(currentTextureINSIDE, currentTextureOUTSIDE);

            if (hc != null && hc.RightHingeTransform != null && hc.LeftHingeTransform != null)
            {
                StartCoroutine(AlignToHingeKeepY(RightWindow.myDoor, hc.RightHingeTransform.position));
                yield return StartCoroutine(AlignToHingeKeepY(LeftWindow.myDoor, hc.LeftHingeTransform.position));
            }

            if (RightWindow.hinges[0].BigHandTarget != null && RightWindow.hinges[0].HingeTarget)
            {
                ConstraintSource hingeSource = new ConstraintSource();
                hingeSource.sourceTransform = RightWindow.hinges[0].HingeTarget;
                hingeSource.weight = 1f;
                hc.HingeR.AddSource(hingeSource);
                ConstraintSource handSource = new ConstraintSource();
                handSource.sourceTransform = RightWindow.hinges[0].BigHandTarget;
                handSource.weight = 1f;
                hc.BigHandR.AddSource(handSource);
                hc.HingeR.weight = 1f;
                hc.HingeR.constraintActive = true;

                hc.BigHandR.weight = 1f;
                hc.BigHandR.constraintActive = true;
            }
            if (LeftWindow.hinges[0].BigHandTarget != null && LeftWindow.hinges[0].HingeTarget)
            {
                ConstraintSource hingeSource = new ConstraintSource();
                hingeSource.sourceTransform = LeftWindow.hinges[0].HingeTarget;
                hingeSource.weight = 1f;
                hc.HingeL.AddSource(hingeSource);
                ConstraintSource handSource = new ConstraintSource();
                handSource.sourceTransform = LeftWindow.hinges[0].BigHandTarget;
                handSource.weight = 1f;
                hc.BigHandL.AddSource(handSource);
                hc.HingeL.weight = 1f;
                hc.HingeL.constraintActive = true;

                hc.BigHandL.weight = 1f;
                hc.BigHandL.constraintActive = true;
            }

        }
    }

    public void SetGlassToId(int id)
    {
        if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
        {
            RightWindow.NewGlass(glass[id]);
        }
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
        {
            LeftWindow.NewGlass(glass[id]);
        }
    }

    public void setMechanismSwing()
    {
        GameManager.Instance.UngrabLeft();
        GameManager.Instance.UngrabRight();
        if (RightWindow != null && RightWindow.gameObject.activeSelf)
            RightWindow.SetMechanism(WindowBrain.WindowMechanism.SWING);
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf)
            LeftWindow.SetMechanism(WindowBrain.WindowMechanism.SWING);
    }

    public void setMechanismTilt()
    {
        GameManager.Instance.UngrabLeft();
        GameManager.Instance.UngrabRight();
        if (RightWindow != null && RightWindow.gameObject.activeSelf)
            RightWindow.SetMechanism(WindowBrain.WindowMechanism.SWING_TILT);
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf)
            LeftWindow.SetMechanism(WindowBrain.WindowMechanism.SWING_TILT);
    }

    public void setMechanismSlide()
    {
        GameManager.Instance.UngrabLeft();
        GameManager.Instance.UngrabRight();
        if (RightWindow != null && RightWindow.gameObject.activeSelf)
            RightWindow.SetMechanism(WindowBrain.WindowMechanism.SLIDE);
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf)
            LeftWindow.SetMechanism(WindowBrain.WindowMechanism.SLIDE);
    }

    private void Update()
    {
        
    }
    /*
     * This manager will be the core of the modular window editing system of this project
     * it will store the two window states (for one window and dual window showcase)
     * Each state will hold information about how the window is currently opened - 
     * the degree of the handle, the degree to which the window is opened, how it is opened etc.
     * The WindowManager will also hold the state of the window parts, which are selected. 
     * it will be key to replacing parts of the modular window editor
     * 
     * Thus we need the transform of the window to replace the entire frame, 
     * the transform of the glass pane that will move with the window door,
     * the transform of the knob for opening the window
     * references to all moving parts and parts that can change their material if the user chooses.
     * The window itself should know where its hinges at, all of them for multi-hige designs, as well as paths for sliding design.
     */
}
