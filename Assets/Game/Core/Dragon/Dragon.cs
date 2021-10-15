using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reccy.UnityBezierCurve;

public class Dragon : MonoBehaviour
{
    [SerializeField] private float m_t;
    [SerializeField] private DragonPathSegment m_currentSegment;

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
        
        transform.position = Curve.Point(m_t);

        var rot = Curve.Tangent(m_t);

        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, rot));
    }
}
