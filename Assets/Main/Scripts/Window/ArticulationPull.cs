using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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

    public string AnimName;
    void Start()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        artBody = GetComponent<ArticulationBody>();

        simpleInteractable.selectEntered.AddListener(OnGrab);
        simpleInteractable.selectExited.AddListener(OnRelease);
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

        currentInteractor.transform.parent.gameObject.GetComponent<HandSnap>().AnimateMe(AnimName);
        offsetInAttachSpace = pullTarget.InverseTransformPoint(extendedWorldGrabPoint);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        pullTarget = null;
        currentInteractor.transform.parent.gameObject.GetComponent<HandSnap>().AnimateMe("GrabExit");
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