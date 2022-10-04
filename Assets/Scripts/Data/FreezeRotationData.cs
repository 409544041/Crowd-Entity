using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct FreezeRotationData : IComponentData
{
    public bool3 flags;
}
