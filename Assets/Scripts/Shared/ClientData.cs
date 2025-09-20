using System;
using UnityEngine;

public class ClientData
{
    public int PlayerID;
    public Vector2 InputDirection;

    public byte[] ToByteArray()
    {
        byte[] result = new byte[4 + 8];

        Buffer.BlockCopy(BitConverter.GetBytes(PlayerID), 0, result, 0, 4);
        Buffer.BlockCopy(DataConverter.Vector2ToBytes(InputDirection), 0, result, 4, 8);

        return result;
    }
    public void FromByteArray(byte[] data)
    {
        // Read PlayerID (first 4 bytes)
        PlayerID = BitConverter.ToInt32(data, 0);

        // Read InputDirection (next 8 bytes: X and Y as floats)
        InputDirection = DataConverter.BytesToVector2(data, 4);
    }

}