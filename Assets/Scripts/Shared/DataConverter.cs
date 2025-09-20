using System;
using Unity.Collections;
using UnityEngine;

public static class DataConverter
{
    public static byte[] Vector3ToBytes(Vector3 v)
    {
        byte[] result = new byte[12];
        Buffer.BlockCopy(BitConverter.GetBytes(v.x), 0, result, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(v.y), 0, result, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(v.z), 0, result, 8, 4);
        return result;
    }

    public static byte[] Vector2ToBytes(Vector2 v)
    {
        byte[] result = new byte[8];
        Buffer.BlockCopy(BitConverter.GetBytes(v.x), 0, result, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(v.y), 0, result, 4, 4);
        return result;
    }

    public static Vector2 BytesToVector2(byte[] data, int startIdx)
    {
        float x = BitConverter.ToSingle(data, startIdx);
        float y = BitConverter.ToSingle(data, startIdx + 4);
        return new Vector2(x, y);
    }

    public static Vector3 BytesToVector3(byte[] data, int startIdx)
    {
        float x = BitConverter.ToSingle(data, startIdx);
        float y = BitConverter.ToSingle(data, startIdx + 4);
        float z = BitConverter.ToSingle(data, startIdx + 8);
        return new Vector3(x, y, z);
    }

    public static byte[] StreamDataToByteList(DataStreamReader stream)
    {
        byte[] inData = new byte[stream.Length];
        for (int x = 0; x < stream.Length; x++)
        {
            inData[x] = stream.ReadByte();
        }
        return inData;
    }
}