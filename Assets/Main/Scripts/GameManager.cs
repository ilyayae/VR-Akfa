using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[System.Serializable]
public class inputConfig
{
    public string inputName = "New Input Configuration";
    public KeyCode holdKey = KeyCode.LeftShift;
    public UnityEvent onKeyPressed;
    public UnityEvent<int> onNumberSubmitted;

    private string currentTypedString = "";
    private bool isHolding = false;

    public void ProcessInput()
    {
        if (Input.GetKeyDown(holdKey))
        {
            onKeyPressed?.Invoke();
            isHolding = true;
            currentTypedString = "";
        }
        if (isHolding)
        {
            CheckForNumberKeys();
        }
        if (Input.GetKeyUp(holdKey))
        {
            isHolding = false;

            if (!string.IsNullOrEmpty(currentTypedString))
            {
                if (int.TryParse(currentTypedString, out int result))
                {
                    if (result - 1 >= 0)
                        onNumberSubmitted?.Invoke(result - 1);
                }
                else
                {
                    Debug.LogWarning($"Number typed ({currentTypedString}) was too large for an integer!");
                }

                currentTypedString = "";
            }
        }
    }

    private void CheckForNumberKeys()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                currentTypedString += i.ToString();
            }
        }
    }
}

[System.Serializable]
public struct AudioData
{
    public AudioClip clip;
    public float volume;
}

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
    [SerializeField] private AudioData AmbientClip = new AudioData { volume = 1f };
    [SerializeField] private AudioData UI_Activate = new AudioData { volume = 1f };
    [SerializeField] private AudioData UI_AlreadyActivated = new AudioData { volume = 1f };
    [SerializeField] private AudioData TP_Teleport = new AudioData { volume = 1f };
    [SerializeField] private AudioData TP_CantTeleport = new AudioData { volume = 1f };
    [SerializeField] private AudioData TP_TeleportHumm = new AudioData { volume = 1f }; // Continuous loop of sound
    [SerializeField] private AudioData Grab_Object = new AudioData { volume = 1f };
    [Space]
    [Header("Audio Sources")]
    [SerializeField] private AudioSource AmbientSource;
    [SerializeField] private AudioSource SpecialEffectsSource;
    [SerializeField] private AudioSource LeftTeleportSource;
    [SerializeField] private AudioSource RightTeleportSource;
    [SerializeField] private AudioSource LeftHandSource;
    [SerializeField] private AudioSource RightHandSource;
    [Space]
    [Header("HOTKEY Config, dont touch without knowledge")]
    public List<inputConfig> configs;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Apply both the clip and the specific volume to persistent AudioSources
        AmbientSource.clip = AmbientClip.clip;
        AmbientSource.volume = AmbientClip.volume;

        LeftTeleportSource.clip = TP_TeleportHumm.clip;
        LeftTeleportSource.volume = TP_TeleportHumm.volume;

        RightTeleportSource.clip = TP_TeleportHumm.clip;
        RightTeleportSource.volume = TP_TeleportHumm.volume;

        LeftHandSource.clip = Grab_Object.clip;
        LeftHandSource.volume = Grab_Object.volume;

        RightHandSource.clip = Grab_Object.clip;
        RightHandSource.volume = Grab_Object.volume;

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
        if (UI_Activate.clip != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(UI_Activate.clip, UI_Activate.volume);
    }

    public void PlayUIAlreadyActivated()
    {
        if (UI_AlreadyActivated.clip != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(UI_AlreadyActivated.clip, UI_AlreadyActivated.volume);
    }

    public void PlayTeleport()
    {
        if (TP_Teleport.clip != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(TP_Teleport.clip, TP_Teleport.volume);
    }

    public void PlayCantTeleport()
    {
        if (TP_CantTeleport.clip != null && SpecialEffectsSource != null)
            SpecialEffectsSource.PlayOneShot(TP_CantTeleport.clip, TP_CantTeleport.volume);
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
    void Update()
    {
        foreach (var config in configs)
        {
            config.ProcessInput();
        }
    }
}
