using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct AvoidanceDatasEnemy
{
    public SpeedData speedData;
    public GraphData graphData;
}

[UpdateInGroup(typeof(AIGroup))]
[UpdateAfter(typeof(AIGraphMove))]
public partial class AvoidanceSystem : SystemBase
{
    [BurstCompile]
    public partial struct GetAIEntity: IJobEntity
    {
        public NativeList<Translation>.ParallelWriter translations;
        public NativeList<AvoidanceDatasEnemy>.ParallelWriter avoidanceDatasEnemy;
        void Execute(in Translation translation, in SpeedData speedData, in GraphData graphData)
        {
            translations.AddNoResize(translation);

            avoidanceDatasEnemy.AddNoResize(new AvoidanceDatasEnemy { graphData = graphData, speedData = speedData });
        }
    }

    protected override void OnUpdate()
    {
        NativeList<Translation> translations = new NativeList<Translation>(300,Allocator.TempJob);
        NativeList<AvoidanceDatasEnemy> avoidanceDatasEnemy = new NativeList<AvoidanceDatasEnemy>(300, Allocator.TempJob);

        EntityQuery entityQuery= GetEntityQuery(typeof(GraphTag));

        JobHandle jh = new GetAIEntity { translations = translations.AsParallelWriter(), avoidanceDatasEnemy = avoidanceDatasEnemy.AsParallelWriter() }.ScheduleParallel();
        jh.Complete();
        

        var translationParallel = translations.AsParallelReader();
        var avoidanceDatasEnemyParallel = avoidanceDatasEnemy.AsParallelReader();

        Entities.WithAll<GraphTag>().ForEach((Entity entity, ref GraphData graphData, ref PhysicsVelocity physicsVelocity, ref AvoidanceData avoidanceData
                , in Translation translation, in SpeedData speedData) =>
        {
            for (int i = 0; i < translationParallel.Length; i++)
            {
                if (graphData.graphIndex != avoidanceDatasEnemyParallel[i].graphData.graphIndex) continue;

                var equals = (translation.Value == translationParallel[i].Value);
                if (equals.x && equals.y && equals.z) continue;

                if (math.distance(translationParallel[i].Value, translation.Value) <= avoidanceData.distance)
                {
                    var angle = math.dot(math.normalize(physicsVelocity.Linear), math.normalize(translationParallel[i].Value - translation.Value));
                    //Debug.Log("menizle girdim açým : " + angel + " Ben: " + entity.Index);
                    if (angle > -0.3f && avoidanceDatasEnemyParallel[i].speedData.speed < speedData.speed)
                    {
                        if (!avoidanceData.isAvoidanceRun)
                        {
                            avoidanceData.isAvoidanceRun = true;
                            avoidanceData.distance = 4f;
                            avoidanceData.Velocity = physicsVelocity.Linear;
                            //Debug.Log("Avoidance sistemi baþladý Ben :" + entity.Index);

                        }
                    }
                    else
                    {
                        if (avoidanceData.isAvoidanceRun)
                        {
                            avoidanceData.isAvoidanceRun = false;
                            avoidanceData.distance = avoidanceData.baseDistance;
                            avoidanceData.Velocity = float3.zero;
                            //Debug.Log("Avoidance sistemi bitti Ben :" + entity.Index);

                        }
                    }
                    if (avoidanceData.isAvoidanceRun)
                    {
                        var dir = math.normalize(physicsVelocity.Linear.xz);
                        float3 dir3 = float3.zero;
                        dir3.xz = dir;
                        dir3.y = 0;
                        dir3 = Utils.Rotate(dir3, angle* 2);
                        physicsVelocity.Linear = dir3 * speedData.speed;

                    }
                }
            }

        }).WithBurst().ScheduleParallel();
        Dependency.Complete();
        translations.Dispose();
        avoidanceDatasEnemy.Dispose();
    }
}
