using System.Collections.Generic;
using UnityEngine;

public class TextureChanger : MonoBehaviour
{
    [SerializeField] private List<Renderer> whatToChange;

    // The last material is always roomside, the first material is always inside, if there are three materials the one in the middle is always outside
    // inside and should not be changed. 
    public void ChangeTextureOutdoors(Material text)
    {
        UpdateTextures(text, updateOutdoors: true, updateRoom: false);
    }

    public void ChangeTextureRoom(Material text)
    {
        UpdateTextures(text, updateOutdoors: false, updateRoom: true);
    }

    public void ChangeTextureBoth(Material text)
    {
        UpdateTextures(text, updateOutdoors: true, updateRoom: true);
    }

    private void UpdateTextures(Material text, bool updateOutdoors, bool updateRoom)
    {
        foreach (Renderer r in whatToChange)
        {
            if (r == null || r.sharedMaterials.Length == 0) continue;

            Material[] mats = r.materials;
            int length = mats.Length;
            bool materialsChanged = false;
            if (updateOutdoors && length >= 3)
            {
                mats[1] = text;
                materialsChanged = true;
            }
            if (updateRoom && length >= 2)
            {
                mats[length - 1] = text;
                materialsChanged = true;
            }
            if (materialsChanged)
            {
                r.materials = mats;
            }
        }
    }
}