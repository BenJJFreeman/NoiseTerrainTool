using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TexturePainting : MonoBehaviour
{
    public Camera cam;
    public float projectionHeight = 40;
    public int radius = 10;
    public Color color = Color.black;
    void Start()
    {
        
    }
    void OnSceneGUI()
    {
        Update();
    }
    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            return;


        RaycastHit[] hits;
        hits = Physics.RaycastAll(cam.ScreenPointToRay(Input.mousePosition));

        Texture2D tex = new Texture2D(0, 0);
        Texture2D colourTex = new Texture2D(0, 0);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                continue;
            tex = rend.sharedMaterial.mainTexture as Texture2D;
            colourTex = rend.sharedMaterial.GetTexture("_ColorTex") as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            float height = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).r;

            if (height < hit.transform.position.y / projectionHeight)
                continue;


            if (Input.GetMouseButton(0))
            {
                Circle(colourTex, (int)pixelUV.x, (int)pixelUV.y, radius, color);
            }
            //tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.white);
            //tex.Apply();
            break;
        }

    }
    void Increase()
    {

    }
    void Decrease()
    {

    }
 
    public void Circle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int x, y, px, nx, py, ny, d;
        Color[] tempArray = tex.GetPixels();

        for (x = 0; x <= r; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
            for (y = 0; y <= d; y++)
            {
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                if ((py * tex.width + px) < tempArray.Length && (py * tex.width + px) >= 0)
                {
                    tempArray[py * tex.width + px] = col;
                }

                if ((py * tex.width + nx) < tempArray.Length && (py * tex.width + nx) >= 0)
                {

                    tempArray[py * tex.width + nx] = col;
                }

                if ((ny * tex.width + px) < tempArray.Length && (ny * tex.width + px) >= 0)
                {

                    tempArray[ny * tex.width + px] = col;
                }

                if ((ny * tex.width + nx) < tempArray.Length && (ny * tex.width + nx) >= 0)
                {

                    tempArray[ny * tex.width + nx] = col;
                }
            }
        }
        tex.SetPixels(tempArray);
        tex.Apply();
    }
}
