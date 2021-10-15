using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomDebugGizmo : MonoBehaviour
{
    [SerializeField] private Vector2Int m_minRange;
    [SerializeField] private Vector2Int m_maxRange;

    private void OnDrawGizmosSelected()
    {
        for (int x = m_minRange.x; x < m_maxRange.x; ++x)
        {
            for (int y = m_minRange.y; y < m_maxRange.y; ++y)
            {
                DrawForCell(x, y);
            }
        }
    }

    private void DrawForCell(int x, int y)
    {
        Vector3 c = new Vector3(x * 32, y * 30 + 12);
        Vector3 s = new Vector3(32, 30);
        Gizmos.DrawWireCube(c, s);
    }
}
