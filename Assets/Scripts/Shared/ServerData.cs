using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains data for each player that's modified by server side.
/// </summary>
public class PlayerServerData
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

/// <summary>
/// Contains entire of the server side data that's sent by server to clients.
/// </summary>
public class ServerData
{
    public List<PlayerServerData> PlayerDataList = new List<PlayerServerData>();

    public byte[] ToByteArray()
    {
        // Each PlayerData = 16 bytes
        int count = PlayerDataList.Count;
        int totalSize = 4 + count * 16; // 4 bytes for count + player data
        byte[] result = new byte[totalSize];

        // Copy player count first
        Buffer.BlockCopy(BitConverter.GetBytes(count), 0, result, 0, 4);

        // Copy each PlayerData
        for (int i = 0; i < count; i++)
        {
            byte[] playerBytes = PlayerDataList[i].ToByteArray();
            Buffer.BlockCopy(playerBytes, 0, result, 4 + i * 16, 16);
        }

        return result;
    }

    public void FromByteArray(byte[] data)
    {
        PlayerDataList = new List<PlayerServerData>();

        // Read player count
        int count = BitConverter.ToInt32(data, 0);

        // Read each PlayerData
        for (int i = 0; i < count; i++)
        {
            byte[] playerBytes = new byte[16];
            Buffer.BlockCopy(data, 4 + i * 16, playerBytes, 0, 16);

            PlayerServerData pd = new PlayerServerData();
            pd.FromByteArray(playerBytes);

            PlayerDataList.Add(pd);
        }
    }
}

