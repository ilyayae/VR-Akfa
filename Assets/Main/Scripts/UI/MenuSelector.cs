using System.Collections.Generic;
using UnityEngine;

public class MenuSelector : MonoBehaviour
{
    [SerializeField] List<GameObject> menus = new();

    public void SwitchToMenu(int id)
    {
        foreach (GameObject menu in menus)
        {
            menu.SetActive(false);
        }
        menus[id].SetActive(true);
    }
}
