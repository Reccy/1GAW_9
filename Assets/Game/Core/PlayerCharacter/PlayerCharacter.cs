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
    [SerializeField] private float m_accelerationRate = 1.0f;
    [SerializeField] private float m_maxHorizontalSpeed = 5.0f;
    [SerializeField] private float m_maxVerticalSpeed = 100.0f;
    [SerializeField] private float m_decelerationRate = 1.0f;
    [SerializeField] private float m_gravity = 1.0f;
    [SerializeField] private float m_jumpGravity = 0.5f;
    [SerializeField] private float m_coyoteTimeSeconds = 0.01f;
    [SerializeField] private float m_inputBufferSeconds = 0.1f;
    private PlayerCharacterSprite m_playerSprite;

    public Vector3Int CurrentCell => m_obj.CurrentCellPosition;

    private float m_timeAirborne = 0.0f;

    private Vector2 velocity
    {
        get => m_rb.velocity;
        set => m_rb.velocity = value;
    }

    #region INPUT
    private bool m_inputJumpDown = false;
    private float m_inputJumpDownBuffer = 0.0f;
    private bool m_inputJumpHeld = false;

    private const string JUMP = "Jump";
    private const string MOVE_HORIZONTAL = "MoveHorizontal";
    #endregion

    private enum JumpState { NOT_STARTED, JUMPING, FALLING }
    private JumpState m_jumpState = JumpState.NOT_STARTED;

    private void Awake()
    {
        m_player = ReInput.players.GetPlayer(PLAYER_ID);
        m_obj = GetComponent<TilemapCollider>();
        m_rb = GetComponent<Rigidbody2D>();
        m_playerSprite = GetComponentInChildren<PlayerCharacterSprite>();
    }

    private void Update()
    {
        if (m_player.GetButtonDown(JUMP))
        {
            m_inputJumpDownBuffer = m_inputBufferSeconds;
            m_inputJumpDown = true;
        }

        m_inputJumpHeld = m_player.GetButton(JUMP);
    }

    private void FixedUpdate()
    {
        HandleRunning();

        HandleJumping();
    }

    private void HandleRunning()
    {
        float moveInput = m_player.GetAxis(MOVE_HORIZONTAL);

        float acceleration = moveInput * m_accelerationRate * Time.fixedDeltaTime;
        float deceleration = m_decelerationRate * Time.fixedDeltaTime;

        float xVel = velocity.x;
        
        if (velocity.x > 0 && moveInput <= 0)
        {
            xVel -= deceleration;

            if (xVel <= 0)
                xVel = 0;
        }
        else if (velocity.x < 0 && moveInput >= 0)
        {
            xVel += deceleration;

            if (xVel >= 0)
                xVel = 0;
        }

        xVel += acceleration;

        xVel = Mathf.Clamp(xVel, -m_maxHorizontalSpeed * Time.fixedDeltaTime, m_maxHorizontalSpeed * Time.fixedDeltaTime);

        velocity = new Vector2(xVel, velocity.y);
    }

    private void HandleJumping()
    {
        if (m_inputJumpDown)
        {
            bool doJump = m_timeAirborne <= m_coyoteTimeSeconds && m_jumpState == JumpState.NOT_STARTED;

            if (doJump)
            {
                Jump();
            }

            if (doJump || m_inputJumpDownBuffer <= 0)
                m_inputJumpDown = false;
        }
        
        m_inputJumpDownBuffer -= Time.fixedDeltaTime;
        m_inputJumpDownBuffer = Mathf.Max(m_inputJumpDownBuffer, 0);

        float yVel = velocity.y;

        // Jumping -> Falling when get to jump apex
        if (m_jumpState == JumpState.JUMPING)
        {
            if (yVel < 0)
                m_jumpState = JumpState.FALLING;
        }

        // Jumping -> Not started when grounded
        if (m_obj.IsGrounded && m_jumpState != JumpState.JUMPING)
        {
            if (m_timeAirborne > 0)
            {
                m_playerSprite.PlayLandAnim();
            }

            m_timeAirborne = 0;
            m_jumpState = JumpState.NOT_STARTED;
        }
        else
        {
            // Airborne logic

            m_timeAirborne += Time.fixedDeltaTime;

            if (m_jumpState == JumpState.JUMPING && m_inputJumpHeld)
                yVel -= m_jumpGravity * Time.fixedDeltaTime;
            else
                yVel -= m_gravity * Time.fixedDeltaTime;
        }

        yVel = Mathf.Clamp(yVel, -m_maxVerticalSpeed * Time.fixedDeltaTime, m_maxVerticalSpeed * Time.fixedDeltaTime);
        velocity = new Vector2(velocity.x, yVel);
    }

    private void Jump()
    {
        velocity = new Vector2(velocity.x, m_jumpImpulse * Time.fixedDeltaTime);
        m_playerSprite.PlayJumpAnim();
        m_jumpState = JumpState.JUMPING;
    }
}
