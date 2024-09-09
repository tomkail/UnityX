using System;
using UnityEngine;

[Serializable]
public class SpringHandler {
    [SerializeField] Spring _spring = Spring.snappy;
    public Spring spring {
        get => _spring;
        set => _spring = value;
    }
    
    public float time;
    
    public float startValue;
    public float endValue;
    public float initialVelocity;
    
    public float value => Spring.Value(startValue, endValue, initialVelocity, time, spring.mass, spring.stiffness, spring.damping);
    public float velocity => Spring.Velocity(startValue, endValue, initialVelocity, time, spring.mass, spring.stiffness, spring.damping);
    public float settlingDuration => Spring.SettlingDuration(startValue, endValue, initialVelocity, spring.mass, spring.stiffness, spring.damping, spring.epsilon);
    public bool isActive => !Spring.IsDone(time, startValue, endValue, initialVelocity, spring.mass, spring.stiffness, spring.damping);
    public bool IsActive(float epsilon) => !Spring.IsDone(time, startValue, endValue, initialVelocity, spring.mass, spring.stiffness, spring.damping, epsilon);
    
    Action<float> onChange;

    SpringHandler() {
        _spring = Spring.snappy;
    }

    public SpringHandler(Spring spring, Action<float> onChange = null) {
        this.spring = spring;
        this.onChange = onChange;
    }
    
    public SpringHandler(Spring spring, float startValue, float endValue, Action<float> onChange = null) {
        this.spring = spring;
        this.startValue = startValue;
        this.endValue = endValue;
        this.onChange = onChange;
    }
    
    public SpringHandler(Spring spring, float startValue, float endValue, float initialVelocity, Action<float> onChange = null) {
        this.spring = spring;
        this.startValue = startValue;
        this.endValue = endValue;
        this.initialVelocity = initialVelocity;
        this.onChange = onChange;
    }
    
    public float Update(float deltaTime) => SetTime(time + deltaTime);
    
    public float SetTime(float time) {
        this.time = time;
        onChange?.Invoke(value);
        return value;
    }
}