using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerCharacter : MonoBehaviour
{
    private TilemapCollider m_obj;
    private Rigidbody2D m_rb;
    private Player m_player;
    private const int PLAYER_ID = 0;

    [SerializeField] private float m_jumpImpulse = 0.5f;
    [SerializeField] private float m_horizontalSpeed = 0.2f;
    [SerializeField] private float m_gravity = 1.0f;
    [SerializeField] private float m_jumpGravity = 0.5f;
    [SerializeField] private float m_coyoteTimeSeconds = 0.01f;

    private float m_timeAirborne = 0.0f;

    private Vector2 velocity
    {
        get => m_rb.velocity;
        set => m_rb.velocity = value;
    }

    #region INPUT
    private bool m_inputJump = false;
    #endregion

    private enum JumpState { NOT_STARTED, JUMPING, FALLING }
    private JumpState m_jumpState = JumpState.NOT_STARTED;

    private void Awake()
    {
        m_player = ReInput.players.GetPlayer(PLAYER_ID);
        m_obj = GetComponent<TilemapCollider>();
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (m_player.GetButtonDown("Jump"))
            m_inputJump = true;
    }

    private void FixedUpdate()
    {
        Vector2 move = new Vector2(m_player.GetAxis("MoveHorizontal") * m_horizontalSpeed, 0);

        HandleJumping();

        velocity = new Vector2(move.x * Time.fixedDeltaTime, velocity.y);
    }

    private void HandleJumping()
    {
        if (m_inputJump)
        {
            if (m_timeAirborne <= m_coyoteTimeSeconds && m_jumpState == JumpState.NOT_STARTED)
            {
                Debug.Log(m_timeAirborne);

                velocity = new Vector2(velocity.x, m_jumpImpulse * Time.fixedDeltaTime);
                m_jumpState = JumpState.JUMPING;
            }

            m_inputJump = false;
        }

        // Jumping -> Falling when get to jump apex
        if (m_jumpState == JumpState.JUMPING)
        {
            if (velocity.y < 0)
                m_jumpState = JumpState.FALLING;
        }

        // Jumping -> Not started when grounded
        if (m_obj.IsGrounded && m_jumpState != JumpState.JUMPING)
        {
            m_timeAirborne = 0;
            m_jumpState = JumpState.NOT_STARTED;
        }
        else
        {
            // Airborne logic

            m_timeAirborne += Time.fixedDeltaTime;

            if (m_jumpState == JumpState.JUMPING)
                velocity += Vector2.down * m_jumpGravity * Time.fixedDeltaTime;
            else
                velocity += Vector2.down * m_gravity * Time.fixedDeltaTime;
        }
    }
}
