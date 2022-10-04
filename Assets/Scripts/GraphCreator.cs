using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



public class GraphCreator : MonoBehaviour
{
    public static GraphCreator instance;

    [SerializeField] public List<VertexBuffer> vertexDatas;

    [SerializeField] public List<Utils.ListWrapper<AdjBuffer>> adjBuffers;

    public GameObject AIPrefab;
    public Entity EntityAI;
    
    EntityManager entityManager;
    BlobAssetStore blobAsset;

    [SerializeField] bool spawner;

    void Start()
    {

        StartCoroutine(Spawner());
    }

    private void Awake()
    {
        instance = this;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAsset = new BlobAssetStore();
        GameObjectConversionSettings gameObjectConversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAsset);
        EntityAI = GameObjectConversionUtility.ConvertGameObjectHierarchy(AIPrefab, gameObjectConversionSettings);
        
    }

    void Update()
    {
        
    }

    IEnumerator Spawner()
    {
        if (!spawner)
        {
            CreateEntityAI(0);
        }
        while (spawner)
        {
            CreateEntityAI(UnityEngine.Random.Range(0, adjBuffers[0].myList.Length - 1));
            yield return new WaitForSeconds(0.5f);
        }
    }

    void CreateEntityAI(int index)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var entity = ecb.Instantiate(EntityAI);
        ecb.AddComponent(entity, new GraphData { graphIndex = index });
        ecb.AddComponent(entity, new GraphTag { });
        ecb.SetComponent<Translation>(entity, new Translation { Value = vertexDatas[adjBuffers[0].myList[index].direction].translation });
        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    private void OnDestroy()
    {
        blobAsset.Dispose();
    }

    private void OnDrawGizmos()
    {
        foreach (var items in adjBuffers)
        {
            for (int i = 1; i < items.myList.Length; i++)
            {
                float3 vector =  -vertexDatas[items.myList[i].direction].translation + vertexDatas[items.myList[i - 1].direction].translation;
                Gizmos.DrawLine(vertexDatas[items.myList[i - 1].direction].translation, vertexDatas[items.myList[i].direction].translation);
                Gizmos.DrawLine(math.normalize(Utils.Rotate(in vector, 30)) + vertexDatas[items.myList[i].direction].translation, vertexDatas[items.myList[i].direction].translation);
                Gizmos.DrawLine(math.normalize(Utils.Rotate(in vector, -30)) + vertexDatas[items.myList[i].direction].translation, vertexDatas[items.myList[i].direction].translation);
            }
        }
        
    }

    
}
