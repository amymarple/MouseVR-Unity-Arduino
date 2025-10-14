using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class single_frequency_task : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 0f;
    // float duration = 60f;
    public Material SphereMaterial;
    public bool cylinder_material;
    float time;
    public float time_1=0f;
    public float time_task;
    public int grating;
    public float angley;
    public float nextActionTime = 0.0f;
    public float nextActionTime_1 = 0.0f;
    public float period = 3.0f;
    public int trial_num = 0;
    private int[] turn_order={ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    public EventLogger eventLogger;

    void change_material_1()
    {
        SphereMaterial = Resources.Load<Material>("Materials/1_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }
    void change_material_2()
    {
        SphereMaterial = Resources.Load<Material>("Materials/2_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }
    void change_material_4()
    {
        SphereMaterial = Resources.Load<Material>("Materials/4_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }

    void change_material_6()
    {
        SphereMaterial = Resources.Load<Material>("Materials/6_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }
    void change_material_8()
    {
        SphereMaterial = Resources.Load<Material>("Materials/8_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }
    void change_material_12()
    {
        SphereMaterial = Resources.Load<Material>("Materials/12_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }
    void change_material_24()
    {
        SphereMaterial = Resources.Load<Material>("Materials/24_deg");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }

    void change_material_Gray()
    {
        SphereMaterial = Resources.Load<Material>("Materials/CylindarShaderGraphMaterial");
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = SphereMaterial;
    }


    void Start()
    {
        change_material_2();
        grating = 0;
        speed = 0;
        turn_order.Shuffle();
        foreach (var x in turn_order)
        {
            Debug.Log(x.ToString()+",");
        }

    }

    // Update is called once per frame

    void Update()
    {
        time_1 += Time.deltaTime;
        time_task = Time.time;
        if (time_1 > nextActionTime)

        {

            nextActionTime += 2 * period;
            if (time_1 > nextActionTime_1)

            {
                nextActionTime_1 = nextActionTime + period;
                // execute block of code here

                if (turn_order[trial_num] == 1)
                {
                    speed = 12f;
                    change_material_6();

                }
                else if (turn_order[trial_num] == 0)
                {
                    speed = -12f;
                    change_material_6();

                }
            }
            else
            {
                //change_material_Gray();
                speed =0f;
                trial_num++;
            }


          
        }
   
        else if (trial_num == 19)
        {
            time_1 = -12.0f;
            change_material_Gray();
            trial_num = 0;
            nextActionTime = 0f;
            nextActionTime_1 = 0f;
            turn_order.Shuffle();
        }
        else if (time_task > 900f)
        {
            Application.Quit();
        }
        transform.Rotate(0f, 0f, Time.deltaTime * speed);
        eventLogger.Add(new Event("speed", speed));
        eventLogger.Add(new Event("trial_num", trial_num));
    }
}



//public static class IListExtensions
//{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
   // public static void Shuffle<T>(this IList<T> ts)
    //{
        //var count = ts.Count;
        //var last = count - 1;
        //for (var i = 0; i < last; ++i)
       // {
          //  var r = UnityEngine.Random.Range(i, count);
        //    var tmp = ts[i];
          //  ts[i] = ts[r];
         //   ts[r] = tmp;
    //    }
 //   }
//}

