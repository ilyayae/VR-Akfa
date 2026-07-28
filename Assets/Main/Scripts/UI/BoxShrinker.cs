using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxShrinker : MonoBehaviour
{
    public List<GameObject> boxes;
    float duration = 0.125f;

    private Coroutine scaleCoroutine;

    public void ScaleTo(float targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    private IEnumerator AnimateScale(float targetScale)
    {
        float timeElapsed = 0f;

        Vector3[] startScales = new Vector3[boxes.Count];
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i] != null)
                startScales[i] = boxes[i].transform.localScale;
        }

        Vector3 finalScale = Vector3.one * targetScale;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            for (int i = 0; i < boxes.Count; i++)
            {
                if (boxes[i] != null)
                {
                    boxes[i].transform.localScale = Vector3.Lerp(startScales[i], finalScale, t);
                }
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i] != null)
                boxes[i].transform.localScale = finalScale;
        }
    }
}