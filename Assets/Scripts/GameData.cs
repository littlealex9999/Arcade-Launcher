using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public string gameTitle { get; private set; }
    public string gameDescription;

    public string applicationPath { get { return GameManager.gamesDirectory + gameTitle + "/app.lnk"; } }

    public GameData() { }
    public GameData(string gameTitle)
    {
        this.gameTitle = gameTitle;
    }
}
