// Compiled only when TextMeshPro is present (TMP_PRESENT is defined by the asmdef's versionDefines
// when com.unity.ugui / com.unity.textmeshpro is installed). This keeps SLayout free of a hard
// dependency on TextMeshPro — the `textMeshPro` shortcut simply doesn't exist without it.
#if TMP_PRESENT
using TMPro;

namespace UnityX.SLayouts {
	/// <summary>
	/// Shortcut to get a TextMeshPro from an SLayout.
	/// </summary>
	public partial class SLayout {
		public TextMeshProUGUI textMeshPro {
			get {
				return graphic as TextMeshProUGUI;
			}
		}
	}
}
#endif
