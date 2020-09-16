﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://en.wikipedia.org/wiki/Units_of_information#Systematic_multiples
// Ignores KiB/KB debate. 1024 bytes = 1KB 
public class ByteFormatter : MonoBehaviour {
	public enum SI {
		B,KB,MB,GB,TB,PB,EB
	}
	
	// public static double FromToSize (long from, SI fromOrder, SI targetOrder) {}
	// 1 Indexed. 3 is MB
	public static double ToSize (long bytes, SI targetOrder) {
		int orderIndex = 0;
		int targetOrderIndex = EnumX.IndexOf(targetOrder);
		double num = bytes;
		while (orderIndex < targetOrderIndex) {
			orderIndex++;
			num = num/1024;
		}
		return num;
	}
	
	public static long ToSizeAuto (long bytes, out SI order) {
		int orderIndex = 0;
		int maxLength = EnumX.Length<SI>() - 1;
		long num = bytes;
		while (num >= 1024 && orderIndex < maxLength) {
			orderIndex++;
			num = num/1024;
		}
		order = (SI)orderIndex;
		return num;
	}

	public static string ToString (long bytes) {
		SI order;
		var num = ToSizeAuto(bytes, out order);
		return string.Format("{0:0.##} {1}", num, order.ToString());
	}
}
