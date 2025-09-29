using JetBrains.Annotations;
using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;



[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{

    [Min (0.01f)]public float Mult = 0.5f;
    public float Offset;
    public RenderTexture SourceBuffer;
    public bool DisableRendering = true;
    public Mesh Quad;
    public float InterpolationRate = 0.1f;
    public float BaseZoomRate = 25f;
    public float CtrlZoomRateMult = 2f;
    public float ShiftZoomRateMult = 2f;

    public Camera Camera;
    public bool Follow;
    [Min(1f)]public float Scale;
    public Vector2Int TextureDims;
    private Vector3 _StartingScreenPos;
    private Vector3 _StartingWorldPos;
    public MeshRenderer Canvas;
    public ComputeShader Fullscreen;
    public ComputeShader Fade;

    public RenderTexture OutputTexture;
    public ComputeBuffer Circles;
    public Material fullscreenmat;
    public Material UIOverwriteMat;
    public GravityController GC;
    public static Vector2 WorldMousePosition;
    public Shader CircleDraw;
    public ClickDetectionUI BackDropUI;
    public ClickDetectionUI BodyDragCollider;

    public Material circleMat;
    public bool RenderCirlces = true;
    [SerializeField]private Texture Background;
    [Min(1)][SerializeField] private float BackGroundTiling;


    private RenderTexture _BackgroundRT;
    public FollowTarget Target;
    public Vector2 TargetPos;
    public enum FollowTarget
    {
         COM = 0,
         Object = 1
    }

    private void Start()
    {
        Camera = GetComponent<Camera>();
        GC = FindFirstObjectByType<GravityController>();
        Circles = GC.InputBuffer;
    }
    private void OnDisable()
    {
        if (OutputTexture != null)
        {
            OutputTexture.Release();

        }

    }

    void Update()
    {
        
        HandleInputs();
        HandleTransformations();

    }
    private void OnPreCull()
    {
        if (GC.BodiesCount > 0)
        {
            Circles = GC.InputBuffer;
            circleMat.SetBuffer("Circles", Circles);
            RenderParams rp = new RenderParams(circleMat);
            rp.worldBounds = new Bounds(Vector3.zero, GC.SimulationSize); // use tighter bounds
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Scale(new Vector3(1, 1, 0)));
            rp.matProps.SetFloat("_NumInstances", GC.BodiesCount);
            Graphics.RenderMeshPrimitives(rp, Quad, 0, GC.BodiesCount);
        }
     
    }




    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        this.Camera.orthographicSize = Mult * (Scale * Scale);

        if (Camera.current.name != "SceneCamera" && !DisableRendering)
        {
            Circles = GC.InputBuffer;

            if (OutputTexture == null || OutputTexture.width != Screen.width || OutputTexture.height != Screen.height)
            {
                OutputTexture = new(Screen.width, Screen.height, 0);
                OutputTexture.enableRandomWrite = true;
                OutputTexture.filterMode = FilterMode.Point;
                OutputTexture.Create();
            }


            if (SourceBuffer == null)
            {
                SourceBuffer = new(source);
                SourceBuffer.enableRandomWrite = true;
                SourceBuffer.Create();
            }
            else
            {
                Graphics.Blit(source, SourceBuffer);

            }

            int kernelfade = Fade.FindKernel("CSMain");
            Fade.SetFloat("RawScale", Scale);

            Fade.SetFloat("Offset", Offset);

            Fade.SetFloat("Scale",  OutputTexture.height / (Scale * Scale));

          
            Fade.SetVector("CameraPos", (Vector2)transform.position);
            Fade.SetVector("Dims", new(OutputTexture.width, OutputTexture.height));
            Fade.SetVector("SimDims", GC.SimulationSize);
            Fade.SetVector("Color", GC.FadeColor);
            Fade.SetFloat("BackGroundTiling", BackGroundTiling);
            Fade.SetTexture(kernelfade, "Result", OutputTexture);

            if (_BackgroundRT == null || _BackgroundRT.name != Background.name)
            {
                _BackgroundRT = new RenderTexture(Background.width, Background.height, 0)
                {
                    enableRandomWrite = true,
                    filterMode = FilterMode.Point
                };

                _BackgroundRT.name = Background.name;
                _BackgroundRT.Create();
                Graphics.Blit(Background, _BackgroundRT);

            }

            Fade.SetVector("BackgroundDims", new Vector2(_BackgroundRT.width, _BackgroundRT.height));
            Fade.SetTexture(kernelfade, "Background", _BackgroundRT);
            int X = Mathf.CeilToInt(OutputTexture.width / 8f);
            int Y = Mathf.CeilToInt(OutputTexture.height / 8f);

            Fade.Dispatch(kernelfade, X, Y, 1);

            Graphics.Blit(source, OutputTexture, UIOverwriteMat);


            if (Circles != null && GC.BodiesCount > 0 && RenderCirlces)
            {
                int kernel = Fullscreen.FindKernel("CSMain");
                int threadGroupsX = Mathf.CeilToInt(GC.Bodies.Count / 8f);
                Fullscreen.SetBuffer(kernel, "Circles", Circles);
                Fullscreen.SetInt("Bodies", GC.BodiesCount - 1);

                Fullscreen.SetFloat("Scale", OutputTexture.height / (Scale * Scale));
                Fullscreen.SetTexture(kernel, "Result", OutputTexture);
                Fullscreen.SetVector("CameraPos", (Vector2)transform.position);
                Fullscreen.SetVector("Dims", new(OutputTexture.width, OutputTexture.height));

                Fullscreen.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            Graphics.Blit(OutputTexture, destination);
        }
        else
        {
            Graphics.Blit(source, destination);

        }


    }




    void SetFollowTargetPos()
    {
        switch (Target)
        {
            case FollowTarget.COM:
                TargetPos = GC.COM;
                break;
            case FollowTarget.Object:

                if (GC.BodyRef != null && GC.BodyRef.Body != null)
                {

                    if (!GC.Paused())
                    {
                        TargetPos = GC.BodyRef.Body.Position;

                        TargetPos += GC.BodyRef.Body.Velocity;
                    }
                    else
                    {
                        TargetPos = GC.BodyRef.Body.LastPosition;

                    }

                }
                else
                {
                    TargetPos = Vector2.zero;
                }
                    break;
            default:
                break;
        }


    }    




   



    void HandleTransformations()
    {
        SetFollowTargetPos();
        

        if (Follow)
        {
            Vector3 newPosition = Logic.LerpVector(Camera.transform.position, (Vector3)TargetPos, InterpolationRate);
            newPosition.z = Camera.transform.position.z;

            Camera.transform.position = newPosition;
        }
    }


    public void StartDrag()
    {
        _StartingScreenPos = Input.mousePosition;
        _StartingWorldPos = transform.position;
    }

    public void ApparentSize(ref Vector2 s)
    {
        s /= (Scale * Scale) / OutputTexture.width ;
    }
    public void WorldToScreen(ref Vector2 pos)
    {
        pos -= (Vector2)transform.position;
        pos /= (Scale * Scale) / OutputTexture.height;
      
        pos.x /= Screen.width;
        pos.y /= Screen.height;
        pos += new Vector2(0.5f, 0.5f);
        pos.x *= Screen.width;
        pos.y *= Screen.height;
    }

    public void ScreenToWorld(ref Vector2 pos)
    {
        //pos = Input.mousePosition;
        pos.x /= Screen.width;
        pos.y /= Screen.height;
        pos -= new Vector2(0.5f, 0.5f);
        pos.x *= Screen.width;
        pos.y *= Screen.height;
        pos *= (Scale * Scale) / OutputTexture.height;


        pos += (Vector2)transform.position;
    }

  


    void HandleInputs()
    {
        if (Mouse.current != null && !GC.MenuOpen)
        {
            Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
            float verticalScrollDistance = scrollDelta.y;

            if (verticalScrollDistance != 0)
            {
                float ZoomRate = BaseZoomRate;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    ZoomRate *= CtrlZoomRateMult;
                }
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    ZoomRate *= ShiftZoomRateMult;
                }

                Scale -= verticalScrollDistance * ZoomRate;
                Scale = Mathf.Clamp(Scale, 16, 3000);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Follow = !Follow;
        }
            
        if (OutputTexture != null)
        {
            WorldMousePosition = Input.mousePosition;
            ScreenToWorld(ref WorldMousePosition);
        }

        if (BackDropUI.HoldingMiddle || BodyDragCollider.HoldingMiddle)
        {
            Vector3 mouseScreenPos = Input.mousePosition;

            // transform.position = _StartingWorldPos + (_StartingScreenPos - mouseScreenPos);
            Vector3 screenPosDifference = (_StartingScreenPos - mouseScreenPos) ;
            screenPosDifference *= (Scale * Scale) / Mathf.Min(OutputTexture.width, OutputTexture.height);
            

            Vector3 newPosition = Logic.LerpVector(Camera.transform.position, _StartingWorldPos + screenPosDifference, 0.5f);
            newPosition.z = Camera.transform.position.z;
            Camera.transform.position = newPosition;
        }

        if ((BackDropUI.HoldingRight || BodyDragCollider.HoldingRight) && GC.WorldRect.Contains(WorldMousePosition) && !GC.MenuOpen)
        {
            GC.Generator.seed++;
            GC.Generator.seed %= 1000000;
            GC.Generator.Generate(WorldMousePosition);
            GC.Refresh();
        }
     
    }

}
