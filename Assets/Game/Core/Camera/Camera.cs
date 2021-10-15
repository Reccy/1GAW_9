using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    private PlayerCharacter m_player;

    [SerializeField] private Vector2Int m_offset;

    private const float ROOM_WIDTH = 32;
    private const float ROOM_HEIGHT = 30;

    Vector3 m_targetPosition;

    private Vector2 m_playerOffset = new Vector2(16, 3);

    private void Awake()
    {
        m_player = FindObjectOfType<PlayerCharacter>();
    }

    private void Update()
    {
        float x = (m_player.CurrentCell.x + m_playerOffset.x) / ROOM_WIDTH;
        float y = (m_player.CurrentCell.y + m_playerOffset.y) / ROOM_HEIGHT;

        Vector2Int playerRoom = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));

        m_targetPosition = new Vector3(playerRoom.x * ROOM_WIDTH, playerRoom.y * ROOM_HEIGHT + 12, -10);

        transform.position = Vector3.Lerp(transform.position, m_targetPosition, Time.deltaTime * 10);
    }
}
