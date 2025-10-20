using UnityEngine;

public class Rotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Derece/saniye cinsinden hız

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.World);
    }
}
