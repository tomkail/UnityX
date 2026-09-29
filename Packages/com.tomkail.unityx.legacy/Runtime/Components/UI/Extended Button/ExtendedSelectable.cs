using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Intentionally mirrors ExtendedButton but derives from Selectable (not Button) — a Selectable has no
// button click/navigation behaviour. C# single-inheritance means the shared pointer/select-event
// boilerplate can't be factored out into a common base. Not accidental duplication.
public class ExtendedSelectable : Selectable {

	[Serializable]
	public class ButtonDownEvent : UnityEvent<PointerEventData> {}
	
	[Serializable]
	public class ButtonUpEvent : UnityEvent<PointerEventData> {}
	
	[Serializable]
	public class ButtonEnterEvent : UnityEvent<PointerEventData> {}
	
	[Serializable]
	public class ButtonExitEvent : UnityEvent<PointerEventData> {}

	[Serializable]
	public class ButtonSelectEvent : UnityEvent<BaseEventData> {}

	[Serializable]
	public class ButtonDeselectEvent : UnityEvent<BaseEventData> {}


	public ButtonDownEvent onDown = new();
	public ButtonUpEvent onUp = new();
	public ButtonEnterEvent onEnter = new();
	public ButtonExitEvent onExit = new();
	public ButtonSelectEvent onSelect = new();
	public ButtonDeselectEvent onDeselect = new();

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
