using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EndSequence : MonoBehaviour
{
    [SerializeField] Tilemap m_endingTilemap;
    [SerializeField] TileBase m_tileBase;
    private Tilemap m_realTilemap;
    private List<Vector2Int> m_endingTileCoords;

    private bool m_endingStarted = false;

    private void Awake()
    {
        m_realTilemap = GetComponentInChildren<Tilemap>();

        m_endingTilemap.gameObject.SetActive(false);

        m_endingTileCoords = new List<Vector2Int>();

        for (int y = m_endingTilemap.cellBounds.yMin; y < m_endingTilemap.cellBounds.yMax; ++y)
        {
            for (int x = m_endingTilemap.cellBounds.xMax; x > m_endingTilemap.cellBounds.xMin; --x)
            {
                if (m_endingTilemap.GetTile(new Vector3Int(x, y, 0)))
                    m_endingTileCoords.Add(new Vector2Int(x, y));
            }
        }
    }

    public void StartEnding()
    {
        if (m_endingStarted)
            return;

        m_endingStarted = true;
        StartCoroutine(EndingCoroutine());
    }

    private IEnumerator EndingCoroutine()
    {
        for (int i = 0; i < m_endingTileCoords.Count; ++i)
        {
            yield return new WaitForSeconds(0.2f);

            m_realTilemap.SetTile((Vector3Int)m_endingTileCoords[i], m_tileBase);
        }
    }
}
