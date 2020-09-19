using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class NoiseLayer 
{
    public bool addition;
    [SerializeField] public Texture2D noiseTex;
    Color[] pix;
    public string seed;
    public float scale = 3.0F;
    public float xOrg = 0;
    public float yOrg = 0;
    public float baseLevel = .1f;
    int pixWidth, pixHeight;
    static int IntParseFast(string value)
    {
        int result = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char letter = value[i];
            result = 10 * result + (letter - 48);
        }
        return result;
    }
    public void GenerateNoiseTexture(int _id, int _pixWidth, int _pixHeight, NoiseType noiseType)
    {
        pixWidth = _pixWidth;
        pixHeight = _pixHeight;

        Random.InitState(IntParseFast(seed));
        xOrg = Random.Range(0, 99999);
        yOrg = Random.Range(0, 99999);

        noiseTex = new Texture2D(_pixWidth, _pixHeight);
        pix = new Color[pixWidth * pixHeight];
        //material.SetTexture("_MainTex", noiseTex);

        CalcNoise(noiseType);
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(noiseTex, "Assets/NoiseTerrainGenerator/Terrain/noiseTexLayer" + _id);
#endif


    }
    void GenerateNoiseTexture(float xDir, float yDir, NoiseType noiseType)
    {
        Random.InitState(IntParseFast(seed));
        xOrg = xOrg + xDir;
        yOrg = yOrg + yDir;

        CalcNoise(noiseType);
    }
    public void UpdateNoiseTexture(float xDir,float yDir, NoiseType noiseType)
    {
        GenerateNoiseTexture(xDir * Time.deltaTime, yDir * Time.deltaTime, noiseType);
    }
    void CalcNoise(NoiseType noiseType)
    {
        float y = 0.0F;
        while (y < pixHeight)
        {
            float x = 0.0F;
            while (x < pixWidth)
            {
                float xCoord = xOrg + x / pixWidth * scale;
                float yCoord = yOrg + y / pixHeight * scale;
                float sample = 0;

                if (noiseType == NoiseType.perlin)
                {
                    sample = Mathf.PerlinNoise(xCoord, yCoord);
                }
                else if (noiseType == NoiseType.simplex)
                {
                    sample = SimplexNoise.SeamlessNoise(xOrg + x / pixWidth, yOrg + y / pixHeight, scale, scale, IntParseFast(seed));
                   // sample = SimplexNoise.Noise(xCoord, yCoord);
                }

                if (sample < baseLevel)
                {
                    sample = baseLevel;
                }
                pix[(int)y * pixWidth + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }

        noiseTex.SetPixels(pix);
        noiseTex.Apply();

    }
}
public enum NoiseType {perlin,simplex }
