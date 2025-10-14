using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class looming : MonoBehaviour
{
    public ArduinoInterface arduinoInterface;
    public float alpha;
    public float speed=0.1f;
    public float smoothedSpeed = 0.0f;
    public float smoothingFactor = 0.01f;
    //public Gameobject predator;
    public GameObject Player;
    public GameObject predator;
    public int[] leftorright = { -1, -1, -1, -1, -1, 1, 1, 1, 1, 1 };
    public float[] pre_speed = { 125f, 250f, 500f };
    public float distance;
    private int pre_speed_num=0;
    public float nextActionTime = 0.0f;
    public float nextActionTime1= 4.0f;
    public float t1= 4.0f;
    public float walkingtime = 4.0f;
    public float loomingtime = 6.0f;
    public int repeats_num = 0;
    public int trial_num = 0;
    public EventLogger eventLogger;
    // Start is called before the first frame update
    void Start()
    {
        predator.SetActive(false);
        leftorright.Shuffle();
        pre_speed.Shuffle();
    }

    // Update is called once per frame
    void Update()
    {

        t1 += Time.deltaTime;
        if (t1 > nextActionTime1)
        {
            predator.SetActive(true);
            nextActionTime1 = nextActionTime + walkingtime;
        }
        if (t1 > nextActionTime)
        {
            predator.transform.position = new Vector3(200f, 346.4f, 200f * leftorright[repeats_num]);
            nextActionTime += walkingtime + loomingtime;
        }

        if (predator.activeSelf == true)
        { predator.transform.position = Vector3.MoveTowards(predator.transform.position, Player.transform.position, pre_speed[pre_speed_num] * Time.deltaTime);
          distance = Vector3.Distance(predator.transform.position, Player.transform.position);
            if (distance < 5.774)
            {
                predator.SetActive(false);
                eventLogger.Add(new Event("pre_speed", pre_speed[pre_speed_num]));
                eventLogger.Add(new Event("leftorright", leftorright[repeats_num]));
                eventLogger.Add(new Event("trial_num", trial_num));
                repeats_num++;
                trial_num++;

            }
        }
        if (repeats_num> 9)
         {
            repeats_num = 0;
            leftorright.Shuffle();
            pre_speed_num++;
        }
        if (trial_num > 29)
        {
            Application.Quit();
        }
        //let mouse move
        //speed = arduinoInterface.Rx;
        //float dt = Time.deltaTime;
        //alpha = Mathf.Pow(smoothingFactor, dt / (1 / 120.0f));
        //smoothedSpeed = alpha * smoothedSpeed + (1.0f - alpha) * speed;
        Player.transform.Translate(Vector3.forward * speed * Time.deltaTime);//move player view
        eventLogger.Add(new Event("t1", t1));

    }
}
