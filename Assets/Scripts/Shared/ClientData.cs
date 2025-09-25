using System;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Contains those data that's sent from client to server.
/// </summary>
public struct ClientData
{
    public int PlayerID;
    public Vector2 InputDirection;

    public void WriteToStream(ref DataStreamWriter writer)
    {
        writer.WriteInt(PlayerID);
        writer.WriteFloat(InputDirection.x);
        writer.WriteFloat(InputDirection.y);
    }

    public void ReadFromStream(ref DataStreamReader reader)
    {
        PlayerID = reader.ReadInt();
        InputDirection = new Vector2(reader.ReadFloat(), reader.ReadFloat());
    }

}