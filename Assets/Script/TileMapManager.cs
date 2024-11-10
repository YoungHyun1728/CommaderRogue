using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    public Tilemap playerTilemap;
    public Tilemap enemyTilemap;
    public Tile highlightTile; // 배치 가능한 타일을 표시할 하이라이트 타일
    private int[,] tileStatus; // 각각의 타일상태를 저장하기 위한 2차원배열

    void Start()
    {
        InitializeTileStatus();
    }

    void InitializeTileStatus()
    {
        BoundsInt bounds = playerTilemap.cellBounds;
        tileStatus = new int[bounds.size.x, bounds.size.y];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (playerTilemap.HasTile(tilePosition))
                {
                    tileStatus[x - bounds.xMin, y - bounds.yMin] = 0; // 초기 상태는 비어 있음
                }
            }
        }
    }

    public void HighlightPlacementTiles()
    {
        BoundsInt bounds = playerTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (tileStatus[x - bounds.xMin, y - bounds.yMin] == 0) // 비어 있는 타일만 하이라이트
                {
                    playerTilemap.SetTile(tilePosition, highlightTile);
                }
            }
        }
    }

    public void PlaceCharacter(Vector3Int tilePosition)
    {
        if (tileStatus[tilePosition.x, tilePosition.y] == 0) // 비어 있는 타일이면 캐릭터 배치
        {
            tileStatus[tilePosition.x, tilePosition.y] = 1;
            Debug.Log("캐릭터가 배치되었습니다: " + tilePosition);
            playerTilemap.SetTile(tilePosition, null); // 하이라이트 제거
        }
        else
        {
            Debug.Log("타일에 이미 다른 오브젝트가 있습니다.");
        }
    }


}
