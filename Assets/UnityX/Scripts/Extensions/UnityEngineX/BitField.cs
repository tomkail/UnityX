using System;
using System.Text;

/// <summary>
/// General purpose bitfield with 64 bits of storage that takes an enum type.
/// I feel like I've seen an implementation of somewhere before already but I can't remember where?
/// </summary>
[Serializable]
public struct BitField<TEnum> where TEnum : struct, IConvertible // enum
{
	public BitField(TEnum initial) {
		_bits = Convert.ToUInt64(initial);
	}

	public void Add(TEnum t)
	{
		_bits = _bits | Bit(t);
	}

	public void Add(params TEnum[] multipleValues)
	{
		foreach(var v in multipleValues) Add(v);
	}

	System.UInt64 Bit(TEnum t) {
		var val = Convert.ToInt32(t);
		if( val >= kNumBits ) throw new System.ArgumentException("Enum value is larger than can be stored in BitField: "+val+" ("+t+")");
		return (ulong)1 << val;
	}

	public void Remove(TEnum t)
	{
		_bits = _bits & (~Bit(t));
	}

	public void Remove(params TEnum[] multipleValues)
	{
		foreach(var v in multipleValues) Remove(v);
	}
		
	public void Clear()
	{
		_bits = 0;
	}

	public bool Contains(TEnum t)
	{
		return (_bits & Bit(t)) > 0;
	}

	public bool isEmpty {
		get {
			return _bits == 0;
		}
	}

	public override string ToString ()
	{
		var sb = new StringBuilder();

		bool isFirst = true;

		// TODO: Some ingenious algorithm that does clever bit maths
		// to iterate through only the bits that are set!
		for(int i=0; i<kNumBits; i++) {
			var bit = ((ulong)1) << i;
			if( (_bits & bit) > 0 ) {
				if( !isFirst )
					sb.Append(", ");
				var enumVal = (TEnum) Enum.ToObject(typeof(TEnum), i);
				sb.Append(enumVal.ToString());
				isFirst = false;
			}
		}

		return sb.ToString();
	}

	static BitField() {
		if( !typeof(TEnum).IsEnum ) throw new System.NotSupportedException("Must use an enum as generic type for BitField but saw "+typeof(TEnum).Name);
	}
		
	const int kNumBits = 64;

	[UnityEngine.SerializeField]
	System.UInt64 _bits;
}