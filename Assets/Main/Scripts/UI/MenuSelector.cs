using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSelector : MonoBehaviour
{
    [SerializeField] List<GameObject> menus = new();

    private Dictionary<GameObject, Coroutine> activeCoroutines = new Dictionary<GameObject, Coroutine>();
    float duration = 0.25f;
    int currMenu = 0;

    // FIX: Clean up broken states if the object was disabled mid-fade
    private void OnEnable()
    {
        if (menus.Count > 0)
        {
            activeCoroutines.Clear(); // Clear dead coroutines
            for (int i = 0; i < menus.Count; i++)
            {
                ApplyInstantState(menus[i], i == currMenu);
            }
        }
    }

    public void SwitchToMenuNext()
    {
        int nextid = currMenu + 1;
        if (nextid >= menus.Count)
        {
            nextid = 0;
        }
        SwitchToMenu(nextid);
    }

    public void SwitchToMenu(int id)
    {
        // FIX: Allow switching if it's not the current menu, even if it's currently active (fading out)
        if (currMenu != id || !menus[id].activeSelf)
        {
            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].activeSelf && i != id)
                {
                    StartFade(menus[i], false);
                }
            }
            currMenu = id;
            StartFade(menus[id], true);
        }
    }

    private void StartFade(GameObject menu, bool isFadingIn)
    {
        // FIX: If the object is inactive, DO NOT start a Coroutine. Apply changes instantly.
        if (!gameObject.activeInHierarchy)
        {
            ApplyInstantState(menu, isFadingIn);
            return;
        }

        if (activeCoroutines.ContainsKey(menu) && activeCoroutines[menu] != null)
        {
            StopCoroutine(activeCoroutines[menu]);
        }

        activeCoroutines[menu] = StartCoroutine(animFade(menu, isFadingIn));
    }

    // FIX: New helper function to instantly snap visuals (bypassing animations)
    private void ApplyInstantState(GameObject target, bool isFadingIn)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = target.AddComponent<CanvasGroup>();

        if (isFadingIn) target.SetActive(true);
        canvasGroup.alpha = isFadingIn ? 1f : 0f;

        BoxShrinker bs = target.GetComponent<BoxShrinker>();
        if (bs != null) bs.ScaleTo(isFadingIn ? 1f : 0f);

        if (!isFadingIn) target.SetActive(false);
    }

    private IEnumerator animFade(GameObject target, bool isFadingIn)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }
        if (isFadingIn) target.SetActive(true);

        float startAlpha = canvasGroup.alpha;
        float targetAlpha = isFadingIn ? 1f : 0f;

        BoxShrinker bs = target.GetComponent<BoxShrinker>();
        if (bs != null)
        {
            bs.ScaleTo(isFadingIn ? 1f : 0f);
        }

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (!isFadingIn)
        {
            target.SetActive(false);
        }
    }
}