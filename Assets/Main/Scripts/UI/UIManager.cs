using UnityEngine;
using System.Collections;
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject FacadeSelection;
    private bool FacadeSelectionOpened = false;
    [SerializeField] private GameObject Menu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float scaleDuration = 0.3f;
    private Coroutine scaleCoroutine;

    public void OpenAndCloseFacadeSelection()
    {
        if (FacadeSelectionOpened)
        {
            FacadeSelectionOpened = false;
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false));
        }
        else
        {
            FacadeSelectionOpened = true;
            FacadeSelection.SetActive(true);
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true));
        }
    }

    private IEnumerator AnimateScale(Vector3 targetScale, bool isOpening)
    {
        Vector3 startScale = FacadeSelection.transform.localScale;
        float timeElapsed = 0f;

        while (timeElapsed < scaleDuration)
        {
            FacadeSelection.transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed / scaleDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        FacadeSelection.transform.localScale = targetScale;
        if (!isOpening)
        {
            FacadeSelection.SetActive(false);
        }
    }
}
