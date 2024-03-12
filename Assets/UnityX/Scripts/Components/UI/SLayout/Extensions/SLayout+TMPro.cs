using System;
using TMPro;

/// <summary>
/// Shortcut to get a TextMeshPro from an SLayout. Don't want to include it
/// directly in SLayout directly since we don't want a dependency on TMPro.
/// </summary>
public partial class SLayout {
	public TextMeshProUGUI textMeshPro => graphic as TextMeshProUGUI;

	public enum SizeMode {
		None,
		Fixed,
		HugContents,
		FillContainer,
	}
	[Serializable]
	public struct TextAutoSizeParams {
		public readonly SizeMode mode;
		public readonly float fixedModeSize;
		public readonly float minMargin;
		public readonly float maxMargin;

		public static readonly TextAutoSizeParams None = default;
		
		public TextAutoSizeParams (SizeMode mode, float fixedModeSize, float minMargin, float maxMargin) {
			this.mode = mode;
			this.fixedModeSize = fixedModeSize;
			this.minMargin = minMargin;
			this.maxMargin = maxMargin;
		}

		public float totalMargin => minMargin + maxMargin;

		public static TextAutoSizeParams FixedSize (float fixedModeSize) {
			return new TextAutoSizeParams(SizeMode.Fixed, fixedModeSize, 0, 0);
		}
		public static TextAutoSizeParams FixedSize (float fixedModeSize, float margin) {
			return new TextAutoSizeParams(SizeMode.Fixed, fixedModeSize, margin, margin);
		}
		public static TextAutoSizeParams FixedSize (float fixedModeSize, float minMargin, float maxMargin) {
			return new TextAutoSizeParams(SizeMode.Fixed, fixedModeSize, minMargin, maxMargin);
		}
		
		public static TextAutoSizeParams HugContents () {
			return new TextAutoSizeParams(SizeMode.HugContents, 0, 0, 0);
		}
		public static TextAutoSizeParams HugContents (float margin) {
			return new TextAutoSizeParams(SizeMode.HugContents, 0, margin, margin);
		}
		public static TextAutoSizeParams HugContents (float minMargin, float maxMargin) {
			return new TextAutoSizeParams(SizeMode.HugContents, 0, minMargin, maxMargin);
		}
		
		public static TextAutoSizeParams FillContainer () {
			return new TextAutoSizeParams(SizeMode.FillContainer, 0, 0, 0);
		}
		public static TextAutoSizeParams FillContainer (float margin) {
			return new TextAutoSizeParams(SizeMode.FillContainer, 0, margin, margin);
		}
		public static TextAutoSizeParams FillContainer (float minMargin, float maxMargin) {
			return new TextAutoSizeParams(SizeMode.FillContainer, 0, minMargin, maxMargin);
		}
	}

	public void SetSizeAndPositionText(string text, TextAutoSizeParams widthParams = default, TextAutoSizeParams heightParams = default) {
		textMeshPro.text = text;
		SetSizeAndPositionFromText(widthParams, heightParams);
	}

	public void SetAndSizeText(string text, TextAutoSizeParams widthParams = default, TextAutoSizeParams heightParams = default) {
		textMeshPro.text = text;
		SetSizeFromText(widthParams, heightParams);
	}
	
	public void SetSizeAndPositionFromText(TextAutoSizeParams widthParams = default, TextAutoSizeParams heightParams = default) {
		textMeshPro.ForceMeshUpdate();
		SetSizeFromText(widthParams, heightParams);
		
		x = widthParams.mode switch {
			SizeMode.Fixed => widthParams.minMargin,
			SizeMode.FillContainer => widthParams.minMargin,
			SizeMode.HugContents => widthParams.minMargin,
			_ => x
		};
		
		y = heightParams.mode switch {
			SizeMode.Fixed => heightParams.minMargin,
			SizeMode.FillContainer => heightParams.minMargin,
			SizeMode.HugContents => heightParams.minMargin,
			_ => y
		};
	}
	
	public void SetSizeFromText(TextAutoSizeParams widthParams = default, TextAutoSizeParams heightParams = default) {
		textMeshPro.ForceMeshUpdate();
		width = widthParams.mode switch {
			SizeMode.Fixed => widthParams.fixedModeSize + widthParams.totalMargin,
			SizeMode.FillContainer => targetParentRect.width - widthParams.totalMargin,
			SizeMode.HugContents => textMeshPro.GetTightPreferredValues().x,
			_ => width
		};

		height = heightParams.mode switch {
			SizeMode.Fixed => heightParams.fixedModeSize + heightParams.totalMargin,
			SizeMode.FillContainer => targetParentRect.height - heightParams.totalMargin,
			SizeMode.HugContents => textMeshPro.GetTightPreferredValues(width).y,
			_ => height
		};
	}
}