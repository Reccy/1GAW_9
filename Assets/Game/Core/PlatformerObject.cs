using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reccy.DebugExtensions;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerObject : MonoBehaviour
{
    private struct CollisionInfo
    {
        public Collidable collidable;
        public RaycastHit2D hit;
        public float length;
    }

    [SerializeField] private BoxCollider2D m_groundedCollider;
    [SerializeField] private BoxCollider2D m_movementCollider;
    [SerializeField] private float m_maxXSpeed = 5.0f;
    [SerializeField] private float m_maxYSpeed = 5.0f;
    [SerializeField] private float m_minXSpeed = 0.0001f;
    [SerializeField] private float m_minYSpeed = 0.0001f;
    [SerializeField] private float m_horizontalDampening = 1.0f;

    private const int CAST_ARRAY_SIZE = 8;

    private Rigidbody2D m_rb;

    private Vector2 m_velocity;
    private Vector2 m_inputMovement;

    private Vector2 NextPos => transform.position + (Vector3)m_velocity;
    
    private float Width => m_movementCollider.size.x;
    private float Height => m_movementCollider.size.y;

    private bool m_isGrounded = false;
    public bool IsGrounded => m_isGrounded;

    public void Move(Vector2 vel)
    {
        m_inputMovement = vel;
    }

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        m_velocity += m_inputMovement;
        m_velocity += Vector2.down * m_rb.gravityScale * Time.fixedDeltaTime;

        ClampSpeed();

        GroundedCheck();

        List<CollisionInfo> collidables = DetectCollisions();
        ResolveCollisions(collidables);

        transform.position = NextPos;
    }

    private void ClampSpeed()
    {
        m_velocity.x = Mathf.Clamp(m_velocity.x, -m_maxXSpeed, m_maxXSpeed);
        m_velocity.y = Mathf.Clamp(m_velocity.y, -m_maxYSpeed, m_maxYSpeed);
    }

    private void GroundedCheck()
    {
        RaycastHit2D[] hits = new RaycastHit2D[CAST_ARRAY_SIZE];
        int size = m_groundedCollider.Cast(m_velocity, hits, m_velocity.magnitude);

        for (int i = 0; i < size; ++i)
        {
            RaycastHit2D hit = hits[i];

            Collidable collidable = hit.collider.GetComponentInChildren<Collidable>();

            if (collidable != null && collidable.IsStatic)
            {
                m_isGrounded = true;
                return;
            }
        }

        m_isGrounded = false;
    }

    private List<CollisionInfo> DetectCollisions()
    {
        RaycastHit2D[] hits = new RaycastHit2D[CAST_ARRAY_SIZE];
        int size = m_movementCollider.Cast(m_velocity, hits, m_velocity.magnitude);

        List<CollisionInfo> result = new List<CollisionInfo>();

        for (int i = 0; i < size; ++i)
        {
            RaycastHit2D hit = hits[i];

            Collidable collidable = hit.collider.GetComponentInChildren<Collidable>();
            
            if (collidable != null && collidable.IsStatic)
            {
                Vector3 proj = FindNearestPointOnLine(transform.position, m_velocity.normalized, collidable.transform.position);
                float length = (collidable.transform.position - proj).sqrMagnitude;
        
                Debug2.DrawArrow(transform.position, transform.position + (Vector3)m_velocity.normalized * 10.0f, Color.blue);

                // Object is behind us
                if (length < 0)
                    continue;

                CollisionInfo info = new CollisionInfo();
                info.collidable = collidable;
                info.hit = hit;
                info.length = length;

                result.Add(info);
            }
        }

        result.Sort(Comparer<CollisionInfo>.Create((a, b) => {
            if (a.length > b.length)
                return -1;

            if (a.length < b.length)
                return 1;

            return 0;
        }));

        Color[] cs = new Color[3];
        cs[0] = Color.red;
        cs[1] = Color.green;
        cs[2] = Color.blue;

        for (int i = 0; i < result.Count; ++i)
        {
            CollisionInfo r = result[i];

            Vector3 proj = FindNearestPointOnLine(transform.position, m_velocity.normalized, r.collidable.transform.position);
            Debug2.DrawArrow(r.collidable.transform.position, proj, cs[i % 3]);
        }

        return result;
    }

    private void ResolveCollisions(List<CollisionInfo> collidables)
    {
        foreach (var record in collidables)
        {
            ResolveCollision(record.collidable, record.hit);
        }
    }

    private void ResolveCollision(Collidable collidable, RaycastHit2D hit)
    {
        if (Mathf.Abs(m_velocity.x) < m_minXSpeed)
        {
            m_velocity.x = 0;
        }

        if (Mathf.Abs(m_velocity.y) < m_minYSpeed)
        {
            m_velocity.y = 0;
        }

        Vector2 normal = GetCardinalNormal(hit);

        if (normal == Vector2.right)
        {
            float tCenter = collidable.transform.position.x;
            float mCenter = transform.position.x + m_velocity.x;

            float tOverlap = tCenter + (collidable.Width / 2);
            float mOverlap = mCenter - (Width / 2);

            float overlap = tOverlap - mOverlap;

            if (overlap <= 0)
                return;

            m_velocity.x += overlap;
        }
        else if (normal == Vector2.left)
        {
            float tCenter = collidable.transform.position.x;
            float mCenter = transform.position.x + m_velocity.x;

            float tOverlap = tCenter - (collidable.Width / 2);
            float mOverlap = mCenter + (Width / 2);

            float overlap = mOverlap - tOverlap;

            if (overlap <= 0)
                return;

            m_velocity.x -= overlap;
        }
        else if (normal == Vector2.up)
        {
            float tCenter = collidable.transform.position.y;
            float mCenter = transform.position.y + m_velocity.y;

            float tOverlap = tCenter + (collidable.Height / 2);
            float mOverlap = mCenter - (Height / 2);

            float overlap = tOverlap - mOverlap;

            if (overlap <= 0)
                return;

            m_velocity.y += overlap;
        }
        else if (normal == Vector2.down)
        {
            float tCenter = collidable.transform.position.y;
            float mCenter = transform.position.y + m_velocity.y;

            float tOverlap = tCenter - (collidable.Height / 2);
            float mOverlap = mCenter + (Height / 2);

            float overlap = mOverlap - tOverlap;

            if (overlap <= 0)
                return;

            m_velocity.y -= overlap;
        }
    }

    // Adapted from: https://github.com/Reccy/1GAW_1/blob/master/Assets/GameObjects/Player/PlayerController.cs
    private Vector2 GetCardinalNormal(RaycastHit2D hit)
    {
        Vector2 normal = hit.normal;

        if (normal != Vector2.up && normal != Vector2.down && normal != Vector2.left && normal != Vector2.right)
        {
            float upCloseness = Vector2.Dot(normal, Vector2.up);
            float leftCloseness = Vector2.Dot(normal, Vector2.left);
            float rightCloseness = Vector2.Dot(normal, Vector2.right);
            float downCloseness = Vector2.Dot(normal, Vector2.down);

            float upDiff = 1 - upCloseness;
            float leftDiff = 1 - leftCloseness;
            float rightDiff = 1 - rightCloseness;
            float downDiff = 1 - downCloseness;

            float select = Mathf.Min(upDiff, leftDiff, rightDiff, downDiff);

            if (select == upDiff)
            {
                normal = Vector2.up;
            }

            if (select == leftDiff)
            {
                normal = Vector2.left;
            }

            if (select == rightDiff)
            {
                normal = Vector2.right;
            }

            if (select == downDiff)
            {
                normal = Vector2.down;
            }
        }

        Debug2.DrawArrow(hit.centroid, hit.centroid + normal, Color.red);

        return normal;
    }

    public Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 direction, Vector2 point)
    {
        direction.Normalize();
        Vector2 lhs = point - origin;

        float dotP = Vector2.Dot(lhs, direction);
        return origin + direction * dotP;
    }
}
