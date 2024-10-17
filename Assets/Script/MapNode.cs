using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum NodeType
{
    Combat,  // 각각 등장확률은 MapGenerator.cs
    Rest,    
    Boss,    // 보스    10레벨에 보스,이후 10레벨마다 보스
    Event,   
    Trade,   
}

public class MapNode : MonoBehaviour
{
    public NodeType Type { get; private set; }
    public int Level { get; private set; }  // 트리 레벨 (높이)
    public List<MapNode> Connections { get; private set; }  // 연결된 노드들
    public GameObject NodeObject { get; private set; }
    public List<Line> lines = new List<Line>();
    public List<GameObject> prevNodePrefab = new List<GameObject>();

    private void Update()
    {
        if (Level == 0)  // 레벨 0은 파괴하지 않음
        {
            return;
        }

        if (prevNodePrefab == null || prevNodePrefab.Count == 0)
        {
            Destroy(gameObject); // 리스트가 null이거나 비어있을 때 파괴
        }
        else
        {
            bool allNoneOrMissing = true;  // 모든 노드가 None(또는 Missing)인지 확인하는 변수

            foreach (var prevNode in prevNodePrefab)
            {
                if (prevNode != null)  // null(즉, None 상태)이 아닌 노드가 있으면
                {
                    allNoneOrMissing = false;
                    break;  // 더 이상 체크할 필요 없음
                }
            }

            if (allNoneOrMissing)
            {
                Destroy(gameObject);  // 모든 노드가 None(또는 Missing)일 경우 파괴
            }
        }
    }

    public void Initialize(NodeType type, int level, GameObject nodeObject)
    {
        Type = type;
        Level = level;
        NodeObject = nodeObject;
        Connections = new List<MapNode>();
    }

    public void ConnectTo(MapNode otherNode, Line line)
    {
        Connections.Add(otherNode);
        lines.Add(line);
    }
}
