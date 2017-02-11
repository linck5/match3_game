using UnityEngine;
using System;
using System.Collections.Generic;

public enum TimerType
{
    ONE_TIME, ONE_TIME_REUSABLE, PERSISTENT
}

public class Timer{

    float progress;
    float timeAdder = 0;
    bool destroyScheduled = false;

    public TimerType Type;
    public bool Active = true;
    public bool IsComplete { get; private set; }
    public float Duration;
    public float Progress
    {
        get { return progress; }
        set
        {
            progress = Mathf.Clamp01(value);
            timeAdder = Duration * progress;
        }
    }
    public Action OnComplete;
    public Action OnUpdate;
    
    

    public Timer(float duration, TimerType type)
    {
        this.Duration = duration;
        this.Type = type;
        
        TimerManager.inst.timers.Add(this);
    }

    public void Reset()
    {
        timeAdder = 0;
        IsComplete = false;
    }

    public void Update()
    {
        if (!Active || destroyScheduled) return;

        IsComplete = false;

        timeAdder += Time.deltaTime;
        if(timeAdder >= Duration)
        {
            IsComplete = true;
            if(OnComplete != null)
            {
                OnComplete();
                if(Type == TimerType.ONE_TIME)
                {
                    Destroy();
                }
            }
            timeAdder = 0;
        }
        Progress = timeAdder / Duration;


        if(OnUpdate != null && !IsComplete)
        {
            OnUpdate();
        }
    }

    public void Destroy()
    {
        TimerManager.inst.timersToRemove.Add(this);
        destroyScheduled = true;
    }
    
}

internal class TimerManager : MonoBehaviour
{
    private static TimerManager instance = null;
    public static TimerManager inst
    {
        get
        {
            if (!instance)
            {
                instance = new GameObject("TimerManager").AddComponent<TimerManager>();
                instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
            return instance;
        }
    }

    public List<Timer> timersToRemove = new List<Timer>();
    public List<Timer> timers = new List<Timer>();

    void Update()
    {
        if(timersToRemove.Count > 0)
        {
            timers.RemoveAll(element => timersToRemove.Contains(element));
            timersToRemove.Clear();
        }

        for (int i = 0; i < timers.Count; i++)
        {
            Timer timer = timers[i];
            if(timer != null)
                timer.Update();
        }
    }
}

