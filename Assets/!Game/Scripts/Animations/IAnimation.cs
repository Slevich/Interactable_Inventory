using UnityEngine;

public interface IAnimation
{
    public void PlayForward();
    public void PlayBackward();
    public void StopAnimation();
    
    public void ModifySpeed(float Modifier);
    public void ResetSpeed();
}
