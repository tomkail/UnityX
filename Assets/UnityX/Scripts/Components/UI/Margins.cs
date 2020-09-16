using System;

/// <summary>
/// Simple struct that's useful for defining margins or padding when doing UI work.
/// </summary>
[Serializable]
public struct Margins {
	public float top;
	public float bottom;
	public float left;
	public float right;

	public float horizontal { get { return left + right; } }
	public float vertical { get { return top + bottom; } }

	public static Margins All(float val) {
		return new Margins {
			top = val, bottom = val, left = val, right = val
		};
	}
}