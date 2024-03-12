#if DEBUG
//#define DEBUG_SLAYOUT
#endif

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
	public bool originTopLeft = true;

	/// <summary>
	/// Useful to detect changes due to the auto layout system changing the size and position of
	/// a top level RectTransform due to non-SLayout related things like screen size changing or
	/// simply something else controlling the (top level) view.
	/// </summary>
	public event Action<SLayout> onRectChange;

	public bool isAnimating {
		get {
			if(SLayoutAnimator.quitting) return false;
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
	
	// This allows us to cancel animations on an SLayout when it's destroyed
	bool GetIsValidForAnimation() {
		return this != null;
	}
	
	protected override void OnRectTransformDimensionsChange() {
		base.OnRectTransformDimensionsChange();
		if( onRectChange != null ) onRectChange(this);
	}
	

	public AutoSLayoutAnimation Animate(float duration, Action animAction) {
		return Animate(duration, 0.0f, animAction);
	}

	public AutoSLayoutAnimation Animate(float duration, float delay, Action animAction) {
		return Animate(duration, delay, (SLayoutAnimation.EasingFunc)default, animAction);
	}
	
	public AutoSLayoutAnimation Animate(float duration, float delay, SLayoutAnimation.EasingFunc easingFunction, Action animAction) {
		var newAnim = new AutoSLayoutAnimation {
			_duration = duration, 
			_maxDuration = duration, 
			_delay = delay, 
			_maxDelay = delay, 
			_easingFunction = easingFunction, 
			_animAction = animAction,
			_owner = this
		};
		#if UNITY_EDITOR
		if (!EditorApplication.isPlaying) {
			newAnim.Start();
			newAnim.CompleteImmediate();
			return newAnim;
		}
		if (SLayoutAnimator.quitting) {
			return null;
		}
		#endif
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}

	public AutoSLayoutAnimation Animate(float duration, float delay, AnimationCurve customCurve, Action animAction) {
		return Animate(duration, delay, customCurve.Evaluate, animAction);
	}
	
	public AutoSLayoutAnimation Animate(float duration, EasingFunction.Ease easing, Action animAction) {
		return Animate(duration, 0.0f, easing, animAction);
	}
	
	public AutoSLayoutAnimation Animate(float duration, float delay, EasingFunction.Ease easing, Action animAction) {
		return Animate(duration, delay, v => EasingFunction.GetEasingFunction(easing)(0,1,v), animAction);
	}

	public AutoSLayoutAnimation AnimateCustom(float duration, Action<float> customAnimAction) {
		return AnimateCustom(duration, 0, customAnimAction);
	}

	public AutoSLayoutAnimation AnimateCustom(float duration, float delay, Action<float> customAnimAction)
	{
		var newAnim = new AutoSLayoutAnimation {
			_duration = duration, 
			_maxDuration = duration, 
			_delay = delay, 
			_maxDelay = delay, 
			_animAction = () => Animatable(customAnimAction),
			_owner = this
		};
		#if UNITY_EDITOR
		if (!EditorApplication.isPlaying) {
			newAnim.Start();
			newAnim.CompleteImmediate();
			return newAnim;
		}
		if (SLayoutAnimator.quitting) {
			return null;
		}
		#endif
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}
	
	public AutoSLayoutAnimation After(float delay, Action nonAnimatedAction) {
		var newAnim = new AutoSLayoutAnimation {
			_duration = 0.0f, 
			_maxDuration = 0.0f, 
			_delay = delay, 
			_maxDelay = delay, 
			_nonAnimatedAction = nonAnimatedAction,
			_owner = this
		};
#if UNITY_EDITOR
		if (!EditorApplication.isPlaying) {
			newAnim.Start();
			newAnim.CompleteImmediate();
			return newAnim;
		}
		if (SLayoutAnimator.quitting) {
			return null;
		}
#endif
		SLayoutAnimator.instance.StartAnimation(newAnim);
		return newAnim;
	}
	
	/// <summary>
	/// While an animation is currently being defined, insert an extra delay, so that any animated values
	/// that are set after this call begin a bit later than previously defined elements.
	/// </summary>
	public void AddDelay(float extraDelay)
	{
		if(SLayoutAnimator.quitting) return;
		SLayoutAnimator.instance.AddDelay(extraDelay);
	}

	/// <summary>
	/// While an animation is currently being defined, insert an extra duration, so that any animated values
	/// that are set after this call have a longer duration than those defined before it.
	/// </summary>
	public void AddDuration(float extraDuration)
	{
		if(SLayoutAnimator.quitting) return;
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
		if(SLayoutAnimator.quitting) return;
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
		if(SLayoutAnimator.quitting) return;
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
		if(SLayoutAnimator.quitting) return;
		SLayoutAnimator.instance.Animatable(t => setter(Color.Lerp(initial, target, t)));
	}

	public void CancelAnimations()
	{
		if(SLayoutAnimator.quitting) return;
		if(SLayoutAnimator.instance == null) return;
		SLayoutAnimator.instance.CancelAnimations(this);
	}

	public void CompleteAnimations()
	{
		if(SLayoutAnimator.quitting) return;
		SLayoutAnimator.instance.CompleteAnimations(this);
	}

    public static void WithoutAnimating(Action action) {
	    SLayoutAnimation.WithoutAnimating(action);
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
			_canvas = transform.GetComponent<Canvas>();
			if(_canvas == null)
				_canvas = transform.GetComponentInParent<Canvas>(true);
			return _canvas;
		}
	}
	Canvas _canvas;

	/// <summary>
	/// Width of canvas, taking into account scaling mode.
	/// </summary>
	public float canvasWidth => ((RectTransform)canvas.transform).rect.width;

	/// <summary>
	/// Height of canvas, taking into account scaling mode.
	/// </summary>
	public float canvasHeight => ((RectTransform)canvas.transform).rect.height;

	public Vector2 canvasSize => ((RectTransform)canvas.transform).rect.size;

	public RectTransform rectTransform => (RectTransform)transform;

	public CanvasGroup canvasGroup {
		get {
			if( _canvasGroup == null ) _canvasGroup = GetComponent<CanvasGroup>();
			return _canvasGroup;
		}
	}
	CanvasGroup _canvasGroup;

	public Image image => graphic as Image;

	public Text text => graphic as Text;

	public Graphic graphic {
		get {
			if( _graphic == null ) _graphic = GetComponent<Graphic>();
			return _graphic;
		}
	}
	Graphic _graphic;

	public SLayout parent => transform.parent.GetComponent<SLayout>();

	public Rect parentRect {
		get {
			var parentRT = parentRectTransform;
			if( parentRT == null ) return Rect.zero; // e.g. if SLayout has been misplaced!
			var localRect = parentRT.rect;
			return new Rect(GetRectTransformX(parentRT), GetRectTransformY(parentRT), localRect.width, localRect.height);
		}
	}
	
	public Rect targetParentRect {
		get {
			if (parent == null) return parentRect;
			return parent.targetRect;
		}
	}

	public RectTransform parentRectTransform => transform.parent as RectTransform;

	/// <summary>
	/// Allows the speed of an animation to be sped up or slowed down independently
	/// of any scalars that exist using the standard Time.timeScale. This is useful
	/// since often UI is used within pause menus.
	/// To apply a time scale, put a SLayoutCanvasTimeScalar on the canvas for this SLayout.
	/// </summary>
	public float timeScale => timeScalar == null ? 1 : timeScalar.timeScale;

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
	
	
	SLayoutFloatProperty _x;
	SLayoutFloatProperty InitX() {
		_x ??= new SLayoutFloatProperty {
			layout = this,
			getter = () => GetRectTransformX(rectTransform),
			setter = SetRectTransformX,
			isValid = GetIsValidForAnimation
		};
		return _x;
	}
	public float x {
		get => InitX().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitX().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startX {
		get => InitX().GetProperty(SLayoutProperty.GetMode.AnimStart);
        set => InitX().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetX {
		get => InitX().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitX().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	
	SLayoutFloatProperty _y;
	SLayoutFloatProperty InitY() {
		_y ??= new SLayoutFloatProperty {
			layout = this,
			getter = () => GetRectTransformY(rectTransform),
			setter = SetRectTransformY,
			isValid = GetIsValidForAnimation
		};
		return _y;
	}
	public float y {
		get => InitY().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitY().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startY {
		get => InitY().GetProperty(SLayoutProperty.GetMode.AnimStart);
        set => InitY().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetY {
		get => InitY().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitY().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	
	SLayoutFloatProperty _width;
	SLayoutFloatProperty InitWidth() {
		_width ??= new SLayoutFloatProperty {
			layout = this,
			getter = () => rectTransform.rect.width,
			setter = SetRectTransformWidth,
			isValid = GetIsValidForAnimation
		};
		return _width;
	}
	public float width {
		get => InitWidth().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitWidth().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startWidth {
		get => InitWidth().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitWidth().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetWidth {
		get => InitWidth().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitWidth().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	
	SLayoutFloatProperty _height;
	SLayoutFloatProperty InitHeight() {
		_height ??= new SLayoutFloatProperty {
			layout = this,
			getter = () => rectTransform.rect.height,
			setter = SetRectTransformHeight,
			isValid = GetIsValidForAnimation
		};
		return _height;
	}
	public float height {
		get => InitHeight().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitHeight().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startHeight {
		get => InitHeight().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitHeight().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetHeight {
		get => InitHeight().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitHeight().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	

	public Vector2 position {
		get => new(x, y);
		set {
			x = value.x;
			y = value.y;
		}
	}
	public Vector2 startPosition {
		get => new(startX, startY);
		set {
			startX = value.x;
			startY = value.y;
		}
	}
	public Vector2 targetPosition {
		get => new(targetX, targetY);
		set {
			targetX = value.x;
			targetY = value.y;
		}
	}

	public Vector2 size {
		get => new(width, height);
		set {
			width = value.x;
			height = value.y;
		}
	}
	public Vector2 startSize {
		get => new(startWidth, startHeight);
		set {
			startWidth = value.x;
			startHeight = value.y;
		}
	}
	public Vector2 targetSize {
		get => new(targetWidth, targetHeight);
		set {
			targetWidth = value.x;
			targetHeight = value.y;
		}
	}
	
	
	SLayoutAngleProperty _rotation;
	SLayoutAngleProperty InitRotation() {
		_rotation ??= new SLayoutAngleProperty {
			layout = this,
			getter = () => transform.localRotation.eulerAngles.z,
			setter = r => transform.localRotation = Quaternion.Euler(0.0f, 0.0f, r),
			isValid = GetIsValidForAnimation
		};
		return _rotation;
	}
	public float rotation {
		get => InitRotation().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitRotation().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startRotation {
		get => InitRotation().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitRotation().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetRotation {
		get => InitRotation().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitRotation().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	
	SLayoutFloatProperty _scale;
	SLayoutFloatProperty InitScale() {
		_scale ??= new SLayoutFloatProperty {
			layout = this,
			getter = () => transform.localScale.x,
			setter = s => transform.localScale = new Vector3(s, s, s),
			isValid = GetIsValidForAnimation
		};
		return _scale;
	}
	public float scale {
		get => InitScale().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitScale().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startScale {
		get => InitScale().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitScale().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetScale {
		get => InitScale().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitScale().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	
	SLayoutFloatProperty _groupAlpha;
	SLayoutFloatProperty InitGroupAlpha() {
		_groupAlpha ??= new SLayoutFloatProperty {
			layout = this,
			getter = () => canvasGroup ? canvasGroup.alpha : 1.0f,
			setter = a => {
				if (canvasGroup) canvasGroup.alpha = a;
			},
			isValid = GetIsValidForAnimation
		};
		return _groupAlpha;
	}
	public float groupAlpha {
		get => InitGroupAlpha().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitGroupAlpha().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startGroupAlpha {
		get => InitGroupAlpha().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitGroupAlpha().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetGroupAlpha {
		get => InitGroupAlpha().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitGroupAlpha().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	
	SLayoutColorProperty _color;
	SLayoutColorProperty InitColor() {
		_color ??= new SLayoutColorProperty {
			layout = this,
			getter = () => graphic ? graphic.color : Color.white,
			setter = c => {
				if (graphic) graphic.color = c;
			},
			isValid = GetIsValidForAnimation
		};
		return _color;
	}
	public Color color {
		get => InitColor().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitColor().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public Color startColor {
		get => InitColor().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitColor().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public Color targetColor {
		get => InitColor().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitColor().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
	}
	
	public float alpha {
		get => InitColor().value.a;
		set {
			InitColor();
			var color = _color.value;
			color.a = value;
			_color.value = color;
		}
	}
	public float targetAlpha => targetColor.a;

	
	public Rect rect {
		get => new(x, y, width, height);
		set {
			x = value.x;
			y = value.y;
			width = value.width;
			height = value.height;
		}
	}
	public Rect startRect {
		get => new(startX, startY, startWidth, startHeight);
		set {
			startX = value.x;
			startY = value.y;
			startWidth = value.width;
			startHeight = value.height;
		}
	}
	public Rect targetRect {
		get => new(targetX, targetY, targetWidth, targetHeight);
		set {
			targetX = value.x;
			targetY = value.y;
			targetWidth = value.width;
			targetHeight = value.height;
		}
	}

	public Rect localRect {
		get => new(0.0f, 0.0f, width, height);
		set {
			x = x + value.x;
			y = y + value.y;
			width = value.width;
			height = value.height;
		}
	}

	public Rect targetLocalRect => new(0.0f, 0.0f, targetWidth, targetHeight);

	public Vector2 center {
		get => new(centerX, centerY);
		set {
			centerX = value.x;
			centerY = value.y;
		}
	}

	public Vector2 targetCenter => new(targetCenterX, targetCenterY);

	public float centerX {
		get => x + 0.5f*width;
		set => x = value - 0.5f*width;
	}

	public float targetCenterX => targetX + 0.5f*targetWidth;

	public float centerY {
		get => y + 0.5f*height;
		set => y = value - 0.5f*height;
	}

	public float targetCenterY => targetY + 0.5f*targetHeight;


	/// <summary>
    /// Local center (i.e. simply 0.5*width, 0.5*height)
    /// </summary>
    public Vector2 middle => new(middleX, middleY);

	/// <summary>
    /// Local center (i.e. simply 0.5*width)
    /// </summary>
	public float middleX => 0.5f*width;

	/// <summary>
    /// Local center (i.e. simply 0.5*height)
    /// </summary>    
	public float middleY => 0.5f*height;


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
		get => new(pivotX, pivotY);
		set {
			pivotX = value.x;
			pivotY = value.y;
		}
	}
	
	/// <summary>
	/// X position of pivot in own pixel space (not normalised)
	/// </summary>
	public float pivotX {
		get => rectTransform.pivot.x * width;
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
		get => rectTransform.pivot.y * height;
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
		get => new(originX, originY);
		set {
			originX = value.x;
			originY = value.y;
		}
	}

	public float rightX {
		get => x + width;
		set => x = value - width;
	}

	public float targetRightX => targetX + targetWidth;

	public float bottomY {
		get {
			if(originTopLeft) return y + height;
			else return y;
		}
		set {
			if(originTopLeft) y = value - height;
			else y = value;
		}
	}

	public float targetBottomY {
		get {
			if(originTopLeft) return targetY + targetHeight;
			else return targetY;
		}
	}

	public float topY {
		get {
			if(originTopLeft) return y;
			else return y + height;
		}
		set {
			if(originTopLeft) y = value;
			else y = value - height;
		}
	}

	public float targetTopY {
		get {
			if(originTopLeft) return targetY;
			else return targetY + targetHeight;
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


    
	// Converts a screen position to a local position in the layout space.
    public Vector2 ScreenToSLayoutPosition (Vector2 screenPoint) {
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, canvas.rootCanvas.worldCamera, out Vector3 worldPoint);
		return WorldToSLayoutPosition(worldPoint);
    }
	
    public Vector2 ScreenToSLayoutVector (Vector2 screenVector) {
        return ScreenToSLayoutPosition(screenVector) - ScreenToSLayoutPosition(Vector2.zero);
    }

	// Converts a screen rect to a rect that can be applied to this layout
    public Rect ScreenToSLayoutRect (Rect screenRect) {
		var min = ScreenToSLayoutPosition(screenRect.min);
		var max = ScreenToSLayoutPosition(screenRect.max);
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }
	
	// This returns a coordinate to be applied to this object's position property
    public Vector2 WorldToSLayoutPosition (Vector3 worldPoint) {
		var rt = rectTransform;
		var parentRectT = rt.parent as RectTransform;

		var localPoint = transform.parent.InverseTransformPoint(worldPoint);
		Vector2 anchoredPos = (Vector2)localPoint + pivot;
		
		float toLeftEdge = rt.pivot.x * width;
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		float leftInset = parentToLeftEdge - toLeftEdge;
		anchoredPos.x += leftInset;
		
		if(originTopLeft) {
			// This calculation can almost certainly be simplied a LOT. This system confuses the heck out of me and I worked it out by just hacking things about.
			float toTopEdge = (1.0f-rt.pivot.y) * height;
			float parentToTopEdge = (1.0f-parentRectT.pivot.y) * parentRectT.rect.height;
			float topInset = parentToTopEdge - toTopEdge;
			anchoredPos.y += topInset;
			
			anchoredPos.y = parentRectT.rect.height - anchoredPos.y;
			anchoredPos.y -= parentRectT.rect.height * (parentRectT.pivot.y - 0.5f) * 2;
			// BUG - this line doesn't work when called when animating the rect, because the height is not yet updated.
			// Fixing it is probably a pain - we probably need to pass in the height (or really a model of the layout with the height already set)
			anchoredPos.y -= height * (1-rt.pivot.y) * 2;
			anchoredPos.y += height;
		} else {
			float toBottomEdge = rt.pivot.y * height;
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
	// I suspect this can convert from canvas to world, and then use WorldToSLayoutPosition, which has been tested to work.
    public Vector2 CanvasToSLayoutSpace (Vector2 canvasSpacePos) {
		Vector2 offset = Vector2.zero;

        var rt = rectTransform;
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return offset;
		
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		offset.x = parentToLeftEdge;

		if(originTopLeft) {
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
		if(originTopLeft) localLayoutPos.y = height - localLayoutPos.y;
		var localPos = localLayoutPos - GetPivotPos(rectTransform);
		return rectTransform.TransformPoint(localPos);
	}
	
	/// <summary>
	/// Converts a point in local space of this SLayout to the local space of another SLayout. Local space is relative to the layout's position and direction as specified by the "origin top left" property
	/// If you pass a null SLayout, it will get the point in the space of the canvas.
	/// </summary>
	public Vector2 ConvertPositionToTarget(Vector2 localLayoutPos, SLayout targetLayout) {
		var worldSpacePoint = ConvertPositionToWorldSpace(localLayoutPos);
		if (targetLayout == null) return worldSpacePoint;

		RectTransform targetRectTransform = targetLayout.rectTransform;

		var targetLocalPos = (Vector2) targetRectTransform.InverseTransformPoint(worldSpacePoint);
		var targetLayoutPos = targetLocalPos + GetPivotPos(targetRectTransform);
		
		if( targetLayout != null && targetLayout.originTopLeft )
			targetLayoutPos.y = targetLayout.height - targetLayoutPos.y;

		return targetLayoutPos;
	}

	public Vector2 LocalToScreenPosition(Vector2 localPos) {
		var worldPos = ConvertPositionToTarget(localPos, null);
		Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace ? canvas.worldCamera : null;
		return RectTransformUtility.WorldToScreenPoint(cam, worldPos);
	}
	
	public Vector2 LocalToScreenVector(Vector2 localPos) {
		return LocalToScreenPosition(localPos) - LocalToScreenPosition(Vector2.zero);
	}

	/// <summary>
	/// Converts a rect in local space of this SLayout to the local space of another SLayout. Local space is relative to the layout's position and direction as specified by the "origin top left" property
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

	/// <summary>
	/// Converts a rect in local space of this SLayout to screen space. Local space is relative to the layout's position and direction as specified by the "origin top left" property
	/// </summary> 
	static Vector3[] worldCorners = new Vector3[4];
	public Rect LocalToScreenRect(Rect localRect) {
		worldCorners[0] = ConvertPositionToTarget(new Vector2(localRect.x, localRect.y), null);
		worldCorners[1] = ConvertPositionToTarget(new Vector2(localRect.xMax, localRect.y), null);
		worldCorners[2] = ConvertPositionToTarget(new Vector2(localRect.x, localRect.yMax), null);
		worldCorners[3] = ConvertPositionToTarget(new Vector2(localRect.xMax, localRect.yMax), null);
		return WorldToScreenRect(rootCanvas, worldCorners);
		Rect WorldToScreenRect(Canvas canvas, Vector3[] worldCorners) {
			Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace ? canvas.worldCamera : null;
            
			float xMin = float.PositiveInfinity;
			float xMax = float.NegativeInfinity;
			float yMin = float.PositiveInfinity;
			float yMax = float.NegativeInfinity;
			for (int i = 0; i < 4; i++) {
				var screenCoord = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
				if (screenCoord.x < xMin)
					xMin = screenCoord.x;
				if (screenCoord.x > xMax)
					xMax = screenCoord.x;
				if (screenCoord.y < yMin)
					yMin = screenCoord.y;
				if (screenCoord.y > yMax)
					yMax = screenCoord.y;
			}
			return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
		}
	}
	
	/// <summary>
	/// Common utility function that uses LocalToScreenRect. Returns the screen space rect of the layout.
	/// </summary> 
	public Rect GetScreenRect() {
		return LocalToScreenRect(localRect);
	}
	
	/// <summary>
	/// Common utility function that uses LocalToScreenRect. Returns the screen space rect of the layout's target rect.
	/// </summary>
	public Rect GetScreenTargetRect() {
		return LocalToScreenRect(new Rect(targetRect.x-rect.x, targetRect.y-rect.y, targetLocalRect.width, targetLocalRect.height)); 
	}
	
	/// <summary>
	/// Common utility function that uses LocalToScreenRect. Returns the screen space rect of a target rect.
	/// </summary>
	public Rect GetScreenTargetRect(Rect _targetRect) {
		return LocalToScreenRect(new Rect(_targetRect.x-rect.x, _targetRect.y-rect.y, targetLocalRect.width, targetLocalRect.height)); 
	}
	

	public float GetRectTransformX(RectTransform rt, bool usingScale = false) {
		float toLeftEdge = rt.pivot.x * rt.rect.width;
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return 0.0f;
		
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		float leftInset = parentToLeftEdge + transform.localPosition.x - toLeftEdge * (usingScale ? transform.localScale.x : 1);
		return leftInset;
	}

	public float GetRectTransformY(RectTransform rt, bool usingScale = false) {
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return 0.0f;
		
		if(originTopLeft) {
			float toTopEdge = (1.0f-rt.pivot.y) * rt.rect.height;
			float parentToTopEdge = (1.0f-parentRectT.pivot.y) * parentRectT.rect.height;
			float topInset = parentToTopEdge - transform.localPosition.y - toTopEdge * (usingScale ? transform.localScale.y : 1);
			return topInset;
		} else {
			float toBottomEdge = rt.pivot.y * rt.rect.height;
			float parentToBottomEdge = parentRectT.pivot.y * parentRectT.rect.height;
			float bottomInset = parentToBottomEdge + transform.localPosition.y - toBottomEdge * (usingScale ? transform.localScale.y : 1);
			return bottomInset;
		}
	}

	void SetRectTransformX(float x) {
		SetRectTransformX(x, false);
	}
	public void SetRectTransformX(float x, bool usingScale)
	{
		var parentRT = parentRectTransform;
		if( parentRT == null ) return; // Happens when SLayout gets displaced outside of UI

		var rt = rectTransform;

		var parentPivotPosX = parentRT.pivot.x * parentRT.rect.width;
		var ownPivotPosX = rt.pivot.x * rt.rect.width;

		// X local to parent pivot (i.e. the localPosition)
		var localX = -parentPivotPosX + x + ownPivotPosX * (usingScale ? transform.localScale.x : 1);

		var localPos = rt.localPosition;
		if(localPos.x == localX) return;
		localPos.x = localX;
		rt.localPosition = localPos;
	}

	public void SetRectTransformY(float y) {
		SetRectTransformY(y, false);
	}
	public void SetRectTransformY(float y, bool usingScale)
	{
		var parentRT = parentRectTransform;
		if( parentRT == null ) return; // Happens when SLayout gets displaced outside of UI

		// Find Y local to parent pivot (i.e. the localPosition)
		float localY;
		
		var rt = rectTransform;
		if(originTopLeft) {
			var parentPivotPosToTop = (1.0f-parentRT.pivot.y) * parentRT.rect.height;
			var ownPivotPosToTop = (1.0f-rt.pivot.y) * rt.rect.height;
			localY = parentPivotPosToTop - y - ownPivotPosToTop * (usingScale ? transform.localScale.y : 1);
		} else {
			var parentPivotPosY = parentRT.pivot.y * parentRT.rect.height;
			var ownPivotPosY = rt.pivot.y * rt.rect.height * (usingScale ? transform.localScale.y : 1);
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
		if(!NearlyEqual(rt.sizeDelta.x, sizeDelta.x)) {
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
		if(!NearlyEqual(rt.sizeDelta.y, sizeDelta.y)) {
			var originalY = GetRectTransformY(rt);
			rt.sizeDelta = sizeDelta;
			// Restore original Y position after size change
			SetRectTransformY(originalY);
		}
	}

	
	protected override void OnTransformParentChanged () {
		base.OnTransformParentChanged();
		_rootCanvas = _canvas = null;
	}
	
	

	//Similar to Unity's epsilon comparison, but allows for any precision.
	static bool NearlyEqual(float a, float b, float maxDifference = 0.001f) {
		if (a == b)  { 
			return true;
		} else {
			return Mathf.Abs(a - b) < maxDifference;
		}
	}
}