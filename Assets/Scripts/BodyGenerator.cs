using System;
using TMPro;
using UnityEngine;
using static UIInitializer;
using static UnityEngine.Mathf;
using Keys = UIInitializer.StorageLookUpKeys;

public class BodyGenerator : MonoBehaviour
{
   

    public bool GenerateOnStart = false;
    public bool GenerateCluster = true;
    public int seed;
    public GravityController GC;
    public Cluster cluster;
    public Single single;


    public void Repair()
    {
        single.RecalculateInitVelocity();
    }

 

    public void ToggleRandom(UnityEngine.UI.Toggle t)
    {
        single.RandomizeDir = t.isOn;
    }

    public void SetOrbitalVelocityCluster(TMP_InputField v)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(v.text, out float vel))
        {
            cluster.BaseOrbitalVelocity = vel;
            SettingsSaveLoad.Save(Keys.ClusterOrbitalVel, vel);

        }
        else
        {
            Debug.LogError($"Invalid input: {v.text}. Please enter a valid float value.");
        }
    }

    public void SetCountCluster(TMP_InputField c)
    {
        //it should be impossible for this to fail but just in case

        if (int.TryParse(c.text, out int count))
        {
            cluster.BodiesCount = count;
            SettingsSaveLoad.Save(Keys.ClusterParticleCount, count);


        }
        else
        {
            Debug.LogError($"Invalid input: {c.text}. Please enter a valid int value.");
        }
    }
    public void SetSpreadCluster(TMP_InputField s)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(s.text, out float size))
        {
            cluster.Size = size;
            SettingsSaveLoad.Save(Keys.ClusterSpread, size);


        }
        else
        {
            Debug.LogError($"Invalid input: {s.text}. Please enter a valid float value.");
        }
    }

    public void SetMassMinCluster(TMP_InputField m)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(m.text, out float mass))
        {
            cluster.MinMass = mass;
            SettingsSaveLoad.Save(Keys.ClusterMinMass, mass);

        }
        else
        {
            Debug.LogError($"Invalid input: {m.text}. Please enter a valid float value.");
        }
    }

    public void SetMassMaxCluster(TMP_InputField m)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(m.text, out float mass))
        {
            cluster.MaxMass = mass;
            SettingsSaveLoad.Save(Keys.ClusterMaxMass, mass);

        }
        else
        {
            Debug.LogError($"Invalid input: {m.text}. Please enter a valid float value.");
        }
    }








    public void SetMassSingle(TMP_InputField m)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(m.text, out float mass))
        {
            single.Mass = mass;
            SettingsSaveLoad.Save(Keys.SingleMass, mass);

        }
        else
        {
            Debug.LogError($"Invalid input: {m.text}. Please enter a valid float value.");
        }
    }

    public void SetAngle(TMP_InputField a)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(a.text, out float angle))
        {
            single.Angle = angle;
            SettingsSaveLoad.Save(Keys.SingleAngle, angle);
        }
        else
        {
            Debug.LogError($"Invalid input: {a.text}. Please enter a valid float value.");
        }

        single.RecalculateInitVelocity();
    }

    public void SetMagnitudeSingle(TMP_InputField m)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(m.text, out float mag))
        {
            single.Speed = mag;
            SettingsSaveLoad.Save(Keys.SingleSpeed, mag);

        }
        else
        {
            Debug.LogError($"Invalid input: {m.text}. Please enter a valid float value.");
        }

        single.RecalculateInitVelocity();

    }


    public void SetMagnitudeCluster(TMP_InputField m)
    {
        //it should be impossible for this to fail but just in case

        if (float.TryParse(m.text, out float mag))
        {
            cluster.MaxVelocity = mag;
            SettingsSaveLoad.Save(Keys.ClusterInitVel, mag);

        }
        else
        {
            Debug.LogError($"Invalid input: {m.text}. Please enter a valid float value.");
        }

        single.RecalculateInitVelocity();

    }


    public void DoCluster(bool y)
    {
        GenerateCluster = y;
    }



    [ContextMenu("Generate")]
    public void Generate()
    {
        if (GC == null)
        {
            GC = FindAnyObjectByType<GravityController>();
        }
        Vector3 pos = transform.position;
        Generate(pos);
    }


    public void Generate(Vector3 pos)
    {
        if (cluster == null)
        {
            cluster = new(GC);
        }
        cluster.GC = GC;

        if (single == null)
        {
            single = new(GC);
        }

        single.GC = GC;


        if (GenerateCluster)
        {
            cluster.Generate(pos);
        }
        else
        {
            single.Generate(pos);
        }
        if (GC.Paused())
        {
            GC.ForceCollisions();

        }

    }

    [System.Serializable]

    public class Cluster
    {
        public GravityController GC;
        public int BodiesCount;
        public float Size = 10f;
        public float MinMass = 1;
        public float MaxMass = 10;
        public float MaxVelocity = 100;
        public float BaseOrbitalVelocity;
        public int Seed;

        public Cluster(GravityController gc)
        {
            GC = gc;
        }

        public void Generate(Vector3 position)
        {

         //   Debug.Log("Cluster");

            if (GC == null)
            {
                Debug.Log("Null");

            }

            Vector2 polarDir = new Vector2(1, 1);
            
            // making it spin with an irrational ratio to avoid sameness
            float startingrot = Seed / PI;

            for (int i = 0; i < BodiesCount; i++)
            {
                if (GC.Bodies.Count >= GravityController.MaxParticles)
                {
                    break;
                }



                float spacing = (i + 1) / (float)BodiesCount;
                spacing = Mathf.Sqrt(spacing) * Size;
                polarDir = new Vector2(Mathf.Sin(spacing + startingrot), Mathf.Cos(spacing + startingrot)) * spacing;
                Vector2 randomdir = new Vector2((float)GC.RNG.NextDouble() - 0.5f, (float)GC.RNG.NextDouble() - 0.5f).normalized;
                Vector2 initialVelocity = randomdir * MaxVelocity;
                Vector2 obdir = new Vector2(-position.y - polarDir.y, position.x + polarDir.x);
                obdir = obdir.normalized;
                Vector2 orbitalVelocity = (BaseOrbitalVelocity) * obdir;

                float mass = (float)(Math.Pow(GC.RNG.NextDouble(), 2)) * (MaxMass - MinMass) + MinMass;


                initialVelocity /= Mathf.Sqrt(mass);
                initialVelocity += orbitalVelocity;



                if (GC == null)
                {
                    break;
                }
                else
                {

                    GravitationalBody g = new(GC, (Vector2)position + polarDir, mass, initialVelocity);
                    GC.Bodies.Add(g);
               
                }
            }


        }

    }




    [System.Serializable]
    public class Single
    {
        public bool RandomizeDir = false;
        public GravityController GC;
        public int Seed;
        public Vector2 InitVelocity;
        public float Mass = 1;

        public float Angle;
        public float Speed;


        public void RecalculateInitVelocity(float inangle = 0)
        {
            // practicing some linear algebra here

            float radians = inangle * Deg2Rad;

            Vector2 init = Vector2.up;

            Vector2 ihat = new Vector2(Cos(radians), Sin(radians));
            Vector2 jhat = new Vector2(Cos(radians + PI / 2), Sin(radians + PI / 2));

            Vector2 result = init.x * ihat + init.y * jhat;

            InitVelocity = result * Speed;

        }

        public Single(GravityController gc)
        {
            GC = gc;
        }

        public void Generate(Vector3 pos)
        {
            if (GC.Bodies.Count >= GravityController.MaxParticles)
            {
                return;
            }

            //  Debug.Log("Single");
            if (RandomizeDir)
            {
                float angle = (float)(GC.RNG.NextDouble() * 360);
                RecalculateInitVelocity(angle);
            }
            GravitationalBody g = new GravitationalBody(GC, pos, Mass, InitVelocity);
            GC.Bodies.Add(g);
        }
    }

    private void OnEnable()
    {
        GC = FindAnyObjectByType<GravityController>();
    }

    private void Start()
    {
     
        if (GC == null)
        {
            GC = FindAnyObjectByType<GravityController>();
        }

        if (cluster == null)
        {
            cluster = new(GC);
        }

        if (single == null)
        {
            single = new(GC);
        }

        single.Seed = seed;
        cluster.Seed = seed;

        if (GenerateOnStart)
        {
            Debug.Log(transform.position);
            Generate(transform.position);
        }
    }
}
