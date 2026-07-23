using UnityEngine;
using UnityEngine.UI;

public class ToggleOneButton : MonoBehaviour
{
    public Sprite on;
    public Sprite off;
    public Image img;
    public void press(bool p)
    {
        if(p)
        {
            img.sprite = on;
        }
        else
        {
            img.sprite = off;
        }
    }
}
