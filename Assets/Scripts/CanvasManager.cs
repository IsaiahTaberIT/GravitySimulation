using System;
using UnityEngine;


[ExecuteAlways]
public class CanvasManager : MonoBehaviour
{
    public Camera Secondary;
    public RenderTexture Canvas;
    public Material FullScreenPassMat;
    public Texture2D test;
    public int height;
    public int width;

    // Update is called once per frame
    void Update()
    {

        if (Canvas == null)
        {
            Canvas = (RenderTexture)Resources.Load("FullScreenCanvas");

            if (Canvas == null)
            {
                return;
            }
        }

        width = Camera.main.pixelWidth;
        height = Camera.main.pixelHeight;



        if (FullScreenPassMat == null)
        {
            FullScreenPassMat = (Material)Resources.Load("FullScreenMat");

            if (FullScreenPassMat == null)
            {
                return;
            }
        }

        if (Canvas.height != Camera.main.pixelHeight || Canvas.width != Camera.main.pixelWidth)
        {

           Canvas = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 1, RenderTextureFormat.Default, 1);
           Secondary.targetTexture = Canvas;
        }

        Secondary.targetTexture = Canvas;

        FullScreenPassMat.SetTexture("_Tex", Canvas);
    }
}
