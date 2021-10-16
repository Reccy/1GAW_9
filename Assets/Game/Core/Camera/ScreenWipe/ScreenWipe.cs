using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenWipe : MonoBehaviour
{
    private Camera m_camera;
    private Material m_wipeMaterial;

    private float m_fullyVisibleAt = 0.72f;

    private Coroutine m_wipeCoroutine;

    private PlayerCharacter m_player;

    [SerializeField] private float m_speedMult = 1.0f;

    private float Cutoff => m_wipeMaterial.GetFloat("_Cutoff");

    private void Awake()
    {
        m_camera = GetComponentInParent<Camera>();
        m_wipeMaterial = GetComponent<MeshRenderer>().material;
        m_player = FindObjectOfType<PlayerCharacter>();
    }

    private void Start()
    {
        Unwipe();
    }

    private void Update()
    {
        transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y, transform.position.z);
    }

    public void Unwipe()
    {
        if (m_wipeCoroutine != null)
        {
            StopCoroutine(m_wipeCoroutine);
        }

        m_wipeCoroutine = StartCoroutine(UnwipeCoroutine());
    }

    private IEnumerator UnwipeCoroutine()
    {
        SetCutoff(0);

        yield return new WaitForEndOfFrame();

        while (Cutoff < m_fullyVisibleAt)
        {
            SetCutoff(Cutoff + Time.deltaTime * m_speedMult);
            yield return new WaitForEndOfFrame();
        }
    }

    public void Wipe()
    {
        if (m_wipeCoroutine != null)
        {
            StopCoroutine(m_wipeCoroutine);
        }

        m_wipeCoroutine = StartCoroutine(WipeCoroutine());
    }

    private IEnumerator WipeCoroutine()
    {
        SetCutoff(1);

        yield return new WaitForEndOfFrame();

        while (Cutoff > 0)
        {
            SetCutoff(Cutoff - Time.deltaTime * m_speedMult);
            yield return new WaitForEndOfFrame();
        }
    }

    private void SetCutoff(float cutoff)
    {
        m_wipeMaterial.SetFloat("_Cutoff", cutoff);
    }
}
