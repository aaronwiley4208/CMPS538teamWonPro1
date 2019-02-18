using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeBlock))]
public class MazeBlockEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Extend")) {
            MazeBlock block = target as MazeBlock;
            block.Extend();
        }
    }

}
