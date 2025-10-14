using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class Loomingdarkening : MonoBehaviour
{
    //loom constant
    public GameObject predator;
    public float velocity = 25f;
    public float loom_yaw_angle = 45f;
    public float loom_pitch_angle = 45f;
    public float loom_start_distance = 20f;
    public float loom_stop_distance = .9f;
    public float trial_duration = 10f;
    public float predelay = 5f;
    public float postdelay = 5f;
    public int num_trials = 4;
    public int num_reps = 5;
    public string scene_name = "loom";
    public bool force_first_trial = true;
    public int[] forced_first_trial_order = { 0, 3, 1, 2 }; //[moveL, 1=darkL, 2=moveR, 3=darkR]
    public bool shiftback = false;
    public bool predatorshift = false;
    public bool updateplayerrotation = false;
    public Vector3 shiftback_vector = new Vector3(0f, 0f, 0f);
    public Vector3 predator_vector = new Vector3(0f, 0f, 0f);

    //loom variables
    public int lv_direction = 0;
    private float xscale = 0f;
    private float yscale = 0f;
    private float zscale = 0f;
    private float maxz = 0.25f; //should be whatever is the spatial wavelength of the floor checkerboard

    //mouse moving 
    public GameObject Player;
    public ArduinoInterface arduinoInterface;
    //movement senitivites
    public float alpha;
    public float speed = 0.01f;
    public float smoothedSpeed = 0.0f;
    public float smoothingFactor = 0.01f;

    public float distance;
    public float nextActionTime = 0.0f;
    public float nextActionTime1 = 4.0f;
    public float t1 = 0.0f;
    public float va = 0.0f;
    public float object_alpha = 1f;

    //save log
    public int current_trial_counter = 1;
    public int current_trial_index = 0;
    public int[] trial_order = { 0, 1, 2, 3 };
    public int current_rep = 1;
    public int phase = 1; //1 = predelay, 2 = loom/darkening, 3 = postdelay
    public float predator_alpha = 1f;
    List<float> yawLog = new List<float>();
    List<float> pitchLog = new List<float>();
    List<float> rollLog = new List<float>();
    List<float> trialLog = new List<float>();
    List<float> distLog = new List<float>();
    List<float> timeLog = new List<float>();
    public string timestamp = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
    // Start is called before the first frame update


    void Start()
    {
        updateplayerrotation = true;

        trial_order.Shuffle();
        if (force_first_trial)
        {
            trial_order = forced_first_trial_order;
        }
        current_trial_counter = 1;
        current_trial_index = trial_order[current_trial_counter - 1];

        zscale = Mathf.Cos(Mathf.Deg2Rad * loom_pitch_angle) * Mathf.Sin(Mathf.Deg2Rad * loom_yaw_angle);
        yscale = Mathf.Sin(Mathf.Deg2Rad * loom_pitch_angle);
        xscale = Mathf.Cos(Mathf.Deg2Rad * loom_pitch_angle) * Mathf.Cos(Mathf.Deg2Rad * loom_yaw_angle);
        Debug.Log(timestamp);

        phase = 1;
        nextActionTime = predelay;


    }

    public void save_logs(int current_rep, List<float> col1, List<float> col2, List<float> col3, List<float> col4, List<float> col5, List<float> col6)
    {
        string filepath = "D:\\HongyuChang\\UnityLogs\\" + timestamp + "_" + scene_name + "_rep" + current_rep.ToString() + "_unitylogs.txt";
        FileStream f = new FileStream(filepath, FileMode.OpenOrCreate);
        StreamWriter s = new StreamWriter(f);

        foreach (var i in Enumerable.Range(0, (col1.Count() - 1)))
        {
            s.WriteLine((col1[i].ToString() + "," + col2[i].ToString() + "," + col3[i].ToString() + "," + col4[i].ToString() + "," + col5[i].ToString() + "," + col6[i].ToString() + "\r"));
        }
        s.Close();
        f.Close();

    }

    //Update is called once per frame
    void Update()
    {
        if (phase == 1 || phase == 3)
        {
            predatorshift = true;
            predator_vector = new Vector3(0f, 0f, -10f);
        }
        if (phase == 2)
        {
            distance = distance - (velocity * Time.deltaTime);
            if (current_trial_index == 0 || current_trial_index == 2) //loom trials
            {
                //predator.transform.position = Player.transform.position + new Vector3(distance * xscale * lv_direction, distance * yscale, distance * zscale);
                predatorshift = true;
                predator_vector = new Vector3(distance * xscale * lv_direction, distance * yscale, distance * zscale);
            }
            else //sky darkening control trials
            {
                va = Mathf.Atan2(loom_start_distance/40, distance);
                object_alpha = 1f - (0.783f * (1f - ((va * va) / (0.5094f))));
                predator.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, object_alpha);
                predatorshift = true;
                predator_vector = new Vector3(loom_stop_distance * xscale * lv_direction, loom_stop_distance * yscale, loom_stop_distance * zscale);
            }
            if (distance < loom_stop_distance) //end of stimulus
            {
                //do postdelay before starting a new trial
                object_alpha = 1f;
                predator.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, object_alpha);
                //predator.transform.position = Player.transform.position + new Vector3(0f, 0f, -10f);
                predatorshift = true;
                predator_vector = new Vector3(0f, 0f, -10f);
                phase = 3;
                nextActionTime = nextActionTime + postdelay;
            }

        }

        t1 += Time.deltaTime;
        if (t1 >= nextActionTime)
        {
            if (phase == 1)
            {
                phase = 2;
                Debug.Log(current_trial_index);
                distance = loom_start_distance;
                if (current_trial_index > 1)
                {
                    lv_direction = 1;
                }
                else
                {
                    lv_direction = -1;
                }
                if (current_trial_index == 0 || current_trial_index == 2)
                {
                    predatorshift = true;
                    predator_vector = new Vector3(loom_start_distance * xscale * lv_direction, loom_start_distance * yscale, loom_start_distance * zscale);
                    //predator.transform.position = Player.transform.position + new Vector3(loom_start_distance * xscale * lv_direction, loom_start_distance * yscale, loom_start_distance * zscale);
                    object_alpha = 1;
                }
                else
                {
                    predatorshift = true;
                    predator_vector = new Vector3(loom_stop_distance * xscale * lv_direction, loom_stop_distance * yscale, loom_stop_distance * zscale);
                    //predator.transform.position = Player.transform.position + new Vector3(loom_stop_distance * xscale * lv_direction, loom_stop_distance * yscale, loom_stop_distance * zscale);
                    object_alpha = 0;
                }
                //Color color=predator.GetComponent<MeshRenderer>().material.color;
                //color.a = object_alpha;
                //predator.GetComponent<MeshRenderer>().material.color = color;
                predator.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, object_alpha);
                predator.transform.eulerAngles = new Vector3(loom_pitch_angle, lv_direction * loom_yaw_angle, 0f);

            }
            if (phase == 3)
            {
                phase = 1;
                nextActionTime = nextActionTime + predelay;
                current_trial_counter++;

                if (current_trial_counter > num_trials)
                {
                    save_logs(current_rep, yawLog, pitchLog, rollLog, trialLog, distLog, timeLog);
                    current_trial_counter = 1;
                    trial_order.Shuffle();
                    current_rep++;
                    Debug.Log("Saving data");
                    yawLog.Clear();
                    pitchLog.Clear();
                    rollLog.Clear();
                    trialLog.Clear();
                    distLog.Clear();
                    timeLog.Clear();
                }

                current_trial_index = trial_order[current_trial_counter - 1];
            }

        }

        

        if (current_rep > num_reps)
        {
            Quit();
        }

        //let mouse move (forward/backward only)
        //speed = arduinoInterface.Rx;
        //float dt = Time.deltaTime;
        //alpha = Mathf.Pow(smoothingFactor, dt / (1 / 120.0f));
        //smoothedSpeed = alpha * smoothedSpeed + (1.0f - alpha) * speed;
        //Player.transform.Translate(Vector3.forward * speed * Time.deltaTime);//move player view
        if (Player.transform.position.z > maxz)
        {
            shiftback = true;
            shiftback_vector.x = 0f;
            shiftback_vector.y = 0f;
            shiftback_vector.z = -2f * maxz;
        }
        if (Player.transform.position.z < -maxz) 
        {
            shiftback = true;
            shiftback_vector.x = 0f;
            shiftback_vector.y = 0f;
            shiftback_vector.z = 2f * maxz;
        }
        //save data
        yawLog.Add(arduinoInterface.Rx);
        pitchLog.Add(arduinoInterface.Ry);
        rollLog.Add(arduinoInterface.Rz);
        trialLog.Add(current_trial_index);
        distLog.Add(distance);
        timeLog.Add(t1);

    }
    public void Quit()
    {
#if UNITY_STANDALONE
                Application.Quit();
#endif
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}




