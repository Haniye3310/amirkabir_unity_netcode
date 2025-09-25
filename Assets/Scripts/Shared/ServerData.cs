using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Contains data for each player that's modified by server side.
/// </summary>
public class PlayerServerData
{
    public int PlayerID;              // 4 bytes
    public Vector3 PlayerPosition;    // 12 bytes

    public void WriteToStream(ref DataStreamWriter writer)
    {
        writer.WriteInt(PlayerID);
        DataConverter.WriteVector3(ref writer, PlayerPosition);
    }

    public void ReadFromStream(DataStreamReader reader)
    {
        PlayerID = reader.ReadInt();
        PlayerPosition = DataConverter.ReadVector3(ref reader);
    }
}

/// <summary>
/// Contains entire of the server side data that's sent by server to clients.
/// </summary>
public class ServerData
{
    public List<PlayerServerData> PlayerDataList = new List<PlayerServerData>();

    public void WriteToStream(ref DataStreamWriter writer)
    {
        // Write player count first
        writer.WriteInt(PlayerDataList.Count);

        // Write each PlayerServerData
        foreach (var playerData in PlayerDataList)
        {
            playerData.WriteToStream(ref writer);
        }
    }

    public void ReadFromStream(ref DataStreamReader reader)
    {
        PlayerDataList = new List<PlayerServerData>();

        // Read player count
        int count = reader.ReadInt();

        // Read each PlayerServerData
        for (int i = 0; i < count; i++)
        {
            PlayerServerData pd = new PlayerServerData();
            pd.ReadFromStream(reader);
            PlayerDataList.Add(pd);
        }
    }
}