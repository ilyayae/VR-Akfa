using System.Collections;
using UnityEngine;

public enum doorState
{
    SWING,
    TILT,
    SLIDE,
    ERROR
}

[RequireComponent(typeof(ArticulationBody))]
public class WindowDoor : MonoBehaviour
{
    [Header("Window Settings")]
    public float maxSwingAngle = 100f;
    public float maxTiltAngle = 15f;
    public float maxSlideDistance = 1f;
    public doorState state = doorState.SWING;

    [Header("Handle Mechanical Simulation")]
    [Tooltip("Is the handle physically turned to the locked position?")]
    public bool isHandleLocked = false;
    public bool left = false;
    [HideInInspector]
    public ArticulationBody myBody;

    [Tooltip("Mechanical block offset percentage (e.g., stops at 10% open)")]
    public float openDegree = 0.1f;

    private Vector3 originalAnchorPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    [SerializeField] float speedOfAnim = 1f;

    void Awake()
    {
        if (myBody == null) myBody = GetComponent<ArticulationBody>();
        if (transform.parent != null)
        {
            initialLocalRotation = transform.localRotation;
            initialLocalPosition = transform.localPosition;
        }
        else
        {
            initialLocalRotation = transform.rotation;
            initialLocalPosition = transform.position;
        }
    }

    void Start()
    {
        originalAnchorPosition = myBody.anchorPosition;
        SetJointSwingDirect();
        if (isHandleLocked) SetJointLocked();
    }
    public void SetHandleLockedState(bool locked)
    {
        isHandleLocked = locked;
        if (isHandleLocked)
        {
            UnblockWindowFromClosing();
            float currentVal = (state == doorState.SLIDE) ? GetCurrentDistance() : GetCurrentAngle();
            if (Mathf.Approximately(currentVal, 0f))
            {
                SetJointLocked();
            }
        }
        else
        {
            BlockWindowFromClosing();
        }
    }

    private IEnumerator TransitionStateRoutine(doorState newState)
    {
        float previousOpenDegree = GetOpenedDegreeOfWindow();
        bool wasLocked = Mathf.Approximately(myBody.xDrive.lowerLimit, myBody.xDrive.upperLimit) || isHandleLocked;
        float targetCloseDegree = isHandleLocked ? 0f : openDegree;

        if (previousOpenDegree > targetCloseDegree)
        {
            yield return StartCoroutine(SetOpenedDegreeOfWindow(targetCloseDegree));
        }

        ResetToClosedPosition();

        switch (newState)
        {
            case doorState.SWING: SetJointSwingDirect(); break;
            case doorState.TILT: SetJointTiltDirect(); break;
            case doorState.SLIDE: SetJointSlideDirect(); break;
        }

        yield return new WaitForFixedUpdate();

        if (wasLocked && isHandleLocked)
        {
            SetJointLocked();
        }
        else if (previousOpenDegree > targetCloseDegree)
        {
            yield return StartCoroutine(SetOpenedDegreeOfWindow(previousOpenDegree));
        }
        else
        {
            if (!isHandleLocked)
            {
                BlockWindowFromClosing();
            }
            else
            {
                ArticulationDrive drive = myBody.xDrive;
                drive.target = 0f;
                myBody.xDrive = drive;
            }
        }
    }

    [ContextMenu("Run SetJointSwing")] public void SetJointSwing() => SwitchState(doorState.SWING);
    [ContextMenu("Run SetJointTilt")] public void SetJointTilt() => SwitchState(doorState.TILT);
    [ContextMenu("Run SetJointSlide")] public void SetJointSlide() => SwitchState(doorState.SLIDE);

    public void SwitchState(doorState newState)
    {
        if (state == newState) return;
        StartCoroutine(TransitionStateRoutine(newState));
    }

    private void SetJointSwingDirect()
    {
        state = doorState.SWING;
        myBody.jointType = ArticulationJointType.RevoluteJoint;
        SetDofLocks(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LockedMotion);
        if (left)
            UpdateJointAnchors(Quaternion.Euler(0, 0, 90));
        else
            UpdateJointAnchors(Quaternion.Euler(0, 0, -90));
        SetLimits(0f, maxSwingAngle);
        UnlockDrive();
    }

    private void SetJointTiltDirect()
    {
        state = doorState.TILT;
        myBody.jointType = ArticulationJointType.RevoluteJoint;
        SetDofLocks(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LockedMotion);
        UpdateJointAnchors(Quaternion.Euler(0, 0, 0));
        SetLimits(0f, maxTiltAngle);
        UnlockDrive();
    }

    private void SetJointSlideDirect()
    {
        state = doorState.SLIDE;
        myBody.jointType = ArticulationJointType.PrismaticJoint;
        SetDofLocks(ArticulationDofLock.LockedMotion, ArticulationDofLock.LimitedMotion);
        UpdateJointAnchors(Quaternion.Euler(0, 180, 0));
        SetLimits(0f, maxSlideDistance);
        UnlockDrive();
    }

    private void SetDofLocks(ArticulationDofLock rotationLock, ArticulationDofLock linearLock)
    {
        myBody.twistLock = rotationLock;
        myBody.swingYLock = ArticulationDofLock.LockedMotion;
        myBody.swingZLock = ArticulationDofLock.LockedMotion;
        myBody.linearLockX = linearLock;
        myBody.linearLockY = ArticulationDofLock.LockedMotion;
        myBody.linearLockZ = ArticulationDofLock.LockedMotion;
    }

    public void SetLimits(float low, float high)
    {
        ArticulationDrive drive = myBody.xDrive;
        drive.lowerLimit = low;
        drive.upperLimit = high;
        myBody.xDrive = drive;
    }

    public float GetCurrentAngle() => (myBody.jointType == ArticulationJointType.RevoluteJoint && myBody.jointPosition.dofCount > 0) ? myBody.jointPosition[0] * Mathf.Rad2Deg : 0f;
    public float GetCurrentDistance() => (myBody.jointType == ArticulationJointType.PrismaticJoint && myBody.jointPosition.dofCount > 0) ? myBody.jointPosition[0] : 0f;

    public float GetOpenedDegreeOfWindow()
    {
        if (myBody.jointPosition.dofCount == 0) return 0f;
        switch (state)
        {
            case doorState.SWING: return Mathf.Clamp01(Mathf.Abs(GetCurrentAngle()) / maxSwingAngle);
            case doorState.TILT: return Mathf.Clamp01(Mathf.Abs(GetCurrentAngle()) / maxTiltAngle);
            case doorState.SLIDE: return Mathf.Clamp01(Mathf.Abs(GetCurrentDistance()) / maxSlideDistance);
        }
        return 0f;
    }

    public IEnumerator SetOpenedDegreeOfWindow(float degree)
    {
        degree = Mathf.Clamp01(degree);
        float targetVal = 0f;
        switch (state)
        {
            case doorState.SWING: targetVal = maxSwingAngle * degree; break;
            case doorState.TILT: targetVal = maxTiltAngle * degree; break;
            case doorState.SLIDE: targetVal = maxSlideDistance * degree; break;
        }

        float currentVal = (state == doorState.SLIDE) ? GetCurrentDistance() : GetCurrentAngle();

        ArticulationDrive drive = myBody.xDrive;
        drive.stiffness = 100000f;
        drive.damping = 10000f;
        myBody.xDrive = drive;

        float elapsedTime = 0f;
        float duration = 1f / speedOfAnim;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            drive = myBody.xDrive;
            drive.target = Mathf.Lerp(currentVal, targetVal, t);
            myBody.xDrive = drive;
            yield return new WaitForFixedUpdate();
        }

        drive = myBody.xDrive;
        drive.target = targetVal;
        myBody.xDrive = drive;

        UnlockDrive();
    }

    [ContextMenu("Run SetJointLocked")]
    public void SetJointLocked()
    {
        float currentVal = (state == doorState.SLIDE) ? GetCurrentDistance() : GetCurrentAngle();
        SetLimits(currentVal, currentVal);

        ArticulationDrive drive = myBody.xDrive;
        drive.stiffness = 100000f;
        drive.damping = 10000f;
        drive.target = currentVal;
        myBody.xDrive = drive;
    }

    [ContextMenu("Run SetJointUnlocked")]
    public void SetJointUnlocked()
    {
        float highLimit = 0f;
        switch (state)
        {
            case doorState.SWING: highLimit = maxSwingAngle; break;
            case doorState.TILT: highLimit = maxTiltAngle; break;
            case doorState.SLIDE: highLimit = maxSlideDistance; break;
        }
        SetLimits(0f, highLimit);
        UnlockDrive();
        if (!isHandleLocked) BlockWindowFromClosing();
    }

    public void BlockWindowFromClosing()
    {
        float highLimit = 0f;
        float blockedLimit = 0f;

        switch (state)
        {
            case doorState.SWING: highLimit = maxSwingAngle; blockedLimit = maxSwingAngle * openDegree; break;
            case doorState.TILT: highLimit = maxTiltAngle; blockedLimit = maxTiltAngle * openDegree; break;
            case doorState.SLIDE: highLimit = maxSlideDistance; blockedLimit = maxSlideDistance * openDegree; break;
        }

        float currentVal = (state == doorState.SLIDE) ? GetCurrentDistance() : GetCurrentAngle();
        float safeLowLimit = Mathf.Min(blockedLimit, currentVal - 0.01f);

        if (safeLowLimit >= highLimit) safeLowLimit = highLimit - 0.05f;
        if (safeLowLimit < 0f) safeLowLimit = 0f;

        if (state == doorState.SLIDE)
            myBody.linearLockX = ArticulationDofLock.LimitedMotion;
        else
            myBody.twistLock = ArticulationDofLock.LimitedMotion;

        SetLimits(safeLowLimit, highLimit);

        ArticulationDrive drive = myBody.xDrive;
        if (state == doorState.TILT)
        {
            drive.stiffness = 5f;
            drive.damping = 2f;
            drive.target = Mathf.Max(currentVal, safeLowLimit);
        }
        else
        {
            drive.stiffness = 0f;
            drive.damping = 10f;
            drive.target = currentVal;
        }
        myBody.xDrive = drive;
    }

    public void UnblockWindowFromClosing()
    {
        if (state == doorState.SLIDE)
            myBody.linearLockX = ArticulationDofLock.LimitedMotion;
        else
            myBody.twistLock = ArticulationDofLock.LimitedMotion;

        float highLimit = 0f;
        switch (state)
        {
            case doorState.SWING: highLimit = maxSwingAngle; break;
            case doorState.TILT: highLimit = maxTiltAngle; break;
            case doorState.SLIDE: highLimit = maxSlideDistance; break;
        }
        SetLimits(0f, highLimit);
        UnlockDrive();
    }

    private void UnlockDrive()
    {
        ArticulationDrive drive = myBody.xDrive;
        float currentVal = (state == doorState.SLIDE) ? GetCurrentDistance() : (myBody.jointPosition.dofCount > 0 ? myBody.jointPosition[0] : 0f);

        if (state == doorState.TILT)
        {
            drive.stiffness = 5f;
            drive.damping = 2f;
            drive.target = currentVal;
        }
        else
        {
            drive.stiffness = 0f;
            drive.damping = 10f;
            drive.target = currentVal;
        }

        myBody.xDrive = drive;
    }

    public void ResetToClosedPosition()
    {
        if (transform.parent != null)
        {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }
        else
        {
            transform.position = initialLocalPosition;
            transform.rotation = initialLocalRotation;
        }

        myBody.jointPosition = new ArticulationReducedSpace(0f);
        myBody.jointVelocity = new ArticulationReducedSpace(0f);
    }

    private void UpdateJointAnchors(Quaternion newAnchorRotation)
    {
        myBody.anchorPosition = originalAnchorPosition;
        myBody.anchorRotation = newAnchorRotation;
        myBody.parentAnchorPosition = initialLocalPosition + (initialLocalRotation * originalAnchorPosition);
        myBody.parentAnchorRotation = initialLocalRotation * newAnchorRotation;

        myBody.jointPosition = new ArticulationReducedSpace(0f);
        myBody.jointVelocity = new ArticulationReducedSpace(0f);
    }
}