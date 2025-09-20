using UnityEngine;

public class BlockEnemy : MonoBehaviour
{
    public float rotationSpeed = 90f; // derece/saniye

    void Update()
    {
        // sadece Z ekseninde dön
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
