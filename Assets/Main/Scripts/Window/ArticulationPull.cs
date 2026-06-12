using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // Added for XRRayInteractor

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(ArticulationBody))]
public class ArticulationPull : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;
    private ArticulationBody artBody;
    
    // Changed from currentHand to pullTarget to better represent Rays and Hands
    private Transform pullTarget; 
    private Vector3 localGrabPoint;

    [Header("Visual Snapping & Physics")]
    [Tooltip("The transform the hand/ray visual will snap to.")]
    public Transform attach;
    [Tooltip("How strongly the object pulls towards your hand/ray.")]
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

        // Set the simple interactable's attach transform to our custom one
        if (attach != null)
        {
            //simpleInteractable.attachTransform = attach;
        }

        simpleInteractable.selectEntered.AddListener(OnGrab);
        simpleInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        IXRSelectInteractor interactor = args.interactorObject;
        Vector3 interactionPoint = interactor.transform.position;

        // 1. If it's a Ray Interactor, get the exact point where the ray hit the handle
        if (interactor is XRRayInteractor rayInteractor)
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                interactionPoint = hit.point;
            }
        }

        // 2. Calculate the attach point based on the hit point (or hand position)
        UpdateAttachPosition(interactionPoint);

        // 3. Get the interactors dynamic attach transform. 
        // For a hand, this is the hand. For a ray, this is the end of the ray line.
        pullTarget = interactor.GetAttachTransform(simpleInteractable);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        pullTarget = null;
    }

    void FixedUpdate()
    {
        if (pullTarget != null)
        {
            // Pull towards the pullTarget (the hand or the end of the ray)
            Vector3 worldGrabPoint = transform.TransformPoint(localGrabPoint);
            Vector3 pullVector = pullTarget.position - worldGrabPoint;
            
            artBody.AddForceAtPosition(pullVector * pullStrength, worldGrabPoint);
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
}