using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using UnityX.Editor.Icon;
#endif

[DisallowMultipleComponent]
public class AutoIcon : MonoBehaviour {
	#if UNITY_EDITOR
	public enum IconType {
		Label,
		Dot,
		Custom
	}

	[SerializeField]
	bool _customForSelected = true;
	public bool customForSelected {
		get {
			return _customForSelected;
		} set {
			if(_customForSelected == value) return;
			_customForSelected = value;
			Refresh();
		}
	}

	[SerializeField]
	IconProperties _defaultIconProperties = new IconProperties(Icon.DiamondGray);
	public IconProperties defaultIconProperties {
		get {
			return _defaultIconProperties;
		} set {
			if(_defaultIconProperties == value) return;
			_defaultIconProperties = value;
			Refresh();
		}
	}

	[SerializeField]
	IconProperties _selectedIconProperties = new IconProperties(LabelIcon.Blue);
	public IconProperties selectedIconProperties {
		get {
			return _selectedIconProperties;
		} set {
			if(_selectedIconProperties == value) return;
			_selectedIconProperties = value;
			Refresh();
		}
	}

	[System.Serializable]
	public struct IconProperties {
		public IconType iconType;
		public Icon icon;
		public LabelIcon labelIcon;
		public Texture2D texture;

		public IconProperties (Icon icon) {
			this.iconType = IconType.Dot;
			this.icon = icon;
			this.labelIcon = default(LabelIcon);
			this.texture = null;
		}

		public IconProperties (LabelIcon labelIcon) {
			this.iconType = IconType.Label;
			this.icon = default(Icon);
			this.labelIcon = labelIcon;
			this.texture = null;
		}

		public IconProperties (Texture2D texture) {
			this.iconType = IconType.Custom;
			this.icon = default(Icon);
			this.labelIcon = default(LabelIcon);
			this.texture = texture;
		}

		public IconProperties (IconType type, Icon icon, LabelIcon labelIcon, Texture2D texture) {
			this.iconType = type;
			this.icon = icon;
			this.labelIcon = labelIcon;
			this.texture = texture;
		}

		public override bool Equals(System.Object obj) {
			if (obj == null) return false;
			IconProperties p = (IconProperties)obj;
			if ((System.Object)p == null) return false;
			return Equals(p);
		}

		public bool Equals(IconProperties p) {
			if ((object)p == null) return false;
			return (iconType == p.iconType) && (icon == p.icon) && labelIcon == p.labelIcon && texture == p.texture;
		}

		public override int GetHashCode() {
			unchecked // Overflow is fine, just wrap
			{
				int hash = 27;
				hash = hash * iconType.GetHashCode();
				hash = hash * icon.GetHashCode();
				hash = hash * labelIcon.GetHashCode();
				hash = hash * texture.GetHashCode();
				return hash;
			}
		}

		public static bool operator == (IconProperties left, IconProperties right) {
			if (System.Object.ReferenceEquals(left, right)) return true;
			if (((object)left == null) || ((object)right == null)) return false;
			return left.Equals(right);
		}

		public static bool operator != (IconProperties left, IconProperties right) {
			return !(left == right);
		}
	}


	void OnEnable () {
		SetFromProperties(defaultIconProperties);
	}
	void OnDisable () {
		IconManager.ClearIcon(gameObject);
	}

	void OnValidate () {
		SetFromProperties(defaultIconProperties);
	}

	public void Refresh () {
		if(customForSelected && UnityEditor.Selection.Contains(gameObject)) SetFromProperties(selectedIconProperties);
		else SetFromProperties(defaultIconProperties);
	}

	public void SetFromProperties (AutoIcon.IconProperties iconProperties) {
		if(iconProperties.iconType == AutoIcon.IconType.Dot) {
			IconManager.SetIcon(gameObject, iconProperties.icon);
		} else if(iconProperties.iconType == AutoIcon.IconType.Label) {
			IconManager.SetIcon(gameObject, iconProperties.labelIcon);
		} else if(iconProperties.iconType == AutoIcon.IconType.Custom) {
			IconManager.SetIcon(gameObject, iconProperties.texture);
		}
	}
	#endif
}
