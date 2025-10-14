using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class multple_freq_2 : MonoBehaviour
{
    // Start is called before the first frame update
    // float duration = 60f;
    public bool cylinder_material;
    float time;
    public float time_1 = 0f;
    public float time_task;
    public float nextActionTime = 0.0f;
    public float nextActionTime_1 = 0.0f;
    public float trial_duration = 5.0f;
    public float inter_duration = 4.0f;
    public int[] direction = { 1, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, -1, -1 };
    public float[] spatial_wavelength = { 24.0f, 12.0f, 8.0f, 6.0f, 3.0f, 2.0f, 0.0f, 24.0f, 12.0f, 8.0f, 6.0f, 3.0f, 2.0f, 0.0f };
    public float[] speed = { 24.0f, 12.0f, 8.0f, 6.0f, 3.0f, 2.0f, 0.0f, 24.0f, 12.0f, 8.0f, 6.0f, 3.0f, 2.0f, 0.0f };
    public float current_speed = 0.0f;
    private int[] trial_order = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13};
    public int trial_index = 0;
    public int repetiton_num = 1;
    public int trial_num = 0;
    public int next_state = 0; //0 = trial, 1 = inter
    public EventLogger eventLogger;
    private int gray = 0;
    private int bright = 5;
    MeshRenderer meshRenderer;
    public float angle = 0f;


    void Start()
    {
        trial_order.Shuffle3(); //random permutation of the trial order
        trial_index = 0; //start on the 1st random trial
        trial_num = trial_order[trial_index]; //get the trial # of the 1st random trial

        //start the experiment with a gray inter trial
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetFloat("Contrast", gray);
        current_speed = 0;
        eventLogger.Add(new Event("inter_num", -1));
        next_state = 0;
        nextActionTime_1 += inter_duration;
    }

    // Update is called once per frame
    void Update()
    {
        time_1 += Time.deltaTime; // add time elapsed
        time_task = Time.time;

        //change the trial condition once enough time has passed
        if (time_1 > nextActionTime_1)
        {
           if (next_state == 0) //run the next trial
            {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (spatial_wavelength[trial_num] == 0)
                { meshRenderer.material.SetFloat("Contrast", gray); }
                else { meshRenderer.material.SetFloat("Contrast", bright);
               meshRenderer.material.SetFloat("degreesPerCycle", spatial_wavelength[trial_num]);
                }
                
                current_speed = direction[trial_num] * speed[trial_num];
                eventLogger.Add(new Event("trial_num", trial_num));
                eventLogger.Add(new Event("state", next_state));
                next_state = 1; //do an intertrial next
                nextActionTime_1 += trial_duration; //wait for the trial duration before doing the next intertrial
            }
           else //run an inter trial
            {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material.SetFloat("Contrast", gray);
                current_speed = 0;
                eventLogger.Add(new Event("inter_num", trial_num));
                eventLogger.Add(new Event("state", next_state));
                next_state = 0; //do a trial next
                nextActionTime_1 += inter_duration; //wait the intertrial duration
                trial_index++; //switch to a new trial
                if (trial_index>13) //if all trials have been completed in this repetition
                {
                    trial_index = 0; //restart the trials
                    repetiton_num++; //add to the repetition number
                    trial_order.Shuffle3(); //reshuffle the trial order to get a new random permutation
                }
                trial_num = trial_order[trial_index]; //get the next trial number

            }

        }
         
        //if desired number of repetitions has been reached
        if (repetiton_num > 15)
        {
            Application.Quit();
        }

        //update the grating angle every frame, based on the current speed
        angle += Time.deltaTime * current_speed;
        meshRenderer.material.SetFloat("gratingOffset",angle);
        
        //transform.Rotate(0f, 0f, Time.deltaTime * speed);
        eventLogger.Add(new Event("speed", current_speed));
        eventLogger.Add(new Event("repetiton_num", repetiton_num));
        eventLogger.Add(new Event("trial_index", trial_index));
    }
}



public static class IListExtensions2
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle3<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}

