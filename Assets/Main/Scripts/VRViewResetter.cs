using UnityEngine;
using Unity.XR.CoreUtils; // Required for XROrigin
using UnityEngine.InputSystem;
using UnityEngine.UI; // Required if using the New Input System

public class VRViewResetter : MonoBehaviour
{
    [Header("Setup")]
    public XROrigin xrOrigin;

    public Transform targetHeadPosition;

    [Header("Input (Optional)")]
    public InputActionReference resetAction;

    public Image fillInCircle;
    public Transform allcircles;

    private void OnEnable()
    {
        if (resetAction != null)
        {
            resetAction.action.Enable();
            resetAction.action.performed += TriggerReset;
            resetAction.action.canceled += UnTriggerReset;
        }
    }

    private void OnDisable()
    {
        if (resetAction != null)
        {
            resetAction.action.performed -= TriggerReset;
            resetAction.action.canceled -= UnTriggerReset;
            resetAction.action.Disable();
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

    bool resetting = false;
    float timer = 0f;
    float maxTimer = 1f;
    float fadeTimer = 0f;
    float fadeMaxTimer = 0.4f;
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
    }
}