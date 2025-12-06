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

    private List<SplineSettings> splineSettingsList = new List<SplineSettings>();
    private SplineSettings currentSplineSettings;

    private float t = 0f;
    private float tDirection = 1f;
    private PlayerInput playerInput;
    private InputAction clickAction;

    private bool detached = false;
    private bool hasShield = false;
    private Vector3 detachedDirection;

    private List<GameObject> collectedTemp = new List<GameObject>();

    private float detachLockTimer = 0f;
    private int lastReconnectFrame = -9999;

    // 🔶 CORNER SYSTEM
    private bool isInsideCorner = false;        // Sadece corner bool
    private bool waitingForCornerSwitch = false; // Corner içinde detach olunduysa true

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions["Click"];
    }

    void Start()
    {
        splineSettingsList.AddRange(FindObjectsOfType<SplineSettings>());

        if (currentSpline != null)
            currentSplineSettings = currentSpline.GetComponent<SplineSettings>();
        else if (splineSettingsList.Count > 0)
        {
            currentSplineSettings = splineSettingsList[0];
            currentSpline = currentSplineSettings.GetSpline();
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (detachLockTimer > 0f)
            detachLockTimer -= Time.deltaTime;

        // Right click → Pause
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Time.timeScale == 0f) gameOverManager.ResumeGame();
            else gameOverManager.PauseGame();
        }

        // -------------------------------
        // ATTACHED MOVEMENT
        // -------------------------------
        if (!detached)
        {
            if (currentSplineSettings == null) return;

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
            Vector3 pos = currentSpline.EvaluatePosition(t);
            pos.z = 0;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, tangent);

            bool frameAllowed = Time.frameCount > lastReconnectFrame + 1;

            // CLICK → DETACH
            if (detachLockTimer <= 0f && frameAllowed && clickAction.WasPerformedThisFrame())
            {
                if (!isInsideCorner)
                {
                    Debug.Log("PM: Normal DETACH");
                    PerformDetach(tangent);
                }
                else
                {
                    Debug.Log("PM: Detached INSIDE CORNER → waiting for next corner");
                    waitingForCornerSwitch = true;
                    PerformDetach(tangent);
                }
            }
        }
        else
        {
            // -------------------------------
            // DETACHED MOVEMENT
            // -------------------------------
            transform.position += detachedDirection * detachedSpeed * Time.deltaTime;
            transform.right = -detachedDirection.normalized;

            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1)
            {
                if (gameManager.gameMode == GameManager.GameMode.Moves &&
                    gameManager.movesLeft <= 0)
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
    }

    // -------------------------------------
    // DETACH
    // -------------------------------------
    private void PerformDetach(Vector3 tangent)
    {
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
        }
        else
        {
            detachedDirection = Vector3.Cross(Vector3.forward, tangent).normalized;
        }

        gameManager.UseMove();
    }

    // -------------------------------------
    // ON TRIGGER ENTER
    // -------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ENTER: " + other.tag);

        // -------------------------
        // CORNER ENTER
        // -------------------------
        if (other.CompareTag("Corner"))
        {
            isInsideCorner = true;

            // Eğer corner içinde detach olduysak
            if (waitingForCornerSwitch && detached)
            {
                Debug.Log("PM: CORNER HIT WHILE WAITING → Reconnecting!");
                ReconnectToSpline();
                return;
            }

            return;
        }

        // -------------------------
        // CORNER EXIT LOGIC
        // (Corner olmayan bir şeye çarptıysan ve corner içindeysen çıkmışsındır)
        // -------------------------
        if (isInsideCorner && !other.CompareTag("Corner"))
        {
            Debug.Log("PM: EXIT CORNER");
            isInsideCorner = false;
        }

        // -------------------------
        // LINE → Reconnect
        // -------------------------
        if (other.CompareTag("Line") && detached)
        {
            ReconnectToSpline();
            return;
        }

        // -------------------------
        // COLLECTIBLE
        // -------------------------
        if (other.CompareTag("Collectible"))
        {
            HandleCollectible(other);
            return;
        }

        // -------------------------
        // ENEMY
        // -------------------------
        if (other.CompareTag("Enemy"))
        {
            HandleEnemyCollision(other);
            return;
        }
    }

    // -------------------------------------
    // RECONNECT
    // -------------------------------------
    private void ReconnectToSpline()
    {
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

        t = currentSplineSettings.FindClosestT(transform.position);
        tDirection *= -1;

        lastReconnectFrame = Time.frameCount;
        detachLockTimer = 0.15f;
    }

    // -------------------------------------
    // COLLECTIBLE
    // -------------------------------------
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

    // -------------------------------------
    // ENEMY
    // -------------------------------------
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
