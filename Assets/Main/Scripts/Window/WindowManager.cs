using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private WindowBrain LeftWindow;
    [SerializeField] private WindowBrain RightWindow;
    [SerializeField] private List<WindowSet> sets = new();
    [SerializeField] private List<GameObject> handles = new();
    [SerializeField] private List<Texture> Textures = new();

    private void Start()
    {
        RightWindow.NewHandle(handles[0]);
        RightWindow.NewWindow(sets[0].doorModel, sets[0].frameModel);
        SetTextureToId(0);
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
