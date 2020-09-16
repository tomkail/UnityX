using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ExtendedButton : Button {

	[System.Serializable]
	public class ButtonDownEvent : UnityEvent<PointerEventData> {}
	
	[System.Serializable]
	public class ButtonUpEvent : UnityEvent<PointerEventData> {}
	
	[System.Serializable]
	public class ButtonEnterEvent : UnityEvent<PointerEventData> {}
	
	[System.Serializable]
	public class ButtonExitEvent : UnityEvent<PointerEventData> {}

	[System.Serializable]
	public class ButtonSelectEvent : UnityEvent<BaseEventData> {}

	[System.Serializable]
	public class ButtonDeselectEvent : UnityEvent<BaseEventData> {}


	public ButtonDownEvent onDown = new ButtonDownEvent();
	public ButtonUpEvent onUp = new ButtonUpEvent();
	public ButtonEnterEvent onEnter = new ButtonEnterEvent();
	public ButtonExitEvent onExit = new ButtonExitEvent();
	public ButtonSelectEvent onSelect = new ButtonSelectEvent();
	public ButtonDeselectEvent onDeselect = new ButtonDeselectEvent();

	public override void OnPointerDown (PointerEventData eventData) {
		base.OnPointerDown (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onDown.Invoke(eventData);
	}

	public override void OnPointerUp (PointerEventData eventData) {
		base.OnPointerUp (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onUp.Invoke(eventData);
	}

	public override void OnPointerEnter (PointerEventData eventData) {
		base.OnPointerEnter (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onEnter.Invoke(eventData);
	}

	public override void OnPointerExit (PointerEventData eventData) {
		base.OnPointerExit (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onExit.Invoke(eventData);
    }

	public override void OnSelect (BaseEventData eventData) {
		base.OnSelect (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onSelect.Invoke(eventData);
	}

	public override void OnDeselect (BaseEventData eventData) {
		base.OnDeselect (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onDeselect.Invoke(eventData);
	}
}