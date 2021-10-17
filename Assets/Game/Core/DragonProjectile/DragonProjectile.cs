using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DragonProjectile : MonoBehaviour
{
    private Rigidbody2D m_rb;
    private TilemapCollider m_obj;
    private SpriteRenderer m_spriteRenderer;
    private Dragon m_dragon;
    private GameObject m_target;

    [SerializeField] private float m_hitForce = 10.0f;
    [SerializeField] private Color m_hitColor = Color.red;
    [SerializeField] private float m_curveballRate = 10.0f;

    private bool m_hitByPlayer = false;
    public bool HitByPlayer => m_hitByPlayer;

    private void Awake()
    {
        m_obj = GetComponent<TilemapCollider>();
        m_rb = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_dragon = FindObjectOfType<Dragon>();

        m_obj.SetTilemap(GameObject.FindGameObjectWithTag("ColliderTilemap").GetComponent<Tilemap>());
    }

    private void OnEnable()
    {
        m_obj.OnTilemapCollided += OnTilemapCollided;
    }

    private void OnDisable()
    {
        m_obj.OnTilemapCollided -= OnTilemapCollided;
    }

    private void FixedUpdate()
    {
        if (!HitByPlayer)
            return;

        if (m_target == null)
            return;

        float magntitude = m_hitForce * Time.fixedDeltaTime;
        Vector2 dir = m_rb.velocity.normalized;

        m_rb.velocity = Vector2.Lerp(dir * magntitude, (m_target.transform.position - transform.position).normalized * magntitude, Time.fixedDeltaTime * m_curveballRate);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
        {
            if (collision.CompareTag("Player"))
            {
                HandleAttack((collision.transform.position - transform.position).normalized);
            }

            if (collision.CompareTag("Dragon") && m_hitByPlayer)
            {
                collision.GetComponentInParent<Dragon>().HitByProjectile(transform.position);
                DestroyThis();
            }
        }
    }

    private void OnTilemapCollided()
    {
        DestroyThis();
    }

    private void HandleAttack(Vector2 attackDir)
    {
        // Prevents player from hitting the projectile twice
        if (m_hitByPlayer)
            return;

        m_spriteRenderer.color = m_hitColor;
        m_hitByPlayer = true;
        m_rb.velocity = -attackDir * m_hitForce * Time.fixedDeltaTime;
        m_target = m_dragon.GetClosestSegment(transform.position);
    }

    private void DestroyThis()
    {
        // todo: polish

        Destroy(gameObject);
    }
}
