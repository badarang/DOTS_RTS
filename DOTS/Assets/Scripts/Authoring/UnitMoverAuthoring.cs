using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;
    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
                targetPosition = new float3(0, 0, 0)
            });
        }
    }
}

public struct UnitMover : IComponentData
{
    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;
}
