using UnityEngine;

public class TimeUpdater : MonoBehaviour
{
    private void Update()
    {
        TimeManager.UpdateTime(Time.deltaTime * TimeManager.timeScale);
    }
}
