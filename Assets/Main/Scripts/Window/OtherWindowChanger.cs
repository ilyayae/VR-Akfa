using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OtherWindowChanger : MonoBehaviour
{
    public List<Renderer> windows;
    public void SetMaterial(Material material)
    {
        foreach(Renderer r in windows)
        {
            Material[] mats = r.materials;
            mats[0] = material;
            mats[1] = material;
            r.materials = mats;
        }
    }
}
