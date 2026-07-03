using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public TMP_Text FPStext;
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

        // 1. Get the direction this Manager object is facing & flatten it.
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;

        // 2. Align Camera. Use Vector3.up strictly instead of transform.up to guarantee ZERO angle/tilt!
        if (flatForward.sqrMagnitude > 0.001f)
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, flatForward.normalized);
        else
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, transform.forward);

        // 3. Compensate XROrigin placement to lock head to the exact offset.
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

    private float FPStimer = 0f;
    private int frameCount = 0;

    private void Update()
    {
        FPStimer += Time.unscaledDeltaTime;
        frameCount++;

        if (FPStimer >= 1f)
        {
            int fps = Mathf.RoundToInt(frameCount / FPStimer);
            FPStext.text = "FPS: " + fps.ToString();
            FPStimer -= 1f;
            frameCount = 0;
        }

        if (resetting)
        {
            if (fadeTimer < fadeMaxTimer)
            {
                fadeTimer += Time.deltaTime;
                foreach (Transform t in allcircles)
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
            if (fadeTimer <= 0)
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
                    // Bugfix: used reloadFadeMaxTimer here instead of fadeMaxTimer
                    img.color = new Color(c.r, c.g, c.b, reloadFadeTimer / reloadFadeMaxTimer);
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

    [SerializeField] Image fadeImage;
    [SerializeField] float fadeDuration = 0.5f;

    public void startTeleport(Transform transform)
    {
        StartCoroutine(TeleportTo(transform));
    }

    public IEnumerator TeleportTo(Transform targetTransform)
    {
        float timer = 0f;
        Color fadeColor = fadeImage.color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }
        fadeColor.a = 1f;
        fadeImage.color = fadeColor;

        // Move Resetter and enforce strict Y=0 flattening to prevent angle issues.
        transform.position = targetTransform.position;
        Vector3 flatForward = targetTransform.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
        else
            transform.rotation = Quaternion.Euler(0f, targetTransform.eulerAngles.y, 0f);

        ResetPlayerView();

        yield return new WaitForSeconds(0.1f);
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }
        fadeColor.a = 0f;
        fadeImage.color = fadeColor;
    }
}