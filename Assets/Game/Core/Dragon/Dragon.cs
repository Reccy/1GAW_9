using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reccy.UnityBezierCurve;
using Reccy.DebugExtensions;

public class Dragon : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_t;
    [SerializeField] private GameObject m_head;
    [SerializeField] private GameObject m_tail;
    [SerializeField] private DragonPathSegment m_currentSegment;
    [SerializeField] private Transform[] m_segments;
    [SerializeField] private float m_distanceBetweenSegments = 0.25f;

    [Header("Projectiles")]
    [SerializeField] private GameObject m_projectile;
    [SerializeField] private float m_shootVelocity = 2.0f;
    [SerializeField] private float m_chargeTime = 0.5f;
    [SerializeField] private float m_cooldownTime = 0.25f;

    private EndSequence m_endSequence;

    private float m_currentDistance;

    private PlayerCharacter m_playerCharacter;

    [SerializeField] private float m_speed = 5.0f;

    private BezierCurve Curve => m_currentSegment.Curve;

    private Coroutine m_shootCoroutine;

    private int m_health;

    private void OnValidate()
    {
        if (m_currentSegment != null)
        {
            UpdatePosition();
        }
    }

    public GameObject GetClosestSegment(Vector2 position)
    {
        GameObject hitSegment = null;
        float dist = float.MaxValue;

        foreach (var segment in m_segments)
        {
            float newDist = Vector2.Distance(segment.transform.position, position);

            if (!segment.gameObject.activeInHierarchy)
                continue;

            if (newDist < dist)
            {
                hitSegment = segment.gameObject;
                dist = newDist;
            }
        }

        if (hitSegment == m_head)
        {
            for (int i = 1; i < m_segments.Length; ++i)
            {
                GameObject seg = m_segments[i].gameObject;

                if (seg.activeInHierarchy && seg != m_tail)
                {
                    return seg;
                }
            }
        }
        else if (hitSegment == m_tail)
        {
            for (int i = m_segments.Length - 1; i > 0; --i)
            {
                GameObject seg = m_segments[i].gameObject;

                if (seg.activeInHierarchy && seg != m_head)
                {
                    return seg;
                }
            }
        }

        return hitSegment;
    }

    public void HitByProjectile(Vector2 position)
    {
        GameObject hitSegment = GetClosestSegment(position);

        if (m_health > 0 && hitSegment != m_head && hitSegment != m_tail)
        {
            m_health--;
            hitSegment.SetActive(false);
        }
        else
        {
            m_health--;
            m_head.SetActive(false);
            m_tail.SetActive(false);
            m_endSequence.StartEnding();
        }
    }

    private void Awake()
    {
        m_playerCharacter = FindObjectOfType<PlayerCharacter>();
        m_endSequence = FindObjectOfType<EndSequence>();
        m_health = m_segments.Length - 2; // Subtract 2 for head and tail
    }

    private void Start()
    {
        m_currentDistance = Curve.Distance(m_t);
    }

    private void FixedUpdate()
    {
        // Dead
        if (m_health < 0)
            return;

        // Alive
        m_currentDistance += m_speed * Time.fixedDeltaTime;

        if (m_currentDistance > Curve.Length)
        {
            m_currentDistance = m_currentDistance - Curve.Length;
            m_currentSegment = m_currentSegment.Next;
        }

        if (CheckPlayerInSight())
        {
            if (m_shootCoroutine == null)
                m_shootCoroutine = StartCoroutine(ShootCoroutine());
        }
        else
        {
            if (m_shootCoroutine != null)
            {
                StopCoroutine(m_shootCoroutine);
                m_shootCoroutine = null;
            }
        }

        UpdatePosition();
    }

    private bool CheckPlayerInSight()
    {
        Vector3 headPos = m_head.transform.position;
        Vector3 playerPos = m_playerCharacter.transform.position;

        Ray ray = new Ray(headPos, (playerPos - headPos).normalized);

        var hit = Physics2D.Raycast(ray.origin, ray.direction, 100.0f, LayerMask.GetMask("Level"));

        if (hit.collider == null)
            return true;

        float distanceToPlayer = Vector3.Distance(headPos, playerPos);
        float distanceToHit = hit.distance;

        return distanceToHit > distanceToPlayer;
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(m_chargeTime);

        GameObject projectile = Instantiate(m_projectile, m_head.transform.position, Quaternion.identity);

        projectile.GetComponentInChildren<Rigidbody2D>().velocity = (m_playerCharacter.transform.position - m_head.transform.position).normalized * m_shootVelocity * Time.fixedDeltaTime;

        yield return new WaitForSeconds(m_cooldownTime);

        m_shootCoroutine = null;
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
