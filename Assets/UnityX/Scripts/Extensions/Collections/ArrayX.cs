using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class ArrayX {	
	public static T[] Fill<T>(this T[] array, T myVar) {
        for(int i = 0; i < array.Length; i++){
			array[i] = myVar;
		}
		return array;
    }
	
	public static T[] GetShiftedRepeating<T>(IList<T> items, int places) {
		places %= items.Count;
		T[] shiftedItems = new T[items.Count];
		for (int i = 0; i < items.Count; i++)
			shiftedItems[i] = items.GetRepeating(i-places);
		return shiftedItems;
	}

	public static void Shift<T>(T[] arr, int shifts) {
		if(shifts == 0) return;
		if(shifts > 0) ShiftRight(arr, shifts);
		if(shifts < 0) ShiftLeft(arr, -shifts);
	}
	static void ShiftLeft<T>(T[] arr, int shifts) {
		Array.Copy(arr, shifts, arr, 0, arr.Length - shifts);
		Array.Clear(arr, arr.Length - shifts, shifts);
	}

	static void ShiftRight<T>(T[] arr, int shifts) {
		Array.Copy(arr, 0, arr, shifts, arr.Length - shifts);
		Array.Clear(arr, 0, shifts);
	}
}