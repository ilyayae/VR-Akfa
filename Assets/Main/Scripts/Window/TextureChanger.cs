using System.Collections.Generic;
using UnityEngine;

public class TextureChanger : MonoBehaviour
{
    [SerializeField] private List<Renderer> whatToChange;
    public void ChangeTexture(Texture text)
    {
        foreach (Renderer r in whatToChange)
        {
            if (r != null && r.materials != null && r.materials.Length > 0)
            {
                int lastIndex = r.materials.Length - 1;
                r.materials[lastIndex].mainTexture = text;
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
