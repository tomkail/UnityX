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
}