using UnityEngine;

public class CompactMatrix4x4Attribute : PropertyAttribute {
    public readonly bool IsAffine = false;

    public CompactMatrix4x4Attribute(bool affin = false) {
        IsAffine = affin;
    }
}
