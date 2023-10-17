using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public string exePath;
    public string gameTitle;
    public string gameDescription;

    public GameData() { }
    public GameData(string exePath, string gameTitle, string gameDescription)
    {
        this.exePath = exePath;
        this.gameTitle = gameTitle;
        this.gameDescription = gameDescription;
    }
}
