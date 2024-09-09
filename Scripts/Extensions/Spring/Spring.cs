using System;
using UnityEngine;

// A representation of a spring’s motion, heavily inspired by Apple's Spring API.
// Can be represented in one of two ways:
// - a physical spring model with mass, stiffness and damping
// - as having a duration (represented by the "response" variable) and a damping ratio (which is defined as 1-bounce and vice-versa)
// Overdamped springs are not fully supported. The following functions have not been properly tested for them, and if they are treated as eventually settling is inconsistent.  
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
    public float duration => response;
    
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
    const float defaultEpsilon = 0.0001f;
    #endregion
    
    #region Contructors
    // Creates a spring with the specified duration and damping ratio.
    // Response is the time taken for one complete cycle of oscellation.
    // Damping ratio determines how bouncy the spring is and should be between 0 and 1.
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
        return (mass, stiffness, damping);
    }
    
    // Converts 3 parameter physical properties to response/damping properties. A mass my be specified.
    public static (float response, float dampingRatio) PhysicalToResponseDamping(float mass, float stiffness, float damping) {
        var omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        var dampingRatio = damping / (2 * omega0); //  damping ratio (zeta, or ζ) is defined as actual damping/critical damping
        float response = 2 * Mathf.PI / omega0; // Response is the time taken for one complete cycle of oscellation.
        return (response, dampingRatio);
    }

    // Get the response of a spring from settling duration, damping ratio and epsilon.
    public static float CalculateResponseFromSettlingTime(float settlingDuration, float dampingRatio, float epsilon) {
        // Critically damped and underdamped
        if (dampingRatio <= 1.0f) {
            var omega0 = Mathf.Log(1 / epsilon) / (dampingRatio * settlingDuration);
            return 2 * Mathf.PI / omega0;
        }
        // Overdamped springs do not settle.
        else return Mathf.Infinity;
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
        var x0 = endValue - startValue;

        var omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        var dampingRatio = damping / (2 * omega0);
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
    
    // Get the total force currently acting a spring with given parameters at a given time.
    public static float Force(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        CalculateDisplacementAndVelocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping, out float displacement, out float velocity);
        return stiffness * displacement + damping * -velocity; // Spring force plus damping force
    }
    
    public static float SpringForce(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        CalculateDisplacementAndVelocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping, out float displacement, out float _);
        return stiffness * displacement;
    }
    
    public static float DampingForce(float startValue, float endValue, float initialVelocity, float time, float mass, float stiffness, float damping) {
        var velocity = Velocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
        return damping * -velocity;
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
    public static float SettlingDuration(float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping, double epsilon = defaultEpsilon) {
        double v0 = -initialVelocity;
        double x0 = Mathf.Abs(endValue - startValue);

        double omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        double dampingRatio = damping / (2 * omega0);
        double omegaZeta = omega0 * dampingRatio;
        
        double t;

        // Underdamped
        if (dampingRatio < 1) {
            // Use absolute values to ensure the argument of the logarithm is positive
            t = Math.Log(epsilon / (x0 + Math.Abs(v0))) / (-omegaZeta);
        }
        // Overdamped (UNTESTED)
        else if (dampingRatio > 1) {
            var omegaD = omega0 * Math.Sqrt(dampingRatio * dampingRatio - 1); // frequency of damped oscillation
            var z1 = dampingRatio * -omega0 - omegaD;
            var z2 = dampingRatio * -omega0 + omegaD;

// Pre-calculate the inverse of the absolute values of z1 and z2 to reduce divisions
            var invAbsZ1 = 1 / Math.Abs(z1);
            var invAbsZ2 = 1 / Math.Abs(z2);

            if (x0 == 0 && v0 != 0) {
                // Adjust the calculation to use initial velocity when x0 is effectively zero
                // Apply the pre-calculated inverse of z1 and z2 as a multiplier
                var t1 = Math.Abs(Math.Log(epsilon / Math.Abs(v0))) * invAbsZ1;
                var t2 = Math.Abs(Math.Log(epsilon / Math.Abs(v0))) * invAbsZ2;
                t = Math.Max(t1, t2);
            }
            else {
                // Original calculation for when x0 is not zero
                // Apply the pre-calculated inverse of z1 and z2 as a multiplier
                var t1 = Math.Abs(Math.Log(epsilon / x0)) * invAbsZ1;
                var t2 = Math.Abs(Math.Log(epsilon / x0)) * invAbsZ2;
                t = Math.Max(t1, t2); // Pick the later time when both decays have settled
            }
        }
        // Critically damped
        else {
            // For critically damped systems, the motion can be described as x(t) = (A + Bt)e^{-omega0*t}.
            // To generalize for different initial conditions, we consider the system's characteristic time scale,
            // which is inversely proportional to omega0, and adjust for initial conditions.

            // Estimate settling time based on adjusted characteristic time scale.
            // The multiplier for the time constant may need adjustment based on system behavior and epsilon.
            var timeScale = 1 / omega0;
            var initialOffset = Mathf.Abs(endValue - startValue);
            var velocityFactor = Mathf.Abs(initialVelocity) / omega0;

            // Adjusting the heuristic to consider both position and velocity contributions
            t = timeScale * Math.Log((initialOffset + velocityFactor) / epsilon);
        }
        return Mathf.Max((float)t, 0);
    }

    public float CalculateMaximumDisplacement(float startValue, float endValue, float initialVelocity) {
        return CalculateMaximumDisplacement(startValue, endValue, initialVelocity, mass, stiffness, damping);
    }

    public static float CalculateMaximumDisplacement(float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping) {
        var time = CalculateTimeOfMaximumDisplacement(startValue, endValue, initialVelocity, mass, stiffness, damping);
        return Value(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    public float CalculateTimeOfMaximumDisplacement(float startValue, float endValue, float initialVelocity) {
        return CalculateTimeOfMaximumDisplacement(startValue, endValue, initialVelocity, mass, stiffness, damping);
    }

    // Calculates the time at which the spring will reach its maximum displacement.
    // Because I don't know how this can be done closed form, we use a loop. This means that it can lose accuracy as damping ratio approaches 1 and the search time grows longer.
    // For critically/overdamped springs, which don't oscellate, we return the settling time using a high epsilon to provide a smoother ramp between damping ratios.
    public static float CalculateTimeOfMaximumDisplacement(float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping) {
        if (startValue == endValue && initialVelocity == 0) return 0;
        
        var omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        var dampingRatio = damping / (2 * omega0);
        if (dampingRatio < 1) {
            int maxNumberIterations = 200;
            float maxTime = CalculateTimeOfNthCrossingOfEndValue(startValue, endValue, initialVelocity, mass, stiffness, damping, 2);
            
            float timeStep = maxTime/maxNumberIterations;
            float currentTime = 0.0f;

            float velocity = initialVelocity;
            var initialVelocitySign = Mathf.Sign(velocity);
            if (initialVelocitySign == 0) {
                currentTime += timeStep;
                velocity = Velocity(startValue, endValue, initialVelocity, currentTime, mass, stiffness, damping);
                initialVelocitySign = Mathf.Sign(velocity);
            } 
            // Simulate until the velocity changes sign (indicating a peak) or maxTime is reached
            for(int i = 0; i < maxNumberIterations; i++) {
                velocity = Velocity(startValue, endValue, initialVelocity, currentTime, mass, stiffness, damping);
                if (Mathf.Sign(velocity) != initialVelocitySign) {
                    // Found the first peak
                    break;
                }
                currentTime += timeStep;
            }

            return currentTime;
        } else {
            return SettlingDuration(startValue, endValue, initialVelocity, mass, stiffness, damping, 0.0000001f);
        }
    }
    
    // Calculates the time at which the spring will cross the end value for the nth time.
    // Floating point values of n are not supported because the time of crossing will not be accurate.
    public static float CalculateTimeOfNthCrossingOfEndValue(float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping, int n) {
        var v0 = -initialVelocity;
        var x0 = endValue - startValue;
        
        var omega0 = Mathf.Sqrt(stiffness / mass); // natural angular frequency (ωn) of the spring measured in radians/s
        var dampingRatio = damping / (2 * omega0);
        
        // Only underdamped springs oscellate
        if (dampingRatio < 1) {
            var omegaD = omega0 * Mathf.Sqrt(1 - dampingRatio * dampingRatio); // damped angular frequency
        
            // Calculate the phase angle phi, considering the corrected direction of initial velocity
            float phi = Mathf.Atan2(omegaD * x0, v0 + dampingRatio * omega0 * x0);
        
            // Adjust phi to ensure it's in the correct range, leading to a positive time calculation
            if (phi < 0) {
                phi += 2 * Mathf.PI; // Normalize phi to ensure positive time
            }
        
            // Calculate the time of maximum displacement, ensuring it's in the future (positive)
            float timeOfMaxDisplacement = (n * Mathf.PI - phi) / omegaD;
        
            // Check for the case where calculated time might be negative due to initial conditions; adjust if necessary
            if (timeOfMaxDisplacement < 0) {
                timeOfMaxDisplacement += 2 * Mathf.PI / omegaD; // Add a full period to get the next peak time
            }

            return timeOfMaxDisplacement;
        }
        // Critically and overdamped springs do not oscellate, but they reach a settling point after a time.
        else {
            return SettlingDuration(startValue, endValue, initialVelocity, mass, stiffness, damping, 0.0000001f);
        }
    }
    
    // Get the settling duration of a spring with given parameters.
    public static bool IsDone(float time, float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        return IsDone(time, 0, 0, mass, stiffness, damping, epsilon);
    }

    public static bool IsDone(float time, float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping, float epsilon = defaultEpsilon) {
        return time >= SettlingDuration(startValue, endValue, initialVelocity, mass, stiffness, damping, epsilon);
    }
    
    #endregion
    
    #region Public Methods

    // Calculates the position of the spring at a given time
    public float Value(float time) {
        return Value(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Value(float startValue, float endValue, float time) {
        return Value(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Value(float startValue, float endValue, float initialVelocity, float time) {
        return Value(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    // Uses the spring like an easing curve, with input values expected in the range 0 to 1 and an output value starting at 0 and ending at 1.
    // Useful for animations.
    public float EasingFunction(float progress) {
        return Value(0, 1, 0, progress * settlingDuration, mass, stiffness, damping);
    }
    
    // Calculates the velocity of the spring at a given time
    public float Velocity(float time) {
        return Velocity(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Velocity(float startValue, float endValue, float time) {
        return Velocity(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Velocity(float startValue, float endValue, float initialVelocity, float time) {
        return Velocity(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    // Calculates the total force acting upon the spring by adding the spring force and the damping force.
    public float Force(float time) {
        return Force(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float Force(float startValue, float endValue, float time) {
        return Force(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float Force(float startValue, float endValue, float initialVelocity, float time) {
        return Force(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    // Calculates the force acting upon the spring without the damping force
    public float SpringForce(float time) {
        return SpringForce(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float SpringForce(float startValue, float endValue, float time) {
        return SpringForce(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float SpringForce(float startValue, float endValue, float initialVelocity, float time) {
        return SpringForce(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
    }
    
    // Calculates the force acting against the spring as a product of velocity and the spring's damping
    public float DampingForce(float time) {
        return DampingForce(1, 0, 0, time, mass, stiffness, damping);
    }
    
    public float DampingForce(float startValue, float endValue, float time) {
        return DampingForce(startValue, endValue, 0, time, mass, stiffness, damping);
    }
    
    public float DampingForce(float startValue, float endValue, float initialVelocity, float time) {
        return DampingForce(startValue, endValue, initialVelocity, time, mass, stiffness, damping);
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
    public float MoveTowards(float currentValue, float targetValue, ref float currentVelocity, float deltaTime) {
        Evaluate(currentValue, targetValue, currentVelocity, deltaTime, mass, stiffness, damping, out currentValue, out currentVelocity);
        return currentValue;
    }

    public void MoveTowards(ref float currentValue, float targetValue, ref float currentVelocity, float deltaTime) {
        Evaluate(currentValue, targetValue, currentVelocity, deltaTime, mass, stiffness, damping, out currentValue, out currentVelocity);
    }
    #endregion
}