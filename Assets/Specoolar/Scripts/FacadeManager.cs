using UnityEngine;
using UnityEngine.SceneManagement;

public class FacadeManager : MonoBehaviour
{
    public string[] facadeSceneNames;

    FacadeData[] facadeSettings;

    public class FacadeData
    {
        public string name;
        public bool enabled = false;
        public FacadeScene facade;
        public OtherWindowChanger changer;
    }
    public static FacadeManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        facadeSettings = new FacadeData[facadeSceneNames.Length];

        for (int i = 0; i < facadeSettings.Length; i++)
        {
            FacadeData item = new();
            item.name = facadeSceneNames[i];
            item.enabled = false;
            SceneManager.LoadScene(item.name, LoadSceneMode.Additive);
            facadeSettings[i] = item;
        }
    }

    public void OnLoadFacade(FacadeScene facade)
    {
        var data = getFacade(facade.sceneName);
        data.facade = facade;
        data.changer = facade.changer;
        onFacadeUpdate(data);
    }
    public void SetActiveFacadeByID(int id)
    {
        SetActiveFacade(facadeSceneNames[id]);
    }
    public void SetActiveFacade(string facadeName)
    {
        foreach (var item in facadeSettings)
        {
            item.enabled = item.name == facadeName;
            onFacadeUpdate(item);
        }
    }
    public void setMaterials(Material material)
    {
        foreach(var facade in facadeSettings)
        {
            if(facade.changer != null)
                facade.changer.SetMaterial(material);
        }
    }
    FacadeData getFacade(string name)
    {
        foreach (var item in facadeSettings)
            if (name == item.name)
                return item;
        return null;
    }

    void onFacadeUpdate(FacadeData data)
    {
        if (data.facade != null)
        {
            data.facade.gameObject.SetActive(data.enabled);
            if (data.enabled) data.facade.LoadLighting();
        }
    }
}
