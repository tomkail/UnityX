//https://en.wikipedia.org/wiki/Units_of_information#Systematic_multiples
// Ignores KiB/KB debate. 1024 bytes = 1KB 
public static class ByteFormatter {
	public enum SI {
		B,KB,MB,GB,TB,PB,EB
	}
	
	// SI is 0-indexed (B=0, KB=1, MB=2, …).
	// Converts a value expressed in fromOrder units into targetOrder units.
	// e.g. FromToSize(1.5, SI.MB, SI.KB) == 1536. Going to a larger unit divides, to a smaller unit multiplies.
	public static double FromToSize (double from, SI fromOrder, SI targetOrder) {
		return from * System.Math.Pow(1024, (int)fromOrder - (int)targetOrder);
	}
	public static double ToSize (long bytes, SI targetOrder) {
		int orderIndex = 0;
		int targetOrderIndex = (int)targetOrder;
		double num = bytes;
		while (orderIndex < targetOrderIndex) {
			orderIndex++;
			num = num/1024;
		}
		return num;
	}
	
	public static double ToSizeAuto (long bytes, out SI order) {
		int orderIndex = 0;
		int maxLength = (int)SI.EB;
		double num = bytes;
		while (num >= 1024 && orderIndex < maxLength) {
			orderIndex++;
			num = num/1024;
		}
		order = (SI)orderIndex;
		return num;
	}

	public static string ToString (long bytes, SI order) {
		var num = ToSize(bytes, order);
		return string.Format("{0:0.##} {1}", num, order.ToString());
	}
    
	public static string ToString (long bytes) {
		SI order;
		var num = ToSizeAuto(bytes, out order);
		return string.Format("{0:0.##} {1}", num, order.ToString());
	}
}