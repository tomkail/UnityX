using UnityEngine;

public static class NaN {
    public static bool Check(float f) {
        if( float.IsNaN(f) ) {
            Debug.LogError("NaN.Check failed!");
            return true;
        }
        return false;
    }

    // Lots of ifs, might be helpful to know which component, this gives you the specific line number
    public static bool Check(Vector3 v) {
        if( Check(v.x) ) return true;
        if( Check(v.y) ) return true;
        if( Check(v.z) ) return true;
        return false;
    }

    // Lots of ifs, might be helpful to know which component, this gives you the specific line number
    public static bool Check(Vector2 v) {
        if( Check(v.x) ) return true;
        if( Check(v.y) ) return true;
        return false;
    }
}