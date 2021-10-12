using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Reccy.DebugExtensions;

public class ObjThree : MonoBehaviour
{
    private Vector2 m_velocity = Vector2.zero;
    private Vector2 m_inputVelocity = Vector2.zero;

    [SerializeField] private BoxCollider2D m_collisionBox;
    [SerializeField] private Tilemap m_obstacleTilemap;
    [SerializeField] private BoxCollider2D m_groundedCollisionBox;
    [SerializeField] private float m_xDampening = 1.0f;
    [SerializeField] private float m_gravity = 9.81f;

    private bool m_isGrounded = true; // todo
    public bool IsGrounded => m_isGrounded;

    private Bounds AABB => m_collisionBox.bounds;
    private float LeftEdgeX => AABB.min.x;
    private float RightEdgeX => AABB.max.x;
    private float UpEdgeY => AABB.max.y;
    private float DownEdgeY => AABB.min.y;

    private bool m_movingRight = false;
    private bool m_movingLeft => !m_movingRight;
    private bool m_movingUp = false;
    private bool m_movingDown => !m_movingUp;

    private Vector3Int CurrentCellPosition => m_obstacleTilemap.WorldToCell(transform.position);

    public void Move(Vector2 o)
    {
        m_inputVelocity = o;
    }

    private void FixedUpdate()
    {
        UpdateMovingFlags();

        DampenVelocity();

        ApplyGravity();

        ApplyInput();

        StepX();
        StepY();
    }

    private void UpdateMovingFlags()
    {
        if (m_velocity.x > 0)
        {
            m_movingRight = true;
        }
        else if (m_velocity.x < 0)
        {
            m_movingRight = false;
        }

        if (m_velocity.y > 0)
        {
            m_movingUp = true;
        }
        else if (m_velocity.y < 0)
        {
            m_movingUp = false;
        }
    }

    private void DampenVelocity()
    {
        if (m_inputVelocity.x == 0)
        {
            m_velocity.x *= m_xDampening * Time.fixedDeltaTime;
        }
    }

    private void ApplyGravity()
    {
        m_velocity += Vector2.down * m_gravity * Time.fixedDeltaTime;
    }

    private void ApplyInput()
    {
        m_velocity += m_inputVelocity * Time.fixedDeltaTime;
    }

    private void StepX()
    {
        Vector3Int nextCellPos;

        if (m_movingRight)
        {
            nextCellPos = CurrentCellPosition + Vector3Int.right;
        }
        else
        {
            nextCellPos = CurrentCellPosition + Vector3Int.left;
        }

        var otherTile = m_obstacleTilemap.GetTile(nextCellPos);

        float overlapCorrection = 0;
        float otherCenterX = m_obstacleTilemap.GetCellCenterWorld(nextCellPos).x;

        if (otherTile != null)
        {
            if (m_movingLeft)
            {
                var otherRightEdge = otherCenterX + 0.5f;
                overlapCorrection = Mathf.Max(otherRightEdge - (LeftEdgeX + m_velocity.x), 0);
            }
            else
            {
                var otherLeftEdge = otherCenterX - 0.5f;
                overlapCorrection = -Mathf.Max((RightEdgeX + m_velocity.x) - otherLeftEdge, 0);
            }
        }

        m_velocity = new Vector2(m_velocity.x + overlapCorrection, m_velocity.y);

        transform.position += new Vector3(m_velocity.x, 0, 0);
    }

    private void StepY()
    {
        Vector3Int nextCellPos;

        if (m_movingUp)
        {
            nextCellPos = CurrentCellPosition + Vector3Int.up;
        }
        else
        {
            nextCellPos = CurrentCellPosition + Vector3Int.down;
        }

        var otherTile = m_obstacleTilemap.GetTile(nextCellPos);

        float overlapCorrection = 0;
        float otherCenterY = m_obstacleTilemap.GetCellCenterWorld(nextCellPos).y;

        if (otherTile != null)
        {
            if (m_movingUp)
            {
                var otherDownEdge = otherCenterY - 0.5f;
                overlapCorrection = -Mathf.Max((UpEdgeY + m_velocity.y) - otherDownEdge, 0);
            }
            else
            {
                var otherUpEdge = otherCenterY + 0.5f;
                overlapCorrection = Mathf.Max(otherUpEdge - (DownEdgeY + m_velocity.y), 0);
            }
        }

        m_velocity = new Vector2(m_velocity.x, m_velocity.y + overlapCorrection);

        transform.position += new Vector3(0, m_velocity.y, 0);
    }
}
