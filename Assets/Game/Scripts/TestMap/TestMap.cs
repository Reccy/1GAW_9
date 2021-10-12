using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMap : MonoBehaviour
{
    [SerializeField] private Sprite m_spritePrototype;

    private Color[] m_colors;

    private float XOFFSET = 15.5f;
    private float YOFFSET = 14.5f;

    private void Awake()
    {
        m_colors = new Color[3];
        m_colors[0] = Color.red;
        m_colors[1] = Color.green;
        m_colors[2] = Color.blue;

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 30; y++)
            {
                Color c = m_colors[(x + y) % 3];

                CreateSprite(x, y, c);
            }
        }
    }

    private void CreateSprite(int x, int y, Color c)
    {
        GameObject obj = new GameObject();
        obj.name = $"Sprite({x}, {y})";
        obj.transform.parent = transform;

        SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
        rend.sprite = m_spritePrototype;
        rend.color = c;

        obj.transform.position = new Vector3(x - XOFFSET, y - YOFFSET, 0);
    }
}
