using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(ArticulationBody))]
public class ArticulationPull : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;
    private ArticulationBody artBody;
    private Transform currentHand;
    private Vector3 localGrabPoint;

    [Header("Visual Snapping & Physics")]
    [Tooltip("The transform the hand visual will snap to.")]
    public Transform attach;
    [Tooltip("How strongly the object pulls towards your hand.")]
    public float pullStrength = 5000f;

    [Header("Dynamic Edge Grabbing")]
    [Tooltip("Optional: Point A of the grabbable edge.")]
    public Transform edgeStart;
    [Tooltip("Optional: Point B of the grabbable edge.")]
    public Transform edgeEnd;

    void Start()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        artBody = GetComponent<ArticulationBody>();

        simpleInteractable.selectEntered.AddListener(OnGrab);
        simpleInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        currentHand = args.interactorObject.transform;
        // Ensure the attach point is calculated
        UpdateAttachPosition(currentHand.position);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        currentHand = null;
    }

    void FixedUpdate()
    {
        if (currentHand != null)
        {
            Vector3 worldGrabPoint = transform.TransformPoint(localGrabPoint);
            Vector3 pullVector = currentHand.position - worldGrabPoint;
            artBody.AddForceAtPosition(pullVector * pullStrength, worldGrabPoint);
        }
    }
    public void UpdateAttachPosition(Vector3 interactorPosition)
    {
        if (attach != null && edgeStart != null && edgeEnd != null)
        {
            Vector3 closestPoint = GetClosestPointOnLineSegment(edgeStart.position, edgeEnd.position, interactorPosition);
            attach.position = closestPoint;
            localGrabPoint = transform.InverseTransformPoint(attach.position);
        }
        else if (attach != null)
        {
            localGrabPoint = transform.InverseTransformPoint(attach.position);
        }
        else
        {
            localGrabPoint = transform.InverseTransformPoint(interactorPosition);
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
}