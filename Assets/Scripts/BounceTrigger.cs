using UnityEngine;

public class BounceTrigger : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceStrength = 12f;

    // Player'ın velocity'sine göre çarptığı yüzeyin normalini hesapla (rotation'ı da hesaba katarak)
    public Vector2 GetBounceNormal(Vector2 playerVelocity)
    {
        if (playerVelocity.magnitude < 0.1f)
        {
            // Velocity çok küçükse, objenin üst yüzeyinin normalini döndür
            return transform.up;
        }

        // Objenin local yönlerini al (rotation'ı hesaba katarak)
        Vector2 objUp = transform.up;
        Vector2 objRight = transform.right;
        Vector2 objDown = -objUp;
        Vector2 objLeft = -objRight;

        // Player'ın velocity'sini objenin local space'ine çevir
        // Objenin rotation'ına göre hangi yüzeye çarptığını belirle
        float dotUp = Vector2.Dot(playerVelocity.normalized, objUp);
        float dotDown = Vector2.Dot(playerVelocity.normalized, objDown);
        float dotRight = Vector2.Dot(playerVelocity.normalized, objRight);
        float dotLeft = Vector2.Dot(playerVelocity.normalized, objLeft);

        // En yüksek dot product'ı bul (player hangi yöne gidiyorsa, o yöndeki yüzeye çarpmıştır)
        float maxDot = Mathf.Max(Mathf.Abs(dotUp), Mathf.Abs(dotDown), Mathf.Abs(dotRight), Mathf.Abs(dotLeft));

        // Hangi yüzeye çarptığını belirle ve normal'i döndür
        if (Mathf.Abs(dotUp) == maxDot)
        {
            // Yukarı gidiyor → alt yüzeye çarpmış → normal: up
            return objUp;
        }
        else if (Mathf.Abs(dotDown) == maxDot)
        {
            // Aşağı gidiyor → üst yüzeye çarpmış → normal: down
            return objDown;
        }
        else if (Mathf.Abs(dotRight) == maxDot)
        {
            // Sağa gidiyor → sol yüzeye çarpmış → normal: right
            return objRight;
        }
        else
        {
            // Sola gidiyor → sağ yüzeye çarpmış → normal: left
            return objLeft;
        }
    }

    public void ApplyBounce(Rigidbody2D rb, Vector2 normal)
    {
        if (rb == null) return;

        Debug.Log("[BOUNCE] Normal = " + normal);

        // Velocity'yi normale göre sektir
        Vector2 reflected = Vector2.Reflect(rb.linearVelocity, normal);

        rb.linearVelocity = reflected.normalized * bounceStrength;
    }
}
