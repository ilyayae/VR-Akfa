using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TactileButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public InputActionReference ButtonAction;
    public InputActionReference VectorAction;
    public Vector2 vectorMoveTreshold;
    Button targetButton;
    public float clickCooldown = 0.5f;
    private float lastClickTime = 0f;
    bool isPhysicallyPressed = false;
    void Start()
    {
        targetButton = transform.parent.gameObject.GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (ButtonAction != null)
        {
            ButtonAction.action.started += PressButton;
            ButtonAction.action.canceled += UnPressButton;
        }
        if (VectorAction != null)
        {
            VectorAction.action.started += MoveVector;
            VectorAction.action.canceled += StopVector;
        }
    }

    private void OnDisable()
    {
        if (ButtonAction != null)
        {
            ButtonAction.action.started -= PressButton;
            ButtonAction.action.canceled -= UnPressButton;
        }
        if (VectorAction != null)
        {
            VectorAction.action.started -= MoveVector;
            VectorAction.action.canceled -= StopVector;
        }
    }

    private void PressButton(InputAction.CallbackContext context)
    {
        EmulatePressDown();
    }
    private void UnPressButton(InputAction.CallbackContext context)
    {
        EmulatePressUp();
    }

    private void MoveVector(InputAction.CallbackContext context)
    {
        Vector2 stickValue = context.ReadValue<Vector2>();
        if(vectorMoveTreshold.x < 0)
        {
            if (stickValue.x < vectorMoveTreshold.x)
            {
                EmulatePressDown();
            }
        }
        else if (vectorMoveTreshold.x > 0)
        {
            if (stickValue.x > vectorMoveTreshold.x)
            {
                EmulatePressDown();
            }
        }
        if (vectorMoveTreshold.y < 0)
        {
            if (stickValue.y < vectorMoveTreshold.y)
            {
                EmulatePressDown();
            }
        }
        else if (vectorMoveTreshold.y > 0)
        {
            if (stickValue.y > vectorMoveTreshold.y)
            {
                EmulatePressDown();
            }
        }
    }
    private void StopVector(InputAction.CallbackContext context)
    {
        EmulatePressUp();
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
