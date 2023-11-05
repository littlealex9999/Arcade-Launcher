using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEditor;
using UnityEngine;

public static class FileManager
{
    #region Helpers
    #region Stream
    static FileStream ReadStream(string path)
    {
        if (File.Exists(path)) {
#if UNITY_EDITOR
            Debug.Log("Reading a file at: \"" + path + "\"");
#endif

            return File.Open(path, FileMode.Open, FileAccess.Read);
        } else {
#if UNITY_EDITOR
            Debug.LogWarning("Failed to read a file at: \"" + path + "\"");
#endif

            return null;
        }
    }

    static FileStream WriteStream(string path)
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

        return File.Open(path, FileMode.CreateNew, FileAccess.Write);
    }
    #endregion

    #region Reading
    static BinaryReader ReadFileBIN(string path, string filename, out FileStream stream)
    {
        return ReadFileBIN(path + "/" + filename, out stream);
    }

    static BinaryReader ReadFileBIN(string path, out FileStream stream)
    {
        stream = ReadStream(path);
        if (stream != null) {
            return new BinaryReader(stream);
        } else {
            return null;
        }
    }

    static StreamReader ReadFileSTR(string path, string filename, out FileStream stream)
    {
        return ReadFileSTR(path + "/" + filename, out stream);
    }

    static StreamReader ReadFileSTR(string path, out FileStream stream)
    {
        stream = ReadStream(path);
        if (stream != null) {
            return new StreamReader(stream);
        } else {
            return null;
        }
    }
    #endregion

    #region Writing
    /// <summary>
    /// Creates a new file at the location. Creates any directories that do not exist on the way.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    static BinaryWriter WriteFileBIN(string path, string filename, out FileStream stream)
    {
        Directory.CreateDirectory(path);
        return WriteFileBIN(path + "/" + filename, out stream);
    }

    static BinaryWriter WriteFileBIN(string path, out FileStream stream)
    {
        stream = WriteStream(path);
        return new BinaryWriter(stream);
    }

    static StreamWriter WriteFileSTR(string path, string filename, out FileStream stream)
    {
        Directory.CreateDirectory(path);
        return WriteFileSTR(path + "/" + filename, out stream);
    }

    static StreamWriter WriteFileSTR(string path, out FileStream stream)
    {
        stream = WriteStream(path);
        return new StreamWriter(stream);
    }
    #endregion

    #region File Closing
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

    static void CloseFile(StreamReader reader, FileStream stream)
    {
        reader.Close();
        stream.Close();
    }

    static void CloseFile(StreamWriter writer, FileStream stream)
    {
        writer.Close();
        stream.Close();
    }
    #endregion

    #region File Dialog
    public static string[] FileDialog()
    {
        return StandaloneFileBrowser.OpenFilePanel("Open File", "", "exe", false);
    }

    public static string[] FileDialog(ExtensionFilter[] extensions, bool multipleSelection = false)
    {
        return StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, multipleSelection);
    }
    #endregion

    #region File Management
    public static bool CheckIfFileExists(string path)
    {
        return File.Exists(path);
    }

    public static string[] GetChildDirectories(string path)
    {
        if (Directory.Exists(path)) {
            return Directory.GetDirectories(path);
        } else {
            return null;
        }
    }

    public static string[] GetFileNamesInDirectory(string path)
    {
        if (Directory.Exists(path)) {
            return Directory.GetFiles(path);
        } else {
            return null;
        }
    }

    public static void DeleteFile(string path)
    {
        File.Delete(path);
    }
    #endregion
    #endregion

    #region String Lists
    public static List<string> ReadStringList(string path, string filename)
    {
        List<string> list = new List<string>();

        BinaryReader reader = ReadFileBIN(path, filename, out FileStream stream);
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
        BinaryWriter writer = WriteFileBIN(path, filename, out FileStream stream);

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
        StreamReader reader = ReadFileSTR(path, out FileStream stream);
        if (reader == null) return null;

        GameData data = new GameData(reader.ReadLine());
        data.gameDescription = reader.ReadLine();

        CloseFile(reader, stream);
        return data;
    }

    public static void WriteGameData(string path, string filename, GameData data)
    {
        StreamWriter writer = WriteFileSTR(path, filename, out FileStream stream);

        writer.WriteLine(data.gameTitle);
        writer.WriteLine(data.gameDescription);

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
    #endregion
}
