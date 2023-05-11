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

        // ��ȡ����ӵ�������ʵ��
        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(RollBall));
        NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);

        // ����ÿ��ʵ�壬����ȡ�����
        foreach (Entity entity in entities)
        {
            var tdata = entityManager.GetComponentData<RollBall>(entity);
            //+1����
            tdata.maxBallCount += 1;
            entityManager.SetComponentData(entity, tdata);
        }

        entities.Dispose();
        entityQuery.Dispose();

        gameObject.SetActive(false);
    }
}
