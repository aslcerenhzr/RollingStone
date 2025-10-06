using UnityEngine;
using UnityEngine.Splines;

public class BlockEnemy : MonoBehaviour
{
    public enum EnemyType
    {
        Rotate,
        MoveUpDown,
        Static,
        SplineFollow,
        Pacman
    }

    [Header("Genel Ayarlar")]
    public EnemyType enemyType = EnemyType.Rotate;

    [Header("Rotate Ayarları")]
    public float rotationSpeed = 90f;

    [Header("MoveUpDown Ayarları")]
    public float moveSpeed = 2f;
    public float moveDistance = 2f;
    private Vector3 startPos;

    [Header("Spline Ayarları")]
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
                // hiçbir şey yapma
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
        // sadece Z ekseninde dön
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void MoveUpDownEnemy()
    {
        // sinüs dalgası ile yukarı-aşağı hareket
        float newY = startPos.y + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void Pacman()
    {
        // Spline üzerinde ilerleme
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

            // pozisyon ve rotasyon
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

        // t değerini direction'a göre artır/azalt
        splineT += direction * splineSpeed * Time.deltaTime;

        // 0-1 arasında tekrar et
        splineT = Mathf.Repeat(splineT, 1f);

        // spline üzerindeki pozisyonu al
        Vector3 position = spline.EvaluatePosition(splineT);
        transform.position = position;

        // spline yönü
        Vector3 tangent = spline.EvaluateTangent(splineT) * direction; // yön ters ise tangent de tersine bakar
        if (tangent != Vector3.zero)
        {
            // Normal yönü hesapla
            Quaternion targetRot = Quaternion.LookRotation(tangent);

            // 90 derece offset ekle
            targetRot *= Quaternion.Euler(0, 90, 90);

            transform.rotation = targetRot;
        }
    }

}
