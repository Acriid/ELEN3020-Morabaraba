using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Camera cam;

    private Drag currentDrag;

    private void Start()
    {
        cam = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartDrag();
        }
        else if (context.canceled)
        {
            StopDrag();
        }
    }

    private void StartDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            Drag drag = hit.collider.GetComponent<Drag>();

            if (drag != null)
            {
                currentDrag = drag;
                currentDrag.BeginDrag(mousePos);
            }
        }
    }

    private void StopDrag()
    {
        if (currentDrag != null)
        {
            currentDrag.EndDrag();
            currentDrag = null;
        }
    }

    private void Update()
    {
        if (currentDrag != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            currentDrag.DragUpdate(mousePos);
        }
    }
}