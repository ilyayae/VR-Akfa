using System.Collections.Generic;
using UnityEngine;

public class TextureChanger : MonoBehaviour
{
    [SerializeField] private List<Renderer> whatToChange;
    [SerializeField] private Material TransparentMat;
    private bool X_Rayed = false;
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
        if (X_Rayed) return;
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
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    public void MakeXRayed()
    {
        if (X_Rayed) return;
        originalMaterials.Clear();
        X_Rayed = true;
        foreach(var r in GetComponentsInChildren<Renderer>())
        {
            if(r.tag != "DontTouchMat")
            {
                originalMaterials.Add(r, r.sharedMaterials);
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = TransparentMat;
                }
                r.sharedMaterials = mats;
            }
        }
    }

    public void UnmakeXRayed()
    {
        if (!X_Rayed) return;
        X_Rayed = false;
        foreach (KeyValuePair<Renderer, Material[]> kvp in originalMaterials)
        {
            if(kvp.Key != null)
                kvp.Key.sharedMaterials = kvp.Value;
        }
    }
}