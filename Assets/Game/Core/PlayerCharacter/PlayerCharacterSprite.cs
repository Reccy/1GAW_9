using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reccy.ScriptExtensions;

public class PlayerCharacterSprite : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] float m_lerpRate = 1.0f;


    [Header("Idle")]
    [SerializeField] private Sprite m_idleSpriteLeft;
    [SerializeField] private Sprite m_idleSpriteRight;

    [Header("Death")]
    [SerializeField] private Sprite m_deadSprite;

    [Header("Run")]
    [SerializeField] private Sprite[] m_runSpriteLeft;
    [SerializeField] private Sprite[] m_runSpriteRight;
    [SerializeField] private float m_runSeconds = 0.02f;
    private float m_runAnimAcc = 0.0f;
    int m_currentRunAnimIdx = 0;

    private PlayerCharacter m_player;
    private SpriteRenderer m_spr;

    private void Awake()
    {
        m_player = GetComponentInParent<PlayerCharacter>();
        m_spr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, m_lerpRate * Time.fixedDeltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, m_lerpRate * Time.fixedDeltaTime);

        if (m_player.Dead)
        {
            m_spr.sprite = m_deadSprite;
            return;
        }

        if (m_player.JumpingState == PlayerCharacter.JumpState.NOT_STARTED)
        {
            // Stationary on ground
            if (Mathf2.Approximately(m_player.Velocity.x, 0, 0.001f) && Mathf2.Approximately(m_player.Velocity.y, 0, 0.001f))
            {
                if (m_player.FacingRight)
                {
                    m_spr.sprite = m_idleSpriteRight;
                }
                else
                {
                    m_spr.sprite = m_idleSpriteLeft;
                }
            }
            else
            {
                m_runAnimAcc += Time.deltaTime;

                if (m_runAnimAcc > m_runSeconds)
                {
                    m_runAnimAcc = 0;
                    m_currentRunAnimIdx = (m_currentRunAnimIdx + 1) % m_runSpriteLeft.Length;

                    // Running on ground
                    if (m_player.FacingRight)
                    {
                        m_spr.sprite = m_runSpriteRight[m_currentRunAnimIdx];
                    }
                    else
                    {
                        m_spr.sprite = m_runSpriteLeft[m_currentRunAnimIdx];
                    }
                }
            }
        }
        else if (m_player.JumpingState == PlayerCharacter.JumpState.JUMPING)
        {

        }
        else if (m_player.JumpingState == PlayerCharacter.JumpState.FALLING)
        {

        }

    }

    public void PlayJumpAnim()
    {
        Scale(0.4f, 1.5f);
    }

    public void PlayLandAnim()
    {
        Scale(1.2f, 0.4f, offsetPositionY: true);
    }

    private void Scale(float x, float y, bool offsetPositionX = false, bool offsetPositionY = false)
    {
        transform.localScale = new Vector3(x, y, 1);

        if (!offsetPositionX && !offsetPositionY)
            return;

        float xOffset = 0;
        if (offsetPositionX)
            xOffset = -x;

        float yOffset = 0;
        if (offsetPositionY)
            yOffset = -y;

        Offset(xOffset, yOffset);
    }

    private void Offset(float x, float y)
    {
        transform.localPosition += new Vector3(x, y, 1);
    }
}
