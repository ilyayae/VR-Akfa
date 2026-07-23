using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UserControlledManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject FacadeSelection;
    [SerializeField] private GameObject WindowSelection;
    [SerializeField] private GameObject outsideMenu;
    [SerializeField] private GameObject insideMenu;
    public float scaleDuration = 0.3f;
    private Coroutine scaleCoroutine;

    [SerializeField] private Transform insideLocation;
    [SerializeField] private Transform outsideLocation;
    [SerializeField] private GameObject GameManager;
    private int currentFacadeID;


    [Header("UI Transforms")]
    [SerializeField] private Transform Outside_Left;
    [SerializeField] private Transform Outside_Right;
    [SerializeField] private Transform Inside_Left;
    [SerializeField] private Transform Inside_Right;
    public bool isInside = false;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static UserControlledManager Instance { get; private set; }
    void Start()
    {
        Instance = this;
        ChangeFacadeById(0);
        SceneManager.LoadScene("Exterior", LoadSceneMode.Additive);
        SceneManager.LoadScene("Room", LoadSceneMode.Additive);
    }
    public void ChangeFacadeById(int id)
    {
        StartCoroutine(ChangeFacadeAnimation(id));
    }
    public IEnumerator ChangeFacadeAnimation(int id)
    {
        yield return null;
        FacadeManager.Instance.SetActiveFacadeByID(id);
        currentFacadeID = id;
    }
    
    public void goInside()
    {
        isInside = true;
        StartCoroutine(TeleportTo(insideLocation.transform));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, FacadeSelection));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, outsideMenu));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, insideMenu));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, WindowSelection));
    }
    public void goOutside()
    {
        isInside = false;
        StartCoroutine(TeleportTo(outsideLocation.transform));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, FacadeSelection));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, outsideMenu));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, insideMenu));
        StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, WindowSelection));
    }
    public void ChangeLanguageByIndex(int localeIndex)
    {
        StartCoroutine(SetLocaleCoroutine(localeIndex));
    }
    private IEnumerator SetLocaleCoroutine(int index)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (index >= 0 && index < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }
    public IEnumerator TeleportTo(Transform targetTransform)
    {
        float timer = 0f;
        Color fadeColor = fadeImage.color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }
        fadeColor.a = 1f;
        fadeImage.color = fadeColor;
        GameManager.transform.position = targetTransform.position;
        GameManager.transform.rotation = targetTransform.rotation;
        yield return new WaitForSeconds(0.1f);
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }
        fadeColor.a = 0f;
        fadeImage.color = fadeColor;
    }

    // 0 = center, 1 = left, 2 = right
    int currentPlaceFacade = 0;
    int currentPlaceWindow = 0;
    public void closeItFacade()
    {
        OpenAndCloseFacadeSelection(currentPlaceFacade);
    }
    public void OpenAndCloseFacadeSelection(int place)
    {
        Vector3 pos;
        switch (place)
        {
            case 1:
                pos = Outside_Left.position;
                break;
            case 0:
                pos = Outside_Right.position;
                break;
            default:
                pos = Outside_Left.position;
                break;
        }
        if (FacadeSelection.activeSelf)
        {
            if (currentPlaceFacade == place)
            {
                if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, FacadeSelection));
            }
            else
            {
                scaleCoroutine = StartCoroutine(AnimateCloseOpen(pos, FacadeSelection));
            }
        }
        else
        {
            FacadeSelection.transform.position = pos;
            FacadeSelection.SetActive(true);
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, FacadeSelection));
        }
        currentPlaceFacade = place;
    }
    public void closeItWindow()
    {
        OpenAndCloseWindowSelection(currentPlaceWindow);   
    }
    public void OpenAndCloseWindowSelection(int place)
    {
        Vector3 pos;
        switch (place)
        {
            case 1:
                pos = Inside_Left.position;
                break;
            case 0:
                pos = Inside_Right.position;
                break;
            default:
                pos = Inside_Left.position;
                break;
        }
        if (WindowSelection.activeSelf)
        {
            if (currentPlaceWindow == place)
            {
                if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, WindowSelection));
            }
            else
            {
                scaleCoroutine = StartCoroutine(AnimateCloseOpen(pos, WindowSelection));
            }
        }
        else
        {
            WindowSelection.transform.position = pos;
            WindowSelection.SetActive(true);
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, WindowSelection));
        }
        currentPlaceWindow = place;
    }
    private IEnumerator AnimateCloseOpen(Vector3 position, GameObject toScale)
    {
        yield return StartCoroutine(AnimateScale(new Vector3(0.001f, 0f, 1f), false, toScale));
        yield return null;
        toScale.transform.position = position;
        yield return null;
        yield return StartCoroutine(AnimateScale(new Vector3(0.001f, 0.001f, 1f), true, toScale));
    }
    private IEnumerator AnimateScale(Vector3 targetScale, bool isOpening, GameObject toScale)
    {
        if (isOpening)
        {
            toScale.SetActive(true);
        }

        Vector3 startScale = toScale.transform.localScale;
        float timeElapsed = 0f;

        while (timeElapsed < scaleDuration)
        {
            toScale.transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed / scaleDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toScale.transform.localScale = targetScale;
        if (!isOpening)
        {
            toScale.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
