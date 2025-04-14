using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;
    private Vector2 selectionStartMousePosition;
    private Vector2 selectionEndMousePosition;

    private void Awake()
    {
        Instance = this;
        
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
            
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Unit, LocalTransform>().WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);
            
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entities[i], false);
            }
            
            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width * selectionAreaRect.height;
            bool isMultipleSelection = selectionAreaSize > 40f;
            
            if (isMultipleSelection)
            {
                SelectUnitsInArea(selectionAreaRect);
            }
            else
            {
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(selectionEndMousePosition);
                uint unitLayerMask = 1u << LayerMask.NameToLayer("Unit");
                
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = ray.origin,
                    End = ray.GetPoint(100f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = unitLayerMask,
                        GroupIndex = 0
                    }
                };
                
                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    Entity entity = raycastHit.Entity;
                    if (entityManager.HasComponent<Unit>(entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(entity, true);
                    }
                }
                
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetMouseWorldPosition();
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);
            
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
    
    private void SelectUnitsInArea(Rect selectionAreaRect)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Unit, LocalTransform>().WithNone<Selected>().Build(entityManager);
        NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<LocalTransform> localTransforms = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            
        for (int i = 0; i < entities.Length; i++)
        {
            LocalTransform localTransform = localTransforms[i];
            Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(localTransform.Position);
            if (selectionAreaRect.Contains(unitScreenPosition))
            {
                entityManager.SetComponentEnabled<Selected>(entities[i], true);
            }
        }
    }
    
    public Rect GetSelectionAreaRect()
    {
        selectionEndMousePosition = Input.mousePosition;
        Vector2 lowerLeftCorner = new Vector2(Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x), Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));
        Vector2 upperRightCorner = new Vector2(Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x), Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y));
        
        return new Rect(lowerLeftCorner, upperRightCorner - lowerLeftCorner);
    }
}
