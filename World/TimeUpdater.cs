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
    private class TimeData: ISaveData
    {
        public float Time;
        public int Day;
    }

    private TimeData _data;
    private void Start()
    {
        CommandConsole.RegisterCommand(new SetTimeCommand());

        if (SaveGameManager.HasData(id)) // Load
        {
            _data = SaveGameManager.GetData(id) as TimeData;
            TimeManager.Time = _data.Time;
            TimeManager.Day = _data.Day;
        }
        else
        {
            _data = new()
            {
                Time = TimeManager.Time,
                Day = TimeManager.Day
            };
            SaveGameManager.Register(id, _data);
        }
    }

    private void Update()
    {
        TimeManager.UpdateTime(Time.deltaTime * TimeManager.timeScale);

        _data.Day = TimeManager.Day;
        _data.Time = TimeManager.Time;
    }
}
