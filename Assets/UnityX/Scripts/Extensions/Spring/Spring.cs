using System;
using UnityEngine;

// A representation of a spring’s motion, heavily inspired by Apple's Spring API.
// Can be represented in one of two ways:
// - a physical spring model with mass, stiffness and damping
// - as having a duration (represented by the "response" variable) and a damping ratio (which is defined as 1-bounce and vice-versa)
// Overdamped springs are not supported. 
[Serializable]
public struct Spring {
    #region Presets
    // A spring with a predefined duration and higher amount of bounce.
    public static Spring bouncy => Create(0.5f, 0.7f);
    // A spring with a predefined duration and higher amount of bounce that can be tuned.
    public static Spring Bouncy(float duration, float extraBounce) => Create(duration, 0.7f-extraBounce);
    
    // A smooth spring with a predefined duration and no bounce.
    public static Spring smooth => Create(0.5f, 1f);
    // A smooth spring with a predefined duration and no bounce that can be tuned.
    public static Spring Smooth(float duration, float extraBounce) => Create(duration, 1f-extraBounce);
    
    // A spring with a predefined duration and small amount of bounce that feels more snappy.
    public static Spring snappy => Create(0.5f, 0.85f);
    // A spring with a predefined duration and small amount of bounce that feels more snappy and can be tuned.
    public static Spring Snappy(float duration, float extraBounce) => Create(duration, 0.85f-extraBounce);
    #endregion

    #region Members
    // A two-parameter struct defining spring behaviour that a physical spring model (SpringProperties) can be derived from.
    // Concept taken from Apple, as described in this WWDC video https://developer.apple.com/videos/play/wwdc2018/803/, and implementation taken from https://medium.com/ios-os-x-development/demystifying-uikit-spring-animations-2bb868446773
    // More info on their API (borrowed for this) at https://developer.apple.com/documentation/swiftui/spring
    
    // The time taken to oscellate once, or to (approximately) come to a stop for fully damped springs.
    // The stiffness of the spring, defined as an approximate duration in seconds.
    [SerializeField] float _response;
    public float response {
        get => _response;
        private set => _response = value;
    }
    public float duration => SettlingDuration(0,1,0,mass,stiffness,damping,epsilon);
    
    // The amount of drag applied, as a fraction of the amount needed to produce critical damping.
    // 0 will oscellate forever and 1 will be "fully damped".
    [SerializeField] float _dampingRatio;
    public float dampingRatio {
        get => _dampingRatio;
        private set => _dampingRatio = value;
    }
    // How bouncy the spring is.
    public float bounce => 1 - dampingRatio;
    
    
    
    // Springs can also be represented by their physical properties, which can be derived from response and dampingRatio
    [SerializeField] float _mass;
    public float mass {
        get => _mass;
        private set => _mass = value;
    }
    [SerializeField] float _stiffness;
    public float stiffness {
        get => _stiffness;
        private set => _stiffness = value;
    }
    [SerializeField] float _damping;
    public float damping {
        get => _damping;
        private set => _damping = value;
    }
    
    public float settlingDuration => SettlingDuration(0,1,0,mass,stiffness,damping,epsilon);
    
    // Epsilon determines the value of the settling time calculation.
    [SerializeField] float _epsilon;
    public float epsilon {
        get => _epsilon;
        private set => _epsilon = value;
    }
    const float defaultEpsilon = 0.001f;
    #endregion
    
    #region Contructors
    // Creates a spring with the specified duration and damping ratio.
    public static Spring Create (float response, float dampingRatio, float epsilon = defaultEpsilon) {
        Debug.Assert(response > 0);
        Debug.Assert(dampingRatio >= 0);
        var physical = ResponseDampingToPhysical(response, dampingRatio);
        return new Spring {
            _mass = physical.mass,
            _stiffness = physical.stiffness,
            _damping = physical.damping,
            _response = response,
            _dampingRatio = dampingRatio,
            _epsilon = epsilon,
        };
    }
    
    public static Spring CreateFromPhysical (float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        Debug.Assert(mass >= 0);
        Debug.Assert(stiffness > 0);
        Debug.Assert(damping >= 0);
        var responseDamping = PhysicalToResponseDamping(mass, stiffness, damping);
        return new Spring {
            _mass = mass,
            _stiffness = stiffness,
            _damping = damping,
            _response = responseDamping.response,
            _dampingRatio = responseDamping.dampingRatio,
            _epsilon = epsilon,
        };
    }
    #endregion
    
    #region Converter methods
    // Converts 2 parameter response/damping properties to physical properties. A mass my be specified.
    public static (float mass, float stiffness, float damping) ResponseDampingToPhysical(float response, float dampingRatio, float existingMass = 1) {
        float mass = existingMass;
        float stiffness = Mathf.Pow(2 * Mathf.PI / response, 2) * mass;
        float damping = 4 * Mathf.PI * dampingRatio * mass / response;
        return (mass, stiffness, damping); // Corrected order to match the function signature
    }
    
    // Converts 3 parameter physical properties to response/damping properties. A mass my be specified.
    public static (float response, float dampingRatio) PhysicalToResponseDamping(float mass, float stiffness, float damping) {
        // Response is the time taken for one complete cycle of oscellation.
        // Mathf.Sqrt(stiffness / mass) is the natural frequency (omega_n, or ωn) of the spring.
        float response = 2 * Mathf.PI / Mathf.Sqrt(stiffness / mass);
        //  damping ratio (zeta, or ζ) is defined as actual damping/critical damping
        float dampingRatio = damping / (2 * Mathf.Sqrt(stiffness * mass));
        return (response, dampingRatio);
    }
    #endregion
    
    #region Static evaluation methods
    // Updates the current value and velocity of a spring, using an interface similar to Mathf.SmoothDamp.
    public static float Update(float value, float target, ref float velocity, float mass, float stiffness, float damping, float deltaTime) {
        Evaluate(value, target, velocity, deltaTime, mass, stiffness, damping, out value, out velocity);
        return value;
    }
    
    // Get the value of a spring with given parameters at a given time.
    public static float Value(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        Evaluate(startValue, endValue, initialVelocity, time, mass, stiffness, damping, out float value, out _);
        return value;
    }

    // Get the velocity of a spring with given parameters at a given time.
    public static float Velocity(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        CalculateDisplacementAndVelocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping, out _, out float velocity);
        return velocity;
    }
    
    // Get the displacement and velocity of a spring with given parameters at a given time.
    public static void CalculateDisplacementAndVelocity(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping, out float displacement, out float velocity) {
        var v0 = -initialVelocity;
        var x0 = endValue-startValue;

        var dampingRatio = damping / (2 * Mathf.Sqrt(stiffness * mass));
        var omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        var omegaZeta = omega0 * dampingRatio;
        
        // Underdamped
        if (dampingRatio < 1) {
            var omegaD = omega0 * Mathf.Sqrt(1 - dampingRatio * dampingRatio); // damped angular frequency
            var e = Mathf.Exp(-omegaZeta * time);
            var c1 = x0;
            var c2 = (v0 + omegaZeta * x0) / omegaD;
            var cos = Mathf.Cos(omegaD * time);
            var sin = Mathf.Sin(omegaD * time);
            displacement = e * (c1 * cos + c2 * sin);
            velocity = e * ((c1 * omegaZeta - c2 * omegaD) * cos + (c1 * omegaD + c2 * omegaZeta) * sin);
            // This line has also been tested to work. I've left it in for reference.
            // velocity = -e * (c2 * omegaD * cos - c1 * omegaD * sin) - omegaZeta * displacement;
        }
        // Overdamped
        else if (dampingRatio > 1) {
            var omegaD = omega0 * Mathf.Sqrt(dampingRatio * dampingRatio - 1); // frequency of damped oscillation
            var z1 = -omegaZeta - omegaD;
            var z2 = -omegaZeta + omegaD;
            var e1 = Mathf.Exp(z1 * time);
            var e2 = Mathf.Exp(z2 * time);
            var c1 = (v0 - x0 * z2) / (-2 * omegaD);
            var c2 = x0 - c1;
            displacement = c1 * e1 + c2 * e2;
            velocity = -(c1 * e1 * z1 + c2 * e2 * z2);
        }
        // Critically damped
        else {
            var e = Mathf.Exp(-omega0 * time);
            var initialRateChange = v0 + omega0 * x0;
            displacement = e * (x0 + initialRateChange * time);
            velocity = -e * (initialRateChange - omega0 * (x0 + initialRateChange * time));
        }
    }

    public static void Evaluate(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping, out float value, out float velocity) {
        CalculateDisplacementAndVelocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping, out float displacement, out velocity);
        value = endValue - displacement;
    }
    
    // Get the force currently acting a spring with given parameters at a given time.
    public static float Force(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        CalculateDisplacementAndVelocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping, out float displacement, out float velocity);
        return -stiffness * displacement - damping * -velocity; // Spring force plus damping force
    }
    
    // Get the acceleration of a spring with given parameters at a given time.
    public static float Acceleration(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        return Force(startValue, endValue, initialVelocity, time, mass, stiffness, damping) / mass;
    }


    // Get the settling duration of a spring with given parameters.
    public static float SettlingDuration(float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        return SettlingDuration(1, 0, 0, mass, stiffness, damping, epsilon);
    }
    
    // Get the settling duration of a spring with given parameters.
    public static float SettlingDuration(float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        var v0 = -initialVelocity;
        var x0 = Mathf.Abs(endValue - startValue);

        var dampingRatio = damping / (2 * Mathf.Sqrt(stiffness * mass));
        var omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        var omegaZeta = omega0 * dampingRatio;
        
        float t;

        // Underdamped
        if (dampingRatio < 1) {
            // Use absolute values to ensure the argument of the logarithm is positive
            t = Mathf.Log(epsilon / (x0 + Mathf.Abs(v0))) / (-omegaZeta);
        }
        // Overdamped (UNTESTED)
        else if (dampingRatio > 1) {
            var omegaD = omega0 * Mathf.Sqrt(dampingRatio * dampingRatio - 1); // frequency of damped oscillation
            var z1 = dampingRatio * -omega0 - omegaD;
            var z2 = dampingRatio * -omega0 + omegaD;

            // Use absolute values to ensure the argument of the logarithm is positive
            // Calculate times for both decays and pick the maximum to ensure we capture when both are below epsilon
            var t1 = Mathf.Log(epsilon / x0) / z1;
            var t2 = Mathf.Log(epsilon / x0) / z2;
            t = Mathf.Max(t1, t2); // Pick the later time when both decays have settled
        }
        // Critically damped
        else {
            // For critically damped systems, the motion can be described as x(t) = (A + Bt)e^{-omega0*t}.
            // To generalize for different initial conditions, we consider the system's characteristic time scale,
            // which is inversely proportional to omega0, and adjust for initial conditions.

            // Estimate settling time based on adjusted characteristic time scale.
            // The multiplier for the time constant may need adjustment based on system behavior and epsilon.
            float timeScale = 1 / omega0;
            float initialOffset = Mathf.Abs(endValue - startValue);
            float velocityFactor = Mathf.Abs(initialVelocity) / omega0;

            // Adjusting the heuristic to consider both position and velocity contributions
            t = timeScale * Mathf.Log((initialOffset + velocityFactor) / epsilon);
        }

        return Mathf.Max(t, 0); // Ensure non-negative time
    }
    
    // Get the settling duration of a spring with given parameters.
    public static float IsDone(float time, float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        return IsDone(1, 0, 0, mass, stiffness, damping, epsilon);
    }

    public static float IsDone(float time, float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        return SettlingDuration(startValue, endValue, initialVelocity, mass, stiffness, damping, epsilon);
    }

    // Get the response of a spring from settling duration, damping ratio and epsilon.
    public static float CalculateResponseFromSettlingTime(float settlingDuration, float dampingRatio, float epsilon) {
        // Critically damped and underdamped
        if (dampingRatio <= 1.0f) {
            var omega_n = Mathf.Log(1 / epsilon) / (dampingRatio * settlingDuration);
            return 2 * Mathf.PI / omega_n;
        }
        // Overdamped UNTESTED!
        else {
            Debug.LogWarning("CalculateResponseFromSettlingTime: Overdamped springs are not supported");
            return 0;
        }
    }
    #endregion
    
    #region Public Methods

    public float Value(float time) {
        return Value(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Value(float startValue, float endValue, float time) {
        return Value(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Value(float startValue, float endValue, float initialVelocity, float time) {
        return Value(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    
    public float Velocity(float time) {
        return Velocity(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Velocity(float startValue, float endValue, float time) {
        return Velocity(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Velocity(float startValue, float endValue, float initialVelocity, float time) {
        return Velocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    
    public float Force(float time) {
        return Force(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Force(float startValue, float endValue, float time) {
        return Force(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Force(float startValue, float endValue, float initialVelocity, float time) {
        return Force(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    
    public float Acceleration(float time) {
        return Acceleration(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Acceleration(float startValue, float endValue, float time) {
        return Acceleration(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Acceleration(float startValue, float endValue, float initialVelocity, float time) {
        return Acceleration(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    
    // Moves a value towards a target using this spring.
    public float MoveTowards(ref float currentValue, float targetValue, ref float currentVelocity, float deltaTime) {
        Evaluate(currentValue, targetValue, currentVelocity, deltaTime, mass, stiffness, damping, out currentValue, out currentVelocity);
        return currentValue;

        // Another approach shown here for reference:
        // Note that due to use of timesteps, if simulated over multiple frames values will diverge from the results of evaluating the spring directly.
        /*
        float force = Force(currentValue, targetValue, currentVelocity, deltaTime, mass, stiffness, damping);
        float acceleration = force / mass;
        currentVelocity += acceleration * deltaTime;
        currentValue += currentVelocity * deltaTime;
        return currentValue;
        */
    }
    #endregion
}