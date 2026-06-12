using System.Collections.Generic;
using UnityEngine;

public class TextureChanger : MonoBehaviour
{
    [SerializeField] private List<Renderer> whatToChange;
    private const int OUTDOORS_MAT_INDEX = 1;
    private const int ROOM_MAT_INDEX = 2;

    public void ChangeTextureOutdoors(Texture text)
    {
        UpdateTextures(text, updateOutdoors: true, updateRoom: false);
    }

    public void ChangeTextureRoom(Texture text)
    {
        UpdateTextures(text, updateOutdoors: false, updateRoom: true);
    }

    public void ChangeTextureBoth(Texture text)
    {
        UpdateTextures(text, updateOutdoors: true, updateRoom: true);
    }

    private void UpdateTextures(Texture text, bool updateOutdoors, bool updateRoom)
    {
        foreach (Renderer r in whatToChange)
        {
            if (r != null && r.sharedMaterials.Length >= 3)
            {
                Material[] mats = r.materials;

                if (updateOutdoors)
                {
                    mats[OUTDOORS_MAT_INDEX].mainTexture = text;
                }

                if (updateRoom)
                {
                    mats[ROOM_MAT_INDEX].mainTexture = text;
                }
            }
        }
    }
}
