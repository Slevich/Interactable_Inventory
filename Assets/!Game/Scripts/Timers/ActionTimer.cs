using System;
using UnityEngine;
using UniRx;

public class ActionTimer : ActionDelayBase
{
    Action _endAction;

    public ActionTimer() : base()
    {
        _delayAction = (action,delay) =>
        {
            Observable
                .Timer(TimeSpan.FromSeconds(delay))
                .Subscribe(_ => action())
                .AddTo(_disposable);
        };
    }

    public void StartTimerAndAction(float TimerValue, Action SomeAction, bool DisposeOnEnd = false)
    {
        if (!ReadyForAction(ref SomeAction, DisposeOnEnd))
        {
            return;
        }

        _endAction = SomeAction;
        _delayAction(_endAction, TimerValue);
    }
    
    public void StopTimer(bool InvokeOnEnd = false)
    {
        if (_busy)
        {
            _busy = false;
            Dispose();

            if (InvokeOnEnd)
                _endAction?.Invoke();
        }
    }
}
