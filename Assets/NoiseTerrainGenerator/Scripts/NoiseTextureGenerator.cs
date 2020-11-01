using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class NoiseTextureGenerator : MonoBehaviour
{
    [Header("Texture Generation")]
    public string textureName;
    public int pixWidth = 300;
    public int pixHeight = 300;
    public NoiseLayer[] noiseLayers;
    public Texture2D noiseTex;
    public NoiseType noiseType;
    public void GenerateNoiseTexture()
    {
        //noiseTex = new Texture2D(pixWidth, pixHeight);
        for (int i = noiseLayers.Length - 1; i >= 0; --i)
        {
            noiseLayers[i].GenerateNoiseTexture(textureName + i, pixWidth, pixHeight, NoiseType.perlin);
        }
    }
    void AddLayerToMain(int _noiseLayer)
    {
        for (int w = 0; w < pixWidth; w++)
        {
            for (int h = 0; h < pixHeight; h++)
            {
                noiseTex.SetPixel(w, h, (noiseTex.GetPixel(w, h) + (noiseLayers[_noiseLayer].noiseTex.GetPixel(w, h) / 2)));
            }
        }

    }
    void SubtractLayerFromMain(int _noiseLayer)
    {
        for (int w = 0; w < pixWidth; w++)
        {
            for (int h = 0; h < pixHeight; h++)
            {
                noiseTex.SetPixel(w, h, (noiseTex.GetPixel(w, h) - (noiseLayers[_noiseLayer].noiseTex.GetPixel(w, h) / 2)));
            }
        }
    }
    public void UpdateNoiseTexture(float dirX,float dirY)
    {
        for (int i = noiseLayers.Length - 1; i >= 0; --i)
        {
            noiseLayers[i].UpdateNoiseTexture(dirX, dirY,noiseType);
        }
    }
    public void CombineLayers()
    {

        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] blankColour = new Color[pixWidth * pixHeight];
        noiseTex.SetPixels(blankColour);
        noiseTex.Apply();
        for (int i = 0; i < noiseLayers.Length; i++)
        {
            switch (noiseLayers[i].addition)
            {
                case true:
                    AddLayerToMain(i);
                    break;
                case false:
                    SubtractLayerFromMain(i);
                    break;
            }
        }


        noiseTex.Apply();
      


#if UNITY_EDITOR
        AssetDatabase.CreateAsset(noiseTex, "Assets/NoiseTerrainGenerator/Terrain/" + textureName +".asset");
#endif

    }
}
