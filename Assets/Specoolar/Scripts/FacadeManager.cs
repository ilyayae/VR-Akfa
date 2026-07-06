using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Rendering.ShadowCascadeGUI;

public class FacadeManager : MonoBehaviour
{
    public string[] facadeSceneNames;

    FacadeData[] facadeSettings;

    public class FacadeData
    {
        public string name;
        public bool enabled = false;
        public FacadeScene facade;
    }
    public static FacadeManager main = null;
    private void Awake()
    {
        main = this;
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
        onFacadeUpdate(data);
    }

    public void SetActiveFacade(string facadeName)
    {
        foreach (var item in facadeSettings)
        {
            item.enabled = item.name == facadeName;
            onFacadeUpdate(item);
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
