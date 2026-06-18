using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(ArticulationBody))]
public class ArticulationHandle : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;
    private ArticulationBody artBody;
    private IXRSelectInteractor currentInteractor;

    [Header("Handle Rotation (Twisting)")]
    public Vector3 controllerTwistAxis = new Vector3(0, 0, 1);

    float driveStiffness = 50000f;
    float driveDamping = 5000f;
    bool invertRotation = true;
    float rotationMultiplier = 1.0f;
    public float overstepBuffer = 45f;

    float pullStrength = 1500f;
    float pullDamping = 500f;

    [Header("Dynamic Edge Grabbing")]
    public Transform attach;
    public Transform edgeStart;
    public Transform edgeEnd;

    private Quaternion previousControllerRotation;
    private float unclampedVirtualAngle;

    private Transform pullTarget;
    private Vector3 localGrabPoint;
    private Vector3 offsetInAttachSpace;

    void Start()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        artBody = GetComponent<ArticulationBody>();

        var drive = artBody.xDrive;
        drive.stiffness = driveStiffness;
        drive.damping = driveDamping;
        artBody.xDrive = drive;

        simpleInteractable.selectEntered.AddListener(OnGrab);
        simpleInteractable.selectExited.AddListener(OnRelease);
    }

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
        Vector3 extendedWorldGrabPoint = transform.TransformPoint(localGrabPoint);
        offsetInAttachSpace = pullTarget.InverseTransformPoint(extendedWorldGrabPoint);

        previousControllerRotation = currentInteractor.transform.rotation;

        unclampedVirtualAngle = artBody.jointPosition[0] * Mathf.Rad2Deg;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        currentInteractor = null;
        pullTarget = null;
    }

    void FixedUpdate()
    {
        if (currentInteractor != null)
        {
            UpdateRotation();
            UpdatePulling();
        }
    }

    private void UpdateRotation()
    {
        Quaternion currentControllerRotation = currentInteractor.transform.rotation;
        Quaternion deltaRotation = currentControllerRotation * Quaternion.Inverse(previousControllerRotation);

        deltaRotation.ToAngleAxis(out float angle, out Vector3 worldAxis);

        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) > 0.001f)
        {
            Vector3 localAxis = currentInteractor.transform.InverseTransformDirection(worldAxis);

            float axisAlignment = Vector3.Dot(localAxis, controllerTwistAxis.normalized);

            float angleChangeThisFrame = angle * axisAlignment * rotationMultiplier;
            if (invertRotation) angleChangeThisFrame = -angleChangeThisFrame;

            unclampedVirtualAngle += angleChangeThisFrame;

            float driveTarget = unclampedVirtualAngle;

            if (artBody.twistLock == ArticulationDofLock.LimitedMotion)
            {
                unclampedVirtualAngle = Mathf.Clamp(unclampedVirtualAngle,
                    artBody.xDrive.lowerLimit - overstepBuffer,
                    artBody.xDrive.upperLimit + overstepBuffer);

                driveTarget = Mathf.Clamp(unclampedVirtualAngle, artBody.xDrive.lowerLimit, artBody.xDrive.upperLimit);
            }

            var drive = artBody.xDrive;
            drive.target = driveTarget;
            artBody.xDrive = drive;
        }

        previousControllerRotation = currentControllerRotation;
    }

    private void UpdatePulling()
    {
        if (pullTarget != null)
        {
            Vector3 extendedWorldGrabPoint = transform.TransformPoint(localGrabPoint);
            Vector3 targetPoint = pullTarget.TransformPoint(offsetInAttachSpace);

            Vector3 pullVector = targetPoint - extendedWorldGrabPoint;
            Vector3 pointVelocity = artBody.GetPointVelocity(extendedWorldGrabPoint);

            Vector3 force = (pullVector * pullStrength) - (pointVelocity * pullDamping);
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

    private void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnGrab);
            simpleInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}