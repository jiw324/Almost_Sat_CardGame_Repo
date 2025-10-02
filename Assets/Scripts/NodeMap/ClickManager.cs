using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    private InputSystem_Actions controls;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        controls.NodeMap.Enable();
        controls.NodeMap.Click.performed += OnClick;
    }

    private void OnDisable()
    {
        controls.NodeMap.Click.performed -= OnClick;
        controls.NodeMap.Disable();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Clicked on: " + hit.collider.gameObject.name);
        }
    }
}