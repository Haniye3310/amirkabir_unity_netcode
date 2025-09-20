using System;
using UnityEngine;

public struct ServerData
{
    public int PlayerID; //4 bytes
    public Vector3 PlayerPosition; //12 bytes

    public byte[] ToByteArray()
    {
        byte[] result = new byte[4 + 12];

        Buffer.BlockCopy(BitConverter.GetBytes(PlayerID), 0, result, 0, 4);
        Buffer.BlockCopy(DataConverter.Vector3ToBytes(PlayerPosition), 0, result, 4, 12);

        return result;
    }
    public void FromByteArray(byte[] data)
    {
        // Read PlayerID (first 4 bytes)
        PlayerID = BitConverter.ToInt32(data, 0);

        // Read InputDirection (next 8 bytes: X and Y as floats)
        PlayerPosition = DataConverter.BytesToVector3(data, 4);
    }
}
