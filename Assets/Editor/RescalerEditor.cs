using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rescaler))]
public class RescalerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Rescale")) serializedObject.targetObject.GetComponent<Rescaler>().RescaleToCamera();
    }
}
