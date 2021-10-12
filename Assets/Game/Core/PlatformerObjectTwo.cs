using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Reccy.DebugExtensions;

public class PlatformerObjectTwo : MonoBehaviour
{
    [SerializeField] private float m_gravity = 9.81f;

    public bool IsGrounded => false; // todo

    private Rigidbody2D m_rb;
    [SerializeField] private BoxCollider2D m_obstacleCollider;

    private Bounds AABB => m_obstacleCollider.bounds;

    private Vector2 m_movement = Vector2.zero;
    private Vector2 m_inputMovement = Vector2.zero;

    [SerializeField] private Tilemap m_obstaclesTilemap;

    private bool IsMovingLeft => m_movement.x < 0;
    private bool IsMovingRight => m_movement.x > 0;
    private bool IsMovingUp => m_movement.y > 0;
    private bool IsMovingDown => m_movement.y < 0;

    private Vector3Int CurrentGridPosition => m_obstaclesTilemap.WorldToCell(transform.position);
    private Vector3Int GridPosition(Vector3 worldPos) => m_obstaclesTilemap.WorldToCell(worldPos);

    private float SpeedX => Mathf.Abs(m_movement.x);
    private float SpeedY => Mathf.Abs(m_movement.y);

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Apply Gravity
        m_movement += Vector2.down * m_gravity * Time.fixedDeltaTime;

        // Apply Input
        m_movement += m_inputMovement;

        StepX();
        StepY();

        Debug2.DrawCross(AABB.center, Color.green);
    }

    private void StepX()
    {
        int minMaxX = GetForwardFacingEdgeX();

        int minY = GridPosition(AABB.min).y;
        int maxY = GridPosition(AABB.max).y;

        BoundsInt collisionBounds = new BoundsInt(minMaxX, minY, 0, 1, maxY - minY, 0);

        Debug2.DrawBounds(collisionBounds);

        float closestDistance = int.MaxValue;

        for (int xCoord = m_obstaclesTilemap.cellBounds.xMin; xCoord < m_obstaclesTilemap.cellBounds.xMax; ++xCoord)
        {
            for (int yCoord = minY; yCoord <= maxY; ++yCoord)
            {
                Vector3Int coord = new Vector3Int(xCoord, yCoord, 0);

                var tile = m_obstaclesTilemap.GetTile(coord);

                if (tile != null)
                {
                    Debug2.DrawArrow(AABB.center, new Vector3(xCoord + 0.5f, yCoord + 0.5f, 0), Color.blue);

                    float dist = Vector3Int.Distance(coord, CurrentGridPosition);
                    
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                    }
                }
            }
        }

        float totalMovement = Mathf.Min(closestDistance, m_movement.x);

        m_rb.MovePosition(new Vector2(m_rb.position.x + totalMovement, m_rb.position.y));
    }

    private int GetForwardFacingEdgeX()
    {
        if (IsMovingRight)
        {
            return GridPosition(AABB.max).x;
        }
        else
        {
            return GridPosition(AABB.min).x;
        }
    }

    private void StepY()
    {
        int y = GetForwardFacingEdgeY();

        float minX = AABB.min.x;
        float maxX = AABB.max.x;

        // do scan
    }

    private int GetForwardFacingEdgeY()
    {
        if (IsMovingUp)
        {
            return GridPosition(AABB.max).y;
        }
        else
        {
            return GridPosition(AABB.min).y;
        }
    }

    public void Move(Vector2 vec)
    {
        m_inputMovement = vec;
    }
}
