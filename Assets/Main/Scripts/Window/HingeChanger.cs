using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class HingeChanger : MonoBehaviour
{
    [Header("General settings")]
    [SerializeField] private List<Renderer> whatToChange;

    [Header("Frame settings")]
    public Transform RightHingeTransform;
    public Transform LeftHingeTransform;
    public Transform SmallHandTargetR;
    public AimConstraint BigHandR;
    public AimConstraint HingeR;
    public Transform SmallHandTargetL;
    public AimConstraint BigHandL;
    public AimConstraint HingeL;

    [Header("Door settings")]
    public Transform handleTransform;
    public Transform BigHandTarget;
    public Transform HingeTarget;
    public AimConstraint SmallHand;
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
