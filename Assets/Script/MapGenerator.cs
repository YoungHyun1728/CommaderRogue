using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int totalLevels = 100;
    public int nodesPerLevel = 5;
    public int bossInterval = 10;  // 10 �������� ���� ���

    public GameObject combatPrefab;
    public GameObject restPrefab;
    public GameObject bossPrefab;
    public GameObject eventPrefab;
    public GameObject tradePrefab;
    public GameObject linePrefab;  // ���� �׸� �� ����� LineRenderer ������

    private float combatProbability = 0.55f;  // ���� ��� Ȯ�� 
    private float eventProbability = 0.25f;   // �̺�Ʈ ��� Ȯ��
    private float restProbability = 0.1f;    // �޽� ��� Ȯ�� 
    private float tradeProbability = 0.1f;   // �ŷ� ��� Ȯ�� 

    private List<List<MapNode>> mapLevels;

    public GameObject scrollViewContent;  // Scroll View�� Content

    void Start()
    {
        GenerateNodes();
        CreateConnections();
        AdjustContentSize();
    }

    NodeType GetRandomNodeTypeByProbability(NodeType[] nodeTypes, float[] probabilities)
    {
        float randomValue = Random.value;  // ���� 0~1 ��ȯ��Ų��.
        float cumulativeProbability = 0f;  // Ȯ�� ������ ����

        for (int i = 0; i < nodeTypes.Length; i++)
        {
            // �������� ��ȯ�� value�� cumulativeProbability�� ���ؼ� ��� ��ȯ
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
        float randomValue = Random.value;  // 0~1 ���� �� ��ȯ

        if (level == 0)                    //ù������ �׻� ����
        {
            return NodeType.Combat;
        }

        if (level % bossInterval == 0) // ���� 10���� ������
        {
            return NodeType.Boss;
        }

        if (level < 4)  // ���� �������� ���ʿ��� �޽�, �ŷ��� ������ �ʰ� ���� 5���� ������ ����
        {
            NodeType[] nodeTypes = { NodeType.Combat, NodeType.Event };
            float[] probabilities = { combatProbability, eventProbability };

            return GetRandomNodeTypeByProbability(nodeTypes, probabilities);
        }

        // ���� ��尡 �޽��̰ų� �ŷ��� ��� �������� ������ �ʰ� ����
        // ���Ḹ �ȵǸ� �Ǵ� ������ �����ؾ� �ҰŰ���
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
                return combatPrefab; // �⺻������ �������
        }
    }

    void DrawConnection(GameObject startNode, GameObject endNode)
    {
        // �� ������Ʈ�� Content�� �ڽ����� ����
        GameObject lineObject = Instantiate(linePrefab, scrollViewContent.transform);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false; // ���� ��ǥ�� ���

        // LineRenderer�� ��ġ�� �θ�(Content)�� ��ǥ�迡 ����
        lineObject.transform.localPosition = Vector3.zero;

        // ����� RectTransform ��������
        RectTransform startRect = startNode.GetComponent<RectTransform>();
        RectTransform endRect = endNode.GetComponent<RectTransform>();

        if (startRect == null || endRect == null)
        {
            Debug.LogError("��忡 RectTransform�� �����ϴ�.");
            return;
        }

        // ����� ���� ��ġ ��������
        Vector3 startPos = startRect.localPosition;
        Vector3 endPos = endRect.localPosition;

        // LineRenderer ����
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // ���� ���� ���� ���� (���� ����)
        lineRenderer.sortingOrder = -1;

        // ���� ���� ���� (�ʿ信 ����)
        MapNode startMapNode = startNode.GetComponent<MapNode>();
        MapNode endMapNode = endNode.GetComponent<MapNode>();

        if (startMapNode != null)
        {
            startMapNode.ConnectTo(endMapNode, null); // ���� �߰�
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

    // ������� ���� ��常 �����ϴ� �Լ�
    void RemoveUnusedNodes()
    {
        for (int level = 1; level <= totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = mapLevels[level];

            foreach (MapNode node in currentLevelNodes)
            {
                // prevNodePrefab�� null�̸� ����
                if (node.prevNodePrefab.Count == 0)
                {
                    Destroy(node.NodeObject);
                }

                if (node.prevNodePrefab.Count == 1)
                {
                    if (node.prevNodePrefab[0] = null)
                        Destroy(node.NodeObject);
                }
            }
        }
    }

    void GenerateNodes()
    {
        mapLevels = new List<List<MapNode>>();
        float iconIntervalX = 250.0f;  
        float iconIntervalY = 175.0f; 

        for (int level = 0; level <= totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = new List<MapNode>();

            for (int nodeIndex = 0; nodeIndex < nodesPerLevel; nodeIndex++)
            {
                NodeType nodeType = GetRandomNodeType(level);
                GameObject nodePrefab = GetPrefabForNodeType(nodeType);

                // ��带 ���η� ��ġ
                Vector3 nodePosition = new Vector3(level * iconIntervalX, nodeIndex * -iconIntervalY, 0);
                GameObject nodeObject = Instantiate(nodePrefab, nodePosition, Quaternion.identity, scrollViewContent.transform);
                nodeObject.name = $"Node_{level}_{nodeIndex}"; // ��� �̸� ����

                // ���� ��ǥ ���� (��ũ�Ѻ��� �ڽ��� ���� ���� ��ǥ�� ����)
                nodeObject.transform.localPosition = nodePosition;

                MapNode mapNode = nodeObject.GetComponent<MapNode>();
                if (mapNode == null)
                {
                    mapNode = nodeObject.AddComponent<MapNode>();
                }
                mapNode.Initialize(nodeType, level, nodeObject);

                currentLevelNodes.Add(mapNode);
            }

            // ���� ������ ���, 3��° ��� ������ ������ 4�� ����
            if (level > 0 && level % bossInterval == 0)
            {
                for (int nodeIndex = 0; nodeIndex < currentLevelNodes.Count; nodeIndex++)
                {
                    if (nodeIndex != 2) // 3��° ��带 �����ϰ� ������ ����
                    {
                        Destroy(currentLevelNodes[nodeIndex].NodeObject);
                    }
                }
                // 3��° ��常 ����� currentLevelNodes�� ���� (����Ʈ ����)
                List<MapNode> bossNodeList = new List<MapNode> { currentLevelNodes[2] };

                // mapLevels�� ���ŵ� ��� ����Ʈ �߰�
                mapLevels.Add(bossNodeList);

                // currentLevelNodes�� ���ο� ���� ��� ����Ʈ�� ��ü
                currentLevelNodes = bossNodeList;
            }
            else
            {
                mapLevels.Add(currentLevelNodes);
            }
            
        }
    }

    void CreateConnections()
    {
        for (int level = 1; level <= totalLevels; level++) // level 1���� ����
        {
            List<MapNode> currentLevelNodes = mapLevels[level];
            List<MapNode> previousLevelNodes = mapLevels[level - 1];

            foreach (MapNode prevNode in previousLevelNodes)
            {
                // ���� �����̸� ���� ���(3��° ���)�� ���� ������ ��� ��忡 ����
                if (prevNode.Type == NodeType.Boss)
                {
                    foreach (MapNode nextNode in currentLevelNodes)
                    {
                        DrawConnection(prevNode.NodeObject, nextNode.NodeObject);
                    }
                }
                else if(level % bossInterval == 0)
                {
                    // ���� �������� ���� ��尡 �ִ��� Ȯ��
                    if (currentLevelNodes.Count > 0)
                    {
                        MapNode bossNode = currentLevelNodes[0]; // ���� ������ ������ ���� ���
                        DrawConnection(prevNode.NodeObject, bossNode.NodeObject);
                    }
                }
                else
                {
                    // �Ϲ� ������ ��� ����
                    int prevIndex = previousLevelNodes.IndexOf(prevNode);
                    if (prevIndex >= 0 && prevIndex < currentLevelNodes.Count)
                    {
                        int minY = Mathf.Max(0, prevIndex - 1); // Y ���� -1
                        int maxY = Mathf.Min(currentLevelNodes.Count - 1, prevIndex + 1); // Y ���� +1

                        // ��ȿ�� ���� ������ ��� ����
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

    void AdjustContentSize()
    {
        RectTransform contentRect = scrollViewContent.GetComponent<RectTransform>();

        // �ִ� X ��ǥ�� ����Ͽ� Content�� Width�� ����
        float contentWidth = (totalLevels + 1) * 250.0f; // iconIntervalX�� ��� ���� ����

        contentRect.sizeDelta = new Vector2(contentWidth, contentRect.sizeDelta.y);
    }
}