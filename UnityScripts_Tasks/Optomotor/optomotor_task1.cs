
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class optomotor_task1 : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 0f;
    // float duration = 60f;
    public Material SphereMaterial;
    public bool cylinder_material;
    float time;
    public float time_1 = 0f;
    public float time_task;
    public float nextActionTime = 0.0f;
    public float nextActionTime_1 = 0.0f;
    public float period_grat = 2.0f;//how long grating go
    public float period_gray = 4.0f;//how long grating go
    public int repeats_num = 0; //from 0 to 9 and reset
    public int grating_num = 0;//from 0 to 7 no rest
    public int trial_num = 0;//from 0 to 69 no reset
    private int[] turn_order = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    public EventLogger eventLogger;
    private string[] materials = { "1_deg", "2_deg", "4_deg", "6_deg", "8_deg", "12_deg", "24_deg" };
    private float[] freq_speed = { 1.0f, 2.0f, 4.0f, 6.0f, 8.0f, 12.0f, 24.0f };
    private int[] material_order = { 0, 1, 2, 3, 4, 5, 6, 7 };
    int[,] order_array = new int[7, 10];
    void Start()
    {
        SphereMaterial = Resources.Load<Material>("Materials/gray");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;

        for (int i = 0; i < 7; i++)
        {
            turn_order.Shuffle();
            for (int j = 0; j< 10; j++)
            {
                order_array[i,j] = turn_order[j];
     
            }
        }
        material_order.Shuffle();//sudorandom

    }

    // Update is called once per frame

    void Update()
    {
        time_1 += Time.deltaTime;
        time_task = Time.time;
        if (time_1 > nextActionTime)

        {

            nextActionTime += period_grat+period_gray;
            if (time_1 > nextActionTime_1)

            {
                nextActionTime_1 = nextActionTime + period_grat;
                // execute block of code here

                    speed = 1.50f*freq_speed[material_order[grating_num]]* order_array[grating_num, repeats_num];
                    SphereMaterial = Resources.Load<Material>("Materials/"+materials[material_order[grating_num]]);
                    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                    meshRenderer.material = SphereMaterial;


                if (grating_num < 7)
                {
                    grating_num++;
                }
                else
                {
                    grating_num= 0;
                    repeats_num++;
                    material_order.Shuffle();
                }
            }
            else
            {
                SphereMaterial = Resources.Load<Material>("Materials/gray");
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = SphereMaterial;
                speed =0f;
                trial_num++;
               
            }


          
        }
   
        if (trial_num>69)
        {
            Application.Quit();

        }
        else if (time_task > 900f)
        {
            Application.Quit();
        }
        transform.Rotate(0f, 0f, Time.deltaTime * speed);
        eventLogger.Add(new Event("grating_wavelength", material_order[grating_num]));
        eventLogger.Add(new Event("grating_direction", speed));
    }
}







