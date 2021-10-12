using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerCharacter : MonoBehaviour
{
    private ObjThree m_obj;
    private Player m_player;
    private const int PLAYER_ID = 0;

    [SerializeField] private float m_jumpImpulse = 0.5f;
    [SerializeField] private float m_horizontalSpeed = 0.2f;

    #region INPUT
    private bool m_inputJump = false;
    #endregion

    private void Awake()
    {
        m_player = ReInput.players.GetPlayer(PLAYER_ID);
        m_obj = GetComponent<ObjThree>();
    }

    private void Update()
    {
        m_inputJump = m_player.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        Vector2 move = new Vector2(m_player.GetAxis("MoveHorizontal") * m_horizontalSpeed, 0);

        if (m_inputJump)
        {
            if (m_obj.IsGrounded)
            {
                move.y = m_jumpImpulse;
            }

            m_inputJump = false;
        }

        m_obj.Move(move);
    }
}
