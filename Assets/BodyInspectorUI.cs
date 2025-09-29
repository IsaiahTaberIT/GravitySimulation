using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static BodyGenerator;
using static Logic;
public class BodyInspectorUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Min(0)] [SerializeField] private int m_SigFigs;
    public int SigFigs
    {
        get { return m_SigFigs; }

        set
        {
            value = Math.Clamp(value,0,int.MaxValue);

            /*
            if (m_Radius != value)
            {
                RadiusUI.Interfered = true;

                if (!RadiusUI.Editing && RadiusUI.InputField != null)
                {
                    RadiusUI.text = value.ToString(Format);
                }
            }
            */

            m_SigFigs = value;
            SetFormatFromSigfigs();
        }
    }



    private string Format = "";
    public bool Test = true;
    [SerializeField] private CanvasGroup SelfGroup;
    [SerializeField] private float VelocityDragRate = 0.25f;
    [SerializeField] private float DragRate = 0.01f;
    [SerializeField] private float Resistance = 0.01f;
    public bool Dragging;
    [SerializeField] private float MinBodyScale = 20;
    [SerializeField] private float BodyScaleMultiplier = 1.2f;

    [SerializeField] private CameraManager Cam;

    [SerializeField] private RectTransform ParticleDragger;
    private Image DragImage;
    public Vector2 LastMousePos;
    [SerializeField] private Timer _FadeTransitionTimer = new Timer(1,0,true);



    public DraggableUIInputField XPosUI;
    public DraggableUIInputField YPosUI;
    public DraggableUIInputField XVelUI;
    public DraggableUIInputField YVelUI;
    public DraggableUIInputField MassUI;
    public DraggableUIInputField RadiusUI;


    [System.Serializable]
    public class DraggableUIInputField
    {
        public float MaxValue;
        public float MinValue;
        public RectTransform DragObject;
        public TMP_InputField InputField;
        public bool Editing;
        public bool Interfered;
        public bool Dragging;

        public void UpdateDragging()
        {

            if (DragObject != null )
            {
                Rect r = DragObject.RectFromRectTransform(GravityController.ScaleFactor);

                if (r.Contains(Input.mousePosition))
                {
                    Dragging = true;
                }
            }
        }

        public void EditBegin(TMP_InputField i)
        {
            if (InputField == i)
            {
                Interfered = false;
                Editing = true;
            }
        }

        public string text
        {
            get
            {
                if (InputField != null)
                {
                    return InputField.text;
                }

                return null;
            }

            set
            {
                InputField.text = value;
            }
        }




    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Logic.RandomColor();
        Rect r = RadiusUI.DragObject.RectFromRectTransform(GravityController.ScaleFactor);
        Logic.DrawBox(r);

        if (r.Contains(Input.mousePosition))
        {
            Gizmos.color = Logic.LerpColor(Gizmos.color, Color.green, 0.75f);

        }
        else
        {
            Gizmos.color = Logic.LerpColor(Gizmos.color, Color.red, 0.75f);

        }
        Gizmos.DrawSphere(Input.mousePosition, 40);

    }



    private void SetFormatFromSigfigs()
    {
        Format = $"N{m_SigFigs}";
    }



    private void OnValidate()
    {
        SetFormatFromSigfigs();
    }




    [SerializeField] private float m_Radius;
    public float Radius
    {
        get { return m_Radius; }

        set
        {
            if (value < 0.01f)
            {
                value = 0.01f;
            }

            if (m_Radius != value)
            {
                RadiusUI.Interfered= true;

                if (!RadiusUI.Editing && RadiusUI.InputField != null)
                {
                    RadiusUI.text = value.ToString(Format);
                }
            }

            m_Radius = value;

        }
    }


    [SerializeField] private float m_Mass;
    public float Mass
    {
        get { return m_Mass; }

        set
        {
            if (value < 0.01f)
            {
                value = 0.01f;
            }



            if (m_Mass != value)
            {
                MassUI.Interfered = true;

                if (!MassUI.Editing && MassUI.InputField != null)
                {
                    MassUI.text = value.ToString(Format);
                }
            }

            m_Mass = value;

        }
    }


    private GravityController GC;

    [SerializeField] private Vector2 m_Velocity;
    public Vector2 Velocity

    {
        get { return m_Velocity; }

        set
        {
            if (m_Velocity.x != value.x)
            {
                XVelUI.Interfered = true;

                if (!XVelUI.Editing && XVelUI.InputField != null)
                {
                    XVelUI.text = value.x.ToString(Format);
                }
            }

            if (m_Velocity.y != value.y)
            {
                YVelUI.Interfered = true;

                if (!YVelUI.Editing && YVelUI.InputField != null)
                {
                    YVelUI.text = value.y.ToString(Format);
                }
            }

            m_Velocity = value;

        }
    }
    
    [SerializeField] private Vector2 m_Position;
    public Vector2 Position
    {
        get { return m_Position; }

        set
        {
            if (m_Position.x != value.x)
            {
                XPosUI.Interfered = true;

                if (!XPosUI.Editing && XPosUI.InputField != null)
                {
                    XPosUI.text = value.x.ToString(Format);
                }
            }

            if (m_Position.y != value.y)
            {
                YPosUI.Interfered = true;

                if (!YPosUI.Editing && YPosUI.InputField != null)
                {
                    YPosUI.text = value.y.ToString(Format);
                }
            }

            m_Position = value;

        }
    }
    

    public void SetDragging(bool value)
    {
        Dragging = value;

    }

    public void OverrideRadius(TMP_InputField InputField)
    {
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            return;
        }

        float newRadius = Radius;
        newRadius = ParseInputField(InputField, newRadius);
        newRadius = Mathf.Max(0.01f, newRadius);

        m_Radius = newRadius;
        GC.BodyRef.Body.Radius = newRadius;
        GC.BodyRef.Body.Mass = GravitationalBody.MassFromRadius(newRadius);
        GC.Refresh();
    }


    public void OverrideMass(TMP_InputField InputField)
    {
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            return;
        }

        float newMass = Mass;
        newMass = ParseInputField(InputField, newMass);
        newMass = Mathf.Max(0.01f, newMass);

        m_Mass = newMass;
        GC.BodyRef.Body.Mass = newMass;
        GC.BodyRef.Body.Radius = GravitationalBody.RadiusFromMass(newMass);
        GC.Refresh();
    }


    public void OverrideVel(TMP_InputField InputField)
    {
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            return;
        }

        bool isX = (InputField == XVelUI.InputField);


        if (!isX && InputField != YVelUI.InputField)
        {
            //if some dingus calls this with a TMP_InputField that isnt either of the ones used
            //by this class do nothing

            Debug.LogWarning("what are you doing my guy? (no VALID TMP_InputField assigned)", gameObject);
            return;
        }
        Vector2 newVel = Velocity;

        if (isX && !XVelUI.Interfered)
        {
            newVel.x = ParseInputField(InputField);
            m_Velocity = newVel;
            GC.BodyRef.Body.Velocity = Velocity;
            GC.Refresh();
        }

        if (!isX && !YVelUI.Interfered)
        {
           // Debug.Log("yvel");
            newVel.y = ParseInputField(InputField);
            m_Velocity = newVel;
            GC.BodyRef.Body.Velocity = Velocity;
            GC.Refresh();

        }
    }

     public void Delete()
    {
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            return;
        }

        for (int i = 0; i < GC.Bodies.Count; i++)
        {
            if (GC.BodyRef.Body == GC.Bodies[i])
            {
                GC.Bodies[i] = null;
                GC.BodyRef.Body = null;
                return;
            }
        }


    }



    public void ClearReference()
    {
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            return;
        }

        GC.BodyRef.Body = null;

    }



    public void OverridePos(TMP_InputField InputField)
    {
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            return;
        }

        bool isX = (InputField == XPosUI.InputField);

        if (!isX && InputField != YPosUI.InputField)
        {
            //if some dingus calls this with a TMP_InputField that isnt either of the ones used
            //by this class do nothing

            Debug.LogWarning("what are you doing my guy? (no VALID TMP_InputField assigned)", gameObject);
            return;
        }

        if (isX && !XPosUI.Interfered)
        {
            m_Position.x = ParseInputField(InputField);
            GC.BodyRef.Body.Position = Position;
            GC.Refresh();
        }

        if (!isX && !YPosUI.Interfered)
        {
            m_Position.y = ParseInputField(InputField);
            GC.BodyRef.Body.Position = Position;
            GC.Refresh();

        }
    }

    public float ParseInputField(TMP_InputField InputField,float Default = 0.0f)
    {
        if (float.TryParse(InputField.text, out float f))
        {
            return f;
        }
        else
        {
            return Default;
        }
    }


    public void EndEdit()
    {
        XPosUI.Editing = false;
        YPosUI.Editing = false;
        XVelUI.Editing = false;
        YVelUI.Editing = false;
        MassUI.Editing = false;
        RadiusUI.Editing = false;


    }


    public void BeginEdit(TMP_InputField InputField)
    {
        XPosUI.EditBegin(InputField);
        YPosUI.EditBegin(InputField);
        XVelUI.EditBegin(InputField);
        YVelUI.EditBegin(InputField);
        MassUI.EditBegin(InputField);
        RadiusUI.EditBegin(InputField);
      
    }
    private void Start()
    {
        SetFormatFromSigfigs();
        DragImage = ParticleDragger.gameObject.GetComponent<Image>();


        SelfGroup = GetComponent<CanvasGroup>();
        Cam = FindAnyObjectByType<CameraManager>();    
        GC = FindAnyObjectByType<GravityController>();
    }


    public void MoveToMousePos(bool isDragging)
    {
        if (isDragging)
        {
            Vector2 newpos = Input.mousePosition;

            ParticleDragger.position = newpos;

            Cam.ScreenToWorld(ref newpos);

            if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
            {
          
            }
            else
            {
                if (GC.Paused())
                {
                    GC.BodyRef.Body.Position = newpos;
                    GC.BodyRef.Body.LastPosition = newpos;
                    GC.Refresh();
                }
                else
                {
                    GC.BodyRef.Body.Velocity += (newpos - Position) * DragRate;
                    GC.BodyRef.Body.Velocity *= 1 - Resistance;
                }
                 
            }
        }
       
    }
    




    public void OnDrag(PointerEventData eventData)
    {
        bool notNull = !(GC == null || GC.BodyRef == null || GC.BodyRef.Body == null);

        Vector2 newpos = Vector2.zero;
        Vector2 newvel = Vector2.zero;
        float newmass = 0;
        float newradius = 0;

        if (notNull)
        {
            newpos = GC.BodyRef.Body.Position;
            newvel = GC.BodyRef.Body.Velocity;
            newmass = GC.BodyRef.Body.Mass;
            newradius = GC.BodyRef.Body.Radius;
        }
  

        //doing this manually because otherwise the warping inteferes
        Vector2 delta =  eventData.position - LastMousePos;

        if (XPosUI.Dragging)
        {
            newpos.x += delta.ComponentAdd();
            Mouse.current.WarpCursorPosition(XPosUI.DragObject.position);
            LastMousePos = XPosUI.DragObject.position;

        }

        if (YPosUI.Dragging)
        {

            newpos.y += delta.ComponentAdd();
            Mouse.current.WarpCursorPosition(YPosUI.DragObject.position);
            LastMousePos = YPosUI.DragObject.position;
        }

        Position = newpos;

        if (notNull)
        {
            GC.BodyRef.Body.Position = newpos;
            GC.Refresh();
        }


        if (XVelUI.Dragging)
        {
            newvel.x += delta.ComponentAdd() * VelocityDragRate;
            Mouse.current.WarpCursorPosition(XVelUI.DragObject.position);
            LastMousePos = XVelUI.DragObject.position;

        }

        if (YVelUI.Dragging)
        {
            newvel.y += delta.ComponentAdd() * VelocityDragRate;
            Mouse.current.WarpCursorPosition(YVelUI.DragObject.position);
            LastMousePos = YVelUI.DragObject.position;

        }

        Velocity = newvel;


        if (MassUI.Dragging)
        {
            newmass += delta.ComponentAdd();
            Mouse.current.WarpCursorPosition(MassUI.DragObject.position);
            LastMousePos = MassUI.DragObject.position;

        }

        Mass = newmass;


        if (RadiusUI.Dragging)
        {
            newradius += delta.ComponentAdd();
            Mouse.current.WarpCursorPosition(RadiusUI.DragObject.position);
            LastMousePos = RadiusUI.DragObject.position;

        }

        Radius = newradius;

        if (notNull)
        {
            GC.BodyRef.Body.Velocity = newvel;
            GC.Refresh();
        }
    }

    private void Update()
    {
        
        if (GC == null || GC.BodyRef == null || GC.BodyRef.Body == null)
        {
            SelfGroup.blocksRaycasts = false;
            DragImage.enabled = false;
            _FadeTransitionTimer.Step(-Time.deltaTime);
        }
        else
        {
            _FadeTransitionTimer.Step();
            SelfGroup.blocksRaycasts = true;

            DragImage.enabled = true;
            Position = GC.BodyRef.Body.LastPosition;
            Velocity = GC.BodyRef.Body.Velocity;
            Radius = GC.BodyRef.Body.Radius;
            Mass = GC.BodyRef.Body.Mass;



            if (ParticleDragger != null)
            {
                Vector2 newpos = Position;

                if (!GC.Paused() && Test)
                {
                    newpos -= Velocity;
                }
               
                Cam.WorldToScreen(ref newpos);


              

                if (float.NaN != newpos.x && float.NaN != newpos.y)
                {
                    ParticleDragger.position = newpos;

                    Vector2 newscale = Vector2.one * GC.BodyRef.Body.Radius * 2 * BodyScaleMultiplier;

                    Cam.ApparentSize(ref newscale);

                    ParticleDragger.sizeDelta = Logic.ClampVector(newscale, MinBodyScale, float.MaxValue);
                }


             
            }

           
        }

     //   if (DraggingXPos || DraggingYPos)
        {
            //im not doing this far too much work for something that doesnt really matter
           // Cursor.SetCursor();
        }

        SelfGroup.alpha = _FadeTransitionTimer.Ratio;
        
        MoveToMousePos(Dragging);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        LastMousePos = eventData.position;

        XPosUI.UpdateDragging();
        YPosUI.UpdateDragging();
        XVelUI.UpdateDragging();
        YVelUI.UpdateDragging();
        MassUI.UpdateDragging();
        RadiusUI.UpdateDragging();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        XPosUI.Dragging = false;
        YPosUI.Dragging = false;
        XVelUI.Dragging = false;
        YVelUI.Dragging = false;
        MassUI.Dragging = false;
        RadiusUI.Dragging = false;
     

    }
}


