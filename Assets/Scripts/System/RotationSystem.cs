using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(AIGroup))]
public partial class RotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var delta = Time.DeltaTime;
        Entities.WithAll<FreezeRotationData>()
            .ForEach((ref Rotation rotation, ref PhysicsVelocity physicsVelocity, in FreezeRotationData freezeRotation) =>
            {
                var a = math.atan2(physicsVelocity.Linear.x, physicsVelocity.Linear.z);
                rotation.Value = quaternion.RotateY(a);
            }).WithBurst().ScheduleParallel();
    }
}
