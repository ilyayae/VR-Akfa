using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GameManager : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private Gradient LaserColor_Normal;
    [SerializeField] private Gradient LaserColor_HowerUI;
    [SerializeField] private Gradient LaserColor_PressUI;
    [SerializeField] private Gradient LaserColor_HowerWindow;
    [SerializeField] private Gradient LaserColor_PressWindow;
    [SerializeField] private Gradient LaserColor_TeleportAppropriate;
    [SerializeField] private Gradient LaserColor_TeleportInapropriate;
    [SerializeField] private HandSnap leftSnap;
    [SerializeField] private HandSnap rightSnap;
    [Space]
    [Header("Audio Settings")]
    [SerializeField] private AudioClip AmbientClip;
    [SerializeField] private AudioClip UI_Activate;
    [SerializeField] private AudioClip UI_AlreadyActivated;
    [SerializeField] private AudioClip TP_Teleport;
    [SerializeField] private AudioClip TP_CantTeleport;
    [SerializeField] private AudioClip TP_TeleportHumm; //Continious loop of sound
    [SerializeField] private AudioClip Grab_Object;
    [Space]
    [Header("Audio Sources")]
    [SerializeField] private AudioSource AmbientSource;
    [SerializeField] private AudioSource SpecialEffectsSource;
    [SerializeField] private AudioSource LeftTeleportSource;
    [SerializeField] private AudioSource RightTeleportSource;
    [SerializeField] private AudioSource LeftHandSource;
    [SerializeField] private AudioSource RightHandSource;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AmbientSource.clip = AmbientClip;
        LeftTeleportSource.clip = TP_TeleportHumm;
        RightTeleportSource.clip = TP_TeleportHumm;
        LeftHandSource.clip = Grab_Object;
        RightHandSource.clip = Grab_Object;

        leftSnap.Normal = LaserColor_Normal;
        leftSnap.HowerUI = LaserColor_HowerUI;
        leftSnap.PressUI = LaserColor_PressUI;
        leftSnap.HoverWindow = LaserColor_HowerWindow;
        leftSnap.PressWindow = LaserColor_PressWindow;
        leftSnap.teleportAppropriate = LaserColor_TeleportAppropriate;
        leftSnap.teleportInappropriate = LaserColor_TeleportInapropriate;
        leftSnap.setGradient();
        rightSnap.Normal = LaserColor_Normal;
        rightSnap.HowerUI = LaserColor_HowerUI;
        rightSnap.PressUI = LaserColor_PressUI;
        rightSnap.HoverWindow = LaserColor_HowerWindow;
        rightSnap.PressWindow = LaserColor_PressWindow;
        rightSnap.teleportAppropriate = LaserColor_TeleportAppropriate;
        rightSnap.teleportInappropriate = LaserColor_TeleportInapropriate;
        rightSnap.setGradient();
    }

    public void PlayUIActivate()
    {
        if (UI_Activate != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(UI_Activate);
    }

    public void PlayUIAlreadyActivated()
    {
        if (UI_AlreadyActivated != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(UI_AlreadyActivated);
    }

    public void PlayTeleport()
    {
        if (TP_Teleport != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(TP_Teleport);
    }

    public void PlayCantTeleport()
    {
        if (TP_CantTeleport != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(TP_CantTeleport);
    }

    public void UngrabRight()
    {
        ForceUngrab(rightSnap);
    }

    public void UngrabLeft()
    {
        ForceUngrab(leftSnap);
    }

    private void ForceUngrab(HandSnap handSnap)
    {
        if (handSnap == null) return;
        XRBaseInteractor[] interactors = handSnap.GetComponentsInChildren<XRBaseInteractor>(true);
        if (interactors.Length == 0)
        {
            interactors = handSnap.GetComponentsInParent<XRBaseInteractor>(true);
        }

        foreach (var interactor in interactors)
        {
            if (interactor == null) continue;
            if (interactor.interactionManager != null && interactor.hasSelection)
            {
                interactor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)interactor);
            }
            StartCoroutine(DisableAndReenableInteractor(interactor));
        }
    }

    private IEnumerator DisableAndReenableInteractor(XRBaseInteractor interactor)
    {
        if (interactor == null) yield break;

        interactor.enabled = false;
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        if (interactor != null)
        {
            interactor.enabled = true;
        }
    }

// Update is called once per frame
void Update()
    {
        
    }
}
