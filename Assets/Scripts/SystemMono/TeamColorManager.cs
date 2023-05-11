using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using System.Collections.Generic;
using TMPro;

public class TeamColorManager : MonoBehaviour
{
    public static TeamColorManager instance;

    public List<TextMeshPro> textMeshPros;

    private Dictionary<Entity, TextMeshPro> runtimeUI = new Dictionary<Entity, TextMeshPro>();



    private void Awake()
    {
        instance = this;
    }
    /// <summary>
    /// 绑定炮弹的显示屏位置
    /// </summary>
    /// <param name="entityPos"></param>
    /// <param name=""></param>
    public void BlindUIData(Vector3 entityPos,Entity entity)
    {
        float minDistance = float.MaxValue;
        TextMeshPro target = textMeshPros[0];
        //找到距离最近的显示屏数据
        foreach(var tmp in textMeshPros)
        {
            if(Vector3.Distance(tmp.transform.position,entityPos) < minDistance)
            {
                target = tmp;
                minDistance = Vector3.Distance(tmp.transform.position, entityPos);
            }
        }
        //绑定
        runtimeUI[entity] = target;
    }
    /// <summary>
    /// System调用，传入数据更新UI
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="countData"></param>
    public void ShowTextData(Entity entity,int countData)
    {
        if (runtimeUI.TryGetValue(entity, out TextMeshPro textMeshPro))
            textMeshPro.text = countData.ToString();
    }
}