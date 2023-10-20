using UnityEngine;
using System.Collections;

[System.Serializable]
public class KeyboardInput {
	public bool anyKeyDown = false;

	public KeyboardInput () {
	
	}
	
	public void UpdateState () {
		anyKeyDown = Input.anyKeyDown;
	}

	public void ResetInput () {
		anyKeyDown = false;
	}

	public void End () {

	}
	
	public static Vector2 GetCardinalDirectionFromArrowKeys (bool alsoUseWASD = true) {
		if(Input.GetKey(KeyCode.UpArrow) || (alsoUseWASD && Input.GetKey(KeyCode.W))) {
			return Vector2.up;
		} else if(Input.GetKey(KeyCode.DownArrow) || (alsoUseWASD && Input.GetKey(KeyCode.S))) {
			return Vector2.down;
		} else if(Input.GetKey(KeyCode.LeftArrow) || (alsoUseWASD && Input.GetKey(KeyCode.A))) {
			return Vector2.left;
		} else if(Input.GetKey(KeyCode.RightArrow) || (alsoUseWASD && Input.GetKey(KeyCode.D))) {
			return Vector2.right;
		}
		return Vector2.zero;
	}
	
	public static Vector2 GetCombinedDirectionFromArrowKeys (bool alsoUseWASD = true) {
		Vector2 direction = Vector2.zero;
		if(Input.GetKey(KeyCode.UpArrow) || (alsoUseWASD && Input.GetKey(KeyCode.W))) {
			direction += Vector2.up;
		} else if(Input.GetKey(KeyCode.DownArrow) || (alsoUseWASD && Input.GetKey(KeyCode.S))) {
			direction += Vector2.down;
		}
		if(Input.GetKey(KeyCode.LeftArrow) || (alsoUseWASD && Input.GetKey(KeyCode.A))) {
			direction += Vector2.left;
		} else if(Input.GetKey(KeyCode.RightArrow) || (alsoUseWASD && Input.GetKey(KeyCode.D))) {
			direction += Vector2.right;
		}
		return direction.normalized;
	}

	public static int? GetNumberKey () {
		foreach (char c in Input.inputString) {
			int num;
			if(int.TryParse(c.ToString(), out num)){
				return num;
			}
		}
		return null;
	}

	public static bool TryGetNumberKey (ref int numberKey) {
		int? _numberKey = GetNumberKey();
		if(_numberKey != null) {
			numberKey = (int)_numberKey;
			return true;
		}
		return false;
	}

	public static KeyCode GetNumberKeyCode () {
		int numberKey = -1;
		if(TryGetNumberKey(ref numberKey)) {
			if(numberKey == 0) return KeyCode.Alpha0;
			if(numberKey == 1) return KeyCode.Alpha1;
			if(numberKey == 2) return KeyCode.Alpha2;
			if(numberKey == 3) return KeyCode.Alpha3;
			if(numberKey == 4) return KeyCode.Alpha4;
			if(numberKey == 5) return KeyCode.Alpha5;
			if(numberKey == 6) return KeyCode.Alpha6;
			if(numberKey == 7) return KeyCode.Alpha7;
			if(numberKey == 8) return KeyCode.Alpha8;
			if(numberKey == 9) return KeyCode.Alpha9;
		}
		return KeyCode.None;
	}

}
