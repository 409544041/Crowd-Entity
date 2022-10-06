using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct AvoidanceData : IComponentData
{
    public float3 Velocity;
    public float distance;
    public float baseDistance;
    public bool isAvoidanceRun;
}
