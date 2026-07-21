using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtons : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float darkenFactor = 0.75f;

    private List<Button> myButtons = new List<Button>();
    private List<ColorBlock> originalButtonColors = new List<ColorBlock>();
    [SerializeField] private Image select;
    RectTransform highlightImage;

    public Button currbutton;
    public float slideDuration = 0.25f;

    private Coroutine slideCoroutine;
    private int currentIndex = 0;
    private bool isSliding = false;

    void Awake()
    {
        if (select != null) highlightImage = select.GetComponent<RectTransform>();

        foreach (Transform child in transform)
        {
            Button potential = child.GetComponent<Button>();
            if (potential != null)
            {
                myButtons.Add(potential);
                originalButtonColors.Add(potential.colors);

                int index = myButtons.Count - 1;
                potential.onClick.AddListener(() => OnButtonClicked(index, false));
            }
        }
    }

    void OnEnable()
    {
        if (myButtons.Count > 0)
        {
            OnButtonClicked(currentIndex, true);
        }
    }

    void LateUpdate()
    {
        if (!isSliding && currbutton != null && highlightImage != null)
        {
            highlightImage.position = currbutton.GetComponent<RectTransform>().position;
        }
    }

    public void OnButtonClicked(int index, bool instant = false)
    {
        if (myButtons == null || myButtons.Count == 0 || index < 0 || index >= myButtons.Count)
            return;

        currentIndex = index;
        currbutton = myButtons[index];

        if (select != null)
        {
            SlideToButton(myButtons[index], instant);
        }
        else
        {
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
        }
    }

    public void SlideToButton(Button targetButton, bool instant = false)
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        RectTransform targetRect = targetButton.GetComponent<RectTransform>();

        if (instant)
        {
            isSliding = false;
            highlightImage.position = targetRect.position;
        }
        else
        {
            isSliding = true;
            slideCoroutine = StartCoroutine(AnimateSlide(targetRect));
        }
    }

    private IEnumerator AnimateSlide(RectTransform target)
    {
        Vector3 startPosition = highlightImage.position;

        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            highlightImage.position = Vector3.Lerp(startPosition, target.position, smoothT);
            yield return null;
        }
        highlightImage.position = target.position;
        isSliding = false;
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