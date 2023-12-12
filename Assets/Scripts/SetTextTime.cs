using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetTextTime : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string format = "HH:mm";

    void Update()
    {
        DateTime now = DateTime.Now;
        text.text = now.ToString(format);
    }
}
