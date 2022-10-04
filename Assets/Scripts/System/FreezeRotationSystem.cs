using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;

public partial class FreezeRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<FreezeRotationData>()
            .ForEach((ref Rotation rotation,ref PhysicsVelocity physicsVelocity,in FreezeRotationData freezeRotation) =>
        {
            rotation.Value = new float4 { xyz = float3.zero, w = 1f };
            physicsVelocity.Angular = float3.zero;
        }).WithBurst().ScheduleParallel();
    }
}
