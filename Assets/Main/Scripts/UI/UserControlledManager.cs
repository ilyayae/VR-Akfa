using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UserControlledManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject FacadeSelection;
    [SerializeField] private GameObject OutsideMenu;
    [SerializeField] private GameObject InsideMenu;
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

    public void OpenAndCloseFacadeSelection()
    {
        if (FacadeSelectionOpened)
        {
            FacadeSelectionOpened = false;
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0f, 1f), false, FacadeSelection));
        }
        else
        {
            FacadeSelectionOpened = true;
            FacadeSelection.SetActive(true);
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(0.0005f, 0.0005f, 1f), true, FacadeSelection));
        }
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
