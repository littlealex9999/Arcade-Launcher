using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    public readonly string gameDataFileName = "gamedata";
    public readonly string textureFileExtension = ".png";

    public readonly string workingDirectory;

    List<string> titles = new List<string>();

    public DataManager(string applicationDataPath)
    {
        workingDirectory = applicationDataPath;

        string[] directoryNames = FileManager.GetChildDirectories(applicationDataPath);

        if (directoryNames != null) {
            titles.AddRange(directoryNames);

            for (int i = 0; i < titles.Count; ++i) {
                titles[i] = titles[i].TrimStart(workingDirectory.ToCharArray());
            }
        }
    }

    public List<GameData> GetAllGameData()
    {
        List<GameData> ret = new List<GameData>();
        for (int i = 0; i < titles.Count; ++i) {
            GameData data = FileManager.ReadGameData(workingDirectory + titles[i] + "/" + gameDataFileName);
            if (data != null) {
                ret.Add(data);
            }
        }

        return ret;
    }

    public void WriteGameData(GameData data)
    {
        FileManager.WriteGameData(workingDirectory + data.gameTitle, gameDataFileName, data);
    }

    public List<Texture> GetTextures(string title)
    {
        string[] fileNames = FileManager.GetFileNamesInDirectory(workingDirectory + title);
        List<string> textureNames = new List<string>();

        for (int i = 0; i < fileNames.Length; i++) {
            if (fileNames[i].EndsWith(textureFileExtension)) {
                textureNames.Add(fileNames[i]);
            }
        }

        List<Texture> textures = new List<Texture>();

        for (int i = 0; i < textureNames.Count; i++) {
            textures.Add(FileManager.ReadTexture(textureNames[i]));
        }

        return textures;
    }
}
