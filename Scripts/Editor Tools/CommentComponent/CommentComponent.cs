using UnityEngine;
using System.Collections;

/// <summary>
/// Used to add written comments to a gameobject. 
/// </summary>
public class CommentComponent : MonoBehaviour {
	[Multiline]
	public string text;
	public enum MessageType {
		Error,
		Info,
		Warning,
		None
	}
	public MessageType messageType = MessageType.None;
}
