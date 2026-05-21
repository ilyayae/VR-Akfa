using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FacadeSelection : MonoBehaviour
{
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private GameObject content;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public int currentID = 0;

    private RectTransform contentRect;
    private bool isAnimating = false;
    private float stepLength = 0f;
    private Vector2 originalPos;

    void Start()
    {
        if (scroll != null)
        {
            scroll.horizontal = false;
            scroll.vertical = false;
        }

        contentRect = content.GetComponent<RectTransform>();
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Sprite sprite in sprites)
        {
            GameObject obj = Instantiate(imagePrefab, content.transform);
            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
            }
        }

        currentID = 0;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        originalPos = contentRect.anchoredPosition;
        if (content.transform.childCount > 1)
        {
            RectTransform first = content.transform.GetChild(0) as RectTransform;
            RectTransform second = content.transform.GetChild(1) as RectTransform;
            stepLength = Mathf.Abs(second.anchoredPosition.x - first.anchoredPosition.x);
        }
    }

    public void GoToNextImage()
    {
        if (isAnimating || sprites.Count <= 1) return;
        StartCoroutine(SlideNext());
    }

    public void GoToPreviousImage()
    {
        if (isAnimating || sprites.Count <= 1) return;
        StartCoroutine(SlidePrev());
    }

    private IEnumerator SlideNext()
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector2 targetPos = originalPos + new Vector2(-stepLength, 0);
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(elapsed / slideDuration);
            contentRect.anchoredPosition = Vector2.Lerp(originalPos, targetPos, t);
            yield return null;
        }
        content.transform.GetChild(0).SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        contentRect.anchoredPosition = originalPos;
        currentID = (currentID + 1) % sprites.Count;
        isAnimating = false;
    }

    private IEnumerator SlidePrev()
    {
        isAnimating = true;
        int lastIndex = content.transform.childCount - 1;
        content.transform.GetChild(lastIndex).SetAsFirstSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Vector2 startPos = originalPos + new Vector2(-stepLength, 0);
        contentRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(elapsed / slideDuration);
            contentRect.anchoredPosition = Vector2.Lerp(startPos, originalPos, t);
            yield return null;
        }

        contentRect.anchoredPosition = originalPos;
        currentID = (currentID - 1 + sprites.Count) % sprites.Count;
        isAnimating = false;
    }
}