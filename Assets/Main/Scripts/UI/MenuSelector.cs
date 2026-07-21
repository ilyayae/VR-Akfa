using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuSelector : MonoBehaviour
{
    [SerializeField] List<GameObject> menus = new();

    Coroutine animCrOut;
    Coroutine animCrIn;
    float duration = 0.25f;
    public void SwitchToMenu(int id)
    {
        if (!menus[id].activeSelf)
        {
            foreach (GameObject menu in menus)
            {
                if(menu.activeSelf)
                {
                    if(animCrOut != null) StopCoroutine(animCrOut);
                    animCrOut = StartCoroutine(animFade(menu));
                }    
            }
            if(animCrIn != null) StopCoroutine(animCrIn);
            animCrIn = StartCoroutine(animFade(menus[id]));
        }
    }

    private IEnumerator animFade(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        float startAlpha;
        float targetAlpha;
        if (target.activeSelf)
        {
            startAlpha = canvasGroup.alpha;
            targetAlpha = 0f;
        }
        else
        {
            targetAlpha = 1f;
            startAlpha = 0f;
            canvasGroup.alpha = 0f;
            target.SetActive(true);
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
        if (targetAlpha == 0f)
        {
            target.SetActive(false);
        }
    }
}
