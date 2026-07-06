using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandSnap : MonoBehaviour
{
    public XRBaseInteractor interactor;
    public Transform handVisual;

    [Header("Animation Settings")]
    public float animationDuration = 0.15f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Coroutine animationCoroutine;

    [SerializeField] private InputActionReference triggerButton;
    [SerializeField] private InputActionReference gripButton;
    [SerializeField] private InputActionReference thumbButton;

    [Header("TeleportMovement")]
    [SerializeField] private VRViewResetter Manager;
    [SerializeField] private List<InputAndFunctionPair> teleportButtons;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private GameObject teleportationReticle;
    [SerializeField] private LayerMask teleportLayerMask;
    [SerializeField] private LayerMask normalLayerMask;
    private Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>> startCallbacks = new Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>>();
    private Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>> cancelCallbacks = new Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>>();

    [Header("Animation Settings")]
    public Animator handAnimator;
    private int grabLayerIndex = 4;
    public float transitionSpeed = 8f;
    private float targetGrabWeight = 0f;
    private void Awake()
    {
        if (interactor == null)
            interactor = GetComponent<XRBaseInteractor>();

        originalParent = handVisual.parent;
        originalLocalPosition = handVisual.localPosition;
        originalLocalRotation = handVisual.localRotation;
    }

    private void OnEnable()
    {
        interactor.selectEntered.AddListener(OnHandGrab);
        interactor.selectExited.AddListener(OnHandRelease);
        foreach (var teleportButton in teleportButtons)
        {
            if (teleportButton.Action != null && teleportButton.Action.action != null)
            {
                if (teleportButton.isVector)
                {
                    Action<InputAction.CallbackContext> start = (InputAction.CallbackContext context) =>
                    {
                        Vector2 stickValue = context.ReadValue<Vector2>();
                        if (teleportButton.vectorMoveTreshold.x < 0)
                        {
                            if (stickValue.x < teleportButton.vectorMoveTreshold.x)
                            {
                                teleportButton.OnActionTriggered?.Invoke();
                            }
                        }
                        else if (teleportButton.vectorMoveTreshold.x > 0)
                        {
                            if (stickValue.x > teleportButton.vectorMoveTreshold.x)
                            {
                                teleportButton.OnActionTriggered?.Invoke();
                            }
                        }
                        if (teleportButton.vectorMoveTreshold.y < 0)
                        {
                            if (stickValue.y < teleportButton.vectorMoveTreshold.y)
                            {
                                teleportButton.OnActionTriggered?.Invoke();
                            }
                        }
                        else if (teleportButton.vectorMoveTreshold.y > 0)
                        {
                            if (stickValue.y > teleportButton.vectorMoveTreshold.y)
                            {
                                teleportButton.OnActionTriggered?.Invoke();
                            }
                        }
                    };
                    Action<InputAction.CallbackContext> cancel = (InputAction.CallbackContext context) =>
                    {
                        teleportButton.OnActionCanceled?.Invoke();
                    };
                    teleportButton.Action.action.started += start;
                    teleportButton.Action.action.canceled += cancel;
                    startCallbacks.Add(teleportButton, start);
                    cancelCallbacks.Add(teleportButton, cancel);
                }
                else
                {
                    Action<InputAction.CallbackContext> start = (InputAction.CallbackContext context) => {
                        teleportButton.OnActionTriggered?.Invoke();
                    };

                    Action<InputAction.CallbackContext> cancel = (InputAction.CallbackContext context) => {
                        teleportButton.OnActionCanceled?.Invoke();
                    };
                    teleportButton.Action.action.started += start;
                    teleportButton.Action.action.canceled += cancel;
                    startCallbacks.Add(teleportButton, start);
                    cancelCallbacks.Add(teleportButton, cancel);
                }
            }
        }
    }
    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnHandGrab);
        interactor.selectExited.RemoveListener(OnHandRelease);


        foreach (InputAndFunctionPair iafp in teleportButtons)
        {
            if (iafp.Action != null && iafp.Action.action != null)
            {
                if (startCallbacks.TryGetValue(iafp, out Action<InputAction.CallbackContext> start))
                {
                    iafp.Action.action.started -= start;
                }
                if (cancelCallbacks.TryGetValue(iafp, out Action<InputAction.CallbackContext> cancel))
                {
                    iafp.Action.action.canceled -= cancel;
                }
            }
        }
        startCallbacks.Clear();
        cancelCallbacks.Clear();
    }


    private void OnHandGrab(SelectEnterEventArgs args)
    {
        GameObject interactedObj = args.interactableObject.transform.gameObject;
        Transform targetTransform = null;

        if (interactedObj.TryGetComponent<ArticulationPull>(out ArticulationPull articulationPull))
        {
            articulationPull.UpdateAttachPosition(args.interactorObject.transform.position);

            targetTransform = articulationPull.attach;

            if (targetTransform == null)
            {
                targetTransform = interactedObj.transform;
            }
        }
        else if (interactedObj.TryGetComponent<ArticulationHandle>(out ArticulationHandle articulationHandle))
        {
            articulationHandle.UpdateAttachPosition(args.interactorObject.transform.position);

            targetTransform = articulationHandle.attach;

            if (targetTransform == null)
            {
                targetTransform = interactedObj.transform;
            }
        }
        else
        {
            targetTransform = interactedObj.transform;
        }

        AnimateHandTo(targetTransform, Vector3.zero, Quaternion.identity);

        if (interactedObj.TryGetComponent<GrabPoseData>(out GrabPoseData poseData))
        {
            handAnimator.SetInteger("GrabPoseID", poseData.poseID);
        }
        else
        {
            handAnimator.SetInteger("GrabPoseID", 1);
        }
        targetGrabWeight = 1f;
    }

    private void OnHandRelease(SelectExitEventArgs args)
    {
        AnimateHandTo(originalParent, originalLocalPosition, originalLocalRotation);
        handAnimator.SetInteger("GrabPoseID", 0);
        targetGrabWeight = 0f;
    }

    private void AnimateHandTo(Transform targetParent, Vector3 targetLocalPos, Quaternion targetLocalRot)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        handVisual.SetParent(targetParent, true);
        animationCoroutine = StartCoroutine(TransitionRoutine(targetLocalPos, targetLocalRot));
    }

    private IEnumerator TransitionRoutine(Vector3 targetLocalPos, Quaternion targetLocalRot)
    {
        Vector3 startLocalPos = handVisual.localPosition;
        Quaternion startLocalRot = handVisual.localRotation;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveT = animationCurve.Evaluate(t);

            handVisual.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, curveT);
            handVisual.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, curveT);

            yield return null;
        }

        handVisual.localPosition = targetLocalPos;
        handVisual.localRotation = targetLocalRot;

        animationCoroutine = null;
    }

    float trigger = 0f;
    float grip = 0f;
    float thumb = 0f;
    bool teleporting = false;
    public void StartTeleport()
    {
        teleporting = true;
        rayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
        rayInteractor.raycastMask = teleportLayerMask;
    }

    public void EndTeleport()
    {
        if (teleporting)
        {
            teleporting = false;
            rayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            rayInteractor.raycastMask = normalLayerMask;
            if (teleportationReticle.activeSelf)
            {
                Manager.startTeleport(teleportationReticle.transform);
            }
            teleportationReticle.SetActive(false);
        }
    }
    public float clickCooldown = 0.5f;
    float goLeftLastTime = 0;

    float goRightLastTime = 0;
    public void GoLeft()
    {
        if (Time.time - goLeftLastTime > clickCooldown)
        {
            goLeftLastTime = Time.time;

            if (Manager != null && Manager.xrOrigin != null)
            {
                Manager.xrOrigin.RotateAroundCameraUsingOriginUp(-30f);
            }
        }
    }

    public void GoRight()
    {
        if (Time.time - goRightLastTime > clickCooldown)
        {
            goRightLastTime = Time.time;

            if (Manager != null && Manager.xrOrigin != null)
            {
                Manager.xrOrigin.RotateAroundCameraUsingOriginUp(30f);
            }
        }
    }
    private void Update()
    {
        if(teleporting)
        {
            rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            if (hit.transform != null && hit.transform.gameObject.tag == "Hittable")
            {
                teleportationReticle.SetActive(true);
                teleportationReticle.transform.position = hit.point;

                Vector3 camPos = (Manager != null && Manager.xrOrigin != null & Manager.xrOrigin.Camera != null)
                    ? Manager.xrOrigin.Camera.transform.position
                    : rayInteractor.transform.position;
                Vector3 direction = hit.point - camPos;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                {
                    teleportationReticle.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }
                else
                {
                    Vector3 camfwd = (Manager != null && Manager.xrOrigin != null & Manager.xrOrigin.Camera != null)
                        ? Manager.xrOrigin.Camera.transform.forward
                        : rayInteractor.transform.forward;

                    camfwd.y = 0f;
                    if(camfwd.sqrMagnitude > 0.001f)
                    {
                        teleportationReticle.transform.rotation = Quaternion.LookRotation(camfwd.normalized, Vector3.up);
                    }
                }
            }
            else
            {
                teleportationReticle.SetActive(false);
            }
        }
        float currentWeight = handAnimator.GetLayerWeight(grabLayerIndex);
        handAnimator.SetLayerWeight(grabLayerIndex, Mathf.Lerp(currentWeight, targetGrabWeight, Time.deltaTime * transitionSpeed));
        float triggerValue = triggerButton.action.ReadValue<float>();
        float gripValue = gripButton.action.ReadValue<float>();
        float thumbValue = thumbButton.action.ReadValue<float>();
        trigger = Mathf.Lerp(trigger, triggerValue, Time.deltaTime * transitionSpeed);
        grip = Mathf.Lerp(grip, gripValue, Time.deltaTime * transitionSpeed);
        thumb = Mathf.Lerp(thumb, thumbValue, Time.deltaTime * transitionSpeed);
        handAnimator.SetFloat("Trigger_Input", trigger);
        handAnimator.SetFloat("Grip_Input", grip);
        handAnimator.SetFloat("Thumb_Input", thumb);
    }
}