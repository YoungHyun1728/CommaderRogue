using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)] // 타일맵매니저가 AStarPathfinder보다 먼저 실행
public class TileMapManager : MonoBehaviour
{
    public Tilemap tilemap; // 타일맵
    public Tile highlightTile; // 배치 가능한 타일을 표시할 하이라이트 타일
    public List<GameObject> playerUnits = new List<GameObject>(); // 플레이어 유닛 리스트
    public List<GameObject> enemyUnits = new List<GameObject>(); // 적 유닛 리스트
    public List<TileData> tileDataList; // 타일의 포지션과 상태(배치가능유무, 장애물)를 관리

    [SerializeField, Tooltip("실시간 타일 상태 확인용")] 
    private List<string> tileStatusDisplay = new List<string>(); // 디버깅용 상태 표시 리스트

    public Vector2Int tilemapOrigin; // 타일맵의 (0,0)
    void Start()
    {
        InitializeTileStatus();
    }

    void Update()
    {
        UpdateTileStatusDisplay(); // 인스펙터에 표시용 데이터 갱신
    }

    //타일 상태 초기화
    void InitializeTileStatus()
    {
        BoundsInt bounds = tilemap.cellBounds; // 타일맵의 경계 초기화
        tilemapOrigin = new Vector2Int(bounds.xMin, bounds.yMin); // 왼쪽아래를 원점으로
        tileDataList = new List<TileData>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                Vector2Int gridPosition = new Vector2Int(x, y); //2D 로 변환
                if (tilemap.HasTile(tilePosition))
                {
                    // 타일이 있으면 데이터 추가 (비어 있음: 0)
                    tileDataList.Add(new TileData(gridPosition, 0));
                }
            }
        }

        Debug.Log("TileData 초기화 완료:");
        foreach (var tileData in tileDataList)
        {
            Debug.Log($"타일 {tileData.Position}: 상태 {tileData.Status}");
        }
    }

     // 모든 유닛의 위치를 기반으로 타일 상태를 업데이트
    public void UpdateTileStatus(Vector2Int currentTile)
    {
        // 모든 타일 초기화
        for (int i = 0; i < tileDataList.Count; i++)
        {
            tileDataList[i] = new TileData(tileDataList[i].Position, 0); // 기본상태 : 비어있음 [0]
        }
        // 플레이어 유닛의 위치를 -1로 설정
        foreach (var unit in playerUnits)
        {
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent != null)
            {
                SetTileStatus(unitComponent.currentTilePosition, -1);
            }
        }

        // 적 유닛의 위치를 -1로 설정
        foreach (var unit in enemyUnits)
        {
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent != null)
            {
                SetTileStatus(unitComponent.currentTilePosition, -1);
            }
        }
    }

    // 특정 타일의 상태를 설정
    public void SetTileStatus(Vector2Int position, int status)
    {
        for(int i = 0; i < tileDataList.Count; i++)
        {
            if (tileDataList[i].Position == position)
            {
                tileDataList[i] = new TileData(position, status); // 상태 업데이트
                break;
            }
        }
    }

    // 특정 좌표에 대한 상태 가져오기
    public int GetTileStatus(Vector2Int position)
    {
        foreach (var tileData in tileDataList)
        {
            if (tileData.Position == position)
            {
                return tileData.Status;
            }
        }

        Debug.LogWarning($"타일 {position}이 존재하지 않습니다.");
        return -1; // 기본값: 이동 불가
    }

    // 특정 좌표가 타일맵에 존재하는지 확인
    public bool IsTileAt(Vector2Int position)
    {
        foreach (var tileData in tileDataList)
        {
            if (tileData.Position == position)
            {
                return true;
            }
        }
        return false;
    }

    // 이동가능한 타일인지 확인
    public bool IsWalkable(Vector2Int tilePosition)
    {
        int status = GetTileStatus(tilePosition);
        return status == 0;
    }

    // 선택가능한 타일은 하이라이트타일로 교체
    public void HighlightPlaceTiles()
    {
        BoundsInt bounds = tilemap.cellBounds;
        //Player가 배치할수있는 타일은 왼쪽 절반뿐이다.
        for(int x = 0; x < (bounds.xMax) / 2; x++)
        {
            for(int y = 0; y < bounds.yMax; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                int status = GetTileStatus(position);

                if(status == 0)
                {
                    Vector3Int setTilePosition = new Vector3Int(x, y, 0); // 절대 좌표로 변환
                    tilemap.SetTile(setTilePosition, highlightTile);
                }     
            }
        }
    }

    public void PlaceCharacter(Vector2Int tilePosition)
    {
        int status = GetTileStatus(tilePosition);

        if (status == 0) // 비어 있는 타일이면 캐릭터 배치
        {
            status = -1;
            Debug.Log("캐릭터가 배치되었습니다: " + tilePosition);
            Vector3Int setTilePosition = new Vector3Int(tilePosition.x, tilePosition.y, 0);
            tilemap.SetTile(setTilePosition, null); // 하이라이트 제거
        }
        else
        {
            Debug.Log("타일에 이미 다른 오브젝트가 있습니다.");
        }
    }

    // 디버깅용 상태 표시 리스트 업데이트
    private void UpdateTileStatusDisplay()
    {
        tileStatusDisplay.Clear();
        foreach (var tileData in tileDataList)
        {
            tileStatusDisplay.Add($"Position: {tileData.Position}, Status: {tileData.Status}");
        }
    }
}
