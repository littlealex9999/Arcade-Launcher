using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureData
{
    public string texturePath;
    public byte[] data;

    public TextureData(string path)
    {
        texturePath = path;
    }
}
