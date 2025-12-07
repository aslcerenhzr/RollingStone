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
    public AudioClip collectSound;
    private AudioSource audioSource;

    public SplineSettings currentSplineSettings;
    private List<SplineSettings> splineSettingsList = new List<SplineSettings>();

    private float t = 0f;
    private float tDirection = 1f;

    private PlayerInput playerInput;
    private InputAction clickAction;
    
    // Input System için doğrudan InputActionAsset kullan (tüm klonlar için çalışır)
    private InputActionAsset inputActions;
    private InputActionMap playerActionMap;
    private InputAction directClickAction;

    private bool detached = false;
    private bool hasShield = false;
    private Vector3 detachedDirection;

    // Public getter/setter for cloning
    public bool Detached { get => detached; set => detached = value; }
    public Vector3 DetachedDirection { get => detachedDirection; set => detachedDirection = value; }

    private float detachLockTimer = 0f;
    private int lastReconnectFrame = -9999;
    private float portalCooldown = 0f;
    private int lastPortalID = -1;

    private List<GameObject> collectedTemp = new List<GameObject>();

    // CORNER SYSTEM
    private bool isInsideCorner = false;
    private bool waitingForCornerSwitch = false;

    // Rigidbody
    private Rigidbody2D rb;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            clickAction = playerInput.actions["Click"];
            // InputActionAsset'i de al (klonlar için)
            inputActions = playerInput.actions;
            if (inputActions != null)
            {
                playerActionMap = inputActions.FindActionMap("Player");
                if (playerActionMap != null)
                {
                    directClickAction = playerActionMap.FindAction("Click");
                    if (directClickAction != null)
                    {
                        directClickAction.Enable();
                    }
                }
            }
        }

        Debug.Log("[INIT] PlayerMovement Awake()");
    }

    void Start()
    {
        Debug.Log("[INIT] PlayerMovement Start()");

        splineSettingsList.AddRange(FindObjectsOfType<SplineSettings>());
        Debug.Log("[INIT] Toplam spline settings sayısı: " + splineSettingsList.Count);

        if (currentSpline != null)
        {
            currentSplineSettings = currentSpline.GetComponent<SplineSettings>();
            Debug.Log("[INIT] Başlangıç spline mevcut: " + currentSpline.name);
        }
        else if (splineSettingsList.Count > 0)
        {
            currentSplineSettings = splineSettingsList[0];
            currentSpline = currentSplineSettings.GetSpline();
            Debug.Log("[INIT] currentSpline null idi, ilk spline atandı: " + currentSplineSettings.name);
        }
        else
        {
            Debug.LogError("[INIT] Hiç spline bulunamadı! Hareket edemez.");
        }

        audioSource = GetComponent<AudioSource>();
    }

    // -------------------------------------------------------
    // FIXED UPDATE — detached physics movement
    // -------------------------------------------------------
    void FixedUpdate()
    {
        if (detached)
        {
            rb.linearVelocity = detachedDirection * detachedSpeed;
            transform.right = -detachedDirection.normalized;

            Debug.Log("[DETACHED MOVE] pos=" + transform.position + " dir=" + detachedDirection);

        }
    }


    // -------------------------------------------------------
    // UPDATE — spline movement + detach inputs
    // -------------------------------------------------------
    void Update()
    {
        if (detachLockTimer > 0f)
            detachLockTimer -= Time.deltaTime;

        if (portalCooldown > 0f)
            portalCooldown -= Time.deltaTime;

        // ATTACHED SPLINE MOVEMENT
        if (!detached)
        {
            if (currentSplineSettings == null)
            {
                Debug.LogError("[ERROR] currentSplineSettings NULL!");
                return;
            }

            t += tDirection * currentSplineSettings.splineSpeed * Time.deltaTime;

            if (currentSplineSettings.isClosed)
            {
                if (t > 1f) t -= 1f;
                if (t < 0f) t += 1f;
            }
            else
            {
                if (t >= 1f) { t = 1f; tDirection = -1f; }
                else if (t <= 0f) { t = 0f; tDirection = 1f; }
            }

            Vector3 tangent = currentSpline.EvaluateTangent(t);
            Vector3 pos = currentSpline.EvaluatePosition(t);
            pos.z = 0;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, tangent);

            bool frameAllowed = Time.frameCount > lastReconnectFrame + 1;

            // Input kontrolü - Input System kullan (tüm klonlar için çalışır)
            bool clickPressed = false;
            if (directClickAction != null)
            {
                // Doğrudan InputAction kullan (klonlar için)
                clickPressed = directClickAction.WasPerformedThisFrame();
            }
            else if (clickAction != null)
            {
                // PlayerInput üzerinden InputAction kullan
                clickPressed = clickAction.WasPerformedThisFrame();
            }

            // INPUT DEBUG
            if (clickPressed)
            {
                Debug.Log("[INPUT] Click algılandı. detached=" + detached +
                          " | detachLockTimer=" + detachLockTimer +
                          " | frameAllowed=" + frameAllowed +
                          " | isInsideCorner=" + isInsideCorner +
                          " | waitingForCornerSwitch=" + waitingForCornerSwitch);
            }

            // DETACH
            if (detachLockTimer <= 0f &&
                frameAllowed &&
                clickPressed)
            {
                if (!isInsideCorner)
                {
                    Debug.Log("[DETACH] Normal detach.");
                    PerformDetach(tangent);
                }
                else
                {
                    Debug.Log("[DETACH] Corner içindeyiz → corner switch modu.");
                    waitingForCornerSwitch = true;
                    PerformDetach(tangent);
                }
            }
        }
        else
        {
            // OFF-SCREEN CHECK
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1)
            {
                Debug.Log("[DEATH] Oyuncu ekran dışına çıktı!");

                if (gameManager.gameMode == GameManager.GameMode.Moves &&
                    gameManager.movesLeft <= 0)
                {
                    Debug.Log("[DEATH] Moves bitti!");
                    gameManager.NoMovesLeft();
                }
                else
                {
                    PlayDeathFX();
                    gameManager.UpdateHealth();
                    detached = false;
                }
            }
        }
    }

    // -------------------------------------------------------
    // DETACH
    // -------------------------------------------------------
    private void PerformDetach(Vector3 tangent)
    {
        Debug.Log("[DETACH] PerformDetach çağrıldı. tangent=" + tangent);

        detached = true;
        if (PlayerFX != null) PlayerFX.SetActive(true);

        if (currentSplineSettings.isClosed)
        {
            Vector3 center = currentSplineSettings.GetCenter();
            Vector3 toCenter = (center - transform.position).normalized;
            Vector3 left = Vector3.Cross(Vector3.forward, tangent).normalized;

            detachedDirection =
                (Vector3.Dot(left, toCenter) > 0)
                ? (currentSplineSettings.Outward ? -left : left)
                : (currentSplineSettings.Outward ? left : -left);
            rb.linearVelocity = detachedDirection * detachedSpeed;

            Debug.Log("[DETACH] Kapalı spline yön → detachedDirection=" + detachedDirection);
        }
        else
        {
            detachedDirection = Vector3.Cross(Vector3.forward, tangent).normalized;
            Debug.Log("[DETACH] Açık spline yön, detachedDirection=" + detachedDirection);
        }

        gameManager.UseMove();
    }

    // -------------------------------------------------------
    // TRIGGER ENTER
    // -------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[TRIGGER] Enter → " + other.tag + " | detached=" + detached);
        Debug.Log("[TRIGGER] Enter → " + other);

        if (other.CompareTag("Bounce"))
        {
            BounceTrigger bounce = other.GetComponent<BounceTrigger>();
            if (bounce != null && detached)
            {
                // Player'ın mevcut velocity'sine göre çarptığı yüzeyin normalini al
                Vector2 oldVelocity = rb.linearVelocity;
                Vector2 normal = bounce.GetBounceNormal(oldVelocity);
                
                // Velocity'yi normale göre sektir
                Vector2 reflected = Vector2.Reflect(oldVelocity, normal);
                Vector2 newVelocity = reflected.normalized * bounce.bounceStrength;
                
                // Hem velocity hem de direction'ı güncelle
                rb.linearVelocity = newVelocity;
                detachedDirection = newVelocity.normalized;
                
                Debug.Log("[BOUNCE] Old Velocity = " + oldVelocity + " | Normal = " + normal + " | New Velocity = " + newVelocity + " | New Direction = " + detachedDirection);
            }
        }

        // PORTAL
        if (other.CompareTag("Portal"))
        {
            Portal portal = other.GetComponent<Portal>();
            if (portal != null && detached && portalCooldown <= 0f)
            {
                Portal otherPortal = portal.GetOtherPortal();
                if (otherPortal != null)
                {

                    Debug.Log("[PORTAL] Portal ID " + portal.portalID + " → Diğer portala ışınlanıyor");
                    
                    // Diğer portaldan çıkış yönünü al
                    Vector2 exitDirection = otherPortal.GetExitDirection();
                    
                    // Player'ı diğer portala taşı
                    transform.position = otherPortal.transform.position;
                    
                    // Velocity'yi çıkış yönüne göre ayarla
                    Vector2 newVelocity = exitDirection.normalized * rb.linearVelocity.magnitude;
                    rb.linearVelocity = newVelocity;
                    detachedDirection = exitDirection.normalized;
                    
                    // Cooldown ve son portal ID'sini kaydet
                    portalCooldown = 0.5f; // 0.5 saniye cooldown
                    lastPortalID = portal.portalID;
                    
                    Debug.Log("[PORTAL] Yeni pozisyon = " + transform.position + " | Yeni yön = " + exitDirection);
                }
                else
                {
                    Debug.LogWarning("[PORTAL] Portal ID " + portal.portalID + " için başka portal bulunamadı!");
                }
            }
        }

        // TRIPLE CLONE
        if (other.CompareTag("TripleClone"))
        {
            TripleClone tripleClone = other.GetComponent<TripleClone>();
            if (tripleClone != null && detached)
            {
                tripleClone.SplitPlayer(gameObject);
            }
        }

        // CORNER
        if (other.CompareTag("Corner"))
        {
            Debug.Log("[CORNER] Corner bölgesine girildi.");
            isInsideCorner = true;

            if (waitingForCornerSwitch && detached)
            {
                Debug.Log("[CORNER] Corner switch tetiklendi → reconnect");
                ReconnectToSpline();
            }
            return;
        }

        // EXIT CORNER
        if (isInsideCorner && !other.CompareTag("Corner"))
        {
            Debug.Log("[CORNER] Corner'dan çıkıldı.");
            isInsideCorner = false;
        }

        // LINE
        if (other.CompareTag("Line") && detached)
        {
            Debug.Log("[LINE] Line ile çarpışıldı → bağlanmaya çalışılıyor.");

            var splineRef = other.GetComponent<SplineSettings>();

            if (splineRef == null)
            {
                Debug.LogError("[LINE] SplineSettings bulunamadı!");
            }
            else
            {
                Debug.Log("[LINE] SplineSettings bulundu: " + splineRef.name);
                currentSpline = splineRef.spline;
                currentSplineSettings = currentSpline.GetComponent<SplineSettings>();
            }

            ReconnectToSpline();
            return;
        }

        // ITEMS
        if (other.CompareTag("Collectible"))
        {
            Debug.Log("[ITEM] Collectible alındı: " + other.name);
            HandleCollectible(other);
            return;
        }

        // ENEMY
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("[ENEMY] Enemy ile çarpışma!");
            HandleEnemyCollision(other);
            return;
        }
    }

    // -------------------------------------------------------
    // RECONNECT TO SPLINE
    // -------------------------------------------------------
    private void ReconnectToSpline()
    {
        Debug.Log("[RECONNECT] ReconnectToSpline() çağrıldı.");

        foreach (GameObject c in collectedTemp)
        {
            gameManager.UpdateCollectible();
            Destroy(c);
        }
        collectedTemp.Clear();

        detached = false;
        waitingForCornerSwitch = false;
        isInsideCorner = false;

        if (PlayerFX != null) PlayerFX.SetActive(false);

        CameraShakerHandler.Shake(cameraShake);

        if (currentSplineSettings == null)
        {
            Debug.LogError("[RECONNECT] currentSplineSettings NULL!");
            return;
        }

        float oldT = t;
        t = currentSplineSettings.FindClosestT(transform.position);

        Debug.Log("[RECONNECT] t eski=" + oldT + " → yeni=" + t);

        tDirection *= -1;
        Debug.Log("[RECONNECT] tDirection now=" + tDirection);

        lastReconnectFrame = Time.frameCount;
        detachLockTimer = 0.15f;

        Debug.Log("[RECONNECT] reconnect tamamlandı.");
    }

    // -------------------------------------------------------
    private void HandleCollectible(Collider2D other)
    {
        Collectibles col = other.GetComponent<Collectibles>();

        if (collectSound != null) audioSource.PlayOneShot(collectSound);

        if (collectFXPrefab != null)
        {
            var fx = Instantiate(collectFXPrefab, other.transform.position, Quaternion.identity);
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
            var fx = Instantiate(enemyDeathFX, other.transform.position, Quaternion.identity);
            fx.transform.localScale = other.transform.localScale * 6;
            Destroy(fx, 0.3f);
            Destroy(other.gameObject);

            hasShield = false;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            foreach (GameObject c in collectedTemp)
                c.SetActive(true);

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
            var fx = Instantiate(deathFXPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.2f);
        }
    }
}
