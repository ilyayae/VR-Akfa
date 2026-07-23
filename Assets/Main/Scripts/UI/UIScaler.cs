using System.Collections;
using UnityEngine;

public class UIScaler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    Coroutine animCr;
    bool isOpen = false;
    float scaleX = 0;
    float duration = 0.25f;
    public ToggleButtons tb;
    public void openOrClose()
    {
        if(animCr != null)
        {
            StopCoroutine(animCr);
        }
        if (isOpen)
        {
            isOpen = false;
            scaleX = 0.0f;
        }
        else
        {
            isOpen = true;
            scaleX = 1.0f;
        }
        animCr = StartCoroutine(anim());
    }

    public IEnumerator anim()
    {
        if (scaleX == 1.0f)
        {
            tb.gameObject.SetActive(true);
        }
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            Vector3 currScale = transform.localScale;
            currScale.x = Mathf.Lerp(currScale.x, scaleX, t);
            transform.localScale = currScale;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        if(scaleX == 0.0f)
        {
            tb.gameObject.SetActive(false);
        }
    }

}
