using UnityEngine;

public static class GeometryX {
	// (4/3)πr^3
	public static float SphereVolumeFromRadius(float radius){
		return (4f/3f)*Mathf.PI*(Mathf.Pow(radius, 3));
	}

	// 4πr^2
	public static float SphereSurfaceAreaFromRadius(float radius){
		return 4 * Mathf.PI * Mathf.Pow(radius, 2);
	}

	// 2πr
	public static float CircleCircumferenceFromRadius(float radius){
		return 2 * Mathf.PI * radius;
	}

	// πr^2
	public static float CircleAreaFromRadius(float radius){
		return Mathf.PI * Mathf.Pow(radius, 2);
	}

	// πr^2(h/3)
	public static float ConeVolume (float radius, float height) {
		return Mathf.PI * Mathf.Pow(radius, 2) * (height/3);
	}

	public static bool TestPlanesPoint (Plane[] planes, Vector3 point) {
		for(int i = 0; i < 6; i++)
			if(Vector3.Dot(planes[i].normal, point) + planes[i].distance < 0) 
				return false;
		return true;
	}
}