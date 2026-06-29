using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ArticulationBody))]
public class WindowHandle : MonoBehaviour
{
    public enum HandleState { CLOSED, SWING, TILT }
    public HandleState currentHandleState = HandleState.CLOSED;
    public Action<HandleState> onHandleStateChanged;

    [Tooltip("Check this if this is a left handle (rotates 0 to 90/180). Uncheck for right handle (rotates 0 to -90/-180).")]
    public bool left = false;

    private ArticulationBody myBody;
    private float maxSwingAngle;

    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    void Awake()
    {
        myBody = GetComponent<ArticulationBody>();
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;
    }

    void Start()
    {
        myBody.twistLock = ArticulationDofLock.LimitedMotion;
        maxSwingAngle = left ? myBody.xDrive.upperLimit : Mathf.Abs(myBody.xDrive.lowerLimit);
        UpdateHandleState(true);
    }

    public void ResetToClosedPosition()
    {
        if (myBody == null) myBody = GetComponent<ArticulationBody>();

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;

        if (myBody != null)
        {
            myBody.jointPosition = new ArticulationReducedSpace(0f);
            myBody.jointVelocity = new ArticulationReducedSpace(0f);

            ArticulationDrive drive = myBody.xDrive;
            drive.target = 0f;
            myBody.xDrive = drive;
        }
    }

    public void SetMaxAngle(float maxAngle)
    {
        if (myBody == null) myBody = GetComponent<ArticulationBody>();

        maxSwingAngle = maxAngle;

        ArticulationDrive drive = myBody.xDrive;

        if (left)
        {
            drive.lowerLimit = 0f;
            drive.upperLimit = maxAngle;

            // Force the joint back into bounds if it's overextended
            if (drive.target > drive.upperLimit)
                drive.target = drive.upperLimit;
        }
        else
        {
            drive.lowerLimit = -maxAngle;
            drive.upperLimit = 0f;

            // Force the joint back into bounds if it's overextended
            if (drive.target < drive.lowerLimit)
                drive.target = drive.lowerLimit;
        }

        myBody.xDrive = drive;
    }

    public float GetCurrentAngleSwing()
    {
        return Mathf.Abs(myBody.jointPosition.dofCount > 0 ? myBody.jointPosition[0] * Mathf.Rad2Deg : 0f);
    }

    public float GetDegreeOfHandle()
    {
        return Mathf.Clamp01(GetCurrentAngleSwing() / maxSwingAngle);
    }

    [ContextMenu("Run test")]
    public IEnumerator test()
    {
        yield return StartCoroutine(SetDegreeOfHandle(0));
    }

    float speedOfAnim = 2f;

    public IEnumerator SetDegreeOfHandle(float degree)
    {
        degree = Mathf.Clamp01(degree);

        float targetAngle = left ? (maxSwingAngle * degree) : (-maxSwingAngle * degree);
        float currentAngle = myBody.jointPosition.dofCount > 0 ? (myBody.jointPosition[0] * Mathf.Rad2Deg) : 0f;

        // 1. EARLY EXIT: If already at target, skip the delay!
        if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
        {
            ArticulationDrive d = myBody.xDrive;
            d.target = targetAngle;
            myBody.xDrive = d;

            var inst = myBody.jointPosition;
            if (inst.dofCount > 0)
            {
                inst[0] = targetAngle * Mathf.Deg2Rad;
                myBody.jointPosition = inst;
            }
            yield break;
        }

        ArticulationDrive drive = myBody.xDrive;
        float rememberStiffness = drive.stiffness;
        float rememberDamping = drive.damping;

        drive.stiffness = 1000000f;
        drive.damping = 100000f;
        myBody.xDrive = drive;

        // 2. SCALE DURATION: Proportional to the distance it needs to travel
        float travelRatio = maxSwingAngle > 0f ? Mathf.Clamp01(Mathf.Abs(currentAngle - targetAngle) / maxSwingAngle) : 0f;
        float elapsedTime = 0f;
        float duration = (1f / speedOfAnim) * travelRatio;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            drive = myBody.xDrive;
            drive.target = Mathf.Lerp(currentAngle, targetAngle, t);
            myBody.xDrive = drive;

            yield return new WaitForFixedUpdate();
        }

        drive = myBody.xDrive;
        drive.target = targetAngle;
        drive.stiffness = rememberStiffness;
        drive.damping = rememberDamping;
        myBody.xDrive = drive;

        var instances = myBody.jointPosition;
        if (instances.dofCount > 0)
        {
            instances[0] = targetAngle * Mathf.Deg2Rad;
            myBody.jointPosition = instances;
        }
    }

    private void Update()
    {
        UpdateHandleState(false);
    }

    private void UpdateHandleState(bool forceInvoke)
    {
        float currentAngle = GetCurrentAngleSwing(); // Works perfectly for both left/right since it uses Mathf.Abs
        HandleState newState;

        // Determine handle state using absolute degrees
        if (currentAngle < 30f) newState = HandleState.CLOSED;
        else if (currentAngle < 135f) newState = HandleState.SWING;
        else newState = HandleState.TILT;

        if (newState != currentHandleState || forceInvoke)
        {
            currentHandleState = newState;
            onHandleStateChanged?.Invoke(currentHandleState);
        }
    }
}