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

    [Tooltip("How strongly the object pulls towards your hand.")]
    public float pullStrength = 5000f;

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
        localGrabPoint = transform.InverseTransformPoint(currentHand.position);
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
}