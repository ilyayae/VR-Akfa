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
    public void SetTextureToId(int id)
    {
        currentTexture = id;
        if (currentInstantiatedFrame != null && currentInstantiatedFrame.gameObject.activeSelf == true)
        {
            TextureChanger texChanger = currentInstantiatedFrame.GetComponentInChildren<TextureChanger>();
            if (texChanger != null) texChanger.ChangeTextureBoth(materials[id].window);

            HingeChanger hingeChanger = currentInstantiatedFrame.GetComponentInChildren<HingeChanger>();
            if (hingeChanger != null) hingeChanger.ChangeTextureBoth(materials[id].hinge);
        }
        if (windowsill != null)
        {
            windowsill.material = materials[id].windowsill;
        }
        if(RightWindow != null && RightWindow.gameObject.activeSelf == true)
        {
            foreach (TextureChanger tchange in RightWindow.changers)
            {
                tchange.ChangeTextureBoth(materials[id].window);
            }
            foreach(HingeChanger tchange in RightWindow.hinges)
            {
                tchange.ChangeTextureBoth(materials[id].hinge);
            }
        }
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
        {
            foreach (TextureChanger tchange in LeftWindow.changers)
            {
                tchange.ChangeTextureBoth(materials[id].window);
            }
            foreach (HingeChanger tchange in LeftWindow.hinges)
            {
                tchange.ChangeTextureBoth(materials[id].hinge);
            }
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
            currentInstantiatedFrame.transform.position = Vector3.zero;
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;

            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelSingular, false);
            }
        }
        else
        {
            currentInstantiatedFrame = Instantiate(sets[id].frameModelDual, transform);
            currentInstantiatedFrame.transform.position = Vector3.zero;
            currentInstantiatedFrame.transform.localPosition = Vector3.zero;
            if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
            {
                RightWindow.NewWindow(sets[id].doorModelR, false);
            }
            if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
            {
                LeftWindow.NewWindow(sets[id].doorModelL, true);
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


    public void SetSingleWindowMode()
    {
        if (isSingleWindowMode) return;

        isSingleWindowMode = true;

        originalLeftPos = LeftWindow.transform.position;
        originalRightPos = RightWindow.transform.position;
        Vector3 centerPos = new Vector3(-0.384f, -0.01400006f, 0.0219999f);

        LeftWindow.gameObject.SetActive(false);
        RightWindow.gameObject.SetActive(false);
        RightWindow.transform.localPosition = centerPos;
        RightWindow.gameObject.SetActive(true);
        SetFrameToId(currentSetId);

        RightWindow.SetHandleOneOrTwo(true);

        SetTextureToId(currentTexture);
        SetHandleToId(currentHandle);
    }

    public void SetDualWindowMode()
    {
        if (!isSingleWindowMode) return;

        isSingleWindowMode = false;

        LeftWindow.gameObject.SetActive(true);
        RightWindow.gameObject.SetActive(false);
        RightWindow.transform.position = originalRightPos;
        RightWindow.gameObject.SetActive(true);
        SetFrameToId(currentSetId);

        LeftWindow.SetHandleOneOrTwo(false);
        RightWindow.SetHandleOneOrTwo(false);

        SetTextureToId(currentTexture);
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
