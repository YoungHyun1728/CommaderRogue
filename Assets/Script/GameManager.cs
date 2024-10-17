using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;   // instance�� static���� ������ �������� ��밡��

    void Awake()
    {
        if(instance != null)
        {
            Destroy(instance);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // �� �̵� �Ŀ��� �ı����� ����
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
