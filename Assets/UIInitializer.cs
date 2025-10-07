using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Keys = UIInitializer.StorageLookUpKeys;
public class UIInitializer : MonoBehaviour
{
    [SerializeField] private TextAsset myTextFile;
    private GravityController GC;
    private BodyGenerator BG;



    [SerializeField] private SingleUI MySingleUI;
    [SerializeField] private ClusterUI MyClusterUI;
    [SerializeField] private MainUI MyMainUI;

    public static class SettingsSaveLoad
    {
        public static void Save(string Key, Color value)
        {
            PlayerPrefs.SetFloat(Key + 'r', value.r);
            PlayerPrefs.SetFloat(Key + 'g', value.g);
            PlayerPrefs.SetFloat(Key + 'b', value.b);
            PlayerPrefs.SetFloat(Key + 'a', value.a);
        }

        public static void Save(string Key, float value)
        {
            PlayerPrefs.SetFloat(Key, value);
        }

        public static void Save(string Key, int value)
        {
            PlayerPrefs.SetInt(Key, value);
        }

        public static void Save(string Key, string value)
        {
            PlayerPrefs.SetString(Key, value);
        }

        public static void Save(string Key, bool value)
        {
            PlayerPrefs.SetInt(Key, value.ToInt());
        }

        public static bool Load(string Key, ref float value)
        {
            value = PlayerPrefs.GetFloat(Key,value);

            return PlayerPrefs.HasKey(Key);

        }

        public static bool Load(string Key, ref int value)
        {
            value = PlayerPrefs.GetInt(Key, value);

            return PlayerPrefs.HasKey(Key);

        }

        public static bool Load(string Key, ref string value)
        {
            value = PlayerPrefs.GetString(Key, value);

            return PlayerPrefs.HasKey(Key);

        }


        public static bool Load(string Key, ref bool value)
        {
            value = PlayerPrefs.GetInt(Key, value.ToInt()).ToBool();

            return PlayerPrefs.HasKey(Key);

        }

        public static bool Load(string Key, ref Color value)
        {
            value.r = PlayerPrefs.GetFloat(Key + 'r',value.r);
            value.g = PlayerPrefs.GetFloat(Key + 'g',value.g);
            value.b = PlayerPrefs.GetFloat(Key + 'b',value.b);
            value.a = PlayerPrefs.GetFloat(Key + 'a',value.a);

            return PlayerPrefs.HasKey(Key+'r');

        }

   
        public static bool Load<T>(string Key,ref T value) where T : Enum
        {
             value = (T)(object)PlayerPrefs.GetInt(Key, (int)(object)value);
            return PlayerPrefs.HasKey(Key);
        }





    }


    public static class StorageLookUpKeys
    {
        //single
        public const string SingleMass = "S_Mass";
        public const string SingleAngle = "S_Angle";
        public const string SingleSpeed = "S_Speed";
        public const string SingleRandomAngle = "S_R_Angle";
        //cluster
        public const string ClusterMinMass = "C_Min_Mass";
        public const string ClusterMaxMass = "C_Max_Mass";
        public const string ClusterInitVel = "C_Init_Vel";
        public const string ClusterOrbitalVel = "C_Orbit_Vel";
        public const string ClusterSpread = "C_Spread";
        public const string ClusterParticleCount = "C_P_Count";
        //main
        public const string MainWorldWidth = "M_W_W";
        public const string MainWorldHeight = "M_W_H";
        public const string MainAutoRecenter = "M_A_Recenter";
        public const string MainBigG = "M_BigG";
        public const string MainTrailColor= "M_Trail_Color";
        public const string MainMaxParticles = "M_Max_Particles";
        public const string MainEdgeBehavior = "M_Edge_Behavior";
        public const string MainNormalizeVelocity = "M_Normalize_Vel";


    }

    [System.Serializable]
    private class SingleUI
    {
        public TMP_InputField TMP_Mass;
        public TMP_InputField TMP_Speed;
        public TMP_InputField TMP_Angle;
        public Toggle RandomAngle;
    }
    [System.Serializable]
    private class ClusterUI
    {
        public TMP_InputField TMP_MinMass;
        public TMP_InputField TMP_MaxMass;
        public TMP_InputField TMP_InitVel;
        public TMP_InputField TMP_OrbitVel;
        public TMP_InputField TMP_Spread;
        public TMP_InputField TMP_ParticleCount;
    }

    [System.Serializable]
    private class MainUI
    {
        public TMP_InputField TMP_WorldWidth;
        public TMP_InputField TMP_WorldHeight;
        public TMP_InputField TMP_BigG;
        public TMP_InputField TMP_MaxParticles;
        public TMP_Dropdown TMP_EdgeBehavior;

        public Toggle AutoRecenter;
        public Toggle NormalizeVel;

        public ColorPickerInterfaceUI ColorPicker;
        public TextMeshProUGUI TMP_Help;


    }

    private void OnValidate()
    {
        MyMainUI.TMP_Help.text = myTextFile.text;

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        GC = FindAnyObjectByType<GravityController>();
        BG = GC.gameObject.GetComponent<BodyGenerator>();

        //Cluster Settings

        SettingsSaveLoad.Load(Keys.ClusterMinMass, ref BG.cluster.MinMass);
        MyClusterUI.TMP_MinMass.text = BG.cluster.MinMass.ToString();

        SettingsSaveLoad.Load(Keys.ClusterMaxMass, ref BG.cluster.MaxMass);
        MyClusterUI.TMP_MaxMass.text = BG.cluster.MaxMass.ToString();

        SettingsSaveLoad.Load(Keys.ClusterInitVel, ref BG.cluster.MaxVelocity);
        MyClusterUI.TMP_InitVel.text = BG.cluster.MaxVelocity.ToString();

        SettingsSaveLoad.Load(Keys.ClusterOrbitalVel, ref BG.cluster.BaseOrbitalVelocity);
        MyClusterUI.TMP_OrbitVel.text = BG.cluster.BaseOrbitalVelocity.ToString();

        SettingsSaveLoad.Load(Keys.ClusterSpread, ref BG.cluster.Size) ;
        MyClusterUI.TMP_Spread.text = BG.cluster.Size.ToString();

        SettingsSaveLoad.Load(Keys.ClusterParticleCount, ref BG.cluster.BodiesCount);
        MyClusterUI.TMP_ParticleCount.text = BG.cluster.BodiesCount.ToString();

        //Single Settings

        SettingsSaveLoad.Load(Keys.SingleMass, ref BG.single.Mass);
        MySingleUI.TMP_Mass.text = BG.single.Mass.ToString();

        SettingsSaveLoad.Load(Keys.SingleAngle, ref BG.single.Angle);
        MySingleUI.TMP_Angle.text = BG.single.Angle.ToString();

        SettingsSaveLoad.Load(Keys.SingleSpeed, ref BG.single.Speed);
        MySingleUI.TMP_Speed.text = BG.single.Speed.ToString();

        SettingsSaveLoad.Load(Keys.SingleRandomAngle, ref BG.single.RandomizeDir);
        MySingleUI.RandomAngle.isOn = BG.single.RandomizeDir;

        // main settings
        SettingsSaveLoad.Load(Keys.MainWorldWidth, ref GC.SimulationSize.x);
        MyMainUI.TMP_WorldWidth.text = GC.SimulationSize.x.ToString();

        SettingsSaveLoad.Load(Keys.MainWorldHeight, ref GC.SimulationSize.y);
        MyMainUI.TMP_WorldHeight.text = GC.SimulationSize.y.ToString();

        SettingsSaveLoad.Load(Keys.MainBigG, ref GC.BigG);
        MyMainUI.TMP_BigG.text = GC.BigG.ToString();

        SettingsSaveLoad.Load(Keys.MainAutoRecenter, ref GC.CenterOnCOM);
        MyMainUI.AutoRecenter.isOn = GC.CenterOnCOM;

        SettingsSaveLoad.Load(Keys.MainNormalizeVelocity, ref GC.NormalizeVelocity);
        MyMainUI.NormalizeVel.isOn = GC.NormalizeVelocity;

        
        SettingsSaveLoad.Load(Keys.MainMaxParticles, ref GravityController.MaxParticles);
        MyMainUI.TMP_MaxParticles.text = GravityController.MaxParticles.ToString();

        SettingsSaveLoad.Load(Keys.MainEdgeBehavior, ref GravityController.EdgeBehavior);
        MyMainUI.TMP_EdgeBehavior.value = (int)GravityController.EdgeBehavior;







        SettingsSaveLoad.Load(Keys.MainTrailColor, ref GC.FadeColor);
   

        MyMainUI.ColorPicker.UpdateDisplayColor();

    }

}
