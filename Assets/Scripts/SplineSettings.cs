using UnityEngine;
using UnityEngine.Splines;

public class SplineSettings : MonoBehaviour
{
    [Header("Spline Ayarları")]
    public bool Outward = false;   // true => detached dışarı doğru
    public bool isClosed = true;   // spline kapalı mı?

    private SplineContainer spline;

    void Awake()
    {
        spline = GetComponent<SplineContainer>();
    }

    public SplineContainer GetSpline()
    {
        return spline;
    }

    // Spline merkezini hesapla
    public Vector3 GetCenter(int steps = 50)
    {
        Vector3 sum = Vector3.zero;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pos = spline.EvaluatePosition(t);
            pos.z = 0f;
            sum += pos;
        }
        return sum / (steps + 1);
    }

    // Spline üzerinde en yakın noktayı bul
    public float FindClosestT(Vector3 position, int steps = 100)
    {
        float closestT = 0f;
        float minDist = float.MaxValue;

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
}
