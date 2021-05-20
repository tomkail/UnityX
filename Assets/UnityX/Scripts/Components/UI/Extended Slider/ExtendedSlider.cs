using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ExtendedSlider : Slider {

	[System.Serializable]
	public class SliderDownEvent : UnityEvent {}
	
	[System.Serializable]
	public class SliderUpEvent : UnityEvent {}
	
	[System.Serializable]
	public class SliderEnterEvent : UnityEvent {}
	
	[System.Serializable]
	public class SliderExitEvent : UnityEvent {}

	[System.Serializable]
	public class SliderSelectEvent : UnityEvent<BaseEventData> {}

	[System.Serializable]
	public class SliderDeselectEvent : UnityEvent<BaseEventData> {}

	[System.Serializable]
	public class SliderMoveEvent : UnityEvent {}

	[System.Serializable]
	public class SliderDragEvent : UnityEvent {}


	public SliderDownEvent onDown = new SliderDownEvent();
	public SliderUpEvent onUp = new SliderUpEvent();
	public SliderEnterEvent onEnter = new SliderEnterEvent();
	public SliderExitEvent onExit = new SliderExitEvent();
	public SliderSelectEvent onSelect = new SliderSelectEvent();
	public SliderDeselectEvent onDeselect = new SliderDeselectEvent();
	public SliderMoveEvent onMove = new SliderMoveEvent();
	public SliderDragEvent onDrag = new SliderDragEvent();

	public override void OnPointerDown (PointerEventData eventData) {
		base.OnPointerDown (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onDown.Invoke();
	}

	public override void OnPointerUp (PointerEventData eventData) {
		base.OnPointerUp (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onUp.Invoke();
	}

	public override void OnPointerEnter (PointerEventData eventData) {
		base.OnPointerEnter (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onEnter.Invoke();
	}

	public override void OnPointerExit (PointerEventData eventData) {
		base.OnPointerExit (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onExit.Invoke();
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

	public override void OnMove (AxisEventData eventData) {
		base.OnMove (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onMove.Invoke();
	}

	public override void OnDrag (PointerEventData eventData) {
		base.OnDrag (eventData);

		if (!IsActive() || !IsInteractable())
            return;
		onDrag.Invoke();
	}
}
