using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reccy.UnityBezierCurve;

public class Dragon : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_t;
    [SerializeField] private DragonPathSegment m_currentSegment;
    [SerializeField] private Transform[] m_segments;
    [SerializeField] private float m_distanceBetweenSegments = 0.25f;

    [Header("Projectiles")]
    [SerializeField] private GameObject m_projectile;

    private float m_currentDistance;
    
    [SerializeField] private float m_speed = 5.0f;

    private BezierCurve Curve => m_currentSegment.Curve;

    private void OnValidate()
    {
        if (m_currentSegment != null)
        {
            UpdatePosition();
        }
    }

    private void Start()
    {
        m_currentDistance = Curve.Distance(m_t);
    }

    private void FixedUpdate()
    {
        m_currentDistance += m_speed * Time.fixedDeltaTime;

        if (m_currentDistance > Curve.Length)
        {
            m_currentDistance = m_currentDistance - Curve.Length;
            m_currentSegment = m_currentSegment.Next;
        }

        UpdatePosition();
    }
    
    private void UpdatePosition()
    {
        m_t = Curve.T(m_currentDistance);

        float distAcc = 0;

        foreach (var bodySegment in m_segments)
        {
            var lineSegment = m_currentSegment;
            var d = m_currentDistance + distAcc;

            while (d > lineSegment.Curve.Length)
            {
                d -= lineSegment.Curve.Length;
                lineSegment = lineSegment.Next;
            }

            bodySegment.position = lineSegment.Curve.PointDist(d);

            var rot = lineSegment.Curve.TangentDist(d);

            bodySegment.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, rot));

            distAcc += m_distanceBetweenSegments;
        }
    }
}
