using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageUpkeep : MonoBehaviour
{
    public ToggleButtons tb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        Locale l = LocalizationSettings.SelectedLocale;
        string code = l.Identifier.Code;
        if(code == "en")
        {
            tb.OnButtonClicked(0);
        } else if (code == "uz")
        {
            tb.OnButtonClicked(1);
        } else if (code == "ru")
        {
            tb.OnButtonClicked(2);
        } else 
        {
            Debug.LogWarning("SOME STRANGE LOCALE DETECTED");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
