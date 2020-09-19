using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseTextureGenerator))]
public class NoiseTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NoiseTextureGenerator main = (NoiseTextureGenerator)target;
        if (GUILayout.Button("Generate Texture"))
        {
            main.GenerateNoiseTexture();
        }
        if (GUILayout.Button("Combine Layers"))
        {
            main.CombineLayers();
        }

    }
}
