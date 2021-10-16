using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private PlayerCharacter m_player;

    [SerializeField] private Vector2Int m_offset;

    private const float ROOM_WIDTH = 32;
    private const float ROOM_HEIGHT = 30;

    private Vector2 m_playerOffset = new Vector2(16, 3);

    private bool m_init = false;

    private void Start()
    {
        m_player = FindObjectOfType<PlayerCharacter>();
    }

    private void Update()
    {
        if (!m_init)
        {
            m_init = true;
            transform.position = GetTargetPosition();
            return;
        }

        transform.position = Vector3.Lerp(transform.position, GetTargetPosition(), Time.deltaTime * 10);
    }

    private Vector3 GetTargetPosition()
    {
        float x = (m_player.CurrentCell.x + m_playerOffset.x) / ROOM_WIDTH;
        float y = (m_player.CurrentCell.y + m_playerOffset.y) / ROOM_HEIGHT;
        Vector2Int playerRoom = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        return new Vector3(playerRoom.x * ROOM_WIDTH, playerRoom.y * ROOM_HEIGHT + 12, -10);
    }
}
