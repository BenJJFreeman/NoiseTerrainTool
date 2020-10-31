using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class TextureDrawing : MonoBehaviour
{
    public Camera cam;
    public float projectionHeight;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;



        RaycastHit[] hits;
        hits = Physics.RaycastAll(cam.ScreenPointToRay(Input.mousePosition));

        Texture2D tex = new Texture2D(0,0);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                continue;

            tex = rend.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            float height = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).r;

            if (height < hit.transform.position.y / projectionHeight)
                continue;


            tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.white);
            tex.Apply();
            break;
        }

#if UNITY_EDITOR
        AssetDatabase.CreateAsset(tex, "Assets/NoiseTerrainGenerator/Terrain/Terrain");
#endif
        /*
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;
        

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;

        tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.white);
        tex.Apply();

       */

    }
}
