using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DragonProjectile : MonoBehaviour
{
    private Rigidbody2D m_rb;
    private TilemapCollider m_obj;

    private void Awake()
    {
        m_obj = GetComponent<TilemapCollider>();
        m_rb = GetComponent<Rigidbody2D>();
        
        m_obj.SetTilemap(GameObject.FindGameObjectWithTag("ColliderTilemap").GetComponent<Tilemap>());

        m_rb.velocity = new Vector2(1.0f * Time.fixedDeltaTime, 1.2f * Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        m_obj.OnTilemapCollided += OnTilemapCollided;
    }

    private void OnDisable()
    {
        m_obj.OnTilemapCollided -= OnTilemapCollided;
    }

    private void OnTilemapCollided()
    {
        Destroy(gameObject);
    }
}
