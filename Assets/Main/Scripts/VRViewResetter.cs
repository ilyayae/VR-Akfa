using Unity.XR.CoreUtils; // Required for XROrigin
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required if using the New Input System

public class VRViewResetter : MonoBehaviour
{
    [Header("Setup")]
    public XROrigin xrOrigin;

    public Transform targetHeadPosition;

    [Header("Input (Optional)")]
    public InputActionReference resetAction;
    public InputActionReference reloadAction;

    public Image fillInCircle;
    public Transform allcircles;

    private void OnEnable()
    {
        if (resetAction != null)
        {
            resetAction.action.Enable();
            reloadAction.action.Enable();
            resetAction.action.performed += TriggerReset;
            resetAction.action.canceled += UnTriggerReset;
            reloadAction.action.performed += TriggerReload;
            reloadAction.action.canceled += UnTriggerReload;
        }
    }

    private void OnDisable()
    {
        if (resetAction != null)
        {
            resetAction.action.performed -= TriggerReset;
            resetAction.action.canceled -= UnTriggerReset;
            reloadAction.action.performed -= TriggerReload;
            reloadAction.action.canceled -= UnTriggerReload;
            resetAction.action.Disable();
            reloadAction.action.Disable();
        }
    }

    private void TriggerReset(InputAction.CallbackContext context)
    {
        resetting = true;
    }

    private void UnTriggerReset(InputAction.CallbackContext context)
    {
        resetting = false;
    }
    private void TriggerReload(InputAction.CallbackContext context)
    {
        reloading = true;
    }

    private void UnTriggerReload(InputAction.CallbackContext context)
    {
        reloading = false;
    }
    public void ResetPlayerView()
    {
        if (xrOrigin == null || targetHeadPosition == null)
        {
            Debug.LogWarning("XR Origin or Target Position is not assigned in VRViewResetter!");
            return;
        }

        xrOrigin.MatchOriginUpCameraForward(targetHeadPosition.up, targetHeadPosition.forward);

        xrOrigin.MoveCameraToWorldLocation(targetHeadPosition.position);
    }
    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    bool resetting = false;
    float timer = 0f;
    float maxTimer = 1f;
    float fadeTimer = 0f;
    float fadeMaxTimer = 0.4f;


    bool reloading = false;
    float reloadTimer = 0f;
    float maxReloadTimer = 1f;
    float reloadFadeTimer = 0f;
    float reloadFadeMaxTimer = 0.4f;
    private void Update()
    {
        if(resetting)
        {
            if (fadeTimer < fadeMaxTimer)
            {
                fadeTimer += Time.deltaTime;
                foreach(Transform t in allcircles) 
                {
                    Image img = t.GetComponent<Image>();
                    Color c = img.color;
                    img.color = new Color(c.r, c.g, c.b, fadeTimer / fadeMaxTimer);
                }
            }
            else
            {
                timer += Time.deltaTime;
                fillInCircle.fillAmount = (timer / maxTimer);
                if (timer > maxTimer)
                {
                    ResetPlayerView();
                    fillInCircle.fillAmount = 0;
                    fadeTimer = 0;
                    timer = 0;
                    resetting = false;
                    foreach (Transform t in allcircles)
                    {
                        Image img = t.GetComponent<Image>();
                        Color c = img.color;
                        img.color = new Color(c.r, c.g, c.b, 0f);
                    }
                }
            }
        }
        else if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            foreach (Transform t in allcircles)
            {
                Image img = t.GetComponent<Image>();
                Color c = img.color;
                img.color = new Color(c.r, c.g, c.b, fadeTimer / fadeMaxTimer);
            }
            if(fadeTimer <= 0)
            {
                fillInCircle.fillAmount = 0;
                fadeTimer = 0;
                timer = 0;
                foreach (Transform t in allcircles)
                {
                    Image img = t.GetComponent<Image>();
                    Color c = img.color;
                    img.color = new Color(c.r, c.g, c.b, 0f);
                }
            }
        }


        if (reloading)
        {
            if (reloadFadeTimer < reloadFadeMaxTimer)
            {
                reloadFadeTimer += Time.deltaTime;
                foreach (Transform t in allcircles)
                {
                    Image img = t.GetComponent<Image>();
                    Color c = img.color;
                    img.color = new Color(c.r, c.g, c.b, reloadFadeTimer / fadeMaxTimer);
                }
            }
            else
            {
                reloadTimer += Time.deltaTime;
                fillInCircle.fillAmount = (reloadTimer / maxReloadTimer);
                if (reloadTimer > maxReloadTimer)
                {
                    ReloadCurrentScene();
                    fillInCircle.fillAmount = 0;
                    reloadFadeTimer = 0;
                    reloadTimer = 0;
                    reloading = false;
                    foreach (Transform t in allcircles)
                    {
                        Image img = t.GetComponent<Image>();
                        Color c = img.color;
                        img.color = new Color(c.r, c.g, c.b, 0f);
                    }
                }
            }
        }
        else if (reloadFadeTimer > 0)
        {
            reloadFadeTimer -= Time.deltaTime;
            foreach (Transform t in allcircles)
            {
                Image img = t.GetComponent<Image>();
                Color c = img.color;
                img.color = new Color(c.r, c.g, c.b, reloadFadeTimer / reloadFadeMaxTimer);
            }
            if (reloadFadeTimer <= 0)
            {
                fillInCircle.fillAmount = 0;
                reloadFadeTimer = 0;
                reloadTimer = 0;
                foreach (Transform t in allcircles)
                {
                    Image img = t.GetComponent<Image>();
                    Color c = img.color;
                    img.color = new Color(c.r, c.g, c.b, 0f);
                }
            }
        }
    }
}