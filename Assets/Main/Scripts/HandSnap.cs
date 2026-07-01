using System.Collections;
using UnityEngine;
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
    }

    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnHandGrab);
        interactor.selectExited.RemoveListener(OnHandRelease);
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
    private void Update()
    {
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