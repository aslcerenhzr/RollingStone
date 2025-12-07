using UnityEngine;

public class TripleClone : MonoBehaviour
{
    [Header("Clone Settings")]
    [Tooltip("Klonların ayrılma açısı (derece)")]
    public float splitAngle = 15f;

    /// <summary>
    /// Klonun PlayerInput ve diğer component'lerini düzgün initialize eder
    /// </summary>
    private void SetupClone(GameObject clone, PlayerMovement pm, Vector2 direction, float speed, PlayerMovement originalPlayer)
    {
        if (pm == null) return;

        // Orijinal player'ın spline bilgilerini klona kopyala
        if (originalPlayer != null)
        {
            pm.currentSpline = originalPlayer.currentSpline;
            pm.currentSplineSettings = originalPlayer.currentSplineSettings;
            
            // SplineSettings listesini de kopyala
            if (originalPlayer.currentSplineSettings != null)
            {
                // Klonun Start metodunda splineSettingsList doldurulacak, ama şimdilik currentSplineSettings'i ayarla
                pm.currentSplineSettings = originalPlayer.currentSplineSettings;
            }
        }

        pm.Detached = true;
        pm.DetachedDirection = direction;
        
        Rigidbody2D rb = clone.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        
        if (pm.PlayerFX != null) pm.PlayerFX.SetActive(true);

        // PlayerInput component'ini kontrol et ve InputActionAsset'i initialize et
        UnityEngine.InputSystem.PlayerInput playerInput = clone.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
        {
            // Klonlar için InputActionAsset'i düzgün initialize et
            var actionMap = playerInput.actions.FindActionMap("Player");
            if (actionMap != null)
            {
                var clickAction = actionMap.FindAction("Click");
                if (clickAction != null)
                {
                    clickAction.Enable();
                }
            }
        }
    }

    /// <summary>
    /// Player'ı 3'e böler: biri düz, biri +15°, biri -15° açıyla hareket eder
    /// </summary>
    public void SplitPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning("[TRIPLE CLONE] Player null!");
            return;
        }

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerMovement == null || playerRb == null)
        {
            Debug.LogWarning("[TRIPLE CLONE] PlayerMovement veya Rigidbody2D bulunamadı!");
            return;
        }

        if (!playerMovement.Detached)
        {
            Debug.LogWarning("[TRIPLE CLONE] Player detached modda değil!");
            return;
        }

        Debug.Log("[TRIPLE CLONE] Player 3'e bölünüyor!");

        // Mevcut player'ın pozisyonu ve velocity'si
        Vector3 currentPos = player.transform.position;
        Vector2 currentVelocity = playerRb.linearVelocity;
        float currentSpeed = currentVelocity.magnitude;
        Vector2 currentDirection = currentVelocity.normalized;

        // Açıları hesapla (derece cinsinden)
        float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float angleRight = currentAngle + splitAngle;  // +15 derece sağa
        float angleLeft = currentAngle - splitAngle;   // -15 derece sola

        // Yönleri hesapla
        Vector2 directionRight = new Vector2(Mathf.Cos(angleRight * Mathf.Deg2Rad), Mathf.Sin(angleRight * Mathf.Deg2Rad));
        Vector2 directionLeft = new Vector2(Mathf.Cos(angleLeft * Mathf.Deg2Rad), Mathf.Sin(angleLeft * Mathf.Deg2Rad));
        Vector2 directionStraight = currentDirection; // Düz yön

        // 3 yeni player klonu oluştur
        GameObject cloneStraight = Instantiate(player, currentPos, Quaternion.identity);
        GameObject cloneRight = Instantiate(player, currentPos, Quaternion.identity);
        GameObject cloneLeft = Instantiate(player, currentPos, Quaternion.identity);

        // Klonların scale'ini ayarla (1.5, 1.5, 1)
        Vector3 newScale = new Vector3(1.5f, 1.5f, 1f);
        cloneStraight.transform.localScale = newScale;
        cloneRight.transform.localScale = newScale;
        cloneLeft.transform.localScale = newScale;

        // Klonların PlayerMovement component'lerini al
        PlayerMovement pmStraight = cloneStraight.GetComponent<PlayerMovement>();
        PlayerMovement pmRight = cloneRight.GetComponent<PlayerMovement>();
        PlayerMovement pmLeft = cloneLeft.GetComponent<PlayerMovement>();

        // Klonları detached moda al ve yönlerini ayarla
        SetupClone(cloneStraight, pmStraight, directionStraight, currentSpeed, playerMovement);
        SetupClone(cloneRight, pmRight, directionRight, currentSpeed, playerMovement);
        SetupClone(cloneLeft, pmLeft, directionLeft, currentSpeed, playerMovement);

        // Orijinal player'ı destroy et
        Destroy(player);

        Debug.Log("[TRIPLE CLONE] 3 clone oluşturuldu: Düz, +" + splitAngle + "°, -" + splitAngle + "° (Scale: 1.5)");
        
        // Bir kere kullanıldıktan sonra kendini destroy et
        Destroy(gameObject);
    }
}
