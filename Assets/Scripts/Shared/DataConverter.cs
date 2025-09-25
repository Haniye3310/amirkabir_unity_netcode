using System;
using Unity.Collections;
using UnityEngine;

public static class DataConverter
{
    public static void WriteVector2(ref DataStreamWriter writer, Vector2 v)
    {
        writer.WriteFloat(v.x);
        writer.WriteFloat(v.y);
    }

    public static Vector2 ReadVector2(ref DataStreamReader reader)
    {
        float x = reader.ReadFloat();
        float y = reader.ReadFloat();
        return new Vector2(x, y);
    }

    public static void WriteVector3(ref DataStreamWriter writer, Vector3 v)
    {
        writer.WriteFloat(v.x);
        writer.WriteFloat(v.y);
        writer.WriteFloat(v.z);
    }

    public static Vector3 ReadVector3(ref DataStreamReader reader)
    {
        float x = reader.ReadFloat();
        float y = reader.ReadFloat();
        float z = reader.ReadFloat();
        return new Vector3(x, y, z);
    }
}