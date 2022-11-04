#if DEBUG
//#define DEBUG_SLAYOUT
#endif

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;


/// <summary>
/// SLayout does two things:
///  - Provides an easy-to-use interface for dynamic UI Layout that uses a consistent coordinate space for positioning
///    UI rather than being dependent on Unity's anchoring etc.
///  - Allows iOS-style "implicit animation" by wrapping calls to change the layout in an animation function, causing
///    the elements to animate to the target setup.
///  - Partial so you can compile in your own shortcut properties (e.g. for TextMeshPro), but without having a full
///    dependency on 3rd party APIs.
/// </summary>
public partial class SLayout : UIBehaviour {

	/// <summary>
	/// Normally, the Unity origin of bottom left is used, but sometimes it's useful to put
	/// the origin in the top left with the Y axis increasing as it does downwards, similar to
	/// on the web or on iOS. It's useful for laying out content progressively from the top of,
	/// the canvas for example for text layout or other document content, since at the end you
	/// know the expected height of the parent layout to fit that content.
	/// </summary>
	public bool originTopLeft;

	/// <summary>
	/// Useful to detect changes due to the auto layout system changing the size and position of
	/// a top level RectTransform due to non-SLayout related things like screen size changing or
	/// simply something else controlling the (top level) view.
	/// </summary>
	public event Action<SLayout> onRectChange;

	public bool isAnimating {
		get {
			return SLayoutAnimator.instance.IsAnimating(this);
		}
	}
	
	// Reset all private fields to deal with skipping Scene Reload
	protected override void OnDisable () {
		base.OnDisable();
		_x = null;
		_y = null;
		_width = null;
		_height = null;
		_rotation = null;
		_scale = null;				 
		_groupAlpha = null;
		_color = null;
		_rootCanvas = null;
		_canvas = null;
		_canvasGroup = null;
		_graphic = null;
		_searchedForTimeScalar = false;
		_timeScalar = null;
    	CancelAnimations();
	}

	protected override void OnRectTransformDimensionsChange() {
		base.OnRectTransformDimensionsChange();
		if( onRectChange != null ) onRectChange(this);
	}

	public SLayoutAnimation Animate(float duration, System.Action animAction)
	{
		return Animate(duration, 0.0f, animAction);
	}

	public SLayoutAnimation Animate(float duration, float delay, System.Action animAction)
	{
		return Animate(duration, delay, null, animAction);
	}

	public SLayoutAnimation After(float delay, System.Action nonAnimatedAction)
	{
		// null curve, null animAction
		var newAnim = new SLayoutAnimation() {
			_duration = 0.0f, 
			_maxDuration = 0.0f, 
			_delay = delay, 
			_maxDelay = delay, 
			_nonAnimatedAction = nonAnimatedAction,
			_owner = this
		};
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}

	public SLayoutAnimation Animate(float duration, float delay, AnimationCurve customCurve, System.Action animAction)
	{
		var newAnim = new SLayoutAnimation() {
			_duration = duration, 
			_maxDuration = duration, 
			_delay = delay, 
			_maxDelay = delay, 
			_customCurve = customCurve, 
			_animAction = animAction,
			_owner = this
		};
		SLayoutAnimator.instance.StartAnimation(newAnim);
		#if UNITY_EDITOR
		if(!Application.isPlaying) newAnim.CompleteImmediate();
		#endif
		return newAnim;
	}
	
	public SLayoutAnimation Animate(float duration, EasingFunction.Ease easing, System.Action animAction)
	{
		return Animate(duration, 0.0f, easing, animAction);
	}
	
	public SLayoutAnimation Animate(float duration, float delay, EasingFunction.Ease easing, System.Action animAction)
	{
		var newAnim = new SLayoutAnimation() {
			_duration = duration, 
			_maxDuration = duration, 
			_delay = delay, 
			_maxDelay = delay, 
			_easingFunction = EasingFunction.GetEasingFunction(easing), 
			_animAction = animAction,
			_owner = this
		};
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}

	public SLayoutAnimation AnimateCustom(float duration, System.Action<float> customAnimAction)
	{
		var newAnim = new SLayoutAnimation() {
			_duration = duration, 
			_maxDuration = duration, 
			_delay = 0.0f, 
			_maxDelay = 0.0f, 
			_animAction = () => Animatable(customAnimAction),
			_owner = this
		};
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}

	public SLayoutAnimation AnimateCustom(float duration, float delay, System.Action<float> customAnimAction)
	{
		var newAnim = new SLayoutAnimation() {
			_duration = duration, 
			_maxDuration = duration, 
			_delay = delay, 
			_maxDelay = delay, 
			_animAction = () => Animatable(customAnimAction),
			_owner = this
		};
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}

	/// <summary>
	/// While an animation is currently being defined, insert an extra delay, so that any animated values
	/// that are set after this call begin a bit later than previously defined elements.
	/// </summary>
	public void AddDelay(float extraDelay)
	{
		SLayoutAnimator.instance.AddDelay(extraDelay);
	}

	/// <summary>
	/// While an animation is currently being defined, insert an extra duration, so that any animated values
	/// that are set after this call have a longer duration than those defined before it.
	/// </summary>
	public void AddDuration(float extraDuration)
	{
		SLayoutAnimator.instance.AddDuration(extraDuration);
	}

	/// <summary>
	/// Allow a custom value to be animated.
	/// If an animation is currently being defined using layout.Animate, then it 
	/// will used the callback every frame that the animation is running, passing 
	/// the normalised time. If no animation is being defined, then it will simply 
	/// call the callback immediately, passing 1.0 to ensure that it is set to its 
	/// final value.
	/// </summary>
	public static void Animatable(Action<float> customAnim)
	{
		SLayoutAnimator.instance.Animatable(customAnim);
	}

	/// <summary>
	/// Allow a custom float value to be animated, starting at initial, and tweening
	/// to the target. The callback "setter" must be passed which sets the float to
	/// its intermediate value during animation.
	/// If no animation is currently being defined, then the value will immediately
	/// be set to its target value.
	/// </summary>
	public static void Animatable(float initial, float target, Action<float> setter)
	{
		SLayoutAnimator.instance.Animatable(t => setter(Mathf.LerpUnclamped(initial, target, t)));
	}

	/// <summary>
	/// Allow a custom color value to be animated, starting at initial, and tweening
	/// to the target. The callback "setter" must be passed which sets the color to
	/// its intermediate value during animation.
	/// If no animation is currently being defined, then the color will immediately
	/// be set to its target value.
	/// </summary>
	public static void Animatable(Color initial, Color target, Action<Color> setter)
	{
		SLayoutAnimator.instance.Animatable(t => setter(Color.Lerp(initial, target, t)));
	}

	public void CancelAnimations()
	{
		if(SLayoutAnimator.instance == null) return;
		SLayoutAnimator.instance.CancelAnimations(this);
	}

	public void CompleteAnimations()
	{
		SLayoutAnimator.instance.CompleteAnimations(this);
	}

    public static void WithoutAnimating(Action action) {
        SLayoutAnimation.StartPreventAnimation();
        action();
        SLayoutAnimation.EndPreventAnimation();
    }

	public Canvas rootCanvas {
		get {
			// if( _rootCanvas == null )
				_rootCanvas = canvas.rootCanvas;
			return _rootCanvas;
		}
	}
	Canvas _rootCanvas;
	public Canvas canvas {
		get {
			// if( _canvas == null )
				_canvas = transform.GetComponentInParent<Canvas>(true);
			return _canvas;
		}
	}
	Canvas _canvas;

	/// <summary>
	/// Width of canvas, taking into account scaling mode.
	/// </summary>
	public float canvasWidth {
		get {
			return ((RectTransform)canvas.transform).rect.width;
		}
	}

	/// <summary>
	/// Height of canvas, taking into account scaling mode.
	/// </summary>
	public float canvasHeight {
		get {
			return ((RectTransform)canvas.transform).rect.height;
		}
	}

	public Vector2 canvasSize {
		get {
			return ((RectTransform)canvas.transform).rect.size;
		}
	}
		
	public RectTransform rectTransform {
		get {
			return (RectTransform)transform;
		}
	}

	public CanvasGroup canvasGroup {
		get {
			if( _canvasGroup == null ) _canvasGroup = GetComponent<CanvasGroup>();
			return _canvasGroup;
		}
	}
	CanvasGroup _canvasGroup;

	public Image image {
		get {
			return graphic as Image;
		}
	}

	public Text text {
		get {
			return graphic as Text;
		}
	}

	public Graphic graphic {
		get {
			if( _graphic == null ) _graphic = GetComponent<Graphic>();
			return _graphic;
		}
	}
	Graphic _graphic;

	public SLayout parent {
		get {
			return transform.parent.GetComponent<SLayout>();
		}
	}

	public Rect parentRect {
		get {
			var parentRT = parentRectTransform;
			if( parentRT == null ) return Rect.zero; // e.g. if SLayout has been misplaced!
			var localRect = parentRT.rect;
			return new Rect(GetRectTransformX(parentRT), GetRectTransformY(parentRT), localRect.width, localRect.height);
		}
	}

	public RectTransform parentRectTransform {
		get {
			return transform.parent as RectTransform;
		}
	}
		
	/// <summary>
	/// Allows the speed of an animation to be sped up or slowed down independently
	/// of any scalars that exist using the standard Time.timeScale. This is useful
	/// since often UI is used within pause menus.
	/// To apply a time scale, put a SLayoutCanvasTimeScalar on the canvas for this SLayout.
	/// </summary>
	public float timeScale {
		get {
			return timeScalar == null ? 1 : timeScalar.timeScale;
		}
	}
	SLayoutCanvasTimeScalar timeScalar {
		get {
			if(!_searchedForTimeScalar) {
				// Search all canvases in hierarchy until we find the component
				var canvases = transform.GetComponentsInParent<Canvas>();
				foreach(var canvas in canvases) {
					_timeScalar = canvas.GetComponent<SLayoutCanvasTimeScalar>();
					if(_timeScalar != null) break;
				}
				_searchedForTimeScalar = true;
			}
			return _timeScalar;
		}
	}
	bool _searchedForTimeScalar;
	SLayoutCanvasTimeScalar _timeScalar;
		
	public float x {
		get {
			InitX();
			return _x.value;
		}
		set {
			InitX();
			_x.value = value; 
		}
	}

	public float targetX {
		get {
			InitX();
			return _x.animatedProperty != null ? _x.animatedProperty.end : _x.value;
		}
	}

	public float y {
		get {
			InitY();
			return _y.value;
		}
		set {
			InitY();
			_y.value = value;
		}
	}

	public float targetY {
		get {
			InitY();
			return _y.animatedProperty != null ? _y.animatedProperty.end : _y.value;
		}
	}

	public float width {
		get {
			InitWidth();
			return _width.value;
		}
		set {
			InitWidth();
			_width.value = value;
		}
	}

	public float targetWidth {
		get {
			InitWidth();
			return _width.animatedProperty != null ? _width.animatedProperty.end : _width.value;
		}
	}

	public float height {
		get {
			InitHeight();
			return _height.value;
		}
		set {
			InitHeight();
			_height.value = value;
		}
	}

	public float targetHeight {
		get {
			InitHeight();
			return _height.animatedProperty != null ? _height.animatedProperty.end : _height.value;
		}
	}

	public Vector2 position {
		get {
			return new Vector2(x, y);
		}
		set {
			x = value.x;
			y = value.y;
		}
	}

	public Vector2 targetPosition {
		get {
			return new Vector2(targetX, targetY);
		}
	}

	public Vector2 size {
		get {
			return new Vector2(width, height);
		}
		set {
			width = value.x;
			height = value.y;
		}
	}

	public Vector2 targetSize {
		get {
			return new Vector2(targetWidth, targetHeight);
		}
	}

	public float rotation {
		get {
			InitRotation();
			return _rotation.value;
		}
		set {
			InitRotation();
			_rotation.value = value;
		}
	}

	public float targetRotation {
		get {
			InitRotation();
			return _rotation.animatedProperty != null ? _rotation.animatedProperty.end : _rotation.value;
		}
	}

	public float scale {
		get {
			InitScale();
			return _scale.value;
		}
		set {
			InitScale();
			_scale.value = value;
		}
	}

	public float targetScale {
		get {
			InitScale();
			return _scale.animatedProperty != null ? _scale.animatedProperty.end : _scale.value;
		}
	}

	public float groupAlpha {
		get {
			InitGroupAlpha();
			return _groupAlpha.value;
		}
		set {
			InitGroupAlpha();
			_groupAlpha.value = value;
		}
	}

	public float targetGroupAlpha {
		get {
			InitGroupAlpha();
			return _groupAlpha.animatedProperty != null ? _groupAlpha.animatedProperty.end : _groupAlpha.value;
		}
	}

	public Color color {
		get {
			InitColor();
			return _color.value;
		}
		set {
			InitColor();
			_color.value = value;
		}
	}

	public Color targetColor {
		get {
			InitColor();
			return _color.animatedProperty != null ? _color.animatedProperty.end : _color.value;
		}
	}

	public float alpha {
		get {
			InitColor();
			return _color.value.a;
		}
		set {
			InitColor();
			var color = _color.value;
			color.a = value;
			_color.value = color;
		}
	}

	public float targetAlpha {
		get {
			return targetColor.a;
		}
	}

	public Rect rect {
		get {
			return new Rect(x, y, width, height);
		}
		set {
			x = value.x;
			y = value.y;
			width = value.width;
			height = value.height;
		}
	}

	public Rect targetRect {
		get {
			return new Rect(targetX, targetY, targetWidth, targetHeight);
		}
	}

	public Rect localRect {
		get {
			return new Rect(0.0f, 0.0f, width, height);
		}
		set {
			x = x + value.x;
			y = y + value.y;
			width = value.width;
			height = value.height;
		}
	}

	public Rect targetLocalRect {
		get {
			return new Rect(0.0f, 0.0f, targetWidth, targetHeight);
		}
	}

	public Vector2 center {
		get {
			return new Vector2(centerX, centerY);
		}
		set {
			centerX = value.x;
			centerY = value.y;
		}
	}

	public Vector2 targetCenter {
		get {
			return new Vector2(targetCenterX, targetCenterY);
		}
	}

	public float centerX {
		get {
			return x + 0.5f*width;
		}
		set {
			x = value - 0.5f*width;
		}
	}

	public float targetCenterX {
		get {
			return targetX + 0.5f*targetWidth;
		}
	}

	public float centerY {
		get {
			return y + 0.5f*height;
		}
		set {
			y = value - 0.5f*height;
		}
	}

	public float targetCenterY {
		get {
			return targetY + 0.5f*targetHeight;
		}
	}


    /// <summary>
    /// Local center (i.e. simply 0.5*width, 0.5*height)
    /// </summary>
    public Vector2 middle {
		get {
			return new Vector2(middleX, middleY);
		}
	}

    /// <summary>
    /// Local center (i.e. simply 0.5*width)
    /// </summary>
	public float middleX {
		get {
			return 0.5f*width;
		}
	}

    /// <summary>
    /// Local center (i.e. simply 0.5*height)
    /// </summary>    
	public float middleY {
		get {
			return 0.5f*height;
		}
	}


	/// <summary>
	/// X position of own pivot in parent's space
	/// </summary>
	public float originX {
		get {
			float pivotX = rectTransform.pivot.x * width;
			return x + pivotX;
		}
		set {
			float pivotX = rectTransform.pivot.x * width;
			x = value - pivotX;
		}
	}

	/// <summary>
	/// Y position of own pivot in parent's space
	/// </summary>
	public float originY {
		get {
			float pivotY = rectTransform.pivot.y * height;
			return y + pivotY;
		}
		set {
			float pivotY = rectTransform.pivot.y * height;
			y = value - pivotY;
		}
	}

	/// <summary>
	/// Position of pivot in own local pixel space (not normalised)
	/// </summary>
	public Vector2 pivot {
		get {
			return new Vector2(pivotX, pivotY);
		}
		set {
			pivotX = value.x;
			pivotY = value.y;
		}
	}


	/// <summary>
	/// X position of pivot in own pixel space (not normalised)
	/// </summary>
	public float pivotX {
		get {
			return rectTransform.pivot.x * width;
		}
		set {
			var pivot = rectTransform.pivot;
			pivot.x = value / width;
			rectTransform.pivot = pivot;
		}
	}

	/// <summary>
	/// Y position of pivot in own pixel space (not normalised)
	/// </summary>
	public float pivotY {
		get {
			return rectTransform.pivot.y * height;
		}
		set {
			var pivot = rectTransform.pivot;
			pivot.y = value / height;
			rectTransform.pivot = pivot;
		}
	}

	/// <summary>
	/// Position of own pivot in parent's space
	/// </summary>
	public Vector2 origin {
		get {
			return new Vector2(originX, originY);
		}
		set {
			originX = value.x;
			originY = value.y;
		}
	}

	public float rightX {
		get {
			return x + width;
		}
		set {
			x = value - width;
		}
	}

	public float targetRightX {
		get {
			return targetX + targetWidth;
		}
	}

	public float bottomY {
		get {
			if( originTopLeft )
				return y + height;
			else
				return y;
		}
		set {
			if( originTopLeft )
				y = value - height;
			else
				y = value;
		}
	}

	public float targetBottomY {
		get {
			if( originTopLeft )
				return targetY + targetHeight;
			else
				return targetY;
		}
	}

	public float topY {
		get {
			if( originTopLeft )
				return y;
			else
				return y + height;
		}
		set {
			if( originTopLeft )
				y = value;
			else
				y = value - height;
		}
	}

	public float targetTopY {
		get {
			if( originTopLeft )
				return targetY;
			else
				return targetY + targetHeight;
		}
	}
		
	Vector2 GetPivotPos(RectTransform rt)
	{
		var rectSize = rt.rect.size;
		var rectPivot = rt.pivot;
		return new Vector2(
			rectSize.x * rectPivot.x,
			rectSize.y * rectPivot.y
		);
	}


    
	// public Rect ScreenToSLayoutRect (Rect screenRect) {
    //     return RectX.CreateEncapsulating(ScreenToSLayoutPosition(screenRect.min), ScreenToSLayoutPosition(screenRect.max));
    // }
    
	// Converts a screen position to a local position in the layout space.
    public Vector2 ScreenToSLayoutPosition (Vector2 screenPoint) {
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, canvas.rootCanvas.worldCamera, out Vector3 worldPoint);
        return WorldToSLayoutPosition(worldPoint);
        // return (Vector2)CanvasToSLayoutSpace(canvas.ScreenToCanvasPoint(screenPoint));
    }
	
    public Vector2 ScreenToSLayoutVector (Vector2 screenVector) {
        return ScreenToSLayoutPosition(screenVector) - ScreenToSLayoutPosition(Vector2.zero);
    }

	// Converts a screen rect to a local rect in the layout space.
    public Rect ScreenToSLayoutRect (Rect screenRect) {
		var min = ScreenToSLayoutPosition(screenRect.min);
		var max = ScreenToSLayoutPosition(screenRect.max);
        var layoutRect = RectX.CreateEncapsulating(min, max);
		if(originTopLeft) {
			layoutRect.y += layoutRect.height;
		}
		return layoutRect;
    }
	
	// This returns a coordinate to be applied to this object's position property. Setting it to the center property may result in the object being offset by the pivot!
    public Vector2 WorldToSLayoutPosition (Vector3 worldPoint) {
		var rt = rectTransform;
		var parentRectT = rt.parent as RectTransform;

		var localPoint = transform.parent.InverseTransformPoint(worldPoint);
		Vector2 anchoredPos = (Vector2)localPoint + pivot;
		
		float toLeftEdge = rt.pivot.x * rt.rect.width;
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		float leftInset = parentToLeftEdge - toLeftEdge;
		anchoredPos.x += leftInset;
		
		if( originTopLeft ) {
			// This calculation can almost certainly be simplied a LOT. This system confuses the heck out of me and I worked it out by just hacking things about.
			float toTopEdge = (1.0f-rt.pivot.y) * rt.rect.height;
			float parentToTopEdge = (1.0f-parentRectT.pivot.y) * parentRectT.rect.height;
			float topInset = parentToTopEdge - toTopEdge;
			anchoredPos.y += topInset;
			
			anchoredPos.y = parentRectT.rect.height - anchoredPos.y;
			anchoredPos.y -= parentRectT.rect.height * (parentRectT.pivot.y - 0.5f) * 2;
			anchoredPos.y -= rt.rect.height * (1-rt.pivot.y) * 2;
		} else {
			float toBottomEdge = rt.pivot.y * rt.rect.height;
			float parentToBottomEdge = parentRectT.pivot.y * parentRectT.rect.height;
			float bottomInset = parentToBottomEdge - toBottomEdge;
			anchoredPos.y += bottomInset;
		}
		return anchoredPos;
	}

    // Converts a canvas space coordinate to the space of this slayout.
    // "Canvas space" meaning local to the canvas' transform, where 0,0 is the center of the canvas.
    // If the canvas size was (1000,500) the canvas top left would be (-500,-250) and the slayout space would be (0,0)
    // This function "corrects" for that difference
    //  (assuming set to top-left mode but this works for either)

	// WARNING! This seems not to work when the object is in a non-full-size container! It moves more than it should from the center. We probably need to offset it by the container's position.
	// IT's been commented out for that reason.
    public Vector2 CanvasToSLayoutSpace (Vector2 canvasSpacePos) {
		Vector2 offset = Vector2.zero;

        var rt = rectTransform;
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return offset;
		
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		offset.x = parentToLeftEdge;

		if( originTopLeft ) {
			canvasSpacePos.y = -canvasSpacePos.y;
			float parentToTopEdge = (1.0f-parentRectT.pivot.y) * parentRectT.rect.height;
			float topInset = parentToTopEdge;
			offset.y = topInset;
		} else {
			float parentToBottomEdge = parentRectT.pivot.y * parentRectT.rect.height;
			float bottomInset = parentToBottomEdge;
			offset.y = bottomInset;
		}
        return canvasSpacePos + offset;
    }

	public Vector2 ConvertPositionToWorldSpace(Vector2 localLayoutPos) {
		if( originTopLeft ) localLayoutPos.y = height - localLayoutPos.y;
		var localPos = localLayoutPos - GetPivotPos(rectTransform);
		return rectTransform.TransformPoint(localPos);
	}
	/// <summary>
	/// Converts a point in local space of this SLayout to the local space of another SLayout.
	/// If you pass a null SLayout, it will get the point in the space of the canvas.
	/// Note - there might be a bug when layout is null, because it returns canvas space relative to the bottom corner rather than the canvas's pivot?
	/// </summary>
	public Vector2 ConvertPositionToTarget(Vector2 localLayoutPos, SLayout targetLayout) {
		var worldSpacePoint = ConvertPositionToWorldSpace(localLayoutPos);

		RectTransform targetRectTransform = targetLayout ? targetLayout.rectTransform : null;
		if( targetRectTransform == null ) targetRectTransform = (RectTransform)canvas.transform;

		var targetLocalPos = (Vector2) targetRectTransform.InverseTransformPoint(worldSpacePoint);
		var targetLayoutPos = targetLocalPos + GetPivotPos(targetRectTransform);

		if( targetLayout != null && targetLayout.originTopLeft )
			targetLayoutPos.y = targetLayout.height - targetLayoutPos.y;

		return targetLayoutPos;
	}

	/// <summary>
	/// Converts a rect in local space of this SLayout to the local space of another SLayout.
	/// If you pass a null SLayout, it will get the rect in the space of the canvas.
	/// </summary>
	public Rect ConvertRectToTarget(Rect localRect, SLayout targetLayout = null)
	{
		var convertedMin = ConvertPositionToTarget(localRect.min, targetLayout);
		var convertedMax = ConvertPositionToTarget(localRect.max, targetLayout);

		// Coordinate system may be flipped compared between SLayouts
		// (or if converting to canvas space)
		return new Rect(
			convertedMin.x,
			Mathf.Min(convertedMin.y, convertedMax.y),
			convertedMax.x - convertedMin.x,
			Mathf.Abs(convertedMin.y - convertedMax.y)
		);
	}

	float GetRectTransformX(RectTransform rt) {
		float toLeftEdge = rt.pivot.x * rt.rect.width;
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return 0.0f;
		
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		float leftInset = parentToLeftEdge + transform.localPosition.x - toLeftEdge;
		return leftInset;
	}

	float GetRectTransformY(RectTransform rt) {
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return 0.0f;
		
		if( originTopLeft ) {
			float toTopEdge = (1.0f-rt.pivot.y) * rt.rect.height;
			float parentToTopEdge = (1.0f-parentRectT.pivot.y) * parentRectT.rect.height;
			float topInset = parentToTopEdge - transform.localPosition.y - toTopEdge;
			return topInset;
		} else {
			float toBottomEdge = rt.pivot.y * rt.rect.height;
			float parentToBottomEdge = parentRectT.pivot.y * parentRectT.rect.height;
			float bottomInset = parentToBottomEdge + transform.localPosition.y - toBottomEdge;
			return bottomInset;
		}
	}

	void SetRectTransformX(float x)
	{
		var parentRT = parentRectTransform;
		if( parentRT == null ) return; // Happens when SLayout gets displaced outside of UI

		var rt = rectTransform;

		var parentPivotPosX = parentRT.pivot.x * parentRT.rect.width;
		var ownPivotPosX = rt.pivot.x * rt.rect.width;

		// X local to parent pivot (i.e. the localPosition)
		var localX = -parentPivotPosX + x + ownPivotPosX;

		var localPos = rt.localPosition;
		if(localPos.x == localX) return;
		localPos.x = localX;
		rt.localPosition = localPos;
	}

	void SetRectTransformY(float y)
	{
		var parentRT = parentRectTransform;
		if( parentRT == null ) return; // Happens when SLayout gets displaced outside of UI

		// Find Y local to parent pivot (i.e. the localPosition)
		float localY;
		
		var rt = rectTransform;
		if( originTopLeft ) {
			var parentPivotPosToTop = (1.0f-parentRT.pivot.y) * parentRT.rect.height;
			var ownPivotPosToTop = (1.0f-rt.pivot.y) * rt.rect.height;
			localY = parentPivotPosToTop - y - ownPivotPosToTop;
		} else {
			var parentPivotPosY = parentRT.pivot.y * parentRT.rect.height;
			var ownPivotPosY = rt.pivot.y * rt.rect.height;
			localY = -parentPivotPosY + y + ownPivotPosY;
		}
			
		var localPos = rt.localPosition;
		if(localPos.y == localY) return;
		localPos.y = localY;
		rt.localPosition = localPos;
	}
	
	void SetRectTransformWidth(float width)
	{
		var rt = rectTransform;
		// Always grow/shrink outward from left edge.
		// Reason is so that we have a consistent model and so it doesn't depend on the anchoring.
		// This can be faff sometimes (for example, if you only want to set the width while it's anchored
		// automatically to the right edge of the screen). But on balance we prefer the "full manual" model
		// since if you *do* want to change the X position it becomes a lot more complicated.
			
		var anchorsSep = (rt.anchorMax.x - rt.anchorMin.x) * parentRect.width;

		var sizeDelta = rt.sizeDelta;
		sizeDelta.x = width - anchorsSep;
		if(rt.sizeDelta.x != sizeDelta.x) {
			var originalLeftX = GetRectTransformX(rt);
			rt.sizeDelta = sizeDelta;
			// Restore original X position
			SetRectTransformX(originalLeftX);
		}
	}

	void SetRectTransformHeight(float height)
	{
		var rt = rectTransform;
		// Always grow/shrink outward from consistent edge based on originTopLeft flag.
		// Reason is so that we have a consistent model and so it doesn't depend on the anchoring.
		// This can be faff sometimes (for example, if you only want to set the height while it's anchored
		// automatically to the top edge of the screen). But on balance we prefer the "full manual" model
		// since if you *do* want to change the Y position it becomes a lot more complicated.

		var anchorsSep = (rt.anchorMax.y - rt.anchorMin.y) * parentRect.height;
		
		var sizeDelta = rt.sizeDelta;
		sizeDelta.y = height - anchorsSep;
		if(rt.sizeDelta.y != sizeDelta.y) {
			var originalY = GetRectTransformY(rt);
			rt.sizeDelta = sizeDelta;
			// Restore original Y position after size change
			SetRectTransformY(originalY);
		}
	}

	void InitX() {
		if( _x == null ) {
			_x = new SLayoutFloatProperty {
				getter = () => GetRectTransformX(rectTransform),
				setter = SetRectTransformX
			};
		}
	}

	void InitY() {
		if( _y == null ) {
			_y = new SLayoutFloatProperty {
				getter = () => GetRectTransformY(rectTransform),
				setter = SetRectTransformY
			};
		}
	}

	void InitWidth() {
		if( _width == null ) {
			_width = new SLayoutFloatProperty {
				getter = () => rectTransform.rect.width,
				setter = SetRectTransformWidth
			};
		}
	}

	void InitHeight() {
		if( _height == null ) {
			_height = new SLayoutFloatProperty {
				getter = () => rectTransform.rect.height,
				setter = SetRectTransformHeight
			};
		}
	}

	void InitRotation() {
		if( _rotation == null ){
			_rotation = new SLayoutAngleProperty {
				getter = () => transform.localRotation.eulerAngles.z,
				setter = r => transform.localRotation = Quaternion.Euler(0.0f, 0.0f, r)
			};
		}
	}

	void InitScale() {
		if( _scale == null ) {
			_scale = new SLayoutFloatProperty {
				getter = () => transform.localScale.x,
				setter = s =>  transform.localScale = new Vector3(s, s, s)
			};
		}
	}

	void InitGroupAlpha() {
		if( _groupAlpha == null ) {
			_groupAlpha = new SLayoutFloatProperty {
				getter = () => canvasGroup ? canvasGroup.alpha : 1.0f,
				setter = a => { if( canvasGroup ) canvasGroup.alpha = a; }
			};
		}
	}

	void InitColor() {
		if( _color == null ) {
			_color = new SLayoutColorProperty {
				getter = () => graphic ? graphic.color : Color.white,
				setter = c => {  if( graphic ) graphic.color = c;  }
			};
		}
	}
	

	protected override void OnTransformParentChanged () {
		base.OnTransformParentChanged();
		_rootCanvas = _canvas = null;
	}
	
	SLayoutFloatProperty _x;
	SLayoutFloatProperty _y;
	SLayoutFloatProperty _width;
	SLayoutFloatProperty _height;
	SLayoutAngleProperty _rotation;
	SLayoutFloatProperty _scale;
									 
	SLayoutFloatProperty _groupAlpha;
	SLayoutColorProperty _color;

}
