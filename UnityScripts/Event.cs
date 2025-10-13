using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using UnityEngine;
using System;


public class Event
{
    public string code;
    public float time;
    public float[] data;
    public const string WATER_DELIVERY = "water";

    public Event(string code, float[] data, float time)
    {
        this.code = code;
        this.time = time;
        this.data = data;
    }

    public Event(string code, float[] data)
    {
        this.code = code;
        this.time = UnityEngine.Time.time;
        this.data = data;
    }

    public Event(string code, float data)
    {
        this.code = code;
        this.time = UnityEngine.Time.time;
        float[] x = { data };
        this.data = x;
    }

    public Event(string code, int data)
    {
        this.code = code;
        this.time = UnityEngine.Time.time;
        float[] x = { (float)data };
        this.data = x;
    }

    public Event(string code)
    {
        this.code = code;
        this.time = UnityEngine.Time.time;
        this.data = null;
    }

    public string Code
    {
        get { return code; }
        set { code = value; }
    }

    public float Time
    {
        get { return time; }
        set { time = value; }
    }

    public float[] Data
    {
        get { return data; }
        set { data = value; }
    }
}
