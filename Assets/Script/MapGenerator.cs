using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public int totalLevels = 100;
    public int nodesPerLevel = 5;
    public int bossInterval = 10;  // 10레벨마다 보스

    public GameObject combatPrefab;
    public GameObject restPrefab;
    public GameObject bossPrefab;
    public GameObject eventPrefab;
    public GameObject tradePrefab;
    public GameObject linePrefab;  

    private float combatProbability = 0.55f;  // 각노드들이 등장할 확률
    private float eventProbability = 0.25f;   
    private float restProbability = 0.1f;     
    private float tradeProbability = 0.1f;   

    private List<List<MapNode>> mapLevels;

    public GameObject scrollViewContent;  

    void Start()
    {
        if (scrollViewContent == null)
        {
            Debug.LogError("ScrollViewContent is not assigned in the inspector.");
            return;
        }

        AdjustContentSizeAndPosition();
        GenerateNodes();
        CreateConnections();
    }

    NodeType GetRandomNodeTypeByProbability(NodeType[] nodeTypes, float[] probabilities)
    {
        float randomValue = Random.value;  // 랜덤 0~1 반환시킨다.
        float cumulativeProbability = 0f;  // 확률 누적용 변수

        for (int i = 0; i < nodeTypes.Length; i++)
        {
            // 랜덤으로 반환된 value를 cumulativeProbability와 비교해서 노드 반환
            cumulativeProbability += probabilities[i];
            if (randomValue < cumulativeProbability)
            {
                return nodeTypes[i];
            }
        }

        return NodeType.Combat;
    }

    NodeType GetRandomNodeType(int level)
    {
        float randomValue = Random.value;  // 0~1 사이 값 반환

        if (level == 0)                    //첫번째는 항상 전투
        {
            return NodeType.Combat;
        }

        if (level % bossInterval == 0) //레벨 10마다 보스전
        {
            return NodeType.Boss;
        }

        if (level < 4)  // 게임 시작하자 불필요한 휴식, 거래가 나오지 않고 레벨 5부터 나오게 제어
        {
            NodeType[] nodeTypes = { NodeType.Combat, NodeType.Event };
            float[] probabilities = { combatProbability, eventProbability };

            return GetRandomNodeTypeByProbability(nodeTypes, probabilities);
        }

        // 이전 노드가 휴식이거나 거래일 경우 연속으로 나오지 않게 조정
        // 연결만 안되면 되는 문제라 수정해야 할거같음
        /*if (previousNodeType == NodeType.Trade || previousNodeType == NodeType.Rest)
        {
            NodeType[] nodeTypes = { NodeType.Combat, NodeType.Event };
            float[] probabilities = { combatProbability, eventProbability };

            return GetRandomNodeTypeByProbability(nodeTypes, probabilities);
        }*/

        NodeType[] allNodeTypes = { NodeType.Combat, NodeType.Event, NodeType.Rest, NodeType.Trade };
        float[] allProbabilities = { combatProbability, eventProbability, restProbability, tradeProbability };

        return GetRandomNodeTypeByProbability(allNodeTypes, allProbabilities);

    }

    GameObject GetPrefabForNodeType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Combat:
                return combatPrefab;
            case NodeType.Rest:
                return restPrefab;
            case NodeType.Boss:
                return bossPrefab;
            case NodeType.Event:
                return eventPrefab;
            case NodeType.Trade:
                return tradePrefab;
            default:
                return combatPrefab; // 기본값으로 전투노드
        }
    }

    void DrawConnection(GameObject startNode, GameObject endNode)
    {
        // 라인 생성
        GameObject lineObject = Instantiate(linePrefab, scrollViewContent.transform);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        // Line 초기화
        Line line = lineObject.AddComponent<Line>();
        line.startNode = startNode;
        line.endNode = endNode;
        line.lineRenderer = lineRenderer;

        lineRenderer.useWorldSpace = false; //월드좌표계 사용

        // LineRenderer의 위치를 부모(Content)의 좌표계에 맞춤
        lineObject.transform.localPosition = Vector3.zero;

        // 노드의 RectTransform 가져오기
        RectTransform startRect = startNode.GetComponent<RectTransform>();
        RectTransform endRect = endNode.GetComponent<RectTransform>();

        if (startRect == null || endRect == null)
        {
            Debug.LogError("��???���� RectTransform?? ������?��?��?.");
            return;
        }

        // 위치가져오기
        Vector3 startPos = startRect.localPosition;
        Vector3 endPos = endRect.localPosition;

        // LineRenderer 설정
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        
        //노드의 뒤로 선정리
        lineRenderer.sortingOrder = -1;

        //연결 정보
        MapNode startMapNode = startNode.GetComponent<MapNode>();
        MapNode endMapNode = endNode.GetComponent<MapNode>();

        if (startMapNode != null)
        {
            startMapNode.ConnectTo(endMapNode, null); // 연결추가
        }

        if (endMapNode != null)
        {
            if (endMapNode.prevNodePrefab == null)
            {
                endMapNode.prevNodePrefab = new List<GameObject>();
            }
            endMapNode.prevNodePrefab.Add(startNode);
        }
    }

    void GenerateNodes()
    {
        mapLevels = new List<List<MapNode>>();
        float iconIntervalX = 250.0f;
        float iconIntervalY = 175.0f;

        float contentHalfWidth = scrollViewContent.GetComponent<RectTransform>().sizeDelta.x / 2;
        float contentHalfHeight = scrollViewContent.GetComponent<RectTransform>().sizeDelta.y / 2;

        float xOffset = 80.0f; 
        float yOffset = 60.0f; 

        for (int level = 0; level <= totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = new List<MapNode>();

            for (int nodeIndex = 0; nodeIndex < nodesPerLevel; nodeIndex++)
            {
                NodeType nodeType = GetRandomNodeType(level);
                GameObject nodePrefab = GetPrefabForNodeType(nodeType);

                // 노드 배치
                Vector2 nodePosition = new Vector2(
                    (level * iconIntervalX) - contentHalfWidth + xOffset,  // X offset ?���Ƣ�
                    (-nodeIndex * iconIntervalY) + contentHalfHeight - yOffset  // Y offset ?���Ƣ�
                );


                // 노드생성, 이름설정
                GameObject nodeObject = Instantiate(nodePrefab, scrollViewContent.transform);
                nodeObject.name = $"Node_{level}_{nodeIndex}";

                // RectTransForm이용
                RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
                nodeObject.GetComponent<RectTransform>().anchoredPosition = nodePosition;


                MapNode mapNode = nodeObject.GetComponent<MapNode>();
                if (mapNode == null)
                {
                    mapNode = nodeObject.AddComponent<MapNode>();
                }
                mapNode.Initialize(nodeType, level, nodeObject);

                currentLevelNodes.Add(mapNode);
            }

            // 보스 레벨인 경우, 3번째 노드 제외한 나머지 4개 삭제
            if (level > 0 && level % bossInterval == 0)
            {
                for (int nodeIndex = 0; nodeIndex < currentLevelNodes.Count; nodeIndex++)
                {
                    if (nodeIndex != 2) // 3번째 노드를 제외하고 나머지 삭제
                    {
                        Destroy(currentLevelNodes[nodeIndex].NodeObject);
                    }
                }
                // 3번째 노드만 남기고 currentLevelNodes를 갱신 (리스트 복사)
                List<MapNode> bossNodeList = new List<MapNode> { currentLevelNodes[2] };

                // mapLevels에 갱신된 노드 리스트 추가
                mapLevels.Add(bossNodeList);

                // currentLevelNodes를 새로운 보스 노드 리스트로 교체
            }
            else
            {
                mapLevels.Add(currentLevelNodes);
            }

        }
    }

    void CreateConnections()
    {
        for (int level = 1; level <= totalLevels; level++) 
        {
            List<MapNode> currentLevelNodes = mapLevels[level];
            List<MapNode> previousLevelNodes = mapLevels[level - 1];

            foreach (MapNode prevNode in previousLevelNodes)
            {
                // 보스 레벨이면 보스 노드(3번째 노드)를 다음 레벨의 모든 노드에 연결
                if (prevNode.Type == NodeType.Boss)
                {
                    foreach (MapNode nextNode in currentLevelNodes)
                    {
                        DrawConnection(prevNode.NodeObject, nextNode.NodeObject);
                    }
                }
                else if (level % bossInterval == 0)
                {
                    // 현재 레벨에서 보스 노드가 있는지 확인
                    if (currentLevelNodes.Count > 0)
                    {
                        MapNode bossNode = currentLevelNodes[0];
                        DrawConnection(prevNode.NodeObject, bossNode.NodeObject);
                    }
                }
                else
                {
                    // 일반 레벨의 노드 연결
                    int prevIndex = previousLevelNodes.IndexOf(prevNode);
                    if (prevIndex >= 0 && prevIndex < currentLevelNodes.Count)
                    {
                        int minY = Mathf.Max(0, prevIndex - 1); // Y ��??�� -1
                        int maxY = Mathf.Min(currentLevelNodes.Count - 1, prevIndex + 1); // Y ��??�� +1

                        // 유효한 범위 내에서 노드 선택
                        if (minY <= maxY)
                        {
                            MapNode nextNode = currentLevelNodes[Random.Range(minY, maxY + 1)];
                            DrawConnection(prevNode.NodeObject, nextNode.NodeObject);
                        }
                    }
                }
            }
        }
    }

    void AdjustContentSizeAndPosition() // contents크기 설정 및 위치조정
    {
        RectTransform contentRect = scrollViewContent.GetComponent<RectTransform>();

        float contentWidth = (totalLevels + 1) * 250.0f; 
        contentRect.sizeDelta = new Vector2(contentWidth, contentRect.sizeDelta.y);

        contentRect.localPosition = new Vector2(0, 0);
    }
}