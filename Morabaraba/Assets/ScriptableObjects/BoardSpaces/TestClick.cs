using UnityEngine;

public class TestClick : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log($"Clicked {this.gameObject.name}");
    }
}
