using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
public static class Logic
{
    public static Color RandomColor(bool RanAlpha = false, float alpha = 1)
    {
        int iterations = 3;

        Color output = new Color(1, 1, 1, 1);

        if (RanAlpha)
        {
            iterations++;
        }
    
        output.a = alpha;


        for (int i = 0; i < iterations; i++)
        {
            output[i] = UnityEngine.Random.Range(0f,1f);
        }

        return output;
    }
    public static float ComponentAdd(this Vector2 v)
    {
        return v.x + v.y;
    }
    public static float Max(this Vector2 v)
    {
        return Mathf.Max(v.x, v.y);
    }
    public static float Max(this Vector3 v)
    {
        return Mathf.Max(v.x, v.y);
    }
    public static float Max(this Vector4 v)
    {
        return Mathf.Max(v.x, v.y);
    }

    public static Vector2 Abs(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }
    public static float Min(this Vector2 v)
    {
        return Mathf.Min(v.x, v.y);
    }
    public static float Min(this Vector3 v)
    {
        return Mathf.Min(v.x, v.y);
    }
    public static float Min(this Vector4 v)
    {
        return Mathf.Min(v.x, v.y);
    }



    public static float Sqrt2 = Mathf.Sqrt(2); 
    public static Vector2 ClampVector(Vector2 value, float min,float max)
    {
        return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
    }

    public static Vector2 ClampVector(Vector2 value, Vector2 min, Vector2 max)
    {
        return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
    }

    public static Vector2 MaxVector(Vector2 value, Vector2 max)
    {
        return new Vector2(Mathf.Max(value.x, max.x), Mathf.Max(value.y, max.y));
    }

    public static Vector2 MinVector(Vector2 value, Vector2 min)
    {
        return new Vector2(Mathf.Min(value.x, min.x), Mathf.Min(value.y, min.y));
    }


    public static string CapitalizeFirst(this string str)
    {
        string first = str[0].ToString();
        first = first.ToUpper();
        str = str.Substring(1);
        str = first + str;
        return str;
    }
    public static List<GameObject> GetRootParents(this Transform obj)
    {
        List<GameObject> objs = new();

        if (obj.parent == null)
        {
            objs.Add(obj.gameObject);
            return objs;
        }
        else
        {
            return obj.parent.GetRootParents();
        }
    }


    public static Vector2[] ToVector2(this Vector3[] points, Vector2 Offset = default)
    {
        Vector2[] outpoints = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            outpoints[i] = points[i] + (Vector3)Offset;
        }

        return outpoints;
    }
    public static GameObject GetRootParent(this Transform obj)
    {
        if (obj.parent == null)
        {
            return obj.gameObject;
        }
        else
        {
            return obj.parent.gameObject.GetRootParent();
        }
    }
    public static GameObject GetRootParent(this GameObject obj)
    {
        if (obj.transform.parent == null)
        {
            return obj;
        }
        else
        {
            return obj.transform.parent.gameObject.GetRootParent();
        }
    }
    public static float EaseInOutQuart(float t)
    {
        return (t < 0.5f) ? 8f * Mathf.Pow(t,4f) : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;
    }
    public static float EaseIn(float t,float power)
    {
        return Mathf.Pow(t, power);
    }

    public static float EaseInOut(float t, float power)
    {
        return Mathf.Lerp(EaseIn(t, power),EaseOut(t, power),t);
    }

    public static float EaseOut(float t, float power)
    {
        return Mathf.Pow(t,1 / power);
    }

    [System.Serializable]
    public class Operators
    {
        public static implicit operator Func<bool, bool, bool>(Operators O) => O.Operation;

        public LogicalOperations LogicalOperation;
        public enum LogicalOperations
        {
            And = 1,
            Or = 2,
            Xor = 3,
            Nand = -1,
            Nor = -2,
            Xnor = -3,
        }
        public Func<bool,bool,bool> Operation => Getfunction((int)LogicalOperation);
        Func<bool, bool, bool> Getfunction(int op)
        {
            switch ((int)op)
            {
                case 1:
                    return And;
                case 2:
                    return Or;
                case 3:
                    return Xor;
                case -1:
                    return Nand;
                case -2:
                    return Nor;
                case -3:
                    return Xnor;
                default:
                    return And;

            }
        }

        public static readonly Func<bool, bool, bool> And = (a, b) => a && b;
        public static readonly Func<bool, bool, bool> Or = (a, b) => a || b;
        public static readonly Func<bool, bool, bool> Xor = (a, b) => a ^ b;
        public static readonly Func<bool, bool, bool> Nand = (a, b) => !(a && b);
        public static readonly Func<bool, bool, bool> Nor = (a, b) => !(a || b);
        public static readonly Func<bool, bool, bool> Xnor = (a, b) => !(a ^ b);


    }





    public static bool HasComponent(this GameObject obj,System.Type type)
    {
        var component = obj.GetComponent(type);

        if (component == null)
        {
            return false;
        }

        return true;
    }

    public static RaycastHit2D RaycastByTypes(IEnumerable<System.Type> types, Func<bool, bool, bool> inputoperator, Vector3 origin, Vector3 direction, float distance)
    {
        //returns an array of all the hits with objects of type T that the ray encountered

        RaycastHit2D[] Hits = Physics2D.RaycastAll(origin, direction, distance);
        RaycastHit2D Output = new();

        for (int i = 0; i < Hits.Length; i++)
        {
            bool allMatch = true;
            bool anyMatches = false;

            foreach (System.Type type in types)
            {
                bool HasCurrentType = Hits[i].collider.gameObject.HasComponent(type);

                //Debug.Log("CurrentType",Hits[i].collider.gameObject);

                if (HasCurrentType)
                {
                    //Debug.Log("HasCurrentType");
                    anyMatches = true;
                }

                if (HasCurrentType && allMatch)
                {
                    allMatch = true;
                }
                else
                {
                    allMatch = false;
                }
            }
            if (inputoperator(anyMatches, allMatch))
            {
                return Output = Hits[i];
            }
        }

        return Output;
    }


    public static RaycastHit2D[] RaycastByTypesAll(IEnumerable<System.Type> types, Func<bool,bool,bool> inputoperator, Vector3 origin, Vector3 direction, float distance)
    {
        //returns an array of all the hits with objects of type T that the ray encountered
        RaycastHit2D[] Hits = Physics2D.RaycastAll(origin, direction, distance);
        List<RaycastHit2D> Output = new();
      
        for (int i = 0; i < Hits.Length; i++)
        {
            bool allMatch = true;
            bool anyMatches = false;

            foreach (System.Type type in types)
            {
                bool HasCurrentType = Hits[i].collider.gameObject.HasComponent(type);

                if (HasCurrentType)
                {
                    anyMatches = true;
                }

                if (HasCurrentType && allMatch)
                {
                    allMatch = true;
                }
                else
                {
                    allMatch = false;

                }
            }

            if (inputoperator(anyMatches,allMatch))
            {
                Output.Add(Hits[i]);
            }
        }

        return Output.ToArray();
    }

    public static RaycastHit2D[] RaycastByTypeAll<T>(Vector3 origin, Vector3 direction, float distance)
    {
        //returns an array of all the hits with objects of type T that the ray encountered

        RaycastHit2D[] Hits = Physics2D.RaycastAll(origin, direction, distance);
        List<RaycastHit2D> Output = new();

        for (int i = 0; i < Hits.Length; i++)
        {
            if (Hits[i].collider.TryGetComponent(out T _))
            {
                Output.Add(Hits[i]);
            }
        }

        if (Output.Count > 0)
        {
            return Output.ToArray();
        }

        return null;
    }

    public static RaycastHit2D[] RaycastByTypeAll<T>(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
    {
        //returns an array of all the hits with objects of type T that the ray encountered

        RaycastHit2D[] Hits = Physics2D.RaycastAll(origin, direction, distance, layerMask);
        List<RaycastHit2D> Output = new();

        for (int i = 0; i < Hits.Length; i++)
        {
            if (Hits[i].collider.TryGetComponent(out T _))
            {
                Output.Add(Hits[i]);
            }
        }

        if (Output.Count > 0)
        {
            return Output.ToArray();
        }

        return null;
    }

    public static RaycastHit2D RaycastByType<T>(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
    {
        //returns the hit at the first object type T that the ray encountered

        RaycastHit2D[] Hits = Physics2D.RaycastAll(origin, direction, distance, layerMask);
        RaycastHit2D Output = new RaycastHit2D();

        for (int i = 0; i < Hits.Length; i++)
        {
            if (Hits[i].collider.TryGetComponent(out T _))
            {
                Output = Hits[i];
                return Output;
            }
        }
        return Output;
    }

    public static RaycastHit2D RaycastByType<T>(Vector3 origin, Vector3 direction,float distance)
    {
        //returns the hit at the first object type T that the ray encountered


        RaycastHit2D[] Hits = Physics2D.RaycastAll(origin, direction,distance);
        RaycastHit2D Output = new RaycastHit2D();

        for (int i = 0; i < Hits.Length; i++)
        {
            if (Hits[i].collider.TryGetComponent(out T _))
            {
                Output = Hits[i];
                return Output;
            }
        }
        return Output;
    }


    public static Vector2 IntersectionPoint(Vector2 StartPos1, Vector2 Direction1, Vector2 StartPos2, Vector2 Direction2)
    {
        Vector2 Intersection = Vector2.zero;
        float Slope1 = Direction1.y / Direction1.x;
        float Slope2 = (Direction2.y / Direction2.x);

        Intersection.x = 0;

        if (Direction1.x != 0 && Direction2.x != 0 && Slope1 != Slope2)
        {
            // original: Intersection.x = -1 / ((1 - 1 * (Slope1) / Slope2) / ((-Offset1.y + Offset2.y) / Slope2 - Offset2.x + (Offset1.x * Slope1 / Slope2)));

            // this equation is the result of an idiot perfoming algebra in notepad to solve a system of linear equations for "x"
            // said idiot was not used to doing algebra with 6 different variables in a situation that prevents the idiot from resolving terms to decimal values
            // idiot is also unable to re-organise/simplify this to make it more readable...
            // lastly idiot is too stubborn to paste this into chat-GPT to have it fix the algebra for them so, making the good assumption that it can be improved, be comforted in the knowledge that it wont
            Intersection.x = -1 / ((1 - Slope1 / Slope2) / ((-StartPos1.y + StartPos2.y) / Slope2 + (StartPos1.x * Slope1 / Slope2) - StartPos2.x));
            Intersection.y = (Direction1.y / Direction1.x) * (Intersection.x - StartPos1.x) + StartPos1.y;
        }
        else if (Direction1.x == 0)
        {
            Intersection.x = StartPos1.x;
            Intersection.y = Slope2 * (Intersection.x - StartPos2.x) + StartPos2.y;


        }
        else if (Direction2.x == 0)
        {
            Intersection.x = StartPos2.x;
            Intersection.y = Slope1 * (Intersection.x - StartPos1.x) + StartPos1.y;
        }
        else
        {
            Debug.LogWarning("Parralel Lines Never Meet");
        }

        return Intersection;
    }


    public static bool ToBool(this int inint)
    {
        return (inint != 0) ? true : false;

    }

    public static int ToInt(this bool inbool)
    {
        return (inbool) ? 1 : 0;

    }


    [Obsolete]
    public static bool IntToBool(int inint)
    {
        return (inint == 1) ? true : false;
    }
    [Obsolete]
    public static int BoolToInt(bool inbool)
    {
        return (inbool) ? 1 : 0;
    }
    public interface IAngle
    {
        float FloatValue { get; }
    }


    [System.Serializable]
    public struct Timer
    {
        public bool Clamp;
        public float EndTime;
        public float Time;
        public float Ratio => GetRatio();
        public void Step(float stepTime)
        {
            Time += stepTime;

            if (Clamp)
            {
                Time = Mathf.Clamp(Time, 0, EndTime);
            }
            else
            {
                if (EndTime != 0 && Time >= EndTime)
                {
                    Time %= EndTime;
                }
            }
           
        }
      
        public void Step()
        {
            Time += UnityEngine.Time.deltaTime;

            if (Clamp)
            {
                Time = Mathf.Clamp(Time, 0, EndTime);
            }
            else
            {
                if (EndTime != 0 && Time >= EndTime)
                {
                    Time %= EndTime;
                }
            }
           
        }
        public float GetRatio()
        {
            float ratio = 0;

            if (EndTime!=0)
            {
                ratio = Time / EndTime;
            }

            return ratio;
        }
        public Timer(float endTime, float time, bool isClamped)
        {
            Clamp = isClamped;
            EndTime = endTime;
            Time = time;
        }
        public Timer(float endTime, float time)
        {
            Clamp = false;
            EndTime = endTime;
            Time = time;
        }

        public Timer(float endTime)
        {
            Clamp = false;
            EndTime = endTime;
            Time = 0;
        }
    }

    
    static public Color LerpColor(Color c1, Color c2, float t)
    {
        return (Color)LerpVector(c1, c2, t);
    }

    static public Vector2 RoundSnap(Vector2 value, float snap)
    {
        if (snap > 0.01f)
        {
            return new Vector2(Mathf.RoundToInt(value.x / snap), Mathf.RoundToInt(value.y / snap)) * snap;
        }
        else
        {
            return value;
        }
    }

    static public float RoundSnap(float value, float snap)
    {
        if (snap > 0.01f)
        {
            return Mathf.Floor(value / snap) * snap;
        }
        else
        {
            return value;
        }
    }

    static public void RenderRayRenderer(RayRenderer rayRenderer)
    {
        Gizmos.DrawRay(rayRenderer.RayToRender.origin, rayRenderer.RayToRender.direction * rayRenderer.Distance);

        

    }
    static public void RenderRayRenderer(List<RayRenderer> rayRenderers,Color targetColor)
    {
        if (rayRenderers == null)
        {
            return;
        }
        int count = rayRenderers.Count;
        if (count > 0)
        {
            for (int i = 0; i < rayRenderers.Count; i++)
            {
                Gizmos.color = LerpColor(new Color(rayRenderers[i].RenderColor.r, rayRenderers[i].RenderColor.g, rayRenderers[i].RenderColor.b),targetColor,Mathf.InverseLerp(0, count,i));
                Gizmos.DrawRay(rayRenderers[i].RayToRender.origin, rayRenderers[i].RayToRender.direction * rayRenderers[i].Distance);

            }
           
        }

    }


    static public void RenderRayToLineRenderer(List<RayRenderer> rayRenderers,LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = rayRenderers.Count + 6;
        lineRenderer.SetPositions(new Vector3[rayRenderers.Count +6]);
        lineRenderer.SetPosition(0, rayRenderers[0].RayToRender.origin);

        if (rayRenderers.Count > 0)
        {
            for (int i = 0; i < rayRenderers.Count; i++)
            {
                lineRenderer.SetPosition(i + 1, rayRenderers[i].RayToRender.origin + rayRenderers[i].RayToRender.direction * rayRenderers[i].Distance);
            }
        }
           
    }


    static public void RenderRayRenderer(List<RayRenderer> rayRenderers)
    {
        

            if (rayRenderers.Count > 0)
            {
                foreach (RayRenderer rayRenderer in rayRenderers)
                {
                    Gizmos.color = new Color(rayRenderer.RenderColor.r, rayRenderer.RenderColor.g, rayRenderer.RenderColor.b, 0.6f);


                    Gizmos.DrawRay(rayRenderer.RayToRender.origin, rayRenderer.RayToRender.direction * rayRenderer.Distance);
                }
            }
        
    }



    public struct RayRenderer
    {
        public Ray RayToRender;
        public float Distance;
        public Color RenderColor;

      
        public RayRenderer(Ray ray, float distance, Color color)
        {
            RenderColor = color;
            RayToRender = ray;
            Distance = distance;

        }

        public RayRenderer(Ray ray, float distance)
        {
            RenderColor = Color.cyan;
            RayToRender = ray;
            Distance = distance;
        }
    }


    public static Vector4 LerpVector(Vector4 vector1, Vector4 vector2, float t)
    {
       



        float newX = Mathf.Lerp(vector1.x, vector2.x, t);
        float newY = Mathf.Lerp(vector1.y, vector2.y, t);
        float newZ = Mathf.Lerp(vector1.z, vector2.z, t);
        float newW = Mathf.Lerp(vector1.w, vector2.w, t);



        return new Vector4(newX, newY, newZ, newW);
    }


    public static Vector3 LerpVector(Vector3 vector1, Vector3 vector2, float t)
    {
        float newX = Mathf.Lerp(vector1.x, vector2.x, t);
        float newY = Mathf.Lerp(vector1.y, vector2.y, t);
        float newZ = Mathf.Lerp(vector1.z, vector2.z, t);


        return new Vector3(newX, newY, newZ);
    }

    public static Vector2 LerpVector(Vector2 vector1, Vector2 vector2, float t)
    {
        float newX = Mathf.Lerp(vector1.x, vector2.x, t);
        float newY = Mathf.Lerp(vector1.y, vector2.y, t);


        return new Vector2(newX, newY);
    }
    public static Color Reciprocal(Color color)
    {
        return new Color(1 / color.r, 1 / color.g, 1 / color.b);
    }
    public static Vector3 Reciprocal(Vector3 vector)
    {
        return new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);
    }

    static public Vector2 Reciprocal(Vector2 vector)
    {
        return new Vector2(1 / vector.x, 1 / vector.y);
    }
    /// <returns>The product of the components of the vector</returns>
    static public float ComponentProducts(this Vector2 vector)
    {
        return vector.x * vector.y;
    }
    /// <returns>The product of the components of the vector</returns>

    static public float ComponentProducts(this Vector3 vector)
    {
        return vector.x * vector.y * vector.z;
    }
    /// <returns>The product of the components of the vector</returns>

    static public float ComponentProducts(this Vector4 vector)
    {
        return vector.x * vector.y * vector.z * vector.w;
    }
    /// <returns>The product of the components of the vector</returns>

    static public float ComponentProducts(this Vector2Int vector)
    {
        return vector.x * vector.y;
    }
    /// <returns>The product of the components of the vector</returns>

    static public float ComponentProducts(this Vector3Int vector)
    {
        return vector.x * vector.y * vector.z;
    }
    /// <returns>The product of the components of the color</returns>

    static public float ComponentProducts(this Color color)
    {
        return ComponentProducts((Vector4)color);
    }

  
    /// <returns>The product of the components (in this case the image size).</returns>
    static public float ComponentProducts(this RenderTexture rt)
    {
        if (rt == null)
        {
            Debug.LogWarning("RenderTexture is null");
            return 0;

        }
        return rt.width * rt.height;
    }


    static public float MinimumDistanceOverTimeStep(Vector2 V1Start, Vector2 V1End, Vector2 V2Start, Vector2 V2End)
    {
        return DistanceToLine(Vector2.zero, V2Start - V1Start, V2End - V1End);
    }


    static public float DistanceToLine(Vector2 p, Vector2 v1, Vector2 v2)
    {
        Vector2 a = v1 - v2;
        Vector2 b = v1 - p;

        // project vector

        float abDot = Vector2.Dot(a, b);

        float sqrlength = (a.x * a.x) + (a.y * a.y);

        float t = abDot / sqrlength;

        if (t > 1)
        {
            return Vector2.Distance(p, v2);

        }

        if (t < 0)
        {
            return Vector2.Distance(p, v1);
        }

        float2 projectedpoint = LerpVector(v1, v2, t);

        return Vector2.Distance(projectedpoint, p);


    }
    public static void DrawBox(Rect r, Vector2? OffsetPosition = null)
    {

        if (OffsetPosition == null)
        {
            OffsetPosition = Vector2.zero;
        }

        Vector2 NW = new Vector2(r.x, r.y) + (Vector2)OffsetPosition;
        Vector2 NE = new Vector2(r.x, r.y + r.height) + (Vector2)OffsetPosition;
        Vector2 SW = new Vector2(r.x + r.width, r.y) + (Vector2)OffsetPosition;
        Vector2 SE = new Vector2(r.x + r.width, r.y + r.height) + (Vector2)OffsetPosition;

        Gizmos.DrawLine(NW, NE);
        Gizmos.DrawLine(SE, NE);
        Gizmos.DrawLine(SE, SW);
        Gizmos.DrawLine(NW, SW);

    }
    public static Rect RectFromRectTransform(this RectTransform tran, float ScaleFactor = 1)
    {
 

        Rect r = tran.rect;
        r.size *= ScaleFactor;



        r.center = (Vector2)tran.transform.position;
        r.center += (r.size * 0.5f) - (r.size * tran.pivot) ;


        return r;
       

    }
    public static void DrawRectTransform(RectTransform tran, float ScaleFactor = 1)
    {
        Rect r = tran.rect;

        Vector2 pos = (Vector2)tran.transform.position;

        Vector2 dims = (new Vector2(r.width, r.height) * ScaleFactor) / 2f;
    


        Vector2 NW = -dims + r.center + pos;
        Vector2 NE = dims + r.center + pos;
        Vector2 SW = new Vector2(dims.x, -dims.y) + r.center + pos;
        Vector2 SE = new Vector2(-dims.x, dims.y) + r.center + pos;

        Gizmos.DrawLine(NW, NE);
        Gizmos.DrawLine(SE, NE);
        Gizmos.DrawLine(SE, SW);
        Gizmos.DrawLine(NW, SW);

    }




}
