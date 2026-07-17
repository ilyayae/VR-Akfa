using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtons : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float darkenFactor = 0.75f;

    private List<Button> myButtons = new List<Button>();
    private List<ColorBlock> originalButtonColors = new List<ColorBlock>();

    void Awake()
    {
        foreach (Transform child in transform)
        {
            Button potential = child.GetComponent<Button>();
            if (potential != null)
            {
                myButtons.Add(potential);
                originalButtonColors.Add(potential.colors);

                int index = myButtons.Count - 1;
                potential.onClick.AddListener(() => OnButtonClicked(index));
            }
        }

        if (myButtons.Count > 0)
        {
            OnButtonClicked(0);
        }
    }
    public Button currbutton;
    public void OnButtonClicked(int index)
    {
        if (myButtons == null || myButtons.Count == 0 || index < 0 || index >= myButtons.Count)
            return;
        for (int i = 0; i < myButtons.Count; i++)
        {
            if (i == index)
            {
                myButtons[i].colors = DarkenColorBlock(originalButtonColors[i], darkenFactor);
            }
            else
            {
                myButtons[i].colors = originalButtonColors[i];
            }
        }
        currbutton = myButtons[index];
    }
    private ColorBlock DarkenColorBlock(ColorBlock original, float factor)
    {
        ColorBlock darkened = original;

        darkened.normalColor = original.normalColor * factor;
        darkened.highlightedColor = original.highlightedColor * factor;
        darkened.pressedColor = original.pressedColor * factor;
        darkened.selectedColor = original.selectedColor * factor;
        darkened.disabledColor = original.disabledColor * factor;

        return darkened;
    }
}