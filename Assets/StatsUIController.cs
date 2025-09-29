using TMPro;
using UnityEngine;
using static Logic;
using System.IO;
public class StatsUIController : MonoBehaviour
{

    private float[] _FPSSamples = new float[40];
    [SerializeField] private Timer _Timer = new Timer(0.2f,0,true);
  


    private string m_particleCount;
    public string ParticleCount
    {
        get { return m_particleCount; }
        set {
            
            m_particleCount = value;

            if (_ParticleCountUI != null)
            {
                _ParticleCountUI.text = "Particles: " + value;
            }
        }
    }
    private string m_fps;
    public string FPS
    {
        get { return m_fps; }
        set
        {
            if (_FpsUI != null)
            {
                _FpsUI.text = "FPS: " + value;

            }



            m_fps = value;
            
        }
    }

    [SerializeField] private TextMeshProUGUI _ParticleCountUI;
    [SerializeField] private TextMeshProUGUI _FpsUI;

    private void Update()
    {



        _Timer.Step();


        if (_Timer.Ratio >= 1)
        {
            _Timer.Time = 0;
            FPS = (string.Format("{0:N2}", 1 / Time.deltaTime));
        }

    

        
    }


}
