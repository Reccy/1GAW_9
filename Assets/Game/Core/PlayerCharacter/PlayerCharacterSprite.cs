using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterSprite : MonoBehaviour
{
    [SerializeField] float m_lerpRate = 1.0f;

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, m_lerpRate * Time.fixedDeltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, m_lerpRate * Time.fixedDeltaTime);
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
