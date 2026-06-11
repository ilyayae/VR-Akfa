using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private List<WindowSet> sets = new();
    [SerializeField] private List<GameObject> handlesR = new();
    [SerializeField] private List<GameObject> handlesL = new();
    [SerializeField] private List<GameObject> glass = new();
    [SerializeField] private List<Texture> Textures = new();


    [SerializeField] private WindowBrain LeftWindow;
    [SerializeField] private WindowBrain RightWindow;
    [Header("Blockers (Optional)")]
    [Tooltip("Assign prefabs/models here. If left empty, simple cubes will be generated automatically.")]
    [SerializeField] private GameObject leftBlocker;
    [SerializeField] private GameObject rightBlocker;

    private Vector3 originalLeftPos;
    private Vector3 originalRightPos;
    private bool isSingleWindowMode = false;
    private void Start()
    {
        SetupBlockers();
        SetHandleToId(0);
        SetFrameToId(0);
        SetTextureToId(0);
        SetGlassToId(0);
    }

    public void SetTextureToId(int id)
    {
        if(RightWindow != null && RightWindow.gameObject.activeSelf == true)
        {
            RightWindow.changers[0].ChangeTexture(Textures[id]);
        }
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
        {
            LeftWindow.changers[0].ChangeTexture(Textures[id]);
        }
    }

    public void SetHandleToId(int id)
    {
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
        if (RightWindow != null && RightWindow.gameObject.activeSelf == true)
        {
            RightWindow.NewWindow(sets[id].doorModel, sets[id].frameModel, false);
        }
        if (LeftWindow != null && LeftWindow.gameObject.activeSelf == true)
        {
            LeftWindow.NewWindow(sets[id].doorModel, sets[id].frameModel, true);
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
        originalLeftPos = LeftWindow.transform.position;
        originalRightPos = RightWindow.transform.position;
        Vector3 centerPos = (originalLeftPos + originalRightPos) / 2f;
        LeftWindow.gameObject.SetActive(false);
        RightWindow.transform.position = centerPos;
        leftBlocker.transform.position = originalLeftPos;
        rightBlocker.transform.position = originalRightPos;

        leftBlocker.SetActive(true);
        rightBlocker.SetActive(true);

        isSingleWindowMode = true;
    }
    public void SetDualWindowMode()
    {
        if (!isSingleWindowMode) return;
        LeftWindow.gameObject.SetActive(true);
        RightWindow.transform.position = originalRightPos;
        leftBlocker.SetActive(false);
        rightBlocker.SetActive(false);

        isSingleWindowMode = false;
    }
    private void SetupBlockers()
    {
        Vector3 defaultBoxScale = new Vector3(0.3f, 3f, 0.1f);

        if (leftBlocker == null)
        {
            leftBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftBlocker.name = "Left Space Blocker";
            leftBlocker.transform.localScale = defaultBoxScale;
        }

        if (rightBlocker == null)
        {
            rightBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightBlocker.name = "Right Space Blocker";
            rightBlocker.transform.localScale = defaultBoxScale;
        }

        leftBlocker.SetActive(false);
        rightBlocker.SetActive(false);
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
