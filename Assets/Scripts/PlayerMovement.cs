using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public GameManager gameManager;

    [Header("Hareket Ayarları")]
    public float speed = 0.2f;
    public float detachedSpeed = 5f;

    private List<SplineSettings> splineSettingsList = new List<SplineSettings>();
    private SplineSettings currentSplineSettings;
    public SplineContainer currentSpline;

    private float t = 0f;
    private float tDirection = 1f; // 1 = ileri, -1 = geri

    private PlayerInput playerInput;
    private InputAction clickAction;

    private bool detached = false;
    private Vector3 detachedDirection;

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
    }

    void Update()
    {
        if (!detached)
        {
            if (currentSpline == null || currentSplineSettings == null) return;

            // Spline üzerinde ilerleme
            t += tDirection * speed * Time.deltaTime;

            if (currentSplineSettings.isClosed)
            {
                if (t > 1f) t = 0f;
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
                    gameManager.UpdateHealth();
                    detached = false;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!detached) return;

        if (other.CompareTag("Collectible"))
        {
            Destroy(other.gameObject);
            gameManager.UpdateCollectible();
        }
        else
        {
            if (gameManager.gameMode == GameManager.GameMode.Moves && gameManager.movesLeft <= 0)
            {
                gameManager.NoMovesLeft();
            }
            else
            {
                if (other.CompareTag("Line"))
                {
                    SplineSettings newSettings = other.GetComponent<SplineSettings>();
                    if (newSettings != null)
                    {
                        detached = false;
                        currentSplineSettings = newSettings;
                        currentSpline = newSettings.GetSpline();
                        t = currentSplineSettings.FindClosestT(transform.position);
                    }
                }

                if (other.CompareTag("Enemy"))
                {
                    detached = false;
                    gameManager.UpdateHealth();
                }
            }
        }
    }
}

