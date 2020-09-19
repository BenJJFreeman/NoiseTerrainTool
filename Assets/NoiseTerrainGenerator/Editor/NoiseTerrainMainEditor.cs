using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseTerrainMain))]
public class NoiseTerrainMainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NoiseTerrainMain main = (NoiseTerrainMain)target;
        if (GUILayout.Button("Generate Texture"))
        {
            main.GenerateNoiseTexture();
        }
        if (GUILayout.Button("Generate Vegetation Texture"))
        {
            main.GenerateVegetationLayer();
        }
        if (GUILayout.Button("Generate Cloud Texture"))
        {
            main.GenerateCloudLayer();
        }
        if (GUILayout.Button("Generate Water Scrolling Texture"))
        {
            main.GenerateWaterScrollingLayer();
        }
        if (GUILayout.Button("Combine Layers"))
        {
            main.CombineLayers();
        }
        if (GUILayout.Button("Generate Map"))
        {
            main.GenerateMap();
        }        
        if (GUILayout.Button("Clear Map"))
        {
            main.ClearMap();
        }
        if (GUILayout.Button("Add Point"))
        {
            main.AddVisiblePoint();
        }
    }
}
