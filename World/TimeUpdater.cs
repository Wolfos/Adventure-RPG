using System;
using System.Collections;
using Data;
using UnityEngine;

public class TimeUpdater : SaveableObject
{
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
        return TimeManager.Time.ToString();
    }

    public override void Load(string json)
    {
        TimeManager.Time = float.Parse(json);
    }
}
