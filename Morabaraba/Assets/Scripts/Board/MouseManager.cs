using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    private InputSystem_Actions _inputActions;
    private InputAction _mouseAction;

    void Awake()
    {
        _inputActions = new();

        

        _mouseAction = _inputActions.Player.Click;

        _mouseAction.performed += OnClick;

        _mouseAction.Enable();
    }

    void OnDisable()
    {
        _mouseAction.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.zero);

        if(hit.collider != null)
        {
            Debug.Log(hit.collider.name);
        }
    }
}
