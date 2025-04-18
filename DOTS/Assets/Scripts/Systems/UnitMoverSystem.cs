using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        unitMoverJob.ScheduleParallel();
        // foreach ((RefRW<LocalTransform> localTransform, RefRO<UnitMover> unitMover, RefRW<PhysicsVelocity> physicsVelocity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<UnitMover>, RefRW<PhysicsVelocity>>())
        // {
        //     float3 targetPosition = unitMover.ValueRO.targetPosition;
        //     float3 moveDirection = math.normalize(targetPosition - localTransform.ValueRO.Position);
        //
        //     localTransform.ValueRW.Rotation = math.slerp(
        //         localTransform.ValueRO.Rotation,
        //         quaternion.LookRotation(moveDirection, math.up()),
        //         SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed
        //     );
        //     
        //     physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
        //     physicsVelocity.ValueRW.Angular = float3.zero;
        // }
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    public void Execute(
        ref LocalTransform localTransform,
        in UnitMover unitMover,
        ref PhysicsVelocity physicsVelocity
    )
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachedTargetDistanceSq = 2f;
        //기존 length는 거리를 구하는 함수인데, lengthsq는 제곱을 구하는 함수여서 연산량이 적다.
        if (math.lengthsq(moveDirection) < reachedTargetDistanceSq)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        
        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * unitMover.rotationSpeed
        );
        
        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}