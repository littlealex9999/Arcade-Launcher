using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetTextTime : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        DateTime now = DateTime.Now;
        text.text = now.Hour.ToString() + ":" + now.Minute.ToString();
    }
}
