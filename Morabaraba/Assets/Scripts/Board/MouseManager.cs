using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    private InputSystem_Actions _inputActions;
    private InputAction _mouseAction;

    void Awake()
    {
        _inputActions = new();

        _inputActions.Enable();

        
    }
}
