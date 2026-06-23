using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

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

    private bool isSingleWindowMode = false;
    private int currentSetId = 0;
    private GameObject currentInstantiatedFrame;

    private Vector3 originalLeftPos;
    private Vector3 originalRightPos;
    private void Start()
    {
        SetHandleToId(0);
        SetFrameToId(0);
        SetTextureToId(0);
        SetGlassToId(0);
    }
    int currentTexture = 0;
    int currSetting = 0;
    public void SetLaminationSettingState(int set)
    {
        currSetting = set;
    }
    public void SetTextureToId(int id)
    {
        currentTexture = id;
        Material windowMat = materials[id].window;
        Material hingeMat = materials[id].hinge;

        if (currentInstantiatedFrame != null && currentInstantiatedFrame.gameObject.activeSelf)
        {
            ApplyToTextureChanger(currentInstantiatedFrame.GetComponentInChildren<TextureChanger>(), windowMat);
            ApplyToHingeChanger(currentInstantiatedFrame.GetComponentInChildren<HingeChanger>(), hingeMat);
        }
        if (windowsill != null && (currSetting == 0 || currSetting == 2))
        {
            windowsill.material = materials[id].windowsill;
        }

        if (RightWindow != null && RightWindow.gameObject.activeSelf)
        {
            foreach (TextureChanger tchange in RightWindow.changers) ApplyToTextureChanger(tchange, windowMat);
            foreach (HingeChanger hchange in RightWindow.hinges) ApplyToHingeChanger(hchange, hingeMat);
        }

        if (LeftWindow != null && LeftWindow.gameObject.activeSelf)
        {
            foreach (TextureChanger tchange in LeftWindow.changers) ApplyToTextureChanger(tchange, windowMat);
            foreach (HingeChanger hchange in LeftWindow.hinges) ApplyToHingeChanger(hchange, hingeMat);
        }
    }
    private void ApplyToTextureChanger(TextureChanger changer, Material mat)
    {
        if (changer == null) return;

        switch (currSetting)
        {
            case 0:
                changer.ChangeTextureBoth(mat);
                break;
            case 1:
                changer.ChangeTextureOutdoors(mat);
                break;
            case 2:
                changer.ChangeTextureRoom(mat);
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
        currentSetId = id;
        if (currentInstantiatedFrame != null)
        {
            Destroy(currentInstantiatedFrame);
        }

        if (isSingleWindowMode)
        {
            currentInstantiatedFrame = Instantiate(sets[id].frameModelSingular, transform);
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;

            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelSingular, false);
            }

            HingeChanger hc = currentInstantiatedFrame.GetComponent<HingeChanger>();
            if (hc != null && hc.RightHingeTransform != null)
            {
                AlignToHingeKeepY(RightWindow.myDoor.transform, hc.RightHingeTransform.position);
            }
        }
        else
        {
            currentInstantiatedFrame = Instantiate(sets[id].frameModelDual, transform);
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;

            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelR, false);
            }
            if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
            {
                LeftWindow.NewWindow(sets[id].doorModelL, true);
            }

            HingeChanger hc = currentInstantiatedFrame.GetComponent<HingeChanger>();
            if (hc != null && hc.RightHingeTransform != null && hc.LeftHingeTransform != null)
            {
                AlignToHingeKeepY(RightWindow.myDoor.transform, hc.RightHingeTransform.position);
                AlignToHingeKeepY(LeftWindow.myDoor.transform, hc.LeftHingeTransform.position);
            }
        }

        SetTextureToId(currentTexture);
    }

    private void AlignToHingeKeepY(Transform windowToMove, Vector3 targetHingeWorldPos)
    {
        Vector3 currentPos = windowToMove.position;
        Vector3 offset = targetHingeWorldPos - currentPos;
        float upMovementAmount = Vector3.Dot(offset, windowToMove.up);
        Vector3 horizontalOffset = offset - (windowToMove.up * upMovementAmount);
        windowToMove.position = currentPos + horizontalOffset;
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


    public void SetSingleWindowMode()
    {
        if (isSingleWindowMode) return;

        isSingleWindowMode = true;

        originalLeftPos = LeftWindow.transform.position;
        originalRightPos = RightWindow.transform.position;
        Vector3 centerPos = new Vector3(-0.384f, -0.01400006f, 0.0219999f);

        LeftWindow.gameObject.SetActive(false);
        RightWindow.gameObject.SetActive(false);
        RightWindow.gameObject.SetActive(true);
        SetFrameToId(currentSetId);

        RightWindow.SetHandleOneOrTwo(true);

        SetHandleToId(currentHandle);
    }

    public void SetDualWindowMode()
    {
        if (!isSingleWindowMode) return;

        isSingleWindowMode = false;

        LeftWindow.gameObject.SetActive(true);
        RightWindow.gameObject.SetActive(false);
        RightWindow.gameObject.SetActive(true);
        SetFrameToId(currentSetId);

        RightWindow.SetHandleOneOrTwo(false);

        SetHandleToId(currentHandle);
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
