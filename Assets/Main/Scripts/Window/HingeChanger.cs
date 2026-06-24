using System.Collections.Generic;
using UnityEngine;

public class HingeChanger : MonoBehaviour
{
    [SerializeField] private List<Renderer> whatToChange;
    public Transform RightHingeTransform;
    public Transform LeftHingeTransform;

    public Transform handle;
    public void ChangeTextureBoth(Material text)
    {
        UpdateTextures(text);
    }

    private void UpdateTextures(Material text)
    {
        foreach (Renderer r in whatToChange)
        {
            if (r != null && r.sharedMaterials.Length > 0)
            {
                Material[] mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = text;
                }
                r.materials = mats;
            }
        }
    }
}
