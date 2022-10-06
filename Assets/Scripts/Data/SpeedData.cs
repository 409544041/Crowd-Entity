using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SpeedData : IComponentData
{
    public float speed;   
}
