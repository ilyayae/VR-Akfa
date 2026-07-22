using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceImage : MonoBehaviour
{
    public List<Texture> textures;

    public void SetTextureToID(int id)
    {
        RawImage img = GetComponent<RawImage>();
        if (img != null && id < textures.Count)
        {
            img.texture = textures[id];
        }
    }
}
