using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTest : Unit
{
    private TileMapManager tileMapManager;
    // Start is called before the first frame update
    void Start()
    {
        // TileMapManager 연결
        tileMapManager = FindObjectOfType<TileMapManager>();
        if (tileMapManager == null)
        {
            Debug.LogError("TileMapManager를 찾을 수 없습니다.");
            return;
        }

        Vector2Int initialTile = new Vector2Int(0, 0);
        Vector3Int initialTile3D = new Vector3Int(initialTile.x, initialTile.y, 0);

         // 초기화
        Initialize(tileMapManager, initialTile);

        // 이동 목표
        Vector2Int targetTile = new Vector2Int(-6, 3);
        Debug.Log($"목표 타일로 이동 테스트: {targetTile}");

        // 목표 타일로 이동
        MoveTo(targetTile);
    }

    

}
