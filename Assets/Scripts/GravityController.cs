
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static ColorPickerUI;
using static GravityController.QuadTree;
using static UIInitializer;
using Keys = UIInitializer.StorageLookUpKeys;
[System.Serializable]
public class GravityController : MonoBehaviour, IColorPickerable
{
    public CanvasScaler scaler;
    public static float ScaleFactor {  get; private set; }
    public bool BruteForce;
    public float ThresholdRadius = 1000;
    public static int MaxParticles = 30000;
    public bool ConstantlyUpdate = false;
    public bool CenterOnCOM = true;
    public bool NormalizeVelocity = true;
    public bool GlobalPause = false;
    public bool MenuOpen = false;
    public int BodiesCount;
    public float TotalMass;
    public Vector2 TotalMomentum;
    public Vector2 SimulationSize = new(10000, 10000);
    private Vector2 _LastSimulationSize = new(0, 0);
    public Color FadeColor;
    public Vector2 WorldToSimulation;
    public float BounceThreshold = 1000f;
    public float SplitTheshold = 1000000;
    public Vector2 COM;
    public Vector2 COV;

    public float BigG = 10f;
    public List<GravitationalBody> Bodies = new List<GravitationalBody>(0);
    public ComputeBuffer FlatTreeBuffer;
    public ComputeBuffer InputBuffer;
    public ComputeBuffer OutputBuffer;
    public ComputeShader GravityCalc;
    public ComputeShader FlatGravityCalc;
    public BodyGenerator Generator;
    public List<BodyInputData> Data = new List<BodyInputData>();

    private const int BufferStepSize = 256;
    public float BaseInertia = 10f;
    public int BufferSteps = -1;
    public int PointCount;
    [Min(1)] public int capacity = 4;
    private List<PointAtIndex> PS = new List<PointAtIndex>();
    public System.Random RNG = new System.Random();
    [SerializeField] private StatsUIController Stats;
    public BodyRefWrapper BodyRef = new();
    public Rectangle WorldRect = new Rectangle();
    [SerializeField] private ClickDetectionUI BackDropUI;
    [SerializeField] private BodyInspectorUI SelectedBodyUI;
    BodyOutputData[] oData;
    public FlatQuadTree FlatTree = new();
    public static EdgeCollisionBehavior EdgeBehavior;


    public void SetCollisionBehavior(int value)
    {
        EdgeBehavior = (EdgeCollisionBehavior)value;
        SettingsSaveLoad.Save(Keys.MainEdgeBehavior, value);

    }


    public enum EdgeCollisionBehavior
    {
         Wrap = 0,
         Bounce = 1,
    }





    public void SetMaxParitcles(TMP_InputField TmpInput)
    {
        if (int.TryParse(TmpInput.text, out int num))
        {
            MaxParticles = num;
            SettingsSaveLoad.Save(Keys.MainMaxParticles, num);

        }
        else
        {
            Debug.LogError($"Invalid input: {TmpInput.text}. Please enter a valid value.");
        }
    }


    void UpdateScalingFactor()
    {

        if (Camera.main.pixelWidth > Camera.main.pixelHeight)
        {
            ScaleFactor = Camera.main.pixelWidth/ scaler.referenceResolution.x;

        }
        else
        {
            ScaleFactor = Camera.main.pixelHeight / scaler.referenceResolution.y;

        }


    }


    private void OnValidate()
    {
        UpdateScalingFactor();




        if (BodyRef != null && BodyRef.Body != null)
        {
            BodyRef.Body.RecalculateSize();
        }
    }


    [System.Serializable]

    public class BodyRefWrapper
    {
        [SerializeReference]
        public GravitationalBody Body;
        public bool ReQuery = true;
        public Vector2 QueryPoint;

        /// <summary>
        /// Replaces the GravitationalBody with a new one, by finding the closest one to a given QueryPoint
        /// </summary>
        /// <param name="tree"> the FlatQuadTree that will be queried to find the body</param>
        /// <param name="bodies">the list of bodies that the tree points to</param>
        public void TryReplaceBody(FlatQuadTree tree, List<GravitationalBody> bodies, GravityController gc)
        {
          //  Debug.Log("ran");
            if (QueryPoint == null || !ReQuery)
            {
                return;
            }

            if (tree.IsStale)
            {
                tree.Regenerate(bodies,gc);

            }



            ReQuery = false;
            Body = tree.BreadthFirst(QueryPoint, bodies);

            
        }
    }

    public Color Color
    {
        get { return FadeColor; }
        set

        {
            FadeColor = value;
            SettingsSaveLoad.Save(Keys.MainTrailColor, value);

        }


    }
    public void ToggleNormalizeVelocity(UnityEngine.UI.Toggle t)
    {
        NormalizeVelocity = t.isOn;
        SettingsSaveLoad.Save(Keys.MainNormalizeVelocity, t.isOn);

    }
    public void ToggleCenterOnCOM(UnityEngine.UI.Toggle t)
    {
        CenterOnCOM = t.isOn;
        SettingsSaveLoad.Save(Keys.MainAutoRecenter, t.isOn);

    }

    public void SetWidth(TMP_InputField TmpInput)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(TmpInput.text, out float num))
        {
            SimulationSize.x = num;
            SettingsSaveLoad.Save(Keys.MainWorldWidth, num);

        }
        else
        {
            Debug.LogError($"Invalid input: {TmpInput.text}. Please enter a valid float value.");
        }
    }


    public void SetHeight(TMP_InputField TmpInput)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(TmpInput.text, out float num))
        {
            SimulationSize.y = num;
            SettingsSaveLoad.Save(Keys.MainWorldHeight, num);

        }
        else
        {
            Debug.LogError($"Invalid input: {TmpInput.text}. Please enter a valid float value.");
        }
    }
    public void SetBigG(TMP_InputField TmpInput)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(TmpInput.text, out float num))
        {
            BigG = num;
            SettingsSaveLoad.Save(Keys.MainBigG, num);

        }
        else
        {
            Debug.LogError($"Invalid input for mass: {TmpInput.text}. Please enter a valid float value.");
        }
    }

    public bool Paused()
    {
        return (MenuOpen || GlobalPause);
    }

    public void Pause()
    {
        GlobalPause = true;
        Refresh();
    }
    public void Play()
    {
        GlobalPause = false;

    }

    public void IsMenuOpen(bool tf)
    {
        MenuOpen = tf;
        Refresh();

    }

    public void TryMoveBodyToMouse()
    {

        if (!Input.GetKey(KeyCode.LeftShift) || BodyRef.Body == null || BackDropUI == null || !BackDropUI.HoldingLeft)
        {

            return;
        }

        SelectedBodyUI.Dragging = true;
        BodyRef.Body.Position = CameraManager.WorldMousePosition;
        BodyRef.Body.LastPosition = BodyRef.Body.Position;

    }



    public void ReSelectBody()
    {
        if ((Input.GetKey(KeyCode.LeftShift) || BodyRef == null) && BodyRef.Body != null || MenuOpen)
        {
            return;
        }

        if (!WorldRect.Contains(CameraManager.WorldMousePosition))
        {
            BodyRef.Body = null;
            return;
        }

        BodyRef.ReQuery = true;
        BodyRef.QueryPoint = CameraManager.WorldMousePosition;
        Refresh();
    }



    [ContextMenu("Clear")]
    public void DeleteBodies()
    {
        Bodies.Clear();
        BodyRef.Body = null;

    }

    private void OnDisable()
    {
        if (InputBuffer != null)
        {
            InputBuffer.Dispose();
        }

        if (OutputBuffer != null)
        {
            OutputBuffer.Dispose();
        }

        if (FlatTreeBuffer != null)
        {
            FlatTreeBuffer.Dispose();
        }


        PlayerPrefs.Save();

    }

    private void OnEnable()
    {
        BufferSteps = -1;
    }

    void Start()
    {
       



        Application.targetFrameRate = 45;
        if (GravityCalc == null)
        {
            GravityCalc = Resources.Load<ComputeShader>("GravityCalc");
        }

        UpdateConversion();

        Generator = GetComponent<BodyGenerator>();

    }




    void UpdateConversion()
    {
        if (_LastSimulationSize != SimulationSize)
        {
            WorldRect = new Rectangle(Vector2.zero, SimulationSize / 2);
            WorldToSimulation = Logic.Reciprocal(SimulationSize);
            _LastSimulationSize = SimulationSize;
        }
    }

    public struct BodyInputData
    {

        public Vector2 pos;
        public float mass;
        public float radius;
        public Vector2 last;

    }
    public struct BodyOutputData
    {
        public Vector2 force;
        public int collided;
        public int collisionIndex;


    }

    public void Quit()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR

        EditorApplication.isPlaying = false;

#endif

        Application.Quit();

    }
    

    public void ForceCollisions()
    {

        float runningMass = 0;
        COM = Vector3.zero;
        Vector3 Force = Vector3.zero;
        Vector3 direction = Vector3.zero;
        int threadGroupsX = Mathf.CeilToInt(Bodies.Count / 16f);
        int kernel = GravityCalc.FindKernel("CSMain");
        BodyInputData temp;
        Data.Clear();

        Bodies.RemoveAll(b => b == null);

        TotalMomentum = Vector2.zero;

        BodiesCount = Bodies.Count;
   
        Stats.ParticleCount = BodiesCount.ToString();

        if (FlatTree.ReRun)
        {
            FlatTree.Initialze(WorldRect);
        }


        for (int i = 0; i < BodiesCount; i++)
        {
            TotalMomentum += Bodies[i].Mass * Bodies[i].Velocity;
            // spreading it out like that is easier to read than passing everything into a constructor

            runningMass += Bodies[i].Mass;
            COM += Bodies[i].Mass * Bodies[i].Position;
            temp.pos = Bodies[i].Position;
            temp.mass = Bodies[i].Mass;
            temp.radius = Bodies[i].Radius;
            temp.last = Bodies[i].LastPosition;
            Data.Add(temp);
            FlatTree.InsertPoint(Bodies, i);

        }

        FlatTree.Create();

        TotalMass = runningMass;

        int expectedSteps = Mathf.CeilToInt(BodiesCount / (float)BufferStepSize);

        if (expectedSteps != BufferSteps || InputBuffer == null || OutputBuffer == null)
        {
            if (expectedSteps != 0)
            {
                if (InputBuffer != null)
                {
                    InputBuffer.Dispose();
                }
                if (OutputBuffer != null)
                {
                    OutputBuffer.Dispose();
                }


                BufferSteps = expectedSteps;
                InputBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, sizeof(float) * 6);
                OutputBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, Marshal.SizeOf<BodyOutputData>());

            }
        }

        if (FlatTreeBuffer != null)
        {
            FlatTreeBuffer.Dispose();
        }

        if (FlatTree.NodeListC.Count > 0)
        {
            FlatTreeBuffer = new ComputeBuffer(FlatTree.NodeListS.Count, 68);
        }

        runningMass = Mathf.Max(0.0001f, runningMass);

        COM /= runningMass;

        oData = new BodyOutputData[BodiesCount];

        InputBuffer.SetData(Data);
        FlatTreeBuffer.SetData(FlatTree.NodeListS);
        FlatGravityCalc.SetBuffer(0, "Tree", FlatTreeBuffer);
        FlatGravityCalc.SetBuffer(0, "IData", InputBuffer);
        FlatGravityCalc.SetBuffer(0, "OData", OutputBuffer);
        FlatGravityCalc.SetFloat("DistanceThreshold", ThresholdRadius);
        FlatGravityCalc.SetFloat("BigG", BigG);
        FlatGravityCalc.SetFloat("ArrayLength", BodiesCount);
        FlatGravityCalc.Dispatch(0, threadGroupsX, 1, 1);
        OutputBuffer.GetData(oData);


        for (int i = 0; i < BodiesCount; i++)
        {
            if (Bodies[i] == null)
            {
                continue;
            }

            if (oData[i].collided.ToBool())
            {
                Bodies[i].HandleCollision(oData[i].collisionIndex, i);
            }

            if (Bodies[i] == null)
            {
                continue;
            }
            Bodies[i].HandleBoundsCollision();
        }

        FlatTree.SetStale();

    }



    private void Update()
    {
        UpdateScalingFactor();



        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }

        UpdateConversion();




        if (Bodies.Count > 0)
        {
            
            if (!Paused())
            {
                PhysicsStep();

            }
        }
        else
        {
            BodyRef.ReQuery = false;
            BodiesCount = 0;
        }

        Stats.ParticleCount = BodiesCount.ToString();

    }
    /// <summary>
    /// This is to be called when changes are made that effect rendering, if the game is paused
    /// it Reconstructs the computeBuffer otherwise it does nothing
    /// </summary>
    public void Refresh()
    {
        if (!Paused())
        {
            return;
        }

        BodyInputData temp;
        Data.Clear();
        Bodies.RemoveAll(b => b == null);
        BodiesCount = Bodies.Count;
     

        if (BodyRef != null && BodyRef.Body != null)
        {
            if (BodyRef.Body.DestroyedFlag)
            {
                BodyRef.Body = null;
            }
        }

        for (int i = 0; i < BodiesCount; i++)
        {
            // spreading it out like that is easier to read than passing everything into a constructor
            temp.pos = Bodies[i].Position;
            temp.mass = Bodies[i].Mass;
            temp.radius = Bodies[i].Radius;
            temp.last = Bodies[i].LastPosition;
            Data.Add(temp);
        }

        if (BodyRef != null)
        {
            BodyRef.TryReplaceBody(FlatTree,Bodies,this);
        }

        int expectedSteps = Mathf.CeilToInt(BodiesCount / (float)BufferStepSize);

        if (expectedSteps != BufferSteps || InputBuffer == null)
        {
            if (expectedSteps != 0)
            {
                if (InputBuffer != null)
                {
                    InputBuffer.Dispose();
                }
                if (OutputBuffer != null)
                {
                    OutputBuffer.Dispose();
                }
                if (OutputBuffer != null)
                {
                    FlatTreeBuffer.Dispose();
                }

                BufferSteps = Mathf.Max(1, expectedSteps);
                FlatTreeBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, Marshal.SizeOf<FlatQuadTree.Node>());

                InputBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, sizeof(float) * 6);
                OutputBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, Marshal.SizeOf<BodyOutputData>());

            }
        }

        if (Data != null && Data.Count > 0)
        {
            InputBuffer.SetData(Data);
        }

    }





    void PhysicsStep()
    {
        TryMoveBodyToMouse();

        float runningMass = 0;
        Vector2 OldCov = COV;
        Vector2 OldCom = COM;
        COM = Vector3.zero;
        Vector3 Force = Vector3.zero;
        Vector3 direction = Vector3.zero;
        int threadGroupsX = Mathf.CeilToInt(Bodies.Count / 16f);
        int kernel = GravityCalc.FindKernel("CSMain");
        BodyInputData temp;
        Data.Clear();

        Bodies.RemoveAll(b => b == null);

        TotalMomentum = Vector2.zero;

        BodiesCount = Bodies.Count;
        



        if (FlatTree.ReRun)
        {
            FlatTree.Initialze(WorldRect);
        }

        if (BodyRef != null && BodyRef.Body != null)
        {
            if (BodyRef.Body.DestroyedFlag)
            {
                BodyRef.Body = null;
            }
         
        }
        Vector2 COMCorrection = Vector2.zero;
        Vector2 COVCorrection = Vector2.zero;

        if (NormalizeVelocity)
        {
             COVCorrection = Logic.LerpVector(OldCov, Vector2.zero, 0.95f);
        }

        if (CenterOnCOM)
        {
             COMCorrection = Logic.LerpVector(OldCom, Vector2.zero, 0.95f);
       
        }


        for (int i = 0; i < BodiesCount; i++)
        {
            TotalMomentum += Bodies[i].Mass * Bodies[i].Velocity;

            if (NormalizeVelocity)
            {
           
                Bodies[i].Velocity -= COVCorrection;
            }

            if (CenterOnCOM)
            {
              
                Bodies[i].Position -= COMCorrection;
                Bodies[i].LastPosition -= COMCorrection;
            }
        

            // spreading it out like that is easier to read than passing everything into a constructor

            runningMass += Bodies[i].Mass;
            COM += Bodies[i].Mass * Bodies[i].Position;
            temp.pos = Bodies[i].Position;
            temp.mass = Bodies[i].Mass;
            temp.radius = Bodies[i].Radius;
            temp.last = Bodies[i].LastPosition;
            Data.Add(temp);

            FlatTree.InsertPoint(Bodies, i);

        }

        FlatTree.Create();

        if (BodyRef != null)
        {
            BodyRef.TryReplaceBody(FlatTree, Bodies, this);
        }

        TotalMass = runningMass;

        int expectedSteps = Mathf.CeilToInt(BodiesCount / (float)BufferStepSize);

        if (expectedSteps != BufferSteps || InputBuffer == null || OutputBuffer == null)
        {
            if (expectedSteps != 0)
            {
                if (InputBuffer != null)
                {
                    InputBuffer.Dispose();
                }
                if (OutputBuffer != null)
                {
                    OutputBuffer.Dispose();
                }
              

                BufferSteps = expectedSteps;
                InputBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, sizeof(float) * 6);
                OutputBuffer = new ComputeBuffer(BufferSteps * BufferStepSize, Marshal.SizeOf<BodyOutputData>());

            }
        }

        if (FlatTreeBuffer != null)
        {
            FlatTreeBuffer.Dispose();
        }

        if (FlatTree.NodeListC.Count > 0)
        {
            FlatTreeBuffer = new ComputeBuffer(FlatTree.NodeListS.Count, 68);
        }


        runningMass = Mathf.Max(0.0001f, runningMass);

        COM /= runningMass;
        COV = TotalMomentum / runningMass;



        oData = new BodyOutputData[BodiesCount];
        
        if (BruteForce)
        {
            InputBuffer.SetData(Data);
            GravityCalc.SetBuffer(kernel, "IData", InputBuffer);
            GravityCalc.SetBuffer(kernel, "OData", OutputBuffer);
            GravityCalc.SetFloat("BigG", BigG);
            GravityCalc.SetFloat("ArrayLength", BodiesCount);
            GravityCalc.Dispatch(kernel, threadGroupsX, 1, 1);
            OutputBuffer.GetData(oData);
        }
        else
        {
            InputBuffer.SetData(Data);
            FlatTreeBuffer.SetData(FlatTree.NodeListS);
            FlatGravityCalc.SetBuffer(0, "Tree", FlatTreeBuffer);
            FlatGravityCalc.SetBuffer(0, "IData", InputBuffer);
            FlatGravityCalc.SetBuffer(0, "OData", OutputBuffer);
            FlatGravityCalc.SetFloat("DistanceThreshold", ThresholdRadius);
            FlatGravityCalc.SetFloat("BigG", BigG);
            FlatGravityCalc.SetFloat("ArrayLength", BodiesCount);
            FlatGravityCalc.Dispatch(0, threadGroupsX, 1, 1);
            OutputBuffer.GetData(oData);
        }
        
        for (int i = 0; i < BodiesCount; i++)
        {
            if (Bodies[i] == null)
            {
                continue;
            }

            if (oData[i].force == null)
            {
                continue;
            }

          //  Vector3 force = oData[i].force;
            // Debug.Log(force);
            Bodies[i].AddForce(oData[i].force);
            Bodies[i].Step();
        }

        for (int i = 0; i < BodiesCount; i++)
        {
            
            if (Bodies[i] == null)
            {
                continue;
            }

            if (oData[i].collided.ToBool())
            {
                Bodies[i].HandleCollision(oData[i].collisionIndex, i);
            }

            if (Bodies[i] == null)
            {
                continue;
            }

                 
            Bodies[i].HandleBoundsCollision();
        }

    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public Vector2 Center;
        public Vector2 Scale;

        public Rectangle(Vector2 scale)
        {
            Center = Vector2.zero;
            Scale = scale;
        }
        public Rectangle(float l, float h)
        {
            Center = Vector2.zero;
            Scale = new Vector2(l, h);
        }


        public Rectangle(Vector2 center, Vector2 scale)
        {
            Center = center;
            Scale = scale;
        }

        public Rectangle(float x, float y, float l, float h)
        {
            Center = new Vector2(x, y);
            Scale = new Vector2(l, h);
        }

        public bool Contains(Vector2 point)
        {
            point -= Center;

            if (point.x >= Scale.x)
            {
                return false;
            }

            if (point.x < -Scale.x)
            {
                return false;
            }

            if (point.y >= Scale.y)
            {
                return false;
            }

            if (point.y < -Scale.y)
            {
                return false;
            }

            return true;

        }


        public float SignedDistanceRect(Vector2 point)
        {
           Vector2 d = Logic.Abs(point - Center) - Scale;
           return (Logic.MaxVector(d, Vector2.zero)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0.0f);

        }


        public float SignedDistanceSquare(Vector2 point)
        {
            if (Scale.x != Scale.y)
            {
                Debug.LogWarning("you are trying to use a sdf for squares on a rectangle");
                return 0;
            }

            Vector2 OffsetCenter = point - Center;
            OffsetCenter = new Vector2(Mathf.Abs(OffsetCenter.x), Mathf.Abs(OffsetCenter.y));
            OffsetCenter -= Vector2.one * Scale.x;
            return OffsetCenter.Max();
        }

        public bool Intersects(Rectangle r)
        {
    
            return !(r.Center.x - r.Scale.x > Scale.x + Center.x ||
             r.Center.x + r.Scale.x < Scale.x - Center.x ||
             r.Center.y - r.Scale.y > Scale.y + Center.y ||
             r.Center.y + r.Scale.y < Scale.y - Center.y);

        }


    }

  
    private void OnDrawGizmos()
    {
        Gizmos.color = Logic.LerpColor(Color.red, Color.white, 0.5f);

        DrawBox(WorldRect);


        Gizmos.color = Color.yellow;

        if (FlatTree != null)
        {
            foreach (FlatQuadTree.NodeWrapper n in FlatTree.NodeListC)
            {
                if (n.Leaves.ToBool())
                {
                    DrawBox(n.Data.Bounds);
                }
            }
        }

        Gizmos.color = (Color.blue + Color.white) / 2f;

        foreach (PointAtIndex p in PS)
        {
            Gizmos.DrawSphere(p.Point, 5);
        }


        List<PointAtIndex> points = new List<PointAtIndex>();

        if (ConstantlyUpdate)
        {
            BodyRef.ReQuery = true;

            BodyRef.TryReplaceBody(FlatTree, Bodies, this);
        }

        Gizmos.color = (Color.yellow + Color.green) / 2f;

        Gizmos.DrawSphere(BodyRef.QueryPoint, 40);

        Gizmos.color = (Color.yellow + Color.red) / 2f;

        if (BodyRef.Body != null)
        {
            Gizmos.DrawSphere(BodyRef.Body.Position, 40);

        }

        Gizmos.color = Logic.LerpColor(Color.red, Color.magenta, 0.75f);


        Gizmos.color = Logic.LerpColor(Color.blue, Color.green, 0.65f);
    }

    

   public void DrawBox(Rectangle r)
    {
        Vector2 NW = new Vector2(r.Center.x - r.Scale.x, r.Center.y + r.Scale.y);
        Vector2 NE = new Vector2(r.Center.x + r.Scale.x, r.Center.y + r.Scale.y);
        Vector2 SW = new Vector2(r.Center.x - r.Scale.x, r.Center.y - r.Scale.y);
        Vector2 SE = new Vector2(r.Center.x + r.Scale.x, r.Center.y - r.Scale.y);

        Gizmos.DrawLine(NW, NE);
        Gizmos.DrawLine(SE, NE);
        Gizmos.DrawLine(SE, SW);
        Gizmos.DrawLine(NW, SW);

    }


    [System.Serializable]

    public class QuadTree
    {
        public Vector2 CombinedPoints;
        public float TotalMass;
        public int EncounteredPoints;
        public List<PointAtIndex> Points = new();
        [Min(1)] public int Capacity;
        public Rectangle Bounds;
        public bool Divided = false;
        public List<Rectangle> Boxes = new List<Rectangle>();

        [SerializeReference]
        public QuadTree NW;
        [SerializeReference]
        public QuadTree NE;
        [SerializeReference]
        public QuadTree SW;
        [SerializeReference]
        public QuadTree SE;

        public void Subdivide()
        {
            Rectangle rNW = new Rectangle(Bounds.Center.x - Bounds.Scale.x / 2f, Bounds.Center.y + Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
            Rectangle rNE = new Rectangle(Bounds.Center.x + Bounds.Scale.x / 2f, Bounds.Center.y + Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
            Rectangle rSW = new Rectangle(Bounds.Center.x - Bounds.Scale.x / 2f, Bounds.Center.y - Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
            Rectangle rSE = new Rectangle(Bounds.Center.x + Bounds.Scale.x / 2f, Bounds.Center.y - Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);

            NW = new QuadTree(rNW, Capacity, Boxes);
            NE = new QuadTree(rNE, Capacity, Boxes);
            SW = new QuadTree(rSW, Capacity, Boxes);
            SE = new QuadTree(rSE, Capacity, Boxes);

            Boxes.Add(rNW);
            Boxes.Add(rNE);
            Boxes.Add(rSW);
            Boxes.Add(rSE);

            Divided = true;

        }

        public QuadTree(Rectangle r, int c, List<Rectangle> b)
        {
            Capacity = c;
            Bounds = r;
            Points = new List<PointAtIndex>();
            Boxes = b;

        }


        public QuadTree(Rectangle r, int c)
        {
            Capacity = c;
            Bounds = r;
            Points = new List<PointAtIndex>();

        }


        public bool Insert(PointAtIndex point, List<PointAtIndex> ps)
        {
            CombinedPoints += point.Point;
            EncounteredPoints++;
            TotalMass += point.Mass;

            if (!Bounds.Contains(point.Point))
            {
                return false;
            }

            if (Points.Count < Capacity)
            {
                Points.Add(point);
                ps.Add(point);

                return true;

            }
            else
            {
                if (!Divided)
                {
                    Subdivide();
                }

                if (NW.Insert(point, ps))
                {
                    return true;
                }
                else if (NE.Insert(point, ps))
                {
                    return true;
                }
                else if (SW.Insert(point, ps))
                {
                    return true;
                }
                else
                {
                    SE.Insert(point, ps);
                    return true;
                }
            }
        }

        public PointAtIndex? BreadthFirst(Vector2 point, List<Rectangle> QB, List<Vector2> QP)
        {
            float min = float.MaxValue;

            List<QuadTree> listA = new();
            List<QuadTree> listB = new();

            listA.Add(this);

            return QueryNextLayer(point, ref min, listA, listB, QB, QP);
        }

        public PointAtIndex? QueryNextLayer(Vector2 point, ref float mindist, List<QuadTree> read, List<QuadTree> write, List<Rectangle> QB, List<Vector2> QP)
        {
            PointAtIndex? returnValue = null;

            float newdist;

            for (int i = 0; i < read.Count; i++)
            {

                // counting # of boxes where ive check distance
                QB.Add(read[i].Bounds);



                if (read[i].Bounds.SignedDistanceSquare(point) < mindist)
                {
                    foreach (PointAtIndex p in read[i].Points)
                    {
                        QP.Add(p.Point);
                        newdist = (p.Point - point).magnitude;

                        if (newdist < mindist)
                        {
                            returnValue = p;
                            mindist = newdist;
                        }
                    }

                    if (read[i].Divided)
                    {
                        write.Add(read[i].NW);
                        write.Add(read[i].NE);
                        write.Add(read[i].SW);
                        write.Add(read[i].SE);
                    }

                }
            }

            if (write.Count > 0)
            {
                read.Clear();

                newdist = mindist;

                // this is reversed on purpose so the next call will be correct

                PointAtIndex? p = QueryNextLayer(point, ref mindist, write, read, QB, QP);

                if (p != null && newdist > mindist)
                {
                    returnValue = p;
                }
            }
            return returnValue;
        }




        public void QueryForGravity(Vector2 point, int ExlcusionIndex, float RadiusThreshold, ref List<InterpolatedBody> GravityPullers)
        {
            if (Bounds.SignedDistanceSquare(point) > RadiusThreshold)
            {
                GravityPullers.Add(new InterpolatedBody(CombinedPoints / EncounteredPoints, TotalMass));
            }
            else
            {
                if (Divided)
                {
                    NW.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers);
                    NE.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers);
                    SE.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers);
                    SW.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers);
                }
                else
                {
                    foreach (PointAtIndex p in Points)
                    {
                        if (p.Index != ExlcusionIndex)
                        {
                            GravityPullers.Add(new InterpolatedBody(p.Point, p.Mass));
                        }

                    }
                }

            }
        }



        public void QueryForGravity(Vector2 point, int ExlcusionIndex, float RadiusThreshold, ref List<InterpolatedBody> GravityPullers, ref List<Rectangle> QB)
        {
            if (Bounds.SignedDistanceSquare(point) > RadiusThreshold)
            {
                GravityPullers.Add(new InterpolatedBody(CombinedPoints / EncounteredPoints, TotalMass));
            }
            else
            {
                QB.Add(Bounds);

                if (Divided)
                {
                    NW.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers, ref QB);
                    NE.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers, ref QB);
                    SE.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers, ref QB);
                    SW.QueryForGravity(point, ExlcusionIndex, RadiusThreshold, ref GravityPullers, ref QB);
                }
                else
                {
                    foreach (PointAtIndex p in Points)
                    {
                        if (p.Index != ExlcusionIndex)
                        {
                            GravityPullers.Add(new InterpolatedBody(p.Point, p.Mass));
                        }

                    }
                }

            }
        }










       

        public void Query(Rectangle range, List<PointAtIndex> points)
        {
            if (Bounds.Intersects(range))
            {
                foreach (PointAtIndex p in Points)
                {
                    points.Add(p);
                }
            }

            if (Divided)
            {
                NW.Query(range, points);
                NE.Query(range, points);
                SW.Query(range, points);
                SE.Query(range, points);
            }
        }

        [System.Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct PointAtIndex
        {
            public float Mass;
            public int Index;
            public Vector2 Point;
            public PointAtIndex(int i, Vector2 p, float m)
            {
                Mass = m;
                Index = i;
                Point = p;
            }
        }


        [System.Serializable]
        public struct InterpolatedBody
        {
            public float Mass;
            public Vector2 Point;

            public InterpolatedBody(Vector2 p, float m)
            {
                Mass = m;
                Point = p;
            }
        }


    }


    [System.Serializable]
    public class FlatQuadTree
    {
        public bool IsStale = false;
        public bool IsCreated;
        public bool ReRun = true;
        public List<NodeWrapper> NodeListC;
        public List<Node> NodeListS;
        public int Ptr = 0;

        public void Regenerate(List<GravitationalBody> bodies, GravityController gc)
        {

            Initialze(gc.WorldRect);


            bodies.RemoveAll(b => b == null);

            for (int i = 0; i < bodies.Count; i++)
            {
          //      Debug.Log("inserting");
                InsertPoint(bodies, i);
            }

            Create();
        }




        public void SetStale()
        {
            IsCreated = false;
            IsStale = true;
        }

        public void Create()
        {
            
            NodeListS = new List<Node>(NodeListC.Count);

            for (int i = 0; i < NodeListC.Count; i++)
            {
                NodeListS.Add(NodeListC[i].Data);
            }
            IsStale = false;
            IsCreated = true;
        }


        public class NodeWrapper
        {
            public Node Data;
            public int Leaves { get => Data.Leaves; set => Data.Leaves = value; }

            public NodeWrapper(Rectangle b, int S)
            {
                Data = new(b,S);
            }


            public void SubDivide(FlatQuadTree M)
            {
                Rectangle Bounds = Data.Bounds;




                Rectangle rNW = new Rectangle(Bounds.Center.x - Bounds.Scale.x / 2f, Bounds.Center.y + Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
                Rectangle rNE = new Rectangle(Bounds.Center.x + Bounds.Scale.x / 2f, Bounds.Center.y + Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
                Rectangle rSW = new Rectangle(Bounds.Center.x - Bounds.Scale.x / 2f, Bounds.Center.y - Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
                Rectangle rSE = new Rectangle(Bounds.Center.x + Bounds.Scale.x / 2f, Bounds.Center.y - Bounds.Scale.y / 2, Bounds.Scale.x / 2, Bounds.Scale.y / 2);
                //  Debug.Log("Dividing!");

                M.Ptr++;
                NodeWrapper NW = new NodeWrapper(rNW, M.Ptr);
                M.NodeListC.Add(NW);
                Data.PtrNW = M.Ptr;

                M.Ptr++;
                NodeWrapper NE = new NodeWrapper(rNE, M.Ptr);
                M.NodeListC.Add(NE);
                Data.PtrNE = M.Ptr;

                M.Ptr++;
                NodeWrapper SW = new NodeWrapper(rSW, M.Ptr);
                M.NodeListC.Add(SW);
                Data.PtrSW = M.Ptr;

                M.Ptr++;
                NodeWrapper SE = new NodeWrapper(rSE, M.Ptr);
                M.NodeListC.Add(SE);
                Data.PtrSE = M.Ptr;
                Leaves++;


            }

            public bool Insert(GravitationalBody g, int index, List<GravitationalBody> b, FlatQuadTree M)
            {
                if (!Data.Bounds.Contains(g.Position))
                {
                    return false;
                }

                Data.COM += g.Mass * g.Position;
                Data.TotalMass += g.Mass;

                if (Leaves < 4)
                {
                    switch (Leaves)
                    {
                        case 0: Data.Leaf1Ptr = index;
                            break;
                        case 1: Data.Leaf2Ptr = index;
                            break;
                        case 2: Data.Leaf3Ptr = index;
                            break;
                        case 3: Data.Leaf4Ptr = index;
                            break;
                    }
                    Leaves++;

                }
                else
                {
                    if (Leaves == 4)
                    {
                        SubDivide(M);
                    }

                    if (M.NodeListC[Data.PtrNW].Insert(g, index, b, M))
                    {
                    }
                    else if (M.NodeListC[Data.PtrNE].Insert(g, index, b, M))
                    {
                    }
                    else if (M.NodeListC[Data.PtrSW].Insert(g, index, b, M))
                    {
                    }
                    else if (M.NodeListC[Data.PtrSE].Insert(g, index, b, M))
                    {
                    }

                }

                return true;

            }
        }
        public void Initialze(Rectangle bounds)
        {
            IsCreated = false;
            NodeListC.Clear();
            NodeListS.Clear();

            Ptr = 0;
            NodeListC.Add(new NodeWrapper(bounds,0));
        }

        public void InsertPoint(List<GravitationalBody> bodies,int index)
        {
            if (!ReRun)
            {
                return;
            }

            if(NodeListC.Count > 0)
            {
                NodeListC[0].Insert(bodies[index], index, bodies, this);
            }
            else
            {
                Debug.Log("FlatQuadtree is unintialized");
            }
        }
     
        public FlatQuadTree(int initSize)
        {
            
            NodeListC = new List<NodeWrapper>(initSize);
        }

        public FlatQuadTree()
        {
            
            NodeListC = new List<NodeWrapper>(1000);
        }

        [System.Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct Node
        {
            public int PtrNW;
            public int PtrNE;
            public int PtrSW;
            public int PtrSE;
            public int SelfIndex;

            public int Leaves;
            public Vector2 COM;
            public float TotalMass;


            public Rectangle Bounds;

            // Hard coded fake array Because the GPU is whiny

            public int Leaf1Ptr;
            public int Leaf2Ptr;
            public int Leaf3Ptr;
            public int Leaf4Ptr;


            public Node(Rectangle b, int s)
            {
                SelfIndex = s;

                PtrNW = -1;
                PtrNE = -1;
                PtrSW = -1;
                PtrSE = -1;

                Leaves = 0;
                COM = Vector2.zero;



                Leaf1Ptr = -1;
                Leaf2Ptr = Leaf1Ptr;
                Leaf3Ptr = Leaf1Ptr;
                Leaf4Ptr = Leaf1Ptr;

                TotalMass = 0;
                Bounds = b;

            }
        }

        /// <summary>
        /// Conducts a "Breadth First" search to find the the closest point to the specified queryPoint
        /// </summary>
        /// <param name="queryPoint"></param>
        /// <param name="b"></param>
        /// <param name="min"></param>
        /// <returns>Returns the GravitationalBody body that was closest to the query point</returns>


        public GravitationalBody BreadthFirst(Vector2 queryPoint, List<GravitationalBody> b, float min = float.MaxValue)
        {

            if (NodeListC.Count == 0 || b.Count == 0 || IsStale) return null;
            
            List<NodeWrapper> listA = new();
            List<NodeWrapper> listB = new();

            listA.Add(NodeListC[0]);

            int ptr = QueryNextLayer(queryPoint, ref min, listA, listB, b);

            if (ptr != -1 && ptr < b.Count)
            {
                return b[ptr];
            }

            return null;


        }

        private int QueryNextLayer(Vector2 point, ref float mindist, List<NodeWrapper> read, List<NodeWrapper> write, List<GravitationalBody> b)
        {
            int returnValue = -1;
            int iterations = 0;
            float dist = float.MaxValue;

            for (int i = 0; i < read.Count; i++)
            {
                if (read[i].Data.Bounds.SignedDistanceRect(point) < mindist)
                {
                    //Im not an idiot btw, your not allowed to put arrays into the compute buffer for the gpu
                    // the reult of this function doesnt go into the compute buffer, but the struct it acting on needs to be able to
                    iterations = Mathf.Min(read[i].Data.Leaves, 4);

                    for (int j = 0; j < iterations; j++)
                    {
                        int ptr = -1;

                        switch (j)
                        {
                            case 0:
                                ptr = read[i].Data.Leaf1Ptr;
                                dist = (b[ptr].Position - point).magnitude;
                                break;
                            case 1:
                                ptr = read[i].Data.Leaf2Ptr;
                                dist = (b[ptr].Position - point).magnitude;
                                break;
                            case 2:
                                ptr = read[i].Data.Leaf3Ptr;
                                dist = (b[ptr].Position - point).magnitude;
                                break;
                            case 3:
                                ptr = read[i].Data.Leaf4Ptr;
                                dist = (b[ptr].Position - point).magnitude;
                                break;
                                default:
                                break;
                        }

                        if (dist < mindist)
                        {
                            returnValue = ptr;
                            mindist = dist;
                        }
                    }

                    if (read[i].Leaves == 5)
                    {
                        write.Add(NodeListC[(read[i].Data.PtrNW)]);
                        write.Add(NodeListC[(read[i].Data.PtrNE)]);
                        write.Add(NodeListC[(read[i].Data.PtrSW)]);
                        write.Add(NodeListC[(read[i].Data.PtrSE)]);
                    }

                }
            }

            if (write.Count > 0)
            {
                read.Clear();

                dist = mindist;

                // this is reversed on purpose so the next call will be correct

                int p = QueryNextLayer(point, ref mindist, write, read,b);

                if (p != -1 && dist > mindist)
                {
                    returnValue = p;
                }
            }
            return returnValue;
        }

    }






}
