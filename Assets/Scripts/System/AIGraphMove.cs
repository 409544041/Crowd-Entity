using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
[UpdateInGroup(typeof(AIGroup))]
public partial class AIGraphMove : SystemBase
{


    protected override void OnUpdate()
    {

        var buffer = GetAdjBuffers(0);
        var bufferReadOnly0 = buffer.AsReadOnly();

        var buffer1 = GetAdjBuffers(1);
        var bufferReadOnly1 = buffer1.AsReadOnly();

        var bufferVertex = GetVertexBuffer();
        var bufferVertexReadOnly = bufferVertex.AsReadOnly();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var entityCommandBufferParallel = entityCommandBuffer.AsParallelWriter();
        Entities
            .WithAll<GraphTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref GraphData graphData, ref PhysicsVelocity physicsVelocity, 
                in Translation translation, in SpeedData speedData, in AvoidanceData avoidanceData) =>
            {
                NativeArray<AdjBuffer>.ReadOnly bufferReadOnly;
                if (graphData.graphIndex == 0)
                {
                    bufferReadOnly = bufferReadOnly0;
                }
                else if (graphData.graphIndex == 1)
                {
                    bufferReadOnly = bufferReadOnly1;
                }
                else
                {
                    bufferReadOnly = bufferReadOnly0;
                }
                var distance = math.distance(bufferVertexReadOnly[bufferReadOnly[graphData.nodeIndex].direction].translation.xz, translation.Value.xz);

                var nextNodeDir = bufferVertexReadOnly[bufferReadOnly[graphData.nodeIndex + 1].direction].translation;
                var currNodeDir = bufferVertexReadOnly[bufferReadOnly[graphData.nodeIndex].direction].translation;

                var tansfromToCurrNode = currNodeDir - translation.Value;

                var I_Dot = math.dot(math.normalize(tansfromToCurrNode), math.normalize(nextNodeDir - currNodeDir));

                if (distance <= (avoidanceData.isAvoidanceRun ? avoidanceData.distance  : 2f) || I_Dot < -0.5f)
                {
                    if (graphData.nodeIndex + 1 > bufferReadOnly.Length)
                    {
                        entityCommandBufferParallel.RemoveComponent<GraphTag>(entityInQueryIndex, entity);
                        physicsVelocity.Linear = float3.zero;
                        physicsVelocity.Angular = float3.zero;
                        return;
                    }
                    // loop
                    if (bufferReadOnly[graphData.nodeIndex + 1].direction == bufferReadOnly[0].direction)
                    {
                        graphData.nodeIndex = 0;
                    }
                    else
                    {
                        ++graphData.nodeIndex;
                    }

                }

                float3 dir = bufferVertexReadOnly[bufferReadOnly[graphData.nodeIndex].direction].translation - translation.Value;
                dir.y = 0;
                dir = math.normalize(dir);
                float y = physicsVelocity.Linear.y;
                dir *= speedData.speed;
                dir = math.lerp(physicsVelocity.Linear, dir, 0.1f);
                physicsVelocity.Linear = dir;
                physicsVelocity.Linear.y = y;




            }).WithBurst().ScheduleParallel();

        Dependency.Complete();
        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
        buffer.Dispose();
        buffer1.Dispose();
        bufferVertex.Dispose();
    }

    static NativeArray<AdjBuffer> GetAdjBuffers(int index)
    {

        int lenght = GraphCreator.instance.adjBuffers[index].myList.Length;
        NativeArray<AdjBuffer> adjBuffers = new NativeArray<AdjBuffer>(lenght, Allocator.TempJob);

        for (int i = 0; i < lenght; i++)
        {
            adjBuffers[i] = GraphCreator.instance.adjBuffers[index].myList[i];
        }

        return adjBuffers;
    }

    static NativeArray<VertexBuffer> GetVertexBuffer()
    {
        int lenght = GraphCreator.instance.vertexDatas.Count;
        NativeArray<VertexBuffer> adjBuffers = new NativeArray<VertexBuffer>(lenght, Allocator.TempJob);

        for (int i = 0; i < lenght; i++)
        {
            adjBuffers[i] = GraphCreator.instance.vertexDatas[i];
        }

        return adjBuffers;
    }
}
