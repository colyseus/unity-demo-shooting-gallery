using UnityEngine;

public class RotationAnimation : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotationAxis = Vector3.zero;

    [SerializeField]
    private float rotationSpeed = 0.0f;

    private void FixedUpdate()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}