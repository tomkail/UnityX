using UnityEngine;

/// <summary>
/// Serializable implementation of transform class. 
/// Useful when doing Transform math without the need for a transform.
/// </summary>
[System.Serializable]
public struct SerializableTransform {
	public static SerializableTransform identity => new(Vector3.zero, Quaternion.identity, Vector3.one);

	public SerializableTransform (Vector3 _position) {
		this._position = _position;
		_rotation = Quaternion.identity;
		_localScale = Vector3.one;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}
	
	public SerializableTransform (Vector3 _position, Quaternion _rotation) {
		this._position = _position;
		this._rotation = _rotation;
		_localScale = Vector3.one;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}
	
	public SerializableTransform (Vector3 _position, Vector3 _eulerAngles) {
		this._position = _position;
		_rotation = Quaternion.Euler (_eulerAngles);
		_localScale = Vector3.one;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}
	
	public SerializableTransform (Vector3 _position, Quaternion _rotation, Vector3 _localScale) {
		this._position = _position;
		this._rotation = _rotation;
		this._localScale = _localScale;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}
	
	public SerializableTransform (Vector3 _position, Vector3 _eulerAngles, Vector3 _localScale) {
		this._position = _position;
		_rotation = Quaternion.Euler (_eulerAngles);
		this._localScale = _localScale;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}
	
	public SerializableTransform (Transform transform) {
		_position = transform.position;
		_rotation = transform.rotation;
		_localScale = transform.localScale;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}

	public SerializableTransform (SerializableTransform transform) {
		_position = transform.position;
		_rotation = transform.rotation;
		_localScale = transform.localScale;
        
        _localToWorldMatrix = Matrix4x4.identity;
        _localToWorldMatrixSet = false;
        _worldToLocalMatrix = Matrix4x4.identity;
        _worldToLocalMatrixSet = false;
        _localToWorldDirectionMatrix = Matrix4x4.identity;
        _localToWorldDirectionMatrixSet = false;
        _worldToLocalDirectionMatrix = Matrix4x4.identity;
        _worldToLocalDirectionMatrixSet = false;
	}

	public void ApplyFrom(Transform transform) {
		position = transform.position;
		rotation = transform.rotation;
		localScale = transform.localScale;
	}

	public void ApplyFrom(SerializableTransform transform) {
		position = transform.position;
		rotation = transform.rotation;
		localScale = transform.localScale;
	}

	public void ApplyTo (Transform transform) {
		transform.position = position;
		transform.rotation = rotation;
		transform.localScale = localScale;
	}

	public void ApplyToLocal (Transform transform) {
		transform.localPosition = position;
		transform.localRotation = rotation;
		transform.localScale = localScale;
	}

	public static SerializableTransform FromLocal(Transform transform)
	{
		var t = new SerializableTransform();
		t.position = transform.localPosition;
		t.rotation = transform.localRotation;
		t.localScale = transform.localScale;
		return t;
	}

	public static SerializableTransform Lerp(SerializableTransform t1, SerializableTransform t2, float lerp)
	{
		var result = new SerializableTransform();
		result.position = Vector3.Lerp(t1.position, t2.position, lerp);
		result.rotation = Quaternion.Lerp(t1.rotation, t2.rotation, lerp);
		result.localScale = Vector3.Lerp(t1.localScale, t2.localScale, lerp);
		return result;
	}

	public static SerializableTransform LerpUnclamped(SerializableTransform t1, SerializableTransform t2, float lerp)
	{
		var result = new SerializableTransform();
		result.position = Vector3.LerpUnclamped(t1.position, t2.position, lerp);
		result.rotation = Quaternion.LerpUnclamped(t1.rotation, t2.rotation, lerp);
		result.localScale = Vector3.LerpUnclamped(t1.localScale, t2.localScale, lerp);
		return result;
	}

	[SerializeField]
	private Vector3 _position;
	public Vector3 position
	{
		get => _position;
		set
		{
			_position = value;
            _localToWorldMatrixSet = false;
            _worldToLocalMatrixSet = false;
            _localToWorldDirectionMatrixSet = false;
            _worldToLocalDirectionMatrixSet = false;
		}
	}


	[SerializeField]
	private Quaternion _rotation;
	public Quaternion rotation
	{
		get => _rotation;
		set
		{
			_rotation = value;
            _localToWorldMatrixSet = false;
            _worldToLocalMatrixSet = false;
            _localToWorldDirectionMatrixSet = false;
            _worldToLocalDirectionMatrixSet = false;
		}
	}

	public Vector3 eulerAngles
	{
		get => rotation.eulerAngles;
		set => rotation = Quaternion.Euler (value);
	}

	[SerializeField]
	private Vector3 _localScale;
	public Vector3 localScale
	{
		get => _localScale;
		set
		{
			_localScale = value;
            _localToWorldMatrixSet = false;
            _worldToLocalMatrixSet = false;
		}
	}
	
	public Vector3 forward
	{
		get => rotation * Vector3.forward;
		set => rotation = Quaternion.LookRotation (value);
	}

	public Vector3 right
	{
		get => rotation * Vector3.right;
		set => rotation = Quaternion.FromToRotation (Vector3.right, value);
	}
	
	public Vector3 up
	{
		get => rotation * Vector3.up;
		set => rotation = Quaternion.FromToRotation (Vector3.up, value);
	}

    bool _worldToLocalMatrixSet;
    Matrix4x4 _worldToLocalMatrix;
    public Matrix4x4 worldToLocalMatrix {
        get {
            if(!_worldToLocalMatrixSet) {
                _worldToLocalMatrixSet = true;
                _worldToLocalMatrix = localToWorldMatrix.inverse;
            }
            return _worldToLocalMatrix;
        }
    }
    bool _localToWorldMatrixSet;
    Matrix4x4 _localToWorldMatrix;
    public Matrix4x4 localToWorldMatrix {
        get {
            if(!_localToWorldMatrixSet) {
                _localToWorldMatrixSet = true;
                _localToWorldMatrix = Matrix4x4.TRS(position, rotation, localScale);
            }
            return _localToWorldMatrix;
        }
    }

    bool _worldToLocalDirectionMatrixSet;
    Matrix4x4 _worldToLocalDirectionMatrix;
    public Matrix4x4 worldToLocalDirectionMatrix {
        get {
            if(!_worldToLocalDirectionMatrixSet) {
                _worldToLocalDirectionMatrixSet = true;
                _worldToLocalDirectionMatrix = localToWorldDirectionMatrix.inverse;
            }
            return _worldToLocalDirectionMatrix;
        }
    }

    bool _localToWorldDirectionMatrixSet;
    Matrix4x4 _localToWorldDirectionMatrix;
    public Matrix4x4 localToWorldDirectionMatrix {
        get {
            if(!_localToWorldDirectionMatrixSet) {
                _localToWorldDirectionMatrixSet = true;
                _localToWorldDirectionMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            }
            return _localToWorldDirectionMatrix;
        }
    }
    
	//
	// Methods
	//
	public void Rotate (float xAngle, float yAngle, float zAngle, Space relativeTo = Space.Self) {
		Rotate (new Vector3 (xAngle, yAngle, zAngle), relativeTo);
	}
	
	public void Rotate (Vector3 axis, float angle, Space relativeTo = Space.Self) {
		if(relativeTo == Space.World) rotation = Quaternion.AngleAxis (angle, axis) * rotation;
		else rotation = rotation * Quaternion.AngleAxis (angle, axis);
	}
	
	public void Rotate (Vector3 eulerAngles, Space relativeTo = Space.Self) {
		if(relativeTo == Space.World) rotation = Quaternion.Euler(eulerAngles) * rotation;
		else rotation = rotation * Quaternion.Euler(eulerAngles);
	}

	public void RotateAround (Vector3 point, Vector3 axis, float angle) {
		Vector3 vector = position;
		Quaternion rotation = Quaternion.AngleAxis (angle, axis);
		Vector3 vector2 = vector - point;
		vector2 = rotation * vector2;
		vector = point + vector2;
		position = vector;
		Rotate (axis, angle, Space.World);
	}

	public void LookAt (Vector3 point, Vector3 up) {
		rotation = Quaternion.LookRotation(point - position, up);
	}
	
	public void Translate (Vector3 translation, Transform relativeTo)
	{
		if (relativeTo)
		{
			position += relativeTo.TransformDirection (translation);
		}
		else
		{
			position += translation;
		}
	}
	
	public void Translate (float x, float y, float z, Transform relativeTo)
	{
		Translate (new Vector3 (x, y, z), relativeTo);
	}
	
	public void Translate (float x, float y, float z, Space relativeTo = Space.Self)
	{
		Translate (new Vector3 (x, y, z), relativeTo);
	}
	
	public void Translate (Vector3 translation, Space relativeTo = Space.Self)
	{
		if (relativeTo == Space.World)
		{
			position += translation;
		}
		else
		{
			position += rotation * translation;
		}
	}

	
	public Vector3 InverseTransformDirection (float x, float y, float z) {
		return InverseTransformDirection (new Vector3 (x, y, z));
	}
	
	public Vector3 InverseTransformDirection (Vector3 direction) {
		Vector3 localPoint = worldToLocalDirectionMatrix.MultiplyVector(direction);
		return localPoint;
	}
	
	public Vector3 InverseTransformPoint (float x, float y, float z) {
		return InverseTransformPoint (new Vector3 (x, y, z));
	}
	
	public Vector3 InverseTransformPoint (Vector3 position) {
		Vector3 localPoint = worldToLocalMatrix.MultiplyPoint(position);
		return localPoint;
	}
	
	public Vector3 InverseTransformVector (float x, float y, float z) {
		return InverseTransformVector (new Vector3 (x, y, z));
	}
	
	public Vector3 InverseTransformVector (Vector3 vector) {
		Vector3 localPoint = worldToLocalMatrix.MultiplyVector(vector);
		return localPoint;
	}
	
	
	public Vector3 TransformDirection (float x, float y, float z) {
		return TransformDirection (new Vector3 (x, y, z));
	}
	
	public Vector3 TransformDirection (Vector3 direction) {
		Matrix4x4 transformMatrix = localToWorldDirectionMatrix;
		Vector3 localPoint = transformMatrix.MultiplyVector(direction);
		return localPoint;
	}
	
	public Vector3 TransformPoint (float x, float y, float z) {
		return TransformPoint (new Vector3 (x, y, z));
	}
	
	public Vector3 TransformPoint (Vector3 position) {
		Vector3 localPoint = localToWorldMatrix.MultiplyPoint(position);
		return localPoint;
	}
	
	public Vector3 TransformVector (Vector3 vector) {
		Vector3 localPoint = localToWorldMatrix.MultiplyVector(vector);
		return localPoint;
	}
	
	public Vector3 TransformVector (float x, float y, float z) {
		return TransformVector (new Vector3 (x, y, z));
	}





	public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		// If parameter cannot be cast to SerializableTransform return false.
		SerializableTransform p = (SerializableTransform)obj;
		if ((System.Object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return Equals(p);
	}

	public bool Equals(SerializableTransform p) {
		// If parameter is null return false:
		if ((object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return (position == p.position) && (rotation == p.rotation) && (localScale == p.localScale);
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * position.GetHashCode();
			hash = hash * rotation.GetHashCode();
			hash = hash * localScale.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (SerializableTransform left, SerializableTransform right) {
		if (ReferenceEquals(left, right))
		{
			return true;
		}

		// If one is null, but not both, return false.
		if (((object)left == null) || ((object)right == null))
		{
			return false;
		}

		return left.Equals(right);
	}

	public static bool operator != (SerializableTransform left, SerializableTransform right) {
		return !(left == right);
	}




	public override string ToString() {
		return string.Format("[{0}] position:{1} rotation:{2} localScale:{3}", GetType().Name, position, eulerAngles, localScale);
	}
}