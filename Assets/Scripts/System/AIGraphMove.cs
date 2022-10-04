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
        var bufferReadOnly = buffer.AsReadOnly();

        var bufferVertex = GetVertexBuffer();
        var bufferVertexReadOnly = bufferVertex.AsReadOnly();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var entityCommandBufferParallel = entityCommandBuffer.AsParallelWriter();
        Entities
            .WithAll<GraphTag>()
            .ForEach((Entity entity,int entityInQueryIndex, ref GraphData graphData, ref PhysicsVelocity physicsVelocity, in Translation translation) =>
            {
                var distance = math.distance(bufferVertexReadOnly[graphData.graphIndex].translation.xz, translation.Value.xz);
                if (distance <= 0.1f)
                {
                    if (graphData.graphIndex + 1 > bufferReadOnly.Length)
                    {
                        entityCommandBufferParallel.RemoveComponent<GraphTag>(entityInQueryIndex, entity);
                        physicsVelocity.Linear = float3.zero;
                        physicsVelocity.Angular = float3.zero;
                        return;
                    }
                     graphData.graphIndex = bufferReadOnly[graphData.graphIndex + 1].direction;
                    
                }

                    var dir = bufferVertexReadOnly[graphData.graphIndex ].translation - translation.Value;
                    dir.y = 0;
                    dir = math.normalize(dir);
                    var y = physicsVelocity.Linear.y;
                    dir *= 4f;
                    dir = math.lerp(physicsVelocity.Linear, dir,0.1f);
                    physicsVelocity.Linear = dir;
                    physicsVelocity.Linear.y = y;
                    
                
                
                
            }).WithoutBurst().ScheduleParallel();

        Dependency.Complete();
        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
        buffer.Dispose();
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
