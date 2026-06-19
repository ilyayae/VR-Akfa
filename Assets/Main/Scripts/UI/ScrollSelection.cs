using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollSelection : MonoBehaviour
{
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private GameObject content;

    [Header("Grid Settings")]
    [SerializeField] private int rows = 2;
    [SerializeField] private int columnsToScroll = 1;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform contentRect;
    private bool isAnimating = false;
    private float stepLength = 0f;
    private Vector2 originalPos;
    private int itemsToMove;

    void Start()
    {
        if (scroll != null)
        {
            scroll.horizontal = false;
            scroll.vertical = false;
        }

        contentRect = content.GetComponent<RectTransform>();

        itemsToMove = rows * columnsToScroll;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        originalPos = contentRect.anchoredPosition;

        if (content.transform.childCount > itemsToMove)
        {
            RectTransform firstCol = content.transform.GetChild(0) as RectTransform;

            RectTransform nextCol = content.transform.GetChild(rows) as RectTransform;

            float singleColStep = Mathf.Abs(nextCol.anchoredPosition.x - firstCol.anchoredPosition.x);
            stepLength = singleColStep * columnsToScroll;
        }
        GoToNextImage();
    }

    public void GoToNextImage()
    {
        if (isAnimating || content.transform.childCount <= itemsToMove) return;
        StartCoroutine(SlideNext());
        setAvtiveItems();
    }

    public void GoToPreviousImage()
    {
        if (isAnimating || content.transform.childCount <= itemsToMove) return;
        StartCoroutine(SlidePrev());
        setAvtiveItems();
    }
    int countOfActives = 8;
    private void setAvtiveItems()
    {
        int currentCount = 0;
        foreach(Transform child in content.transform)
        {
            if(currentCount < countOfActives)
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
            currentCount++;
        }
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

        for (int i = 0; i < itemsToMove; i++)
        {
            content.transform.GetChild(0).SetAsLastSibling();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        contentRect.anchoredPosition = originalPos;

        isAnimating = false;
    }

    private IEnumerator SlidePrev()
    {
        isAnimating = true;

        for (int i = 0; i < itemsToMove; i++)
        {
            int lastIndex = content.transform.childCount - 1;
            content.transform.GetChild(lastIndex).SetAsFirstSibling();
        }

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
        isAnimating = false;
    }
}