using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int level = 0;
    public int maxLevel = 25;
    public float hp;
    public float MaxMp = 200;
    public float mp;
    public int exp = 0;
    List<int> Equipment = new List<int>();
    public float[] levelUpExp = new float[24];
    public int strength;
    public int agility;
    public int intelligence;
    //strength 보너스 스탯
    public float maxHp;
    public float hpRecovery;
    //agility 보너스스탯
    public float attackSpeed;
    public float criticalProbability;
    //intelligence 보너스스탯
    public float manaRecovery;
    public float bonusExp;

    // 유닛의 타일맵 관리자 참조
    private TileMapManager tileMapManager;
    // 현재 타일맵에서의 위치
    public Vector2Int currentTilePosition;
    public Vector3Int targetTilePosition;

    public void Initialize(TileMapManager tileMapManager, Vector2Int initialPosition)
    {
        // 타일맵 관리자 참조 저장
        this.tileMapManager = tileMapManager;

        // 유닛 초기 위치 설정
        currentTilePosition = initialPosition;

        // 유닛의 월드 좌표 동기화
        Vector3Int initialPosition3D = new Vector3Int(initialPosition.x, initialPosition.y, 0);
        transform.position = tileMapManager.tilemap.CellToWorld(initialPosition3D);
    }
    void Update()
    {
        Vector2Int newTilePosition = GetTileFromWorldPosition();

        if (currentTilePosition != newTilePosition)
        {
            tileMapManager.UpdateTileStatus(newTilePosition); // 타일맵 상태 업데이트
            currentTilePosition = newTilePosition; // 현재 위치 갱신
        }
    }

    private Vector2Int GetTileFromWorldPosition()
    {
        Vector3Int cellPosition = tileMapManager.tilemap.WorldToCell(transform.position);
        return new Vector2Int(cellPosition.x, cellPosition.y);
    }

    public void LevelUp()
    {
        if(exp > levelUpExp[level] && level < maxLevel)
        {
            level++;
            // 레벨업시 능력치 추가
        }
    }

    public void MeleeAttack()
    {
        //자신의 위치한 타일에서 상하좌우칸 중 한칸을 선택해 공격
        // 공격시 마나 10 회복
    }

    public void RangeAttack()
    {
        // 자신의 공격 범위 내에서 적이 있는 타일한칸을 선택해 공격
        // 공격시 마나 10 회복
    }

    public void Equip()
    {
        //장비장착
        //장비 리스트에서 장비아이템 삭제후 캐릭터의 장비리스트에 추가
    }

    public void UnEquip()
    {
        //장비 해체
        //캐릭터의 장비리스트에서 아이템 삭제 후 장비리스트에 추가
    }

    public void MoveTo(Vector2Int targetTile)
    {
        // 타겟 타일이 현재 타일과 같으면 이동하지 않음
        if (currentTilePosition == targetTile)
        {
            return;
        }

        // 점유된 타일 목록 생성 (현재 유닛의 타일 제외)
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
        foreach (var tileData in tileMapManager.tileDataList)
        {
            if (tileData.Status == -1 && tileData.Position != currentTilePosition)
            {
                occupiedTiles.Add(tileData.Position);
            }
        }

        // 목표 타일이 점유된 경우 이동 불가 처리?? 수정할듯
        if (occupiedTiles.Contains(targetTile))
        {
            Debug.LogWarning($"MoveTo: 목표 타일 {targetTile}이 점유되어 이동 불가.");
            return;
        }

        // 경로 탐색
        List<Vector2Int> path = AStarPathfinder.FindPath(currentTilePosition, targetTile, tileMapManager, occupiedTiles);
        if (path.Count > 0)
        {
            Debug.Log($"MoveTo 성공: 경로 발견. 스텝 수: {path.Count}");
            
            // 경로 확인용 디버그 로그
            foreach (var step in path)
            {
                Debug.Log($"경로 스텝: {step}");
            }

            //경로 따라 이동
            StartCoroutine(FollowPath(path));
        }
        else
        {
            Debug.LogWarning($"MoveTo 실패: 경로를 찾을 수 없습니다. 시작: {currentTilePosition}, 목표: {targetTile}");
        }
    }

    private IEnumerator FollowPath(List<Vector2Int> path)
    {
        foreach (var tile in path)
        {
            // 이전 타일 상태를 비우기
            tileMapManager.SetTileStatus(currentTilePosition, 0);

            // 새 타일 상태를 점유로 설정
            tileMapManager.SetTileStatus(tile, -1);

            // 목표 위치 계산
            Vector3 targetPosition = tileMapManager.tilemap.CellToWorld(new Vector3Int(tile.x, tile.y, 0));

            // 부드럽게 이동
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 3f);
                yield return null;
            }

            // 현재 타일 갱신
            currentTilePosition = tile;
        }
    }

    private Vector3 GetTileCenter(Vector3Int cellPosition)
    {
        Vector3 bottomLeft = tileMapManager.tilemap.CellToWorld(cellPosition);
        Vector3 cellSize = tileMapManager.tilemap.cellSize;

        // 셀의 중심 좌표 계산
        return bottomLeft + new Vector3(cellSize.x / 2, cellSize.y / 2, 0);
    }

}
