using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using FirstGearGames.SmoothCameraShaker;

public class PlayerMovement : MonoBehaviour
{
    public GameManager gameManager;

    [Header("Movement")]
    public float detachedSpeed = 5f;

    [Header("FX")]
    public GameObject collectFXPrefab;
    public GameObject deathFXPrefab;
    public GameObject enemyDeathFX;

    private List<SplineSettings> splineSettingsList = new List<SplineSettings>();
    private SplineSettings currentSplineSettings;
    public SplineContainer currentSpline;

    private float t = 0f;
    private float tDirection = 1f; // 1 = ileri, -1 = geri

    private PlayerInput playerInput;
    private InputAction clickAction;

    private bool detached = false;
    private bool hasShield = false;
    private Vector3 detachedDirection;
    private List<GameObject> collectedTemp = new List<GameObject>();
    public GameObject fxObject;
    public ShakeData cameraShake;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions["Click"];

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
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
    }

    void Update()
    {
        if (!detached)
        {
            if (currentSpline == null || currentSplineSettings == null) return;

            // Spline üzerinde ilerleme
            t += tDirection * currentSplineSettings.splineSpeed * Time.deltaTime;

            if (currentSplineSettings.isClosed)
            {
                if (t > 1f)
                    t -= 1f;
                else if (t < 0f)
                    t += 1f;
            }
            else
            {
                if (t >= 1f)
                {
                    t = 1f;
                    tDirection = -1f;
                }
                else if (t <= 0f)
                {
                    t = 0f;
                    tDirection = 1f;
                }
            }

            // pozisyon ve rotasyon
            Vector3 tangent = currentSpline.EvaluateTangent(t);
            tangent.z = 0f;
            Vector3 pos = currentSpline.EvaluatePosition(t);
            pos.z = 0f;
            transform.position = pos;

            if (tangent != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, tangent);

            // Click action: detach
            if (clickAction != null && clickAction.WasPerformedThisFrame())
            {
                detached = true;
                if (fxObject != null) fxObject.SetActive(true);

                if (currentSplineSettings.isClosed)
                {
                    // Kapalı spline için mevcut mantık
                    Vector3 center = currentSplineSettings.GetCenter();
                    Vector3 toCenter = (center - transform.position).normalized;
                    Vector3 left = Vector3.Cross(Vector3.forward, tangent).normalized;

                    if (currentSplineSettings.Outward)
                    {
                        detachedDirection = (Vector3.Dot(left, toCenter) > 0) ? -left : left;
                    }
                    else
                    {
                        detachedDirection = (Vector3.Dot(left, toCenter) > 0) ? left : -left;
                    }
                }
                else
                {
                    // Açık spline için sadece sağ yön
                    Vector3 left = Vector3.Cross(Vector3.forward, tangent).normalized;
                    detachedDirection = left; // sağ yön
                }

                gameManager.UseMove();
            }
        }
        else
        {
            // detached hareket
            transform.position += detachedDirection * detachedSpeed * Time.deltaTime;

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

                    if (fxObject != null) fxObject.SetActive(false);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Collectible"))
        {
            Collectibles col = other.GetComponent<Collectibles>();

            // 🔹 FX oluştur
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
                Debug.Log("Shield aktif!");
            }
        }
        else
        {
            if (gameManager.gameMode == GameManager.GameMode.Moves && gameManager.movesLeft <= 0)
            {
                gameManager.NoMovesLeft();
            }
            else
            {
                if (other.CompareTag("Line") && detached)
                {
                    foreach (GameObject c in collectedTemp)
                    {
                        gameManager.UpdateCollectible();
                        Destroy(c);
                    }
                    collectedTemp.Clear();

                    SplineSettings newSettings = other.GetComponent<SplineSettings>();
                    if (newSettings != null)
                    {
                        detached = false;
                        if (fxObject != null) fxObject.SetActive(false);
                        CameraShakerHandler.Shake(cameraShake);
                        currentSplineSettings = newSettings;
                        currentSpline = newSettings.GetSpline();
                        t = currentSplineSettings.FindClosestT(transform.position);
                        tDirection = tDirection * -1;
                    }
                }

                if (other.CompareTag("Enemy"))
                {
                    if (hasShield == true)
                    {
                        GameObject fx = Instantiate(enemyDeathFX, other.transform.position, Quaternion.identity);
                        fx.transform.localScale = other.transform.localScale * 6;
                        Destroy(fx, 0.3f); 
                        Destroy(other.gameObject);
                    }
                    else
                    {
                        // 🔹 Enemy'e çarptığında collectible'lar geri gelsin
                        foreach (GameObject c in collectedTemp)
                        {
                            c.SetActive(true);
                        }
                        collectedTemp.Clear();

                        detached = false;
                        if (fxObject != null) fxObject.SetActive(false);

                        PlayDeathFX();
                        gameManager.UpdateHealth();
                    }
                }
            }
        }
    }


    private void PlayDeathFX()
    {
        if (deathFXPrefab != null)
        {
            CameraShakerHandler.Shake(cameraShake);
            GameObject fx = Instantiate(deathFXPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.2f); // 2 saniye sonra otomatik sil
        }
    }
}