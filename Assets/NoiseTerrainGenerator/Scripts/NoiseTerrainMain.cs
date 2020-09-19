using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class NoiseTerrainMain : MonoBehaviour
{
    public int pixWidth = 300;
    public int pixHeight = 300;

    public int height = 5;
    public float offstep = 0.25f;
    public float step = 0.25f;
    public Material material;

    public NoiseLayer[] noiseLayers;
    public Texture2D noiseTex;
    public NoiseLayer cloudLayer;
    public NoiseLayer vegetationLayer;
    public NoiseLayer waterScrollingLayer;
    public float mapSizeWidth;
    public float mapSizeHeight;


    public List<VisiblePoint> visiblePoints = new List<VisiblePoint>();
    public GameObject visiblePointParent;
    public GameObject visiblePoint;

    [Range(0,1)]
    public float waterLevel;
    [Range(0, 1)]
    public float vegatationLevel;
    [Range(0, 1)]
    public float cloudLevel;
    [Range(0, 1)]
    public float cloudHeight;
    void Awake()
    {
        //  visiblePoints.Clear();
        /*
          foreach (Transform child in visiblePointParent.transform)
          {
              visiblePoints.Add(child.GetComponent<VisiblePoint>());
          }*/
        //waterLevel = 0;
    }
    private void Start()
    {
        GenerateCloudLayer();
        GenerateWaterScrollingLayer();
    }
    public void GenerateCloudLayer()
    {
        cloudLayer.GenerateNoiseTexture(noiseLayers.Length +1, 128, 128,NoiseType.perlin);
        material.SetTexture("_CloudTex", cloudLayer.noiseTex);
    }
    public void GenerateVegetationLayer()
    {
        vegetationLayer.GenerateNoiseTexture(noiseLayers.Length, pixWidth, pixHeight, NoiseType.perlin);
        material.SetTexture("_VegatationTex", vegetationLayer.noiseTex);
    }
    public void GenerateWaterScrollingLayer()
    {
        waterScrollingLayer.GenerateNoiseTexture(noiseLayers.Length, pixWidth, pixHeight, NoiseType.simplex);
        material.SetTexture("_WaterScrollingTex", waterScrollingLayer.noiseTex);
    }
    public void GenerateNoiseTexture()
    {
        //noiseTex = new Texture2D(pixWidth, pixHeight);
        for (int i = noiseLayers.Length-1; i >= 0; --i)
        {
            noiseLayers[i].GenerateNoiseTexture(i,pixWidth,pixHeight, NoiseType.perlin);
        }
    }
    void AddLayerToMain(int _noiseLayer)
    {
        for (int w = 0; w < pixWidth; w++)
        {
            for (int h = 0; h < pixHeight; h++)
            {
                noiseTex.SetPixel(w, h, (noiseTex.GetPixel(w, h) + (noiseLayers[_noiseLayer].noiseTex.GetPixel(w, h)/2)));
            }
        }

    }
    void SubtractLayerFromMain(int _noiseLayer)
    {
        for (int w = 0; w < pixWidth; w++)
        {
            for (int h = 0; h < pixHeight; h++)
            {
                noiseTex.SetPixel(w, h, (noiseTex.GetPixel(w, h) - (noiseLayers[_noiseLayer].noiseTex.GetPixel(w, h)/2)));
            }
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

        material.SetTexture("_MainTex", noiseTex);
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(noiseTex, "Assets/NoiseTerrainGenerator/Terrain/mainTex");
#endif
        
    }

    public void GenerateMap()
    {
        ClearMap(); 

        for (float i = offstep; i <= height; i += step)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.material = material;

            if (Mathf.Approximately(i, height / 2) == false)
            {
                DestroyImmediate(go.GetComponent<Collider>());
            }

            go.transform.position = Vector3.up * i;
            go.transform.localScale = new Vector3(mapSizeWidth, mapSizeHeight, 1);
            go.transform.position += new Vector3(go.transform.localScale.x / 2, 0, go.transform.localScale.y / 2);
            go.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            go.transform.parent = transform;
        }


       

    }
    public void ClearMap()
    {
        for (int i = this.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
    }

    void Update()
    {
        List<Vector4> pointList = new List<Vector4>();
        List<float> pointRanges = new List<float>();
        for (int i = 0; i < visiblePoints.Count; i++)
        {
            if(visiblePoints[i] == null)
            {
                visiblePoints.RemoveAt(i);
            }
            pointList.Add(new Vector4(visiblePoints[i].transform.position.x, visiblePoints[i].transform.position.y, visiblePoints[i].transform.position.z, 1));
            pointRanges.Add(visiblePoints[i].range);
        }
        material.SetFloat("_PointCount", pointList.Count);
        material.SetVectorArray("_Point", pointList);
        material.SetFloatArray("_PointRadius", pointRanges);
        //material.SetVector("_PointA", pointList[0]);

        //waterLevel += 1 * Time.deltaTime;
        material.SetFloat("_WaterLevel", waterLevel);
        material.SetFloat("_VegetationLevel", vegatationLevel);
        material.SetFloat("_CloudHeight", cloudHeight);
        material.SetFloat("_CloudLevel", cloudLevel);
        material.SetFloat("_MapHeight", height);
        if (Application.isPlaying)
        {
            UpdateCloudLayer();
        }

    }
    void UpdateCloudLayer()
    {
        cloudLayer.UpdateNoiseTexture(.5f, 0, NoiseType.perlin);
    }
    public VisiblePoint AddVisiblePoint()
    {
        return AddVisiblePoint(Vector3.zero);
    }
    public VisiblePoint AddVisiblePoint(Vector3 _position)
    {
        GameObject g = Instantiate(visiblePoint, _position,Quaternion.identity, visiblePointParent.transform);
        visiblePoints.Add(g.GetComponent<VisiblePoint>());
        return g.GetComponent<VisiblePoint>();
       // GenerateNoiseTexture();
    }
    public void ClearVisiblePoints()
    {

    }
    public float GetWorldHeightAtPoint(Vector3 _point)
    {
        //return noiseTex.GetPixel((int)_point.x, (int)_point.z).r;
        return 0;
    }
}
