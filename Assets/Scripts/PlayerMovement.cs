using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public List<SplineContainer> splines = new List<SplineContainer>();
    private SplineContainer currentSpline;

    public GameManager gameManager;

    public float speed = 0.2f;
    private float t = 0f;
    private float tDirection = 1f; // 1 = ileri, -1 = geri

    private PlayerInput playerInput;
    private InputAction clickAction;

    private bool detached = false;
    private Vector3 detachedDirection;
    public float detachedSpeed = 5f;
    public bool isClosed = true;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions["Click"];
    }

    void Start()
    {
        splines.AddRange(FindObjectsOfType<SplineContainer>());
        if (splines.Count > 0)
            currentSpline = splines[0];
    }

    void Update()
    {
        if (!detached)
        {
            // Spline üzerinde ilerleme
            t += tDirection * speed * Time.deltaTime;

            if (isClosed)
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

            Vector3 left = Vector3.Cross(Vector3.forward, tangent).normalized;

            // Click action: detach
            if (clickAction != null && clickAction.WasPerformedThisFrame())
            {
                detached = true;

                SplineSettings splineSettings = currentSpline.GetComponent<SplineSettings>();

                Vector3 center = GetSplineCenter(currentSpline);
                Vector3 toCenter = (center - transform.position).normalized;

                if (splineSettings != null && splineSettings.detachOutward)
                {
                    // Dışa doğru: her zaman left yönü değil, merkezin tersine
                    detachedDirection = (Vector3.Dot(Vector3.Cross(Vector3.forward, tangent).normalized, toCenter) > 0)
                        ? -Vector3.Cross(Vector3.forward, tangent).normalized
                        : Vector3.Cross(Vector3.forward, tangent).normalized;
                }
                else
                {
                    // Mevcut merkez tabanlı davranış
                    detachedDirection = (Vector3.Dot(Vector3.Cross(Vector3.forward, tangent).normalized, toCenter) > 0)
                        ? Vector3.Cross(Vector3.forward, tangent).normalized
                        : -Vector3.Cross(Vector3.forward, tangent).normalized;
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
        if (detached)
        {
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
                        SplineContainer newSpline = other.GetComponent<SplineContainer>();
                        if (newSpline != null)
                        {
                            detached = false;
                            currentSpline = newSpline;
                            t = FindClosestT(currentSpline, transform.position);
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

    float FindClosestT(SplineContainer spline, Vector3 position)
    {
        float closestT = 0f;
        float minDist = float.MaxValue;

        int steps = 100;
        for (int i = 0; i <= steps; i++)
        {
            float testT = i / (float)steps;
            Vector3 testPos = spline.EvaluatePosition(testT);
            testPos.z = 0f;
            float dist = Vector3.Distance(position, testPos);
            if (dist < minDist)
            {
                minDist = dist;
                closestT = testT;
            }
        }

        return closestT;
    }

    Vector3 GetSplineCenter(SplineContainer spline)
    {
        int steps = 50;
        Vector3 sum = Vector3.zero;

        for (int i = 0; i <= steps; i++)
        {
            float tSample = i / (float)steps;
            Vector3 pos = spline.EvaluatePosition(tSample);
            pos.z = 0f;
            sum += pos;
        }

        return sum / (steps + 1);
    }
}

