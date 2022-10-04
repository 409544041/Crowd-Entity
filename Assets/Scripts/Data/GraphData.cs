using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct GraphData : IComponentData
{
    public int graphIndex;
}
