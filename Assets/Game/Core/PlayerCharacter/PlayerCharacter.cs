using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;

public class PlayerCharacter : MonoBehaviour
{
    private TilemapCollider m_obj;
    private Rigidbody2D m_rb;
    private Player m_player;
    private const int PLAYER_ID = 0;

    private AudioSource m_audioSource;
    private bool m_dead = false;
    public bool Dead => m_dead;

    [Header("Handling")]
    [SerializeField] private float m_jumpImpulse = 0.5f;
    [SerializeField] private float m_accelerationRate = 1.0f;
    [SerializeField] private float m_maxHorizontalSpeed = 5.0f;
    [SerializeField] private float m_maxVerticalSpeed = 100.0f;
    [SerializeField] private float m_decelerationRate = 1.0f;
    [SerializeField] private float m_gravity = 1.0f;
    [SerializeField] private float m_jumpGravity = 0.5f;
    [SerializeField] private float m_coyoteTimeSeconds = 0.01f;
    [SerializeField] private float m_inputBufferSeconds = 0.1f;

    [Header("Juice")]
    [SerializeField] private GameObject m_deathSpriteObject;
    [SerializeField] private AudioClip m_deathSFX;

    [Header("Attacks")]
    [SerializeField] private int m_attackCooldownFrames = 12;
    [SerializeField] private int m_attackSpriteRemainFrames = 3;
    
    [SerializeField] private GameObject m_attackLeftSprite;
    [SerializeField] private GameObject m_attackLeftCollider;
    
    [SerializeField] private GameObject m_attackRightSprite;
    [SerializeField] private GameObject m_attackRightCollider;
    
    [SerializeField] private GameObject m_attackLeftJumpSprite;
    [SerializeField] private GameObject m_attackLeftJumpCollider;
    
    [SerializeField] private GameObject m_attackRightJumpSprite;
    [SerializeField] private GameObject m_attackRightJumpCollider;
    
    [SerializeField] private GameObject m_attackLeftFallSprite;
    [SerializeField] private GameObject m_attackLeftFallCollider;

    [SerializeField] private GameObject m_attackRightFallSprite;
    [SerializeField] private GameObject m_attackRightFallCollider;

    private Coroutine m_attackCoroutine;
    
    private PlayerCharacterSprite m_playerSprite;

    private bool m_facingRight = false; // If false, facing left
    public bool FacingRight => m_facingRight;

    public Vector3Int CurrentCell => m_obj.CurrentCellPosition;

    private float m_timeAirborne = 0.0f;

    private Vector2 velocity
    {
        get => m_rb.velocity;
        set => m_rb.velocity = value;
    }

    public Vector2 Velocity => velocity;

    #region INPUT
    private bool m_inputJumpDown = false;
    private float m_inputJumpDownBuffer = 0.0f;
    private bool m_inputJumpHeld = false;

    private bool m_inputAttackDown = false;

    private const string JUMP = "Jump";
    private const string MOVE_HORIZONTAL = "MoveHorizontal";
    private const string ATTACK = "Attack";
    #endregion

    public enum JumpState { NOT_STARTED, JUMPING, FALLING }
    private JumpState m_jumpState = JumpState.NOT_STARTED;
    public JumpState JumpingState => m_jumpState;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponentInParent<Dragon>() != null)
        {
            if (!m_dead)
                Die((collision.gameObject.transform.position - transform.position).normalized);
        }
    }

    private void Awake()
    {
        m_player = ReInput.players.GetPlayer(PLAYER_ID);
        m_obj = GetComponent<TilemapCollider>();
        m_rb = GetComponent<Rigidbody2D>();
        m_playerSprite = GetComponentInChildren<PlayerCharacterSprite>();
        m_audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!m_dead)
        {
            if (m_player.GetButtonDown(JUMP))
            {
                m_inputJumpDownBuffer = m_inputBufferSeconds;
                m_inputJumpDown = true;
            }

            if (m_player.GetButtonDown(ATTACK))
            {
                m_inputAttackDown = true;
            }

            m_inputJumpHeld = m_player.GetButton(JUMP);
        }
        else
        {
            m_inputJumpHeld = false;
        }
    }

    private void FixedUpdate()
    {
        HandleAttack();
        
        HandleRunning();

        HandleJumping();
    }

    private void HandleAttack()
    {
        if (!m_inputAttackDown)
            return;

        m_inputAttackDown = false;

        if (m_attackCoroutine == null)
        {
            m_attackCoroutine = StartCoroutine(HandleAttackCoroutine());
        }
    }

    private IEnumerator HandleAttackCoroutine()
    {
        if (velocity.y == 0) // on ground
        {
            if (m_facingRight)
            {
                m_attackRightSprite.SetActive(true);
                m_attackRightCollider.SetActive(true);
            }
            else
            {
                m_attackLeftSprite.SetActive(true);
                m_attackLeftCollider.SetActive(true);
            }
        }
        else if (velocity.y > 0) // jumping up
        {
            if (m_facingRight)
            {
                m_attackRightJumpSprite.SetActive(true);
                m_attackRightJumpCollider.SetActive(true);
            }
            else
            {
                m_attackLeftJumpSprite.SetActive(true);
                m_attackLeftJumpCollider.SetActive(true);
            }
        }
        else // falling down
        {
            if (m_facingRight)
            {
                m_attackRightJumpSprite.SetActive(true);
                m_attackRightJumpCollider.SetActive(true);
            }
            else
            {
                m_attackLeftJumpSprite.SetActive(true);
                m_attackLeftJumpCollider.SetActive(true);
            }
        }

        yield return new WaitForFixedUpdate();

        m_attackLeftCollider.SetActive(false);
        m_attackRightCollider.SetActive(false);
        m_attackLeftFallCollider.SetActive(false);
        m_attackRightFallCollider.SetActive(false);
        m_attackLeftJumpCollider.SetActive(false);
        m_attackRightJumpCollider.SetActive(false);

        for (int i = 0; i < m_attackSpriteRemainFrames; ++i)
        {
            yield return new WaitForEndOfFrame();
        }

        m_attackLeftSprite.SetActive(false);
        m_attackRightSprite.SetActive(false);
        m_attackLeftFallSprite.SetActive(false);
        m_attackRightFallSprite.SetActive(false);
        m_attackLeftJumpSprite.SetActive(false);
        m_attackRightJumpSprite.SetActive(false);

        for (int i = 0; i < m_attackCooldownFrames; ++i)
        {
            yield return new WaitForFixedUpdate();
        }

        m_attackCoroutine = null;
    }

    private void HandleRunning()
    {
        float moveInput = m_player.GetAxis(MOVE_HORIZONTAL);

        if (m_dead)
            moveInput = 0;

        float acceleration = moveInput * m_accelerationRate * Time.fixedDeltaTime;
        float deceleration = m_decelerationRate * Time.fixedDeltaTime;

        float xVel = velocity.x;
        
        if (xVel > 0)
            m_facingRight = true;
        else if (xVel < 0)
            m_facingRight = false;

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

    private void Die(Vector3 dirToKiller)
    {
        m_dead = true;

        float force = 5.0f;

        GameObject deathSprite = Instantiate(m_deathSpriteObject);
        deathSprite.transform.position = transform.position;
        deathSprite.GetComponent<Rigidbody2D>().velocity = -dirToKiller * force;

        GameObject deathSprite2 = Instantiate(m_deathSpriteObject);
        deathSprite2.transform.position = transform.position;
        deathSprite2.GetComponent<Rigidbody2D>().velocity = dirToKiller * force;

        GameObject deathSprite3 = Instantiate(m_deathSpriteObject);
        deathSprite3.transform.position = transform.position;
        deathSprite3.GetComponent<Rigidbody2D>().velocity = new Vector3(dirToKiller.y, -dirToKiller.x) * force;

        GameObject deathSprite4 = Instantiate(m_deathSpriteObject);
        deathSprite4.transform.position = transform.position;
        deathSprite4.GetComponent<Rigidbody2D>().velocity = new Vector3(-dirToKiller.y, dirToKiller.x) * force;

        m_audioSource.PlayOneShot(m_deathSFX, 1.0f);

        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        FindObjectOfType<ScreenWipe>().Wipe();

        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
