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
    [SerializeField] private DragonPathSegment m_currentSegment;
    [SerializeField] private Transform[] m_segments;
    [SerializeField] private float m_distanceBetweenSegments = 0.25f;

    [Header("Projectiles")]
    [SerializeField] private GameObject m_projectile;
    [SerializeField] private float m_shootVelocity = 2.0f;
    [SerializeField] private float m_chargeTime = 0.5f;
    [SerializeField] private float m_cooldownTime = 0.25f;

    private float m_currentDistance;

    private PlayerCharacter m_playerCharacter;

    [SerializeField] private float m_speed = 5.0f;

    private BezierCurve Curve => m_currentSegment.Curve;

    private Coroutine m_shootCoroutine;

    private void OnValidate()
    {
        if (m_currentSegment != null)
        {
            UpdatePosition();
        }
    }

    public void HitByProjectile(Vector2 position)
    {
        Debug.Log("Oh FUCK TYOSADUN YA HIT ME !?!??!");
    }

    private void Awake()
    {
        m_playerCharacter = FindObjectOfType<PlayerCharacter>();
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

        Debug.DrawRay(ray.origin, playerPos - headPos, Color.red);

        if (hit.collider == null)
            return true;

        Debug2.DrawArrow(headPos, hit.point, Color.green);

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
