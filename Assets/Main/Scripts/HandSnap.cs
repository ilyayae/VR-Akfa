using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
    }

    private void OnHandRelease(SelectExitEventArgs args)
    {
        AnimateHandTo(originalParent, originalLocalPosition, originalLocalRotation);
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
}