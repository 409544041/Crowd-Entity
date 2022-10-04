using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[UpdateInGroup(typeof(AIGroup))]
public partial class AvoidanceSystem : SystemBase
{

    public partial struct GetAIEntity: IJobEntity
    {
        public NativeList<Translation>.ParallelWriter translations;
        void Execute(in Translation translation)
        {
            translations.AddNoResize(translation);
            
        }
    }

    protected override void OnUpdate()
    {
        NativeList<Translation> translations = new NativeList<Translation>(300,Allocator.TempJob);

        EntityQuery entityQuery= GetEntityQuery(typeof(GraphTag));

        JobHandle jh = new GetAIEntity { translations = translations.AsParallelWriter() }.ScheduleParallel();
        jh.Complete();
        

        var translationParallel = translations.AsParallelReader();
        

        Entities.WithAll<GraphTag>().ForEach((in Translation translation) => {
            for (int i = 0; i < translationParallel.Length; i++)
            {
                var equals = (translation.Value == translationParallel[i].Value);
                if (equals.x && equals.y && equals.z ) return;
                
                if( math.distance(translationParallel[i].Value,translation.Value) <= 2f)
                {
                    Debug.Log("Yakýnýmda biri var");
                }
            }
            
        }).Schedule();
        Dependency.Complete();
        translations.Dispose();
    }
}
