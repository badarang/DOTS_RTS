using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetMouseWorldPosition();
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover>().Build(entityManager);
            
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMovers = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            // for (int i = 0; i < unitMovers.Length; i++)
            // {
            //     UnitMover unitMover = unitMovers[i];
            //     unitMover.targetPosition = mouseWorldPosition;
            //     entityManager.SetComponentData(entities[i], unitMover);
            // }
            
            for (int i = 0; i < entities.Length; i++)
            {
                UnitMover unitMover = unitMovers[i];
                unitMover.targetPosition = mouseWorldPosition;
                unitMovers[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMovers);
        }
    }
}
