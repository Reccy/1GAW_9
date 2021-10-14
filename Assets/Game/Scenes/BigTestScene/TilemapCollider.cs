using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Reccy.DebugExtensions;

// Resolves collisions against an obstacle tilemap
public class TilemapCollider : MonoBehaviour
{
    private Rigidbody2D m_rb;

    [SerializeField] private BoxCollider2D m_collisionBox;
    [SerializeField] private Tilemap m_obstacleTilemap;
    [SerializeField] private BoxCollider2D m_groundedCollisionBox;

    private const float HALF_TILE_SIZE = 0.5f;

    private bool m_isGrounded = true;
    public bool IsGrounded => m_isGrounded;

    private Bounds AABB => m_collisionBox.bounds;
    private float LeftEdgeX => AABB.min.x;
    private float RightEdgeX => AABB.max.x;
    private float UpEdgeY => AABB.max.y;
    private float DownEdgeY => AABB.min.y;

    private bool IsMovingRight => m_rb.velocity.x > 0;
    private bool IsMovingLeft => m_rb.velocity.x < 0;
    private bool IsMovingUp => m_rb.velocity.y > 0;
    private bool IsMovingDown => m_rb.velocity.y < 0;

    private Vector3Int m_currentCellPosition;
    private Vector3Int CurrentCellPosition => m_currentCellPosition;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        m_currentCellPosition = m_obstacleTilemap.WorldToCell(transform.position);

        ResolveCollisions();

        IsGroundedCheck();

        m_rb.MovePosition(m_rb.position + new Vector2(m_rb.velocity.x, m_rb.velocity.y));
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
                var otherRightEdge = otherCenterX + HALF_TILE_SIZE;
                overlapX = Mathf.Max(otherRightEdge - (LeftEdgeX + m_rb.velocity.x), 0);
                overlapCorrectionX = overlapX;
            }
            else if (cellDir.x == 1) // Cell is to our right
            {
                var otherLeftEdge = otherCenterX - HALF_TILE_SIZE;
                overlapX = Mathf.Max((RightEdgeX + m_rb.velocity.x) - otherLeftEdge, 0);
                overlapCorrectionX = -overlapX;
            }
            else
            {
                overlapX = 1;
                overlapCorrectionX = 1;
            }

            if (cellDir.y == 1) // Cell is above
            {
                var otherDownEdge = otherCenterY - HALF_TILE_SIZE;
                overlapY = Mathf.Max((UpEdgeY + m_rb.velocity.y) - otherDownEdge, 0);
                overlapCorrectionY = -overlapY;
            }
            else if (cellDir.y == -1) // Cell is below
            {
                var otherUpEdge = otherCenterY + HALF_TILE_SIZE;
                overlapY = Mathf.Max(otherUpEdge - (DownEdgeY + m_rb.velocity.y), 0);
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
            m_rb.velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.y + overlapCorrectionY);
        }
        else
        {
            m_rb.velocity = new Vector2(m_rb.velocity.x + overlapCorrectionX, m_rb.velocity.y);
        }
    }

    private Vector3Int GetCellDir(Vector3Int nextCellPos)
    {
        return nextCellPos - CurrentCellPosition;
    }

    private void IsGroundedCheck()
    {
        Vector3Int downLeftTilePos = CurrentCellPosition + Vector3Int.down + Vector3Int.left;
        Vector3Int downTilePos = CurrentCellPosition + Vector3Int.down;
        Vector3Int downRightTilePos = CurrentCellPosition + Vector3Int.down + Vector3Int.right;

        if (m_obstacleTilemap.GetTile(downTilePos) != null)
        {
            if (DownEdgeY <= (m_obstacleTilemap.GetCellCenterWorld(downTilePos).y + HALF_TILE_SIZE))
            {
                m_isGrounded = true;
                return;
            }
        }

        if (m_obstacleTilemap.GetTile(downLeftTilePos) != null)
        {
            if (LeftEdgeX < (m_obstacleTilemap.GetCellCenterWorld(downLeftTilePos).x + HALF_TILE_SIZE))
            {
                if (DownEdgeY <= (m_obstacleTilemap.GetCellCenterWorld(downLeftTilePos).y + HALF_TILE_SIZE))
                {
                    m_isGrounded = true;
                    return;
                }
            }
        }

        if (m_obstacleTilemap.GetTile(downRightTilePos) != null)
        {
            if (RightEdgeX > (m_obstacleTilemap.GetCellCenterWorld(downRightTilePos).x - HALF_TILE_SIZE))
            {
                if (DownEdgeY <= (m_obstacleTilemap.GetCellCenterWorld(downRightTilePos).y + HALF_TILE_SIZE))
                {
                    m_isGrounded = true;
                    return;
                }
            }
        }

        m_isGrounded = false;
    }
}
