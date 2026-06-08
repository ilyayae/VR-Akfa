using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ArticulationBody))]
public class WindowHandle : MonoBehaviour
{
    private ArticulationBody myBody;
    private float maxSwingAngle;

    void Start()
    {
        myBody = GetComponent<ArticulationBody>();
        myBody.twistLock = ArticulationDofLock.LimitedMotion;
        maxSwingAngle = myBody.xDrive.upperLimit - myBody.xDrive.lowerLimit;
    }

    public float GetCurrentAngleSwing()
    {
        return Mathf.Abs(myBody.jointPosition[0] * Mathf.Rad2Deg);
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

    [SerializeField] float speedOfAnim = 1f;

    public IEnumerator SetDegreeOfHandle(float degree)
    {
        degree = Mathf.Clamp01(degree);
        float targetAngle = maxSwingAngle * degree;
        float currentAngle = GetCurrentAngleSwing();

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
            drive.target = Mathf.Lerp(currentAngle, targetAngle, t);
            myBody.xDrive = drive;

            yield return new WaitForFixedUpdate();
        }

        drive = myBody.xDrive;
        drive.target = targetAngle;
        drive.stiffness = 0f; // release control back to player
        drive.damping = 10f;
        myBody.xDrive = drive;
    }

    [SerializeField] private float openDegree = 0.5f;
    public bool isOpened = false;
    public Action onOpen;
    public Action onClose;

    private void Update()
    {
        float currentDegree = GetDegreeOfHandle();

        if (currentDegree <= openDegree && isOpened)
        {
            //Close
            onClose?.Invoke();
            isOpened = false;
        }
        else if (currentDegree > openDegree && !isOpened)
        {
            //Open
            onOpen?.Invoke();
            isOpened = true;
        }
    }
}