using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class GMStartButton : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(ButtonClick);
    }

    public void ButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // 获取所有拥有组件的实体
        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(RollBall));
        NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);

        // 遍历每个实体，并获取其组件
        foreach (Entity entity in entities)
        {
            var tdata = entityManager.GetComponentData<RollBall>(entity);
            //+1球数
            tdata.maxBallCount += 1;
            entityManager.SetComponentData(entity, tdata);
        }

        entities.Dispose();
        entityQuery.Dispose();

        gameObject.SetActive(false);
    }
}
