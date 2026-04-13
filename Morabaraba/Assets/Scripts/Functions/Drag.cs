using UnityEngine;

public class Drag : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private float zDepth;

    private void Start()
    {
        cam = Camera.main;
        zDepth = cam.WorldToScreenPoint(transform.position).z;
    }

    public void BeginDrag(Vector2 mousePos)
    {
        Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, zDepth);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        offset = transform.position - worldPos;
    }

    public void DragUpdate(Vector2 mousePos)
    {
        Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, zDepth);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        transform.position = worldPos + offset;
    }

    public void EndDrag()
    {
        
    }
}