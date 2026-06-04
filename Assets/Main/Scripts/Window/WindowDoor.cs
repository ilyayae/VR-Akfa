using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;


public enum doorState
{
    SWING,
    TILT,
    SLIDE,
    ERROR
}

public class WindowDoor : MonoBehaviour
{
    [Header("Window Settings")]
    public float maxSwingAngle = 177f;
    public float maxTiltAngle = 15f;
    public float maxSlideDistance = 1f;
    public doorState state = doorState.SWING;
    [HideInInspector]
    public ConfigurableJoint myJoint;
    private Rigidbody myRigidbody;
    private Vector3 originalConnectedAnchor;


    private Vector3 initialDirectionInBaseSpace;
    private Quaternion initialRotation;
    private Vector3 initialLocation;
    // The direction we use to measure the swing (the "clock hand")
    private Vector3 measureDirection = Vector3.up;

    void Start()
    {
        if (myJoint == null) myJoint = GetComponent<ConfigurableJoint>();

        myRigidbody = GetComponent<Rigidbody>();
        originalConnectedAnchor = myJoint.connectedAnchor;
        myJoint.autoConfigureConnectedAnchor = false;
        SetJointSwing();
        if (myJoint.axis == Vector3.right) measureDirection = Vector3.up;
        else if (myJoint.axis == Vector3.up) measureDirection = Vector3.forward;
        else if (myJoint.axis == Vector3.forward) measureDirection = Vector3.up;
        Vector3 worldDir = transform.TransformDirection(measureDirection);

        if (myJoint.connectedBody != null)
        {
            initialDirectionInBaseSpace = myJoint.connectedBody.transform.InverseTransformDirection(worldDir);
            initialLocation = transform.position;
            initialRotation = transform.rotation;
        }
        else
        {
            initialDirectionInBaseSpace = worldDir;
        }
    }

    [ContextMenu("Run SetJointSwing")]
    public void SetJointSwing()
    {
        state = doorState.SWING;
        myJoint.connectedAnchor = originalConnectedAnchor;
        myJoint.xMotion = ConfigurableJointMotion.Locked;
        myJoint.yMotion = ConfigurableJointMotion.Locked;
        myJoint.zMotion = ConfigurableJointMotion.Locked;
        myJoint.angularXMotion = ConfigurableJointMotion.Limited;
        myJoint.angularYMotion = ConfigurableJointMotion.Locked;
        myJoint.angularZMotion = ConfigurableJointMotion.Locked;
        myJoint.axis = new Vector3(0, 1, 0);
        SetAngularLimits(0f, maxSwingAngle);
        myRigidbody.WakeUp();
    }
    [ContextMenu("Run SetJointTilt")]
    public void SetJointTilt()
    {
        state = doorState.TILT;
        myJoint.connectedAnchor = originalConnectedAnchor;
        myJoint.xMotion = ConfigurableJointMotion.Locked;
        myJoint.yMotion = ConfigurableJointMotion.Locked;
        myJoint.zMotion = ConfigurableJointMotion.Locked;
        myJoint.angularXMotion = ConfigurableJointMotion.Limited;
        myJoint.angularYMotion = ConfigurableJointMotion.Locked;
        myJoint.angularZMotion = ConfigurableJointMotion.Locked;
        myJoint.axis = new Vector3(-1, 0, 0);

        SetAngularLimits(0f, maxTiltAngle);

        myRigidbody.WakeUp();
    }
    [ContextMenu("Run SetJointSlide")]
    public void SetJointSlide()
    {
        state = doorState.SLIDE;
        myJoint.angularXMotion = ConfigurableJointMotion.Locked;
        myJoint.angularYMotion = ConfigurableJointMotion.Locked;
        myJoint.angularZMotion = ConfigurableJointMotion.Locked;

        myJoint.xMotion = ConfigurableJointMotion.Limited;
        myJoint.yMotion = ConfigurableJointMotion.Locked;
        myJoint.zMotion = ConfigurableJointMotion.Locked;

        myJoint.axis = new Vector3(1, 0, 0);

        float halfDistance = maxSlideDistance / 2f;

        SoftJointLimit slideLimit = new SoftJointLimit();
        slideLimit.limit = halfDistance;
        myJoint.linearLimit = slideLimit;
        myJoint.connectedAnchor = originalConnectedAnchor - (myJoint.axis * halfDistance);
        myRigidbody.WakeUp();
    }

    private void SetAngularLimits(float low, float high)
    {
        SoftJointLimit lowLimit = new SoftJointLimit();
        lowLimit.limit = low;
        myJoint.lowAngularXLimit = lowLimit;

        SoftJointLimit highLimit = new SoftJointLimit();
        highLimit.limit = high;
        myJoint.highAngularXLimit = highLimit;
    }

    public float GetCurrentAngle()
    {
        Vector3 zeroDegreeDir;
        if (myJoint.connectedBody != null)
            zeroDegreeDir = myJoint.connectedBody.transform.TransformDirection(initialDirectionInBaseSpace);
        else
            zeroDegreeDir = initialDirectionInBaseSpace;
        Vector3 currentDir = transform.TransformDirection(measureDirection);
        Vector3 worldAxis = transform.TransformDirection(myJoint.axis);
        float angle = Vector3.SignedAngle(zeroDegreeDir, currentDir, worldAxis);
        return angle;
    }
    public float GetCurrentDistance()
    {
        return Vector3.Distance(initialLocation, transform.position);
    }

    public float GetOpenedDegreeOfWindow()
    {
        switch (state)
        {
            case doorState.SWING:
                return Mathf.Clamp01(Mathf.Abs(GetCurrentAngle()) / maxSwingAngle);
            case doorState.TILT:
                return Mathf.Clamp01(Mathf.Abs(GetCurrentAngle()) / maxTiltAngle);
            case doorState.SLIDE:
                return Mathf.Clamp01(GetCurrentDistance() / maxSlideDistance);
        }
        Debug.Log("Unknown type of door state in GetOpenedDegreeOfWindow");
        return -1f;
    }

    [ContextMenu("Run test")]
    public IEnumerator test()
    {
        float deg = GetOpenedDegreeOfWindow();
        yield return StartCoroutine(SetOpenedDegreeOfWindow(0f));
        yield return null;
        SetJointSlide();
        yield return null;
        yield return StartCoroutine(SetOpenedDegreeOfWindow(deg));
    }
    [SerializeField] float speedOfAnim = 1f; 
    public IEnumerator SetOpenedDegreeOfWindow(float degree)
    {
        degree = Mathf.Clamp01(degree);
        myRigidbody.isKinematic = true;

        Quaternion startingRotation = transform.rotation;
        Vector3 startingPosition = transform.position;

        Quaternion targetRotation = initialRotation;
        Vector3 targetPosition = initialLocation;
        Vector3 worldAnchorPoint = transform.TransformPoint(myJoint.anchor);

        switch (state)
        {
            case doorState.SWING:
                float targetSwingAngle = maxSwingAngle * degree;
                targetRotation = initialRotation * Quaternion.AngleAxis(targetSwingAngle, myJoint.axis);
                break;

            case doorState.TILT:
                float targetTiltAngle = maxTiltAngle * degree;
                targetRotation = initialRotation * Quaternion.AngleAxis(targetTiltAngle, myJoint.axis);
                break;

            case doorState.SLIDE:
                float targetDist = maxSlideDistance * degree;
                Vector3 slideDirection = initialRotation * -myJoint.axis;
                targetPosition = initialLocation + (slideDirection.normalized * targetDist);
                break;
        }
        float elapsedTime = 0f;
        float duration = 1f / speedOfAnim;

        transform.rotation = startingRotation;
        transform.position = startingPosition;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            if (state == doorState.SLIDE)
            {
                transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            }
            else
            {
                Quaternion currentTargetRot = Quaternion.Slerp(startingRotation, targetRotation, t);
                transform.rotation = currentTargetRot;
                Vector3 currentAnchorOffset = transform.TransformPoint(myJoint.anchor) - worldAnchorPoint;
                transform.position -= currentAnchorOffset;
            }

            yield return null;
        }
        if (state == doorState.SLIDE)
            transform.position = targetPosition;
        else
        {
            transform.rotation = targetRotation;
            Vector3 finalAnchorOffset = transform.TransformPoint(myJoint.anchor) - worldAnchorPoint;
            transform.position -= finalAnchorOffset;
        }

        UpdateJointTargets(transform.localPosition, transform.localRotation);
        myRigidbody.isKinematic = false;
        myRigidbody.WakeUp();
    }

    private void UpdateJointTargets(Vector3 localTargetPos, Quaternion localTargetRot)
    {
        Vector3 localInitialLocation = transform.parent != null ?
            transform.parent.InverseTransformPoint(initialLocation) : initialLocation;

        if (state == doorState.SLIDE)
        {
            myJoint.targetPosition = localInitialLocation - localTargetPos;
            myJoint.targetRotation = Quaternion.identity;
        }
        else
        {
            myJoint.targetPosition = Vector3.zero;
            myJoint.targetRotation = Quaternion.Inverse(localTargetRot) * initialRotation;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(test());
        }
        float currentRotation = GetCurrentAngle();
        if(state == doorState.SWING || state == doorState.TILT)
            Debug.Log($"Object is currently rotated: {currentRotation:F1} degrees");
        if(state == doorState.SLIDE)
            Debug.Log($"Object is currently at distance: {GetCurrentDistance():F1} meters");
    }
}
