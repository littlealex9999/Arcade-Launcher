using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public string folderPath { get; private set; }
    public string gameTitle { get; private set; }
    public string gameDescription;
    public string playerInfo;
    public Texture bannerTexture;
    public List<Texture> textures;

    public string applicationPath { get { return GameManager.gamesDirectory + gameTitle + "/app.lnk"; } }

    public GameData() { }
    public GameData(string folderPath, string gameTitle)
    {
        this.folderPath = folderPath;
        this.gameTitle = gameTitle;
    }
}
