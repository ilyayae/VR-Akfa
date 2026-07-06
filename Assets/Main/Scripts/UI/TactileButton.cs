using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TactileButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] List<InputAndFunctionPair> inputAndFunctionPairs = new List<InputAndFunctionPair>();
    private Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>> startCallbacks = new Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>>();
    private Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>> cancelCallbacks = new Dictionary<InputAndFunctionPair, Action<InputAction.CallbackContext>>();
    Button targetButton;
    public float clickCooldown = 0.5f;
    private float lastClickTime = 0f;
    bool isPhysicallyPressed = false;
    void Start()
    {
        targetButton = transform.parent.gameObject.GetComponent<Button>();
        //gameObject.AddComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        startCallbacks.Clear();
        cancelCallbacks.Clear();

        foreach (InputAndFunctionPair iafp in inputAndFunctionPairs)
        {
            if (iafp.Action != null && iafp.Action.action != null)
            {
                if (iafp.isVector)
                {
                    Action<InputAction.CallbackContext> start = (InputAction.CallbackContext context) => {
                        Vector2 stickValue = context.ReadValue<Vector2>();
                        if (iafp.vectorMoveTreshold.x < 0)
                        {
                            if (stickValue.x < iafp.vectorMoveTreshold.x)
                            {
                                if (targetButton != null && Time.time - lastClickTime >= clickCooldown)
                                {
                                    lastClickTime = Time.time;
                                    isPhysicallyPressed = true;
                                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                                    targetButton.OnPointerDown(pointerData);
                                    iafp.OnActionTriggered?.Invoke();
                                }
                            }
                        }
                        else if (iafp.vectorMoveTreshold.x > 0)
                        {
                            if (stickValue.x > iafp.vectorMoveTreshold.x)
                            {
                                if (targetButton != null && Time.time - lastClickTime >= clickCooldown)
                                {
                                    lastClickTime = Time.time;
                                    isPhysicallyPressed = true;
                                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                                    targetButton.OnPointerDown(pointerData);
                                    iafp.OnActionTriggered?.Invoke();
                                }
                            }
                        }
                        if (iafp.vectorMoveTreshold.y < 0)
                        {
                            if (stickValue.y < iafp.vectorMoveTreshold.y)
                            {
                                if (targetButton != null && Time.time - lastClickTime >= clickCooldown)
                                {
                                    lastClickTime = Time.time;
                                    isPhysicallyPressed = true;
                                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                                    targetButton.OnPointerDown(pointerData);
                                    iafp.OnActionTriggered?.Invoke();
                                }
                            }
                        }
                        else if (iafp.vectorMoveTreshold.y > 0)
                        {
                            if (stickValue.y > iafp.vectorMoveTreshold.y)
                            {
                                if (targetButton != null && Time.time - lastClickTime >= clickCooldown)
                                {
                                    lastClickTime = Time.time;
                                    isPhysicallyPressed = true;
                                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                                    targetButton.OnPointerDown(pointerData);
                                    iafp.OnActionTriggered?.Invoke();
                                }
                            }
                        }
                    };

                    Action<InputAction.CallbackContext> cancel = (InputAction.CallbackContext context) => {
                        if (targetButton != null)
                        {
                            isPhysicallyPressed = false;
                            PointerEventData pointerData = new PointerEventData(EventSystem.current);
                            targetButton.OnPointerUp(pointerData);
                            EventSystem.current.SetSelectedGameObject(null);
                        }
                    };
                    iafp.Action.action.started += start;
                    iafp.Action.action.canceled += cancel;
                    startCallbacks.Add(iafp, start);
                    cancelCallbacks.Add(iafp, cancel);
                }
                else
                {
                    Action<InputAction.CallbackContext> start = (InputAction.CallbackContext context) => {
                        if (targetButton != null && Time.time - lastClickTime >= clickCooldown)
                        {
                            lastClickTime = Time.time;
                            isPhysicallyPressed = true;
                            PointerEventData pointerData = new PointerEventData(EventSystem.current);
                            targetButton.OnPointerDown(pointerData);
                            iafp.OnActionTriggered?.Invoke();
                        }
                    };

                    Action<InputAction.CallbackContext> cancel = (InputAction.CallbackContext context) => {
                        if (targetButton != null)
                        {
                            isPhysicallyPressed = false;
                            PointerEventData pointerData = new PointerEventData(EventSystem.current);
                            targetButton.OnPointerUp(pointerData);
                            EventSystem.current.SetSelectedGameObject(null);
                        }
                    };
                    iafp.Action.action.started += start;
                    iafp.Action.action.canceled += cancel;
                    startCallbacks.Add(iafp, start);
                    cancelCallbacks.Add(iafp, cancel);
                }
            }
        }
    }

    private void OnDisable()
    {
        foreach (InputAndFunctionPair iafp in inputAndFunctionPairs)
        {
            if (iafp.Action != null && iafp.Action.action != null)
            {
                if (startCallbacks.TryGetValue(iafp, out Action<InputAction.CallbackContext> start))
                {
                    iafp.Action.action.started -= start;
                }
                if (cancelCallbacks.TryGetValue(iafp, out Action<InputAction.CallbackContext> cancel))
                {
                    iafp.Action.action.canceled -= cancel;
                }
            }
        }
        startCallbacks.Clear();
        cancelCallbacks.Clear();
    }

    private void EmulatePressDown()
    {
        if (targetButton != null)
        {
            if (Time.time - lastClickTime >= clickCooldown)
            {
                lastClickTime = Time.time;
                isPhysicallyPressed = true;
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                targetButton.OnPointerDown(pointerData);
                targetButton.onClick.Invoke();
            }
        }
    }
    private void EmulatePressUp()
    {
        if (targetButton != null)
        {
            isPhysicallyPressed = false;
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            targetButton.OnPointerUp(pointerData);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EmulatePressDown();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isPhysicallyPressed)
        {
            EmulatePressUp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isPhysicallyPressed && Time.time - lastClickTime >= clickCooldown)
        {
            lastClickTime = Time.time;
            targetButton.onClick.Invoke();
        }
    }
}

[Serializable]
public class InputAndFunctionPair
{
    public InputActionReference Action;
    public bool isVector;
    public Vector2 vectorMoveTreshold;
    public UnityEvent OnActionTriggered;
    public UnityEvent OnActionCanceled;
}