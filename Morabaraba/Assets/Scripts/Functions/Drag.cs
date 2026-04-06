using UnityEngine;
using UnityEngine.InputSystem;

public class Drag : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;
    private float zDepth;

    private void Start()
    {
        cam = Camera.main;
        zDepth = cam.WorldToScreenPoint(transform.position).z;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, zDepth);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

            transform.position = worldPos + offset;
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TryStartDrag();
        }
        else if (context.canceled)
        {
            isDragging = false;
        }
    }

    private void TryStartDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, zDepth);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

            offset = transform.position - worldPos;
            isDragging = true;
        }
    }
}