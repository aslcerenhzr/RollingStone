using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
public class SplineColliderGenerator : MonoBehaviour
{
    public int resolution = 20;
    private SplineContainer splineContainer;
    private EdgeCollider2D edgeCollider;

    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;

        GenerateCollider();
    }

    public void GenerateCollider()
    {
        Spline spline = splineContainer.Spline;
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 pos3D = spline.EvaluatePosition(t);
            points.Add(new Vector2(pos3D.x, pos3D.y));
        }
        edgeCollider.points = points.ToArray();
    }
}