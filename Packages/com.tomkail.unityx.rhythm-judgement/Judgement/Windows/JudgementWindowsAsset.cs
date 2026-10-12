using UnityEngine;

namespace UnityX.Rhythm {
	// Judgement windows saved as an asset, so games can share and tune them
	[CreateAssetMenu(menuName = "UnityX/Rhythm/Judgement Windows")]
	public class JudgementWindowsAsset : ScriptableObject {
		public JudgementWindows windows = new();
	}
}
