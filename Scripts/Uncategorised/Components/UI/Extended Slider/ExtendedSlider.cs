using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI {
	public class ExtendedSlider : Slider {

		[Serializable]
		public class SliderDownEvent : UnityEvent {
		}

		[Serializable]
		public class SliderUpEvent : UnityEvent {
		}

		[Serializable]
		public class SliderEnterEvent : UnityEvent {
		}

		[Serializable]
		public class SliderExitEvent : UnityEvent {
		}

		[Serializable]
		public class SliderSelectEvent : UnityEvent<BaseEventData> {
		}

		[Serializable]
		public class SliderDeselectEvent : UnityEvent<BaseEventData> {
		}

		[Serializable]
		public class SliderMoveEvent : UnityEvent {
		}

		[Serializable]
		public class SliderDragEvent : UnityEvent {
		}


		public SliderDownEvent onDown = new();
		public SliderUpEvent onUp = new();
		public SliderEnterEvent onEnter = new();
		public SliderExitEvent onExit = new();
		public SliderSelectEvent onSelect = new();
		public SliderDeselectEvent onDeselect = new();
		public SliderMoveEvent onMove = new();
		public SliderDragEvent onDrag = new();

		public override void OnPointerDown(PointerEventData eventData) {
			base.OnPointerDown(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onDown.Invoke();
		}

		public override void OnPointerUp(PointerEventData eventData) {
			base.OnPointerUp(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onUp.Invoke();
		}

		public override void OnPointerEnter(PointerEventData eventData) {
			base.OnPointerEnter(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onEnter.Invoke();
		}

		public override void OnPointerExit(PointerEventData eventData) {
			base.OnPointerExit(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onExit.Invoke();
		}

		public override void OnSelect(BaseEventData eventData) {
			base.OnSelect(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onSelect.Invoke(eventData);
		}

		public override void OnDeselect(BaseEventData eventData) {
			base.OnDeselect(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onDeselect.Invoke(eventData);
		}

		public override void OnMove(AxisEventData eventData) {
			base.OnMove(eventData);

			if (!IsActive() || !IsInteractable())
				return;
			onMove.Invoke();
		}

		public override void OnDrag(PointerEventData eventData) {
			base.OnDrag(eventData);

			if (!MayDrag(eventData))
				return;
			onDrag.Invoke();
		}

		bool MayDrag(PointerEventData eventData) {
			return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
		}
	}
}