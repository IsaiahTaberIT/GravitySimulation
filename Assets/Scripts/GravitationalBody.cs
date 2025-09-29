
using UnityEngine;
using UnityEngine.UIElements;
public class GravitationalBody
{
    public Vector2 Position;
    public Vector2 LastPosition;
    public float Mass;
    public Vector2 Velocity;
    public bool DestroyedFlag = false;
    public GravityController GC;
    public Vector3 InitialVelocity;
    public float Radius = 1;
    public const float BaseScale = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GravitationalBody(GravityController gc, Vector2 position, float mass, Vector2 initialVelocity)
    {
        Position = position;
        LastPosition = position;
        Mass = mass;
        GC = gc;
        InitialVelocity = initialVelocity;

        
        Init();
    }


    public void AddForce(Vector2 Force)
    {
        if (GC == null)
        {
            return;
        }

        Velocity += new Vector2(Force.x / (Mass * GC.BaseInertia), Force.y / (Mass * GC.BaseInertia));
    }

    public void Step()
    {
        LastPosition = Position;
        Position += Velocity;
    }


 

    private void Init()
    {
        Velocity = InitialVelocity;
        //Debug.Log("created");
        RecalculateSize();

    }


    public static float MassFromRadius(float radius)
    {
        return Mathf.Pow(radius / BaseScale,3f) / Mathf.PI;
    }

    public static float RadiusFromMass(float mass)
    {
        return Mathf.Pow(Mathf.PI * mass, 1 / 3f) * BaseScale;
    }

    public void RecalculateSize()
    {
        Radius = RadiusFromMass(Mass);
    }

    [ContextMenu("Arrest")]
    public void Arrest()
    {
       Velocity = Vector2.zero;
    }





    public void Split(int Other, Vector2 Momentum1, Vector2 Momentum2)
    {
     //   float ratioOfInit = 4;
        float InitRadius = Radius * 1.5f;
        float CombinedMass = GC.Bodies[Other].Mass + this.Mass;
        float RemainingMass = CombinedMass;

        Vector2 CombinedMomentum = Momentum1 + Momentum2;
        Vector2 RemainingMomentum = CombinedMomentum;
        Vector2 MomentumDifference = CombinedMomentum - Momentum1;
        Vector2 dir = Vector2.zero;

        dir.x = (float)(GC.RNG.NextDouble() - 0.5);
        dir.y = (float)(GC.RNG.NextDouble() - 0.5);
        dir = dir.normalized;

        float m1 = Mass;
        float m2 = GC.Bodies[Other].Mass;
        Vector2 vs = CombinedMomentum / CombinedMass;
        Vector2 p = CombinedMomentum;

        float n = m2 * GC.Bodies[Other].Velocity.sqrMagnitude * 0.5f + m1 * Velocity.sqrMagnitude * 0.5f;
        float s = (m1 * Vector2.Dot(p - m1 * vs, dir) + Mathf.Sqrt(m1 * m1 * Mathf.Pow(Vector2.Dot(p - m1 * vs, dir), 2)- (Vector2.Dot(dir, dir)) * m1 * (m1 + m2) * (Vector2.Dot(p - m1 * vs, p - m1 * vs) - 2 * n * m2))) / ((Vector2.Dot(dir, dir)) * m1 * (m1 + m2));
        Vector2 v2 = ((p - m1 * ((s * dir) + vs)) / m2 - vs);

        Velocity = s * dir;
        GC.Bodies[Other].Velocity = v2;
        GC.Bodies[Other].Position = Position + GC.Bodies[Other].Velocity.normalized * (GC.Bodies[Other].Radius + Radius);
        GC.Bodies[Other].LastPosition = GC.Bodies[Other].Position;
        LastPosition = Position;

        Debug.Log("old: " + n);
        n = GC.Bodies[Other].Mass * GC.Bodies[Other].Velocity.sqrMagnitude * 0.5f + Mass * Velocity.sqrMagnitude * 0.5f;
        Debug.Log("new: " + n);
        GC.Bodies[Other].RecalculateSize();
        /*
        vs = s * dir;

        dir.x = (float)(GC.RNG.NextDouble() - 0.5);
        dir.y = (float)(GC.RNG.NextDouble() - 0.5);
        dir = dir.normalized;

        m2 = m1 / 10f;
        m1 -= m1 / 10f;
        

        n -= GC.Bodies[Other].Velocity.sqrMagnitude * 0.5f * GC.Bodies[Other].Mass;

        s = (m1 * Vector2.Dot(p - m1 * vs, dir) + Mathf.Sqrt(m1 * m1 * Mathf.Pow(Vector2.Dot(p - m1 * vs, dir), 2) - (Vector2.Dot(dir, dir)) * m1 * (m1 + m2) * (Vector2.Dot(p - m1 * vs, p - m1 * vs) - 2 * n * m2))) / ((Vector2.Dot(dir, dir)) * m1 * (m1 + m2));
        v2 = ((p - m1 * ((s * dir) + vs)) / m2 - vs);

        GravitationalBody g;
        g = new(GC, Position + v2.normalized * 2 * Radius, m2, v2);

        Velocity = s * dir;
        Mass = m1;
        LastPosition = Position;


        /*


        int shares = Mathf.CeilToInt(2 + MomentumDifference.magnitude / 10);
        shares = Math.Clamp(shares, 3, 25);

      //  Debug.Log(shares);
        int MaxShare = 5;
        float TotalShareMass = 0;

        int[] shareMomentum = new int[shares];
        int[] shareMass = new int[shares];


        for (int i = 0; i < shares; i++)
        {
            shareMass[i] = GC.RNG.Next(1, MaxShare);
            TotalShareMass += shareMass[i];

        }

        for (int i = 0; i < shares; i++)
        {
            float shareOfMass = CombinedMass * (shareMass[i] / TotalShareMass);

            Vector2 InitVelocity = RemainingMomentum / RemainingMass;


            RemainingMass -= shareOfMass;


            Vector2 RanVelocity = new((float)(GC.RNG.NextDouble() - 0.5) * ratioOfInit * InitVelocity.x, (float)(GC.RNG.NextDouble() - 0.5) * ratioOfInit * InitVelocity.y);

            Vector2 newVel = InitVelocity + RanVelocity;

            RemainingMomentum -= newVel * shareOfMass;

            //it's apparently easier just to reuse the existing objects instead of deleting them


            if (i == 0)
            {
                
                Velocity = newVel;
                Mass = shareOfMass;
                LastPosition = Position;
                RecalculateSize();


                continue;
            }

            if (i == 1)
            {

                GC.Bodies[Other].Velocity = newVel;
                GC.Bodies[Other].Position = Position + newVel.normalized * InitRadius * 2;
                GC.Bodies[Other].Mass = shareOfMass;
                GC.Bodies[Other].RecalculateSize();
                GC.Bodies[Other].LastPosition = GC.Bodies[Other].Position;
         

                continue;
            }


            GravitationalBody g;

            if (i == shares - 1)
            {
                 g = new(GC, Position + InitVelocity.normalized * InitRadius * 2, shareOfMass, InitVelocity);

            }
            else
            {
                 g = new(GC, Position + newVel.normalized * InitRadius * 2, shareOfMass, newVel);

            }




            GC.Bodies.Add(g);
        }

        */
    }
    public void Merge(int Other,Vector3 Momentum1, Vector3 Momentum2)
    {
       // Debug.Log(Other);

        Vector3 newVelocity = (Momentum1 + Momentum2) / (GC.Bodies[Other].Mass + Mass);

        Position = Position * Mass + GC.Bodies[Other].Position * GC.Bodies[Other].Mass;
        Mass += GC.Bodies[Other].Mass;

        Position /= Mass;

        Velocity = newVelocity;
        Radius = Mathf.Sqrt(3.141592f * Mass * BaseScale);
        RecalculateSize();

        if (GC.BodyRef.Body == GC.Bodies[Other])
        {
            GC.BodyRef.Body = this;
        }

        


        GC.Bodies[Other].DestroyedFlag = true;
        GC.Bodies[Other] = null;



    }

    void Bounce(Vector2 position)
    {
        if (position.x < 0 || position.x > 1)
        {
            Velocity.x *= -1f;
        }

        if (position.y < 0 || position.y > 1)
        {

            Velocity.y *= -1f;
        }

        Position = Logic.ClampVector(Position, -GC.SimulationSize * 0.5f, GC.SimulationSize * 0.5f);
    }

    void Wrap(Vector2 simSpacePos)
    {
        
        // check to see if oob and wrap last pos and pos to the correct values
        // so my fancy collision detection doesnt wig out

        if (simSpacePos.x < 0 || simSpacePos.x > 1)
        {
            LastPosition.x = Mathf.Sign(Position.x) * -GC.SimulationSize.x / 2f;
            Position.x -= GC.SimulationSize.x * Mathf.Sign(Position.x);

        }

        if (simSpacePos.y < 0 || simSpacePos.y > 1)
        {
            LastPosition.y = Mathf.Sign(Position.y) * -GC.SimulationSize.y / 2f;
            Position.y -= GC.SimulationSize.y * Mathf.Sign(Position.y);
        }
      
    }

    public void HandleBoundsCollision()
    {
        if (GC == null)
        {
            return;
        }

        // convert from unity world space to simulation space

        Vector2 simSpacePos = Vector2.Scale(Position, GC.WorldToSimulation) + new Vector2(0.5f, 0.5f);
       
        switch (GravityController.EdgeBehavior)
        {
            case GravityController.EdgeCollisionBehavior.Wrap:
                Wrap(simSpacePos);
                break;
            case GravityController.EdgeCollisionBehavior.Bounce:
                Bounce(simSpacePos);
                break;



            default:
                break;
        }






    }


    public void HandleCollision(int Other,int This)
    {
        //this check is here because sometimes another collision could have destoryed a body
        //before it got to it here

        if (GC.Bodies[Other] == null)
        {
            return;
        }


        float CombinedMass = GC.Bodies[Other].Mass + Mass;
        Vector3 Momentum1 = this.Velocity * this.Mass;
        Vector3 Momentum2 = GC.Bodies[Other].Velocity * GC.Bodies[Other].Mass;
        Vector3 ImpulseHeuristic = (this.Mass + GC.Bodies[Other].Mass) * (GC.Bodies[Other].Velocity - this.Velocity);
        bool alwaystrue = true;

        if (ImpulseHeuristic.magnitude < GC.BounceThreshold)
        {
            // bounce
        }
        else if (alwaystrue)
        {
            //MathF.Sqrt(ImpulseHeuristic.magnitude) < GC.SplitTheshold || CombinedMass < 50
            // merge

            if (this.Mass >= GC.Bodies[Other].Mass)
            {
                this.Merge(Other, Momentum1, Momentum2);
            }
            else 
            {
                GC.Bodies[Other].Merge(This, Momentum1, Momentum2);
            }
        }
        else
        {
            if (this.Mass >= GC.Bodies[Other].Mass)
            {
                this.Split(Other, Momentum1, Momentum2);
            }
            else
            {
                GC.Bodies[Other].Split(This, Momentum1, Momentum2);
            }
        }



    }
}


