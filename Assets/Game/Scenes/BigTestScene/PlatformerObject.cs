using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Reccy.DebugExtensions;

public class PlatformerObject : MonoBehaviour
{
    private Vector2 m_velocity = Vector2.zero;
    private Vector2 m_inputVelocity = Vector2.zero;

    [SerializeField] private BoxCollider2D m_collisionBox;
    [SerializeField] private Tilemap m_obstacleTilemap;
    [SerializeField] private BoxCollider2D m_groundedCollisionBox;
    [SerializeField] private float m_xDampening = 1.0f;
    [SerializeField] private float m_yDampening = 1.0f;
    [SerializeField] private float m_gravity = 9.81f;

    private bool m_isGrounded = true;
    public bool IsGrounded => m_isGrounded;

    private Bounds AABB => m_collisionBox.bounds;
    private float LeftEdgeX => AABB.min.x;
    private float RightEdgeX => AABB.max.x;
    private float UpEdgeY => AABB.max.y;
    private float DownEdgeY => AABB.min.y;

    private bool IsMovingRight => m_velocity.x > 0;
    private bool IsMovingLeft => m_velocity.x < 0;
    private bool IsMovingUp => m_velocity.y > 0;
    private bool IsMovingDown => m_velocity.y < 0;

    private Vector3Int m_currentCellPosition;
    private Vector3Int CurrentCellPosition => m_currentCellPosition;

    public void Move(Vector2 o)
    {
        m_inputVelocity = o;
    }

    private void FixedUpdate()
    {
        m_currentCellPosition = m_obstacleTilemap.WorldToCell(transform.position);

        DampenVelocity();

        ApplyGravity();

        ApplyInput();

        ResolveCollisions();

        IsGroundedCheck();

        transform.position += new Vector3(m_velocity.x, m_velocity.y, 0);
    }

    private void DampenVelocity()
    {
        if (m_inputVelocity.x == 0)
        {
            m_velocity.x *= m_xDampening;
        }

        if (m_inputVelocity.y == 0)
        {
            m_velocity.y *= m_yDampening;
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

    private void ResolveCollisions()
    {
        Vector3Int xCell = CurrentCellPosition;
        Vector3Int yCell = CurrentCellPosition;

        Vector3Int topRightCell = CurrentCellPosition + Vector3Int.up + Vector3Int.right;
        Vector3Int topLeftCell = CurrentCellPosition + Vector3Int.up + Vector3Int.left;
        Vector3Int bottomLeftCell = CurrentCellPosition + Vector3Int.down + Vector3Int.left;
        Vector3Int bottomRightCell = CurrentCellPosition + Vector3Int.down + Vector3Int.right;

        if (IsMovingUp)
        {
            yCell += Vector3Int.up;
        }
        else
        {
            yCell += Vector3Int.down;
        }

        if (IsMovingRight)
        {
            xCell += Vector3Int.right;
        }
        else
        {
            xCell += Vector3Int.left;
        }

        Debug2.DrawArrow(m_obstacleTilemap.GetCellCenterWorld(CurrentCellPosition), m_obstacleTilemap.GetCellCenterWorld(xCell), Color.blue);
        Debug2.DrawArrow(m_obstacleTilemap.GetCellCenterWorld(CurrentCellPosition), m_obstacleTilemap.GetCellCenterWorld(yCell), Color.red);

        ResolveCellCollision(xCell);
        ResolveCellCollision(yCell);

        if (IsMovingUp || IsMovingLeft)
        {
            Debug2.DrawArrow(m_obstacleTilemap.GetCellCenterWorld(CurrentCellPosition), m_obstacleTilemap.GetCellCenterWorld(topLeftCell), Color.green);
            ResolveCellCollision(topLeftCell);
        }

        if (IsMovingUp || IsMovingRight)
        {
            Debug2.DrawArrow(m_obstacleTilemap.GetCellCenterWorld(CurrentCellPosition), m_obstacleTilemap.GetCellCenterWorld(topRightCell), Color.green);
            ResolveCellCollision(topRightCell);
        }

        if (IsMovingDown || IsMovingRight)
        {
            Debug2.DrawArrow(m_obstacleTilemap.GetCellCenterWorld(CurrentCellPosition), m_obstacleTilemap.GetCellCenterWorld(bottomRightCell), Color.green);
            ResolveCellCollision(bottomRightCell);
        }

        if (IsMovingDown || IsMovingLeft)
        {
            Debug2.DrawArrow(m_obstacleTilemap.GetCellCenterWorld(CurrentCellPosition), m_obstacleTilemap.GetCellCenterWorld(bottomLeftCell), Color.green);
            ResolveCellCollision(bottomLeftCell);
        }
    }

    private void ResolveCellCollision(Vector3Int nextCellPos)
    {
        var otherTile = m_obstacleTilemap.GetTile(nextCellPos);

        float overlapCorrectionX = 0;
        float overlapCorrectionY = 0;
        float overlapX = 0;
        float overlapY = 0;
        float otherCenterX = m_obstacleTilemap.GetCellCenterWorld(nextCellPos).x;
        float otherCenterY = m_obstacleTilemap.GetCellCenterWorld(nextCellPos).y;

        var cellDir = GetCellDir(nextCellPos);

        if (otherTile != null)
        {
            if (cellDir.x == -1) // Cell is to our left
            {
                var otherRightEdge = otherCenterX + 0.5f;
                overlapX = Mathf.Max(otherRightEdge - (LeftEdgeX + m_velocity.x), 0);
                overlapCorrectionX = overlapX;
            }
            else if (cellDir.x == 1) // Cell is to our right
            {
                var otherLeftEdge = otherCenterX - 0.5f;
                overlapX = Mathf.Max((RightEdgeX + m_velocity.x) - otherLeftEdge, 0);
                overlapCorrectionX = -overlapX;
            }
            else
            {
                overlapX = 1;
                overlapCorrectionX = 1;
            }

            if (cellDir.y == 1) // Cell is above
            {
                var otherDownEdge = otherCenterY - 0.5f;
                overlapY = Mathf.Max((UpEdgeY + m_velocity.y) - otherDownEdge, 0);
                overlapCorrectionY = -overlapY;
            }
            else if (cellDir.y == -1) // Cell is below
            {
                var otherUpEdge = otherCenterY + 0.5f;
                overlapY = Mathf.Max(otherUpEdge - (DownEdgeY + m_velocity.y), 0);
                overlapCorrectionY = overlapY;
            }
            else
            {
                overlapY = 1;
                overlapCorrectionY = 1;
            }
        }

        // Correct cell on the axis of least displacement
        if (overlapX > overlapY)
        {
             m_velocity = new Vector2(m_velocity.x, m_velocity.y + overlapCorrectionY);
        }
        else
        {
            m_velocity = new Vector2(m_velocity.x + overlapCorrectionX, m_velocity.y);
        }
    }

    private Vector3Int GetCellDir(Vector3Int nextCellPos)
    {
        return nextCellPos - CurrentCellPosition;
    }

    private void IsGroundedCheck()
    {
        Vector3Int downTilePos = CurrentCellPosition + Vector3Int.down;

        if (m_obstacleTilemap.GetTile(downTilePos) == null)
        {
            m_isGrounded = false;
            return;
        }

        if (DownEdgeY > (m_obstacleTilemap.GetCellCenterWorld(downTilePos).y + 0.5f))
        {
            m_isGrounded = false;
            return;
        }

        m_isGrounded = true;
    }
}
