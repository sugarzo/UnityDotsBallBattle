using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Entities;

public class MapCreator : MonoBehaviour
{
    public GameObject gridPrefab;

    public float xCount = 100;
    public float zCount = 100;
    [Space]
    public float xGap = 0.2f;
    public float zGap = 0.2f;

    public void GenerationMap()
    {
#if UNITY_EDITOR
        GameObject parent = new GameObject("棋盘");
        parent.transform.SetParent(transform);
        for(int i = 0;i<xCount;i++)
        {
            for(int j = 0;j<zCount;j++)
            {
                var targetPos = transform.position + new Vector3(xGap * i,0 , zGap * j);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(gridPrefab, parent.transform);
                instance.transform.position = targetPos;
            }
        }
#endif
    }
    public void Clear()
    {
        // 遍历所有子物品，并销毁它们
        for (int i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(MapCreator))]
public class MapCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("生成地图"))
        {
            (target as MapCreator)?.GenerationMap();
        }
        if (GUILayout.Button("Clear All Grid"))
        {
            (target as MapCreator)?.Clear();
        }
    }
}
#endif