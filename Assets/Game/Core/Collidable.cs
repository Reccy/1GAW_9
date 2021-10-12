using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collidable : MonoBehaviour
{
    public enum CollidableType { STATIC, DYNAMIC }

    [SerializeField] private CollidableType m_collidableType;
    public CollidableType Type => m_collidableType;

    public bool IsStatic => Type == CollidableType.STATIC;
    public bool IsDynamic => Type == CollidableType.DYNAMIC;

    public float Width => m_collider.size.x;
    public float Height => m_collider.size.y;

    private Rigidbody2D m_rigidbody;
    [SerializeField] private BoxCollider2D m_collider;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }
}
