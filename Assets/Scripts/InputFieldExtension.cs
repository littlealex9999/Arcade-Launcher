using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldExtension : MonoBehaviour
{
    string[] strings;
    public TMP_InputField inputField;

    public void ApplyNewStrings(string[] newStrings)
    {
        strings = newStrings;

        if (strings.Length > 1) {
            inputField.text = strings[0] + ", ...";
        } else if (strings.Length > 0) {
            inputField.text = strings[0];
        } else {
            inputField.text = "";
        }
    }

    public string[] GetStrings() { return strings; }
}
