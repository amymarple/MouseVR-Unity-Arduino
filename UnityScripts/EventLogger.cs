using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class EventLogger : MonoBehaviour
{
    public static List<Event> events;

    public string mouse_ID = "-1";
    public string experiment_name = "optomotorResponseTask";
    public bool info_entered = false;

    public void Add(Event newEvent)
    {
        events.Add(newEvent);
    }

    /*
    public void addToList(string newCode, float newTime, float newData)
    {
        events.Add(new Event(newCode, newTime, newData));
    }
    */
    
    public void saveEvents()
    {
        string filePath = getPath();
        StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine("code,time,data");
        for (int i = 0; i < events.Count; ++i)
        {
            //writer.Write(events[i]);
            writer.Write(events[i].code);
            writer.Write(",");
            writer.Write(events[i].time);
            if (events[i].data != null)
            {
                for (int j = 0; j < events[i].data.Length; j++)
                {
                    writer.Write(",");
                    writer.Write(events[i].data[j]);
                }
            }
            writer.Write(System.Environment.NewLine);
        }
        writer.Flush();
        writer.Close();
    }
    
    public string getPath()
    {
        //return Application.dataPath + "/Data/" + "Saved_Events.csv";
        return "C:/Users/iellw/OneDrive/Documents/UnityEvents/" + "Saved_Events" + mouse_ID + "_" +  experiment_name + ".csv";
    }
    

    void Start()
    {
        events = new List<Event>();
        info_entered = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
