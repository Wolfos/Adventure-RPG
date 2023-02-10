using System;
using System.Collections;
using Data;
using UnityEngine;


public class TimeUpdater : SaveableObject
{
    [Serializable]
    private class TimeData
    {
        public float Time;
        public int Day;
    }
    
    private void Awake()
    {
        global = true;
    }

    private void Update()
    {
        TimeManager.UpdateTime(Time.deltaTime * TimeManager.timeScale);
    }

    public override string Save()
    {
        var data = new TimeData
        {
            Time = TimeManager.Time,
            Day = TimeManager.Day
        };
        var json = JsonUtility.ToJson(data);
        return json;
    }

    public override void Load(string json)
    {
        var data = JsonUtility.FromJson<TimeData>(json);
        TimeManager.Time = data.Time;
        TimeManager.Day = data.Day;

    }
}
