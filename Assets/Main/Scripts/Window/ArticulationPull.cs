using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(ArticulationBody))]
public class ArticulationPull : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;
    private ArticulationBody artBody;

    private Transform pullTarget;
    private Vector3 localGrabPoint;
    private Vector3 offsetInAttachSpace;

    [Header("Visual Snapping & Physics")]
    public Transform attach;
    public float pullStrength = 15000f;
    public float damp = 0f;

    [Header("Dynamic Edge Grabbing")]
    public Transform edgeStart;
    public Transform edgeEnd;

    private Gradient grabGradient;
    private Gradient origGradient;
    void Start()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        artBody = GetComponent<ArticulationBody>();

        simpleInteractable.selectEntered.AddListener(OnGrab);
        simpleInteractable.selectExited.AddListener(OnRelease);

        grabGradient = new();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(Color.white, 0.0f);
        colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(0.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 0.25f);
        grabGradient.SetKeys(colorKeys, alphaKeys);
    }
    IXRSelectInteractor currentInteractor;
    void OnGrab(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
        Vector3 interactionPoint = currentInteractor.transform.position;

        if (currentInteractor is XRRayInteractor rayInteractor)
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                interactionPoint = hit.point;
        }

        UpdateAttachPosition(interactionPoint);
        pullTarget = currentInteractor.GetAttachTransform(simpleInteractable);

        Vector3 extendedLocalGrabPoint = localGrabPoint;
        Vector3 extendedWorldGrabPoint = transform.TransformPoint(extendedLocalGrabPoint);

        offsetInAttachSpace = pullTarget.InverseTransformPoint(extendedWorldGrabPoint);

        if (currentInteractor.transform.TryGetComponent(out XRInteractorLineVisual line))
        {
            origGradient = line.validColorGradient;
            line.validColorGradient = grabGradient;
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (origGradient != null)
        {
            if (currentInteractor.transform.TryGetComponent(out XRInteractorLineVisual line))
            {
                line.validColorGradient = origGradient;
            }
        }
        pullTarget = null;
    }

    void FixedUpdate()
    {
        if (pullTarget != null)
        {
            Vector3 extendedLocalGrabPoint = localGrabPoint;
            Vector3 extendedWorldGrabPoint = transform.TransformPoint(extendedLocalGrabPoint);
            Vector3 targetPoint = pullTarget.TransformPoint(offsetInAttachSpace);
            Vector3 pullVector = targetPoint - extendedWorldGrabPoint;
            Vector3 pointVelocity = artBody.GetPointVelocity(extendedWorldGrabPoint);

            Vector3 force = (pullVector * pullStrength) - (pointVelocity * damp);
            artBody.AddForceAtPosition(force, extendedWorldGrabPoint);
        }
    }

    public void UpdateAttachPosition(Vector3 hitPosition)
    {
        if (attach != null && edgeStart != null && edgeEnd != null)
        {
            Vector3 closestPoint = GetClosestPointOnLineSegment(edgeStart.position, edgeEnd.position, hitPosition);
            attach.position = closestPoint;
            localGrabPoint = transform.InverseTransformPoint(attach.position);
        }
        else if (attach != null)
        {
            localGrabPoint = transform.InverseTransformPoint(attach.position);
        }
        else
        {
            localGrabPoint = transform.InverseTransformPoint(hitPosition);
        }
    }

    private Vector3 GetClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineDirection = end - start;
        float lineLength = lineDirection.magnitude;
        lineDirection.Normalize();
        Vector3 pointVector = point - start;
        float dotProduct = Vector3.Dot(pointVector, lineDirection);
        dotProduct = Mathf.Clamp(dotProduct, 0f, lineLength);
        return start + lineDirection * dotProduct;
    }

    private void OnDrawGizmos()
    {
        if (edgeStart != null && edgeEnd != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(edgeStart.position, edgeEnd.position);
            Gizmos.DrawSphere(edgeStart.position, 0.02f);
            Gizmos.DrawSphere(edgeEnd.position, 0.02f);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attach != null)
        {
            Vector3 basePoint = attach.position;
            Vector3 extendedPoint = transform.TransformPoint(transform.InverseTransformPoint(basePoint) );

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(basePoint, extendedPoint);
            Gizmos.DrawSphere(extendedPoint, 0.015f);
        }
    }
}