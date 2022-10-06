using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Utils 
{
    public static float3 Rotate(in float3 pos, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = pos.x;
        float ty = pos.z;
        float3 tempPos;
        tempPos.x = (cos * tx) - (sin * ty);
        tempPos.y = pos.y;
        tempPos.z = (sin * tx) + (cos * ty);
        return tempPos;
    }

    [System.Serializable]
    public struct ListWrapper<T>
    {
        public T[] myList;
    }
    public static float Scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }
}
