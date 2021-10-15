using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reccy.UnityBezierCurve;
using Reccy.DebugExtensions;
using UnityEditor;

[SelectionBase]
public class DragonPathSegment : MonoBehaviour
{
    private BezierCurve m_curve;
    public BezierCurve Curve
    {
        get
        {
            if (m_curve == null)
                m_curve = new BezierCurve(m_pointA, m_pointB, m_pointC, m_pointD);

            return m_curve;
        }
    }

    private const int DEBUG_DETAIL_LEVEL = 16;

    [SerializeField] private BezierPoint m_pointA, m_pointB, m_pointC, m_pointD;

    [SerializeField] private DragonPathSegment m_next;
    public DragonPathSegment Next => m_next;

    [SerializeField] GameObject m_segmentPrefab;

#if UNITY_EDITOR
    [InspectorButton("CreateNextSegment", ButtonWidth = 128)] public bool addSegment;

    private void CreateNextSegment()
    {
        if (m_next != null)
            return;

        if (m_segmentPrefab == null)
        { 
            Debug.LogError("Cannot create segment as prefab is null");
            return;
        }

        GameObject nextObj = PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(m_segmentPrefab)) as GameObject;

        if (nextObj == null)
            Debug.LogError("fug");

        DragonPathSegment next = nextObj.GetComponent<DragonPathSegment>();
        next.transform.parent = transform.parent;
        m_next = next;

        Vector2 dir = (m_pointD.Point - m_pointC.Point).normalized;

        m_next.m_pointA.transform.position = m_pointD.transform.position;
        m_next.m_pointB.transform.position = (Vector2)m_next.m_pointA.transform.position + dir * 1;
        m_next.m_pointC.transform.position = (Vector2)m_next.m_pointA.transform.position + dir * 2;
        m_next.m_pointD.transform.position = (Vector2)m_next.m_pointA.transform.position + dir * 3;

        Selection.activeGameObject = m_next.gameObject;
    }
#endif

    private void Awake()
    {
        m_curve = new BezierCurve(m_pointA, m_pointB, m_pointC, m_pointD);
    }

    private void OnValidate()
    {
        if (!gameObject.scene.IsValid())
            return;

        if (m_next == null)
        {
            Debug.LogError($"Segment {gameObject} has no next!", gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        m_curve = new BezierCurve(m_pointA, m_pointB, m_pointC, m_pointD);

        Color c;

        if (Selection.Contains(gameObject))
            c = Color.blue;
        else
            c = Color.red;

        for (int i = 1; i <= DEBUG_DETAIL_LEVEL; ++i)
        {
            float p1 = (float)(i - 1) / (float)DEBUG_DETAIL_LEVEL;
            float p2 = (float)i / (float)DEBUG_DETAIL_LEVEL;

            Debug2.DrawArrow(m_curve.Point(p1), m_curve.Point(p2), c, 0.1f);
        }

        if (Selection.Contains(gameObject) && m_next != null)
        {
            Debug2.DrawArrow(m_curve.Point(0.5f), m_next.m_curve.Point(0.5f), Color.green);
        }
    }
}
