using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using FirstGearGames.SmoothCameraShaker;

public class PlayerMovement : MonoBehaviour
{
    public GameManager gameManager;
    public GameOverManager gameOverManager;
    public SplineContainer currentSpline;
    public ShakeData cameraShake;

    [Header("Movement")]
    public float detachedSpeed = 5f;

    [Header("FX")]
    public GameObject collectFXPrefab;
    public GameObject deathFXPrefab;
    public GameObject enemyDeathFX;
    public GameObject PlayerFX;
    [Header("Audio")]
    public AudioClip collectSound; // Unity Inspector'dan ses dosyasını buraya sürükle
    private AudioSource audioSource;
    private List<SplineSettings> splineSettingsList = new List<SplineSettings>();
    private SplineSettings currentSplineSettings;
    private float t = 0f;
    private float tDirection = 1f; // 1 = ileri, -1 = geri
    private PlayerInput playerInput;
    private InputAction clickAction;
    private bool detached = false;
    private bool hasShield = false;
    private Vector3 detachedDirection;
    private List<GameObject> collectedTemp = new List<GameObject>();
    [SerializeField] private LayerMask splineLayerMask;
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions["Click"];
    }

    void Start()
    {
        splineSettingsList.AddRange(FindObjectsOfType<SplineSettings>());

        if (currentSpline != null)
        {
            currentSplineSettings = currentSpline.GetComponent<SplineSettings>();
        }
        else if (splineSettingsList.Count > 0)
        {
            currentSplineSettings = splineSettingsList[0];
            currentSpline = currentSplineSettings.GetSpline();
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (gameOverManager != null)
            {
                if (Time.timeScale == 0f)
                    gameOverManager.ResumeGame();
                else
                    gameOverManager.PauseGame();
            }
        }
        if (!detached)
                {
                    HandleSplineMovement();
                }
                else
                {
                    HandleDetachedMovement();
                }
    }

    // 🔹 Spline üzerindeki hareket kodu buraya alındı (Update temizliği için)
    private void HandleSplineMovement()
    {
        if (currentSpline == null || currentSplineSettings == null) return;

        t += tDirection * currentSplineSettings.splineSpeed * Time.deltaTime;

        if (currentSplineSettings.isClosed)
        {
            if (t > 1f) t -= 1f;
            else if (t < 0f) t += 1f;
        }
        else
        {
            if (t >= 1f) { t = 1f; tDirection = -1f; }
            else if (t <= 0f) { t = 0f; tDirection = 1f; }
        }

        Vector3 tangent = currentSpline.EvaluateTangent(t);
        tangent.z = 0f;
        Vector3 pos = currentSpline.EvaluatePosition(t);
        pos.z = 0f;
        transform.position = pos;

        if (tangent != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, tangent);

        if (clickAction != null && clickAction.WasPerformedThisFrame())
        {
            DetachFromSpline(tangent);
        }
    }

    private void DetachFromSpline(Vector3 tangent)
    {
        detached = true;

        if (PlayerFX != null)
        {
            PlayerFX.SetActive(true);
            if (hasShield)
            {
                var sprite = PlayerFX.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.color = Color.red;
            }
        }

        if (currentSplineSettings.isClosed)
        {
            Vector3 center = currentSplineSettings.GetCenter();
            Vector3 toCenter = (center - transform.position).normalized;
            Vector3 left = Vector3.Cross(Vector3.forward, tangent).normalized;

            if (currentSplineSettings.Outward)
                detachedDirection = (Vector3.Dot(left, toCenter) > 0) ? -left : left;
            else
                detachedDirection = (Vector3.Dot(left, toCenter) > 0) ? left : -left;
        }
        else
        {
            Vector3 left = Vector3.Cross(Vector3.forward, tangent).normalized;
            detachedDirection = left;
        }

        gameManager.UseMove();
    }

    // 🔹 KRİTİK BÖLÜM: Raycast ile önceden algılama
    private void HandleDetachedMovement()
    {
        float moveDistance = detachedSpeed * Time.deltaTime;

        // Hareket etmeden önce, gideceğimiz yol üzerinde "Line" var mı diye bakıyoruz.
        // moveDistance kadar ileriye ışın atıyoruz.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, detachedDirection, moveDistance);

        // Eğer bir şeye çarptıysak VE çarptığımız şey "Line" ise
        if (hit.collider != null && hit.collider.CompareTag("Line"))
        {
             // Çarpışmayı manuel olarak tetikle ve hareketi durdur
             ConnectToSpline(hit.collider);
             return; // Bu frame'de daha fazla ilerleme
        }

        // Eğer önümüz boşsa normal ilerle
        transform.position += detachedDirection * moveDistance;
        transform.right = -detachedDirection.normalized;

        CheckBounds();
    }

    private void CheckBounds()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            if (gameManager.gameMode == GameManager.GameMode.Moves && gameManager.movesLeft <= 0)
            {
                gameManager.NoMovesLeft();
            }
            else
            {
                PlayDeathFX();
                gameManager.UpdateHealth();
                detached = false;
                if (PlayerFX != null) PlayerFX.SetActive(false);
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Collectible"))
        {
            HandleCollectible(other);
        }
        else if (other.CompareTag("Line") && detached)
        {
            // Raycast kaçırırsa (çok nadir) burası yakalar
            ConnectToSpline(other);
        }
        else if (other.CompareTag("Enemy"))
        {
            HandleEnemyCollision(other);
        }
    }

    // 🔹 Ortak Spline Bağlanma Fonksiyonu
    private void ConnectToSpline(Collider2D splineCollider)
    {
        // Eğer zaten bağlıysak tekrar bağlanma (bazen raycast ve trigger çakışabilir)
        if (!detached) return;

        foreach (GameObject c in collectedTemp)
        {
            gameManager.UpdateCollectible();
            Destroy(c);
        }
        collectedTemp.Clear();

        SplineSettings newSettings = splineCollider.GetComponent<SplineSettings>();
        if (newSettings != null)
        {
            detached = false;
            if (PlayerFX != null) PlayerFX.SetActive(false);
            CameraShakerHandler.Shake(cameraShake);

            currentSplineSettings = newSettings;
            currentSpline = newSettings.GetSpline();
            
            // Player'ı tam olarak çarpışma noktasına veya spline'daki en yakın noktaya taşı
            // Bu, tunneling yüzünden spline'ın içine girmeyi görsel olarak düzeltir
            t = currentSplineSettings.FindClosestT(transform.position);
            
            // Eğer Raycast ile çarptıysak pozisyonu hemen güncelle ki "titreme" olmasın
            Vector3 snapPos = currentSpline.EvaluatePosition(t);
            snapPos.z = 0;
            transform.position = snapPos;

            tDirection = tDirection * -1;
        }
    }

    private void HandleCollectible(Collider2D other)
    {
        Collectibles col = other.GetComponent<Collectibles>();

        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        if (collectFXPrefab != null)
        {
            GameObject fx = Instantiate(collectFXPrefab, other.transform.position, Quaternion.identity);
            Destroy(fx, 0.3f);
        }

        other.gameObject.SetActive(false);
        collectedTemp.Add(other.gameObject);

        if (col.collectibleType == Collectibles.CollectibleType.Shield)
        {
            hasShield = true;
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void HandleEnemyCollision(Collider2D other)
    {
        if (hasShield)
        {
            GameObject fx = Instantiate(enemyDeathFX, other.transform.position, Quaternion.identity);
            fx.transform.localScale = other.transform.localScale * 6;
            Destroy(fx, 0.3f);
            Destroy(other.gameObject);

            hasShield = false;
            GetComponent<SpriteRenderer>().color = Color.white;

            var sprite = PlayerFX.GetComponent<SpriteRenderer>();
            sprite.color = Color.white;
        }
        else
        {
            foreach (GameObject c in collectedTemp)
            {
                c.SetActive(true);
            }
            collectedTemp.Clear();

            detached = false;
            if (PlayerFX != null) PlayerFX.SetActive(false);

            PlayDeathFX();
            gameManager.UpdateHealth();
        }
    }

    private void PlayDeathFX()
    {
        if (deathFXPrefab != null)
        {
            CameraShakerHandler.Shake(cameraShake);
            GameObject fx = Instantiate(deathFXPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.2f);
        }
    }
}