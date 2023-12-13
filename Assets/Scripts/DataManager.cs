using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class DataManager
{
    public readonly string gameDataFileName = "gamedata";
    public readonly string bannerFileName = "banner.png";
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
                titles[i] = titles[i].Substring(workingDirectory.Length);
            }
        }
    }

    public List<GameData> GetAllGameData()
    {
        List<GameData> ret = new List<GameData>();
        for (int i = 0; i < titles.Count; ++i) {
            GameData data = FileManager.ReadGameData(workingDirectory + titles[i] + "/" + gameDataFileName, titles[i]);
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

    public void GetTextureData(string title, out List<TextureData> textureData)
    {
        string[] fileNames = FileManager.GetFileNamesInDirectory(workingDirectory + title);
        List<string> textureNames = new List<string>();

        for (int i = 0; i < fileNames.Length; i++) {
            if (fileNames[i].EndsWith(textureFileExtension)) {
                if (fileNames[i].EndsWith(bannerFileName)) continue;
                textureNames.Add(fileNames[i]);
            }
        }

        textureData = new List<TextureData>();
        for (int i = 0; i < textureNames.Count; i++) {
            textureData.Add(new TextureData(textureNames[i]));
            textureData[i].data = FileManager.ReadTextureBytes(textureData[i].texturePath);
        }
    }

    public Texture LoadTextureData(TextureData textureData)
    {
        Texture2D tex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
        tex.LoadImage(textureData.data);

        //tex.LoadRawTextureData(textureData.data);
        //tex.Apply();

        return tex;
    }

    public List<Texture> LoadTextureData(List<TextureData> textureData)
    {
        List<Texture> textures = new List<Texture>();

        for (int i = 0; i < textureData.Count; i++) {
            textures.Add(LoadTextureData(textureData[i]));
        }

        return textures;
    }

    public void LoadBannerData(ref GameData gameData)
    {
        TextureData bannerData = new TextureData(workingDirectory + gameData.folderPath + "/" + bannerFileName);
        bannerData.data = FileManager.ReadTextureBytes(bannerData.texturePath);

        if (bannerData.data != null) {
            gameData.bannerTexture = LoadTextureData(bannerData);
        }
    }
}
