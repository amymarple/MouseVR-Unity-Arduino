using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;



public class ArduinoInterface : MonoBehaviour
{

    public GameObject player;

    [Header("scales")]

    [Tooltip("Bottom sensor scale")]
    public float back_scale = 0.1f;

    public float camera_movement_ratio = 0.0f;
    public EventLogger eventLogger;

    public float scale_ratio = 1.0f;

    SerialPort stream;
    public string serial_port_name = "COM7";
    byte[] bytes = new byte[4];
    public int bytesToRead=0;

    public float Rx, Ry, Rz;
    private bool firstOffsetRead = true;
    public float percent_arduino_reads_succesful = 0.0f;
    public float percent_non_zero_reads = 0.0f;
    public float dT;
    public int arduino_updates = 0;
    float last_update_time = 0.0f;
    bool arduino_first_read_has_been_made = false;

    public float arduino_loops_since_last_grab = -1234;
    public int totalLicks = 0;

    public float error_grabs = 0.0f;
   

    // Start is called before the first frame update
    void Start()
    {

        stream = new SerialPort(serial_port_name, 115200, Parity.None, 8, StopBits.One);


        stream.ReadTimeout = 0;
        stream.WriteTimeout = 0;
        stream.Open();
        stream.ReadExisting();
        Rx = 0;
        Ry = 0;
        Rz = 0;
        last_update_time = Time.time;
        Debug.Log("SerialPortOpened!");
    }

    // Update is called once per frame
    void Update()
    {


        (bool data_available, int[] offsets) = get_arduino_data();

        percent_arduino_reads_succesful = 0.99f * percent_arduino_reads_succesful + 0.01f * (data_available ? 1.0f : 0.0f);


        if (!arduino_first_read_has_been_made && data_available)
        {
            last_update_time = Time.time;
            arduino_first_read_has_been_made = true;
        } else if (data_available && arduino_first_read_has_been_made)
        {
            float bottom_scale = back_scale * scale_ratio;
            Rx = (offsets[2] * bottom_scale ) ;
            Ry = (offsets[0] * bottom_scale );
            Rz = (0.5f * (offsets[3] * bottom_scale + offsets[1] * bottom_scale)) ;

            gameObject.transform.Rotate(Rx, Ry, Rz, Space.World);

        } 
            
    }

    void Dispose()
    {
        stream.Close();
    }

    public void deliverAirpuff()
    {
        stream.Write("a");
        eventLogger.Add(new Event("AIRPUFF"));

    }

    public void deliverWater()
    {
        stream.Write("w");
        eventLogger.Add(new Event("WATER"));
    }

    

    (bool, int[]) get_arduino_data()
    {
        bytesToRead = stream.BytesToRead;
        //stream.ReadExisting();
        if (bytesToRead <= 6*4*4)
        {
            stream.Write("h");

        } else
        {
            Debug.Log("buffer too full");
        }
        if (stream.BytesToRead >= 6*4)
        {
            if (!firstOffsetRead)
            {
                int[] x = read_ints(6);
                return (true, x);
            }
            else {
                read_ints(6);
                firstOffsetRead = false;
                int[] return_val = { 0, 0, 0, 0, 0, 0 };
                return (false, return_val);
            }
        }
        else
        {
            int[] return_val = { 0, 0, 0, 0, 0, 0};
            return (false, return_val);
        }

    }

    int[] read_ints(int n)
    {
        int[] return_val = new int[n];
        for (int i = 0; i < n; i++)
        {
            stream.Read(bytes, 0, 4);
            return_val[i] = System.BitConverter.ToInt32(bytes, 0);
        }

        return return_val;
    }

}
