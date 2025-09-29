using UnityEngine;

public class CollisionVisualizer : MonoBehaviour
{
    public Line line1= new Line();
    public Line line2 = new Line();
    [Range(0, 1)] public float T;



    public int iterations = 50;
    public float minimum;
    public float CalcMin;
    public float width = 200;
    public bool DrawStandard;
    public bool DrawBoth;



    [System.Serializable]
    public struct Line
    {
        public Vector2 p1;
        public Vector2 p2;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector2 p = transform.position;
        float dist = 0;
        //standard behavior


        if (DrawBoth || DrawStandard)
        {

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(line1.p1 + p, line1.p2 + p);
            Gizmos.DrawSphere(Logic.LerpVector(line1.p1, line1.p2, T) + p, 10);


            Gizmos.color = Logic.LerpColor(Color.white, Color.blue, 0.8f);
            Gizmos.DrawLine(line2.p1 + p, line2.p2 + p);
            Gizmos.DrawSphere(Logic.LerpVector(line2.p1, line2.p2, T) + p, 10);

 

        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 10);

        Vector2 intersect = Logic.IntersectionPoint(line1.p1, line1.p2 - line1.p1, line2.p1, line2.p2 - line2.p1);



        //relative behavior


        if (DrawBoth || !DrawStandard)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(line2.p1 - line1.p1 + p, line2.p2 - line1.p2 + p);

            Gizmos.DrawSphere(p+Logic.LerpVector(line2.p1 - line1.p1, line2.p2 - line1.p2, T), 10);


        }



        Gizmos.color = Color.green;



        Gizmos.DrawSphere(intersect + p, 5);

        Gizmos.DrawLine(new Vector2(-1000, 20) + p, new Vector2(1000, 20) + p);


        minimum = 1000000000000f;

        for (int i = 0; i < iterations; i++)
        {
            float t = i / (float)iterations;

            if (DrawBoth || DrawStandard)
            {

                dist = (Logic.LerpVector(line1.p1, line1.p2, t) - Logic.LerpVector(line2.p1, line2.p2, t)).magnitude;
                minimum = Mathf.Min(minimum, dist);

                if (dist <= 20)
                {
                    Gizmos.color = Logic.LerpColor(Color.blue, Color.green, 0.75f);

                }
                else
                {
                    Gizmos.color = Logic.LerpColor(Color.yellow,Color.red,0.75f);

                }
                Gizmos.DrawSphere(new Vector2(t * width, dist) + p, 2);

            }




            if (DrawBoth || !DrawStandard)
            {
                dist = (Vector2.zero - Logic.LerpVector(line2.p1 - line1.p1, line2.p2 - line1.p2, t)).magnitude;
                minimum = Mathf.Min(minimum, dist);

                if (dist <= 20)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;

                }
                Gizmos.DrawSphere(new Vector2(t * width, dist) + p, 2);
            }
     
        }

        Gizmos.color = Color.blue;
        CalcMin = Logic.MinimumDistanceOverTimeStep(line1.p1 , line1.p2, line2.p1 , line2.p2);

        dist = (Logic.LerpVector(line1.p1, line1.p2, T) - Logic.LerpVector(line2.p1, line2.p2, T)).magnitude;
        Gizmos.DrawSphere(new Vector3(T * width, dist, -3) + (Vector3)p, 3);


    }

}
