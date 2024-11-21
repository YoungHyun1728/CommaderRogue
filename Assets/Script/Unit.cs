using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public enum UnitState{ Idle, Combat, Faint }

    
    [SerializeField]
    private UnitState currentState; // 현재 상태

    public int unitId; // 유닛의 ID 경로 예약할때 사용
    public int level = 0;
    public int maxLevel = 25;    
    public float maxMp = 200;
    public float hp = 100;
    public float mp;
    public int exp = 0;
    List<int> Equipment = new List<int>();
    public float[] levelUpExp = new float[24];
    public int attackRange = 1;
    public int strength;
    public int agility;
    public int intelligence;
    //strength 보너스 스탯
    public float maxHp;
    public float hpRecovery;
    //agility 보너스스탯
    public float attackInretval;
    public float criticalProbability;
    //intelligence 보너스스탯
    public float manaRecovery;
    public float bonusExp;

    // 유닛의 타일맵 관리자 참조
    private TileMapManager tileMapManager;
    // 현재 타일맵에서의 위치
    public Vector2Int currentTilePosition;
    public Vector2Int targetTilePosition;
    public GameObject targetEnemy;

    

    public void Initialize(TileMapManager tileMapManager, Vector2Int initialPosition)
    {
        // 타일맵 관리자 참조 저장
        this.tileMapManager = tileMapManager;

        // 유닛 초기 위치 설정
        currentTilePosition = initialPosition;

        // 유닛의 월드 좌표 동기화
        Vector3Int initialPosition3D = new Vector3Int(initialPosition.x, initialPosition.y, 0);
        Vector3 tileCenter = tileMapManager.tilemap.GetCellCenterWorld(initialPosition3D);

        // 유닛 위치 설정
        transform.position = tileCenter;

        Debug.Log($"[Unit] 초기 위치 설정: {transform.position} (중심: {tileCenter})");
    }

    private void Start()
    {
        tileMapManager = FindObjectOfType<TileMapManager>();
    }

    void Awake()
    {
        unitId = GetInstanceID(); // 고유 ID를 Unity의 InstanceID로 설정
        currentState = UnitState.Idle; // 기본상태를 Idle // 추후 생성으로 옮겨야할듯
    }
    void Update()
    {
        //타일맵 매니저에게 자신의 위치를 계속 알려줌
        Vector2Int newTilePosition = GetTileFromWorldPosition();

        //2초마다 가까운 적의 위치를 받아옴
        UpdateTargetEnemy(2.0f);

        switch (currentState)
        {
            case UnitState.Idle:
                HandleIdleState();
                break;
            case UnitState.Combat:
                HandleCombatState();
                break;
            case UnitState.Faint:
                break;
        }
    }

    private void ChangeState(UnitState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"[Unit] State Changed: {currentState} -> {newState}");
        currentState = newState;

        // 상태 변경 후 초기화 작업
        switch (newState)
        {
            case UnitState.Idle:
                OnEnterIdle();
                break;
            case UnitState.Combat:
                OnEnterCombat();
                break;
            case UnitState.Faint:
                OnEnterFaint();
                break;
        }
    }

    // 상태 진입 시 초기화
    private void OnEnterIdle() 
    {
        // IDLE 애니메이션 실행
    }
    private void OnEnterCombat() 
    { 
        Debug.Log($"{name} : 전투상태 돌입");
    }
    private void OnEnterFaint()
    {
        // 기절시 데이터 저장 후 오브젝트 삭제?
    }

    // IDLE 상태 
    private void HandleIdleState()
    {
        targetEnemy = FindClosestEnemy();
        if( targetEnemy != null)
        {
            ChangeState(UnitState.Combat);
        }
    }

    // MOVE 상태
    private void HandleCombatState()
    {
        if (targetEnemy == null || targetEnemy.GetComponent<Unit>().hp <= 0)
        {
            // 타겟이 없거나 사망한 경우 Idle 상태로 전환
            ChangeState(UnitState.Idle);
            return;
        }

        // 타겟과의 거리 확인
        Vector2Int enemyTilePosition = tileMapManager.GetTileFromWorldPosition(targetEnemy.transform.position);
        if (Vector2Int.Distance(currentTilePosition, enemyTilePosition) <= attackRange)
        {
            // 타겟이 공격 범위 안에 있으면 공격 수행
            PerformAttack(targetEnemy);
        }
        else
        {
            // 타겟이 범위 밖에 있으면 타겟을 향해 이동
            MoveTo(enemyTilePosition);
        }
    }

    // FAINT 상태
    private void HandleFaintState()
    {
        // 사망 로직 (예: 애니메이션, 오브젝트 비활성화 등)
        Debug.Log($"[Unit] Dead. No further actions.");
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
        int reservationState = GetReservationState();

        foreach (var tile in path) 
        {
            /*// 현재 타일 비우기
            tileMapManager.SetTileStatus(currentTilePosition, 0);

            // 다음 타일 예약
            tileMapManager.SetTileStatus(tile, reservationState);*/

            // 다음 타일의 월드 위치 계산
            Vector3 targetPosition = tileMapManager.tilemap.GetCellCenterWorld(new Vector3Int(tile.x, tile.y, 0));

            // 이동하면서 목표 위치에 도달할 때까지 이동
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f) 
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 3f);
                yield return null;
            }        

            // 현재 타일 갱신
            currentTilePosition = tile;
            
            // 현재 타일 점유 상태로 업데이트
            //tileMapManager.SetTileStatus(currentTilePosition, -1); // 현재 타일을 점유 상태로 설정

            // 공격 범위 확인
            if (CheckAttackRange()) 
            {
                yield break; // 이동 종료
            }

            // 다음 타일로 이동 준비
        }
    }

    private IEnumerator RecalculatePath(Vector2Int targetTile)
    {
        Debug.Log($"[Unit] 경로 재계산 시작. 목표: {targetTile}");

        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
        foreach (var tileData in tileMapManager.tileDataList)
        {
            if (tileData.Status != 0 && tileData.Position != currentTilePosition)
            {
                occupiedTiles.Add(tileData.Position);
            }
        }

        List<Vector2Int> newPath = AStarPathfinder.FindPath(currentTilePosition, targetTile, tileMapManager, occupiedTiles);

        if (newPath.Count > 0)
        {
            Debug.Log($"[Unit] 새 경로 발견. 스텝 수: {newPath.Count}");
            yield return StartCoroutine(FollowPath(newPath)); // 새 경로 따라 이동
        }
        else
        {
            Debug.LogWarning($"[Unit] 경로 재계산 실패. 목표 {targetTile}에 도달할 수 없습니다.");
        }
    }

    private int GetReservationState()
    {
        return -2 - unitId; // 고유 예약 상태
    }

    //가장 가까운 적을 지정해주는 함수
    private GameObject FindClosestEnemy()
    {
        GameObject closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject enemy in tileMapManager.enemyUnits)
        {
            if (enemy == null) continue;

            Vector2Int enemyTilePosition = tileMapManager.GetTileFromWorldPosition(enemy.transform.position);
            float distance = Vector2Int.Distance(currentTilePosition, enemyTilePosition);

            if (distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }

        return closestEnemy;
    }

    // 초기값 : 2초마다 업데이트 되게 실행
    IEnumerator UpdateTargetEnemy(float interval) 
    {
        while (true)
        {
            targetEnemy = FindClosestEnemy();
            yield return new WaitForSeconds(interval);
        }
    }

    // 공격 범위 타일 계산 인접타일을 range크기만큼 확장시킴    
    HashSet<Vector2Int> GetTilesInRange(Vector2Int center, int range) 
    {
        HashSet<Vector2Int> tilesInRange = new HashSet<Vector2Int>();
        for (int x = -range; x <= range; x++) {
            for (int y = -range; y <= range; y++) {
                Vector2Int tile = new Vector2Int(center.x + x, center.y + y);
                // 정수 거리 계산을 통해 범위 안에 있는 타일만 추가
                if (Mathf.Abs(tile.x - center.x) + Mathf.Abs(tile.y - center.y) <= range && tileMapManager.IsWalkable(tile)) 
                {
                    tilesInRange.Add(tile);
                }
            }
        }
        
        return tilesInRange;
    }

    // 적이 범위내에 존재하는지 확인
    GameObject FindEnemyInRange(Vector2Int center, int range) 
    {
        HashSet<Vector2Int> tilesInRange = GetTilesInRange(center, range);
        foreach (Vector2Int tile in tilesInRange) 
        {
            foreach (GameObject enemy in tileMapManager.enemyUnits) 
            {
                if (tileMapManager.GetTileFromWorldPosition(enemy.transform.position) == tile) {
                    return enemy; // 범위 내 적 발견
                }
            }
        }
        return null; // 범위 내 적 없음
    }

    private void CheckAndAttack() 
    {
        GameObject targetEnemy = FindEnemyInRange(currentTilePosition, attackRange);
        if (targetEnemy != null) 
        {
            PerformAttack(targetEnemy);
        }
    }

    private void PerformAttack(GameObject enemy) 
    {
        // 공격 로직 (예: 데미지 계산, 적 체력 감소)
        // 공격시 마나 10회복
        Debug.Log($"{enemy.name}를 공격 중");
        // 적이 쓰러졌다면 상태 변경
        if (enemy.GetComponent<Unit>().hp <= 0) 
        {
            Debug.Log($"{enemy.name} 처치 완료!");
            ChangeState(UnitState.Idle);
        }
    }

    private bool CheckAttackRange() 
    {
        HashSet<Vector2Int> tilesInRange = GetTilesInRange(currentTilePosition, attackRange);
        foreach (Vector2Int tile in tilesInRange) 
        {
            foreach (GameObject enemy in tileMapManager.enemyUnits) 
            {
                if (tileMapManager.GetTileFromWorldPosition(enemy.transform.position) == tile) 
                {
                    targetEnemy = enemy; // 타겟 적 설정
                    return true; // 적이 범위 내에 있음
                }
            }
        }
        return false; // 범위 내에 적 없음
    }
}
