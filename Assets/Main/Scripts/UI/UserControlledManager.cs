using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class UserControlledManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject FacadeSelection;
    [SerializeField] private GameObject OutsideMenu;
    [SerializeField] private GameObject InsideMenu;
    [SerializeField] private GameObject WindowSelection;
    public float scaleDuration = 0.3f;
    private bool FacadeSelectionOpened = false;
    private Coroutine scaleCoroutine;

    [Header("Facades")]
    [SerializeField] private List<GameObject> listOfFacades;
    [SerializeField] private Transform chosenFacadeLoaction;
    [SerializeField] private Transform stashedFacadeLoaction;
    [SerializeField] private Transform insideLocation;
    [SerializeField] private Transform outsideLocation;
    [SerializeField] private GameObject GameManager; //Move it to teleport the player
    private int currentFacadeID;


    [Header("UI Transforms")]
    [SerializeField] private Transform Outside_Center;
    [SerializeField] private Transform Outside_Left;
    [SerializeField] private Transform Outside_Right;
    [SerializeField] private Transform Inside_Center;
    [SerializeField] private Transform Inside_Left;
    [SerializeField] private Transform Inside_Right;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(GameObject facade in listOfFacades)
        {
            facade.transform.position = stashedFacadeLoaction.position;
            facade.gameObject.SetActive(false);
        }
        listOfFacades[0].gameObject.SetActive(true);
        listOfFacades[0].transform.position = chosenFacadeLoaction.position;
    }
    public void ChangeFacadeBySelection()
    {
        ChangeFacadeById(FacadeSelection.GetComponent<FacadeSelection>().currentID);
    }
    public void ChangeFacadeById(int id)
    {
        StartCoroutine(ChangeFacadeAnimation(id));
    }
    public IEnumerator ChangeFacadeAnimation(int id)
    {
        yield return null;
        listOfFacades[currentFacadeID].transform.position = stashedFacadeLoaction.position;
        listOfFacades[currentFacadeID].SetActive(false);
        listOfFacades[id].transform.position = chosenFacadeLoaction.position;
        listOfFacades[id].SetActive(true);
        currentFacadeID = id;
    }
    public void goInside()
    {
        StartCoroutine(TeleportTo(insideLocation.transform));
        StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, FacadeSelection));
        StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, OutsideMenu));
        StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true, InsideMenu));
    }
    public void goOutside()
    {
        StartCoroutine(TeleportTo(outsideLocation.transform));
        //StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, WindowConstructorMenu));
        StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, InsideMenu));
        StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true, OutsideMenu));
    }
    public IEnumerator TeleportTo(Transform transform)
    {
        yield return null;
        GameManager.transform.position = transform.position;
        GameManager.transform.rotation = transform.rotation;
    }

    // 0 = center, 1 = left, 2 = right
    int currentPlaceFacade = 0;
    int currentPlaceWindow = 0;
    public void OpenAndCloseFacadeSelection(int place)
    {
        Vector3 pos;
        switch (place)
        {
            case 0:
                pos = Outside_Center.position;
                break;
            case 1:
                pos = Outside_Left.position;
                break;
            case 2:
                pos = Outside_Right.position;
                break;
            default:
                pos = Outside_Center.position;
                break;
        }
        if (FacadeSelection.activeSelf)
        {
            if (currentPlaceFacade == place)
            {
                if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, FacadeSelection));
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
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true, FacadeSelection));
        }
        currentPlaceFacade = place;
    }
    public void OpenAndCloseWindowSelection(int place)
    {
        Vector3 pos;
        switch (place)
        {
            case 0:
                pos = Inside_Center.position;
                break;
            case 1:
                pos = Inside_Left.position;
                break;
            case 2:
                pos = Inside_Right.position;
                break;
            default:
                pos = Inside_Center.position;
                break;
        }
        if (WindowSelection.activeSelf)
        {
            if (currentPlaceWindow == place)
            {
                if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, WindowSelection));
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
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true, WindowSelection));
        }
        currentPlaceWindow = place;
    }
    private IEnumerator AnimateCloseOpen(Vector3 position, GameObject toScale)
    {
        yield return StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, toScale));
        yield return null;
        toScale.transform.position = position;
        yield return null;
        yield return StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true, toScale));
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
