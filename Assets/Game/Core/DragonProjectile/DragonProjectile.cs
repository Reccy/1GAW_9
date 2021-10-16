using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DragonProjectile : MonoBehaviour
{
    private Rigidbody2D m_rb;
    private TilemapCollider m_obj;

    [SerializeField] private float m_hitForce = 10.0f;

    private void Awake()
    {
        m_obj = GetComponent<TilemapCollider>();
        m_rb = GetComponent<Rigidbody2D>();
        
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject, collision.gameObject);
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
        {
            HandleAttack((collision.transform.position - transform.position).normalized);
        }
    }

    private void OnTilemapCollided()
    {
        Destroy(gameObject);
    }

    private void HandleAttack(Vector2 attackDir)
    {
        m_rb.velocity = -attackDir * m_hitForce * Time.fixedDeltaTime;
    }
}
