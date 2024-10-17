using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public GameObject startNode;
    public GameObject endNode;
    public LineRenderer lineRenderer;

    public Line(GameObject start, GameObject end, LineRenderer renderer)
    {
        startNode = start;
        endNode = end;
        lineRenderer = renderer;
    }

    void Update()
    {
        if (startNode == null || endNode == null)
        {
            // 연결된 노드가 삭제되면 라인을 파괴
            Debug.Log("One of the nodes is null. Destroying the line.");
            Destroy(gameObject); // 현재 라인 오브젝트 파괴
        }
    }

    public void DrawLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startNode.transform.position);
            lineRenderer.SetPosition(1, endNode.transform.position);
        }
    }

    public void DestroyIfInvalidLine()
    {
        // 시작 노드나 끝 노드가 null이면 연결선을 파괴
        if (startNode == null || endNode == null)
        {
            if (lineRenderer != null)
            {
                GameObject.Destroy(lineRenderer.gameObject); // 라인 삭제
            }
        }
    }
}
