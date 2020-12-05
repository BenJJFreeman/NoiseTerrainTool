using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
[ExecuteInEditMode]
public class NoiseTerrainMain : MonoBehaviour
{
    [Header("Map Generation")]
    public int height = 5;
    public float offstep = 0.25f;
    public float step = 0.25f;
    public float mapSizeWidth;
    public float mapSizeHeight;
    public Material material;

    [Header("Terrain Generation")]
    public NoiseTextureGenerator terrainNoiseTextureGenerator;

    [Header("Cloud Generation")]
    public NoiseLayer cloudLayer;

    [Header("Water Generation")]
    public NoiseTextureGenerator waterScrollingNoiseTextureGenerator;
  //  public NoiseLayer waterScrollingLayer;

    [Header("Visible Points")]
    public List<VisiblePoint> visiblePoints = new List<VisiblePoint>();
    public GameObject visiblePointParent;
    public GameObject visiblePoint;


    [Header("Settings")]
   [SerializeField] private bool runtimeGeneration;

    [Header("Controls")]
    [Range(0,1)]
    public float waterLevel;
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
        cloudLayer.GenerateNoiseTexture("_CloudTex", 128, 128, NoiseType.perlin);
        material.SetTexture("_CloudTex", cloudLayer.noiseTex);
    }
    public void GenerateWaterScrollingLayer()
    {
        if (waterScrollingNoiseTextureGenerator == null)
            return;
        waterScrollingNoiseTextureGenerator.GenerateNoiseTexture();
        waterScrollingNoiseTextureGenerator.CombineLayers();
        material.SetTexture("_WaterScrollingTex", waterScrollingNoiseTextureGenerator.noiseTex);

        /*
        waterScrollingLayer.GenerateNoiseTexture("_WaterScrollingTex", 512, 512, NoiseType.perlin);
        material.SetTexture("_WaterScrollingTex", waterScrollingLayer.noiseTex);*/
    }
    public void GenerateColourTexture()
    {
        Texture2D tex = new Texture2D(300, 300, TextureFormat.ARGB32, false);

        Color fillColor = Color.clear;
        Color[] fillPixels = new Color[tex.width * tex.height];

        for (int i = 0; i < fillPixels.Length; i++)
        {
            fillPixels[i] = fillColor;
        }

        tex.SetPixels(fillPixels);

        tex.Apply();

#if UNITY_EDITOR
        AssetDatabase.CreateAsset(tex, "Assets/NoiseTerrainGenerator/Terrain/" + "ColourTex" + ".asset");
#endif
        material.SetTexture("_ColourTex", tex);
    }

    public void GenerateNoiseTexture()
    {
        if (terrainNoiseTextureGenerator == null)
            return;
        terrainNoiseTextureGenerator.GenerateNoiseTexture();
        terrainNoiseTextureGenerator.CombineLayers();
        material.SetTexture("_MainTex", terrainNoiseTextureGenerator.noiseTex);
    }
    public void GenerateAllTextures()
    {
        GenerateNoiseTexture();
        GenerateWaterScrollingLayer();
        GenerateCloudLayer();
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
             //   DestroyImmediate(go.GetComponent<Collider>());
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
        cloudLayer.UpdateNoiseTexture(.5f, 0,NoiseType.perlin);
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
