using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[GenerateAuthoringComponent]
[Serializable]
public struct AdjBuffer : IBufferElementData
{
    public int direction;
}
