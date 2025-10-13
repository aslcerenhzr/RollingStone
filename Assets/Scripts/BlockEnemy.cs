using UnityEngine;
using UnityEngine.Splines;
public class BlockEnemy : MonoBehaviour
{
    public enum EnemyType
    {
        Rotate, MoveUpDown, Static, SplineFollow, Pacman
    }

    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Rotate;
    public float rotationSpeed = 90f;
    public float moveSpeed = 2f;
    public float moveDistance = 2f;
    private Vector3 startPos;

    [Header("Spline Settings")]
    public SplineContainer spline;
    public SplineSettings splineSettings; 
    public float splineSpeed = 0.2f;
    private float splineT = 0f;
    public float direction = 1f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        switch (enemyType)
        {
            case EnemyType.Rotate:
                RotateEnemy();
                break;

            case EnemyType.MoveUpDown:
                MoveUpDownEnemy();
                break;

            case EnemyType.Static:
                break;

            case EnemyType.SplineFollow:
                FollowSpline();
                break;
            case EnemyType.Pacman:
                Pacman();
                break;
        }
    }

    void RotateEnemy()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void MoveUpDownEnemy()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void Pacman()
    {
        splineT += direction * splineSpeed * Time.deltaTime;

        if (splineSettings.isClosed)
        {
                if (splineT > 1f)
                    splineT -= 1f;
                else if (splineT < 0f)
                    splineT += 1f;
        }
        else
        {
                if (splineT >= 1f)
                {
                    splineT = 1f;
                    direction = -1f;
                }
                else if (splineT <= 0f)
                {
                    splineT = 0f;
                    direction = 1f;
                }
        }

        Vector3 tangent = spline.EvaluateTangent(splineT);
        tangent.z = 0f;
        Vector3 pos = spline.EvaluatePosition(splineT);
        pos.z = 0f;
        transform.position = pos;

        if (tangent != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, tangent);
    }

    void FollowSpline()
    {
        if (spline == null) return;

        splineT += direction * splineSpeed * Time.deltaTime;
        splineT = Mathf.Repeat(splineT, 1f);
        Vector3 position = spline.EvaluatePosition(splineT);
        transform.position = position;

        Vector3 tangent = spline.EvaluateTangent(splineT) * direction;
        if (tangent != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(tangent);
            targetRot *= Quaternion.Euler(0, 90, 90);
            transform.rotation = targetRot;
        }
    }
}