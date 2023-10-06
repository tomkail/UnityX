using UnityEngine;

/// <summary>
/// This file provides struct types that can be used in place of primitive
/// float or double values, that try to provide early detection of
/// infinite and NaN values that could otherwise "infect" your code,
/// since the values spread like a virus.
/// 
/// To use, simply replace the declaration of your float or double value
/// with one of the types, and it'll be implicitly converted automatically.
/// 
/// When a new value is constructed, it checks whether it's inf or NaN and
/// replaces it with a safe value instead if necessary.
/// 
/// The types available are:
/// 
///  * FloatChecked     - float values, replaces with zero if inf/NaN
///  * FloatCheckedOne  - float values, replaces with one if inf/NaN
///  * DoubleChecked    - float values, replaces with zero if inf/NaN
///  * DoubleCheckedOne - float values, replaces with one if inf/NaN
/// 
/// </summary>
/// 
static class CheckError
{
    /// <summary>
    /// Provide a single breakpoint-able location for easier
    /// debuggability when you do get inf/nan.
    /// </summary>
    public static void Log(string msg) {
        Debug.LogError(msg);
    }
}

public struct FloatChecked
{
    public float value;

    public FloatChecked(float value)
    {
        if( float.IsInfinity(value) ) {
            CheckError.Log("Float is infinity!");
            this.value = 0;
        }
        else if( float.IsNaN(value) ) {
            CheckError.Log("Float is NaN!");
            this.value = 0;
        } else {
            this.value = value;
        }
    }

    public static implicit operator FloatChecked(float val)
    {
        return new FloatChecked(val);
    }

    public static implicit operator float(FloatChecked f)
    {
        return f.value;
    }
}

public struct FloatCheckedOne
{
    public float value;

    public FloatCheckedOne(float value)
    {
        if( float.IsInfinity(value) ) {
            CheckError.Log("Float is infinity!");
            this.value = 1;
        }
        else if( float.IsNaN(value) ) {
            CheckError.Log("Float is NaN!");
            this.value = 1;
        } else {
            this.value = value;
        }
    }

    public static implicit operator FloatCheckedOne(float val)
    {
        return new FloatCheckedOne(val);
    }

    public static implicit operator float(FloatCheckedOne f)
    {
        return f.value;
    }
}



public struct DoubleChecked
{
    public double value;

    public DoubleChecked(double value)
    {
        if( double.IsInfinity(value) ) {
            CheckError.Log("Double is infinity!");
            this.value = 0;
        }
        else if( double.IsNaN(value) ) {
            CheckError.Log("Double is NaN!");
            this.value = 0;
        } else {
            this.value = value;
        }
    }

    public static implicit operator DoubleChecked(double val)
    {
        return new DoubleChecked(val);
    }

    public static implicit operator double(DoubleChecked d)
    {
        return d.value;
    }
}

public struct DoubleCheckedOne
{
    public double value;

    public DoubleCheckedOne(double value)
    {
        if( double.IsInfinity(value) ) {
            CheckError.Log("Double is infinity!");
            this.value = 1;
        }
        else if( double.IsNaN(value) ) {
            CheckError.Log("Double is NaN!");
            this.value = 1;
        } else {
            this.value = value;
        }
    }

    public static implicit operator DoubleCheckedOne(double val)
    {
        return new DoubleCheckedOne(val);
    }

    public static implicit operator double(DoubleCheckedOne d)
    {
        return d.value;
    }
}