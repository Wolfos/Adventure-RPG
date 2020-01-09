using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Range
{
    public float start;
    public float end;
}
public class AnimationRandomStart : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string stateName;
    [SerializeField] private Range timeRange;
    [SerializeField] private Range speedRange;

    void Start()
    {
        animator.Play(stateName, 0, Random.Range(timeRange.start, timeRange.end));
        animator.speed = Random.Range(speedRange.start, speedRange.end);
    }

}
