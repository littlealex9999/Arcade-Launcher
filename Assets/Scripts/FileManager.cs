using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FileManager
{
    #region Helpers
    static BinaryReader ReadFile(string path, string filename, out FileStream stream)
    {
        return ReadFile(path + "/" + filename, out stream);
    }

    static BinaryReader ReadFile(string path, out FileStream stream)
    {
        if (File.Exists(path)) {
#if UNITY_EDITOR
            Debug.Log("Reading a file at: \"" + path + "\"");
#endif

            stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return new BinaryReader(stream);
        } else {
#if UNITY_EDITOR
            Debug.LogWarning("Failed to read a file at: \"" + path + "\"");
#endif

            stream = null;
            return null;
        }
    }

    /// <summary>
    /// Creates a new file at the location. Creates any directories that do not exist on the way.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    static BinaryWriter WriteFile(string path, string filename, out FileStream stream)
    {
        Directory.CreateDirectory(path);

        return WriteFile(path + "/" + filename, out stream);
    }

    static BinaryWriter WriteFile(string path, out FileStream stream)
    {
#if UNITY_EDITOR
        Debug.Log("Writing a file at: \"" + path + "\"");
#endif

        if (File.Exists(path)) {
#if UNITY_EDITOR
            Debug.Log("Found a file at: \"" + path + "\". Deleting it and creating a new file");
#endif

            File.Delete(path);
        }

        stream = File.Open(path, FileMode.CreateNew, FileAccess.Write);
        return new BinaryWriter(stream);
    }

    static void CloseFile(BinaryReader reader, FileStream stream)
    {
        reader.Close();
        stream.Close();
    }

    static void CloseFile(BinaryWriter writer, FileStream stream)
    {
        writer.Close();
        stream.Close();
    }

    public static void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public static string[] FileDialog()
    {
        return StandaloneFileBrowser.OpenFilePanel("Open File", "", "exe", false);
    }

    public static string[] FileDialog(ExtensionFilter[] extensions, bool multipleSelection = false)
    {
        return StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, multipleSelection);
    }

    public static bool CheckIfFileExists(string path)
    {
        return File.Exists(path);
    }
    #endregion

    #region String Lists
    public static List<string> ReadStringList(string path, string filename)
    {
        List<string> list = new List<string>();

        BinaryReader reader = ReadFile(path, filename, out FileStream stream);
        if (reader == null) return null;

        int listCount = reader.ReadInt32();

        for (int i = 0; i < listCount; ++i) {
            list.Add(reader.ReadString());
        }

        CloseFile(reader, stream);
        return list;
    }

    public static void WriteStringList(string path, string filename, List<string> list)
    {
        BinaryWriter writer = WriteFile(path, filename, out FileStream stream);

        writer.Write(list.Count);

        for (int i = 0; i < list.Count; ++i) {
            writer.Write(list[i]);
        }

        CloseFile(writer, stream);
    }
    #endregion

    #region GameData
    public static GameData ReadGameData(string path)
    {
        GameData data = new GameData();

        BinaryReader reader = ReadFile(path, out FileStream stream);
        if (reader == null) return null;

        data.exePath = reader.ReadString();
        data.gameTitle = reader.ReadString();
        data.gameDescription = reader.ReadString();

        CloseFile(reader, stream);
        return data;
    }

    public static void WriteGameData(string path, string filename, GameData data)
    {
        BinaryWriter writer = WriteFile(path, filename, out FileStream stream);

        writer.Write(data.exePath);
        writer.Write(data.gameTitle);
        writer.Write(data.gameDescription);

        CloseFile(writer, stream);
    }
    #endregion

    #region Texture
    public static Texture2D ReadTexture(string path)
    {
        Texture2D tex = null;
        byte[] data;

        if (File.Exists(path)) {
            data = File.ReadAllBytes(path);
            tex = new Texture2D(1, 1);
            tex.LoadImage(data);
        }

        return tex;
    }

    public static Texture2D ReadTexture(string path, string filename)
    {
        return ReadTexture(path + "/" + filename);
    }

    public static void WriteTexture(string path, string filename, Texture tex)
    {
        File.WriteAllBytes(path + "/" + filename, ImageConversion.EncodeToPNG((Texture2D)tex));
    }

    public static void WriteTexture(string path, string filename, Texture2D tex)
    {
        File.WriteAllBytes(path + "/" + filename, ImageConversion.EncodeToPNG(tex));
    }
    #endregion
}
