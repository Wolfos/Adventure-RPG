using System;
using Data;
using UnityEngine;
using WolfRPG.Core.CommandConsole;

public class SetTimeCommand : IConsoleCommand
{
    public string Word => "settime";
    public ConsoleArgumentType[] Arguments { get; } = { ConsoleArgumentType.Int };
    public void Execute(object[] arguments, Action<string> onError)
    {
        var newTime = (int)arguments[0];
        TimeManager.Time = newTime;
    }
}

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

    private void Start()
    {
        CommandConsole.RegisterCommand(new SetTimeCommand());
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
