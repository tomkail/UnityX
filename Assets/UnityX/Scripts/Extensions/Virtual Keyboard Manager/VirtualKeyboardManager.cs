using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class VirtualKeyboardManager : MonoSingleton<VirtualKeyboardManager> {
    public enum EditorSimulationMode {
        Off,
        On,
        OnWhenTextFieldSelected
    }
    public EditorSimulationMode editorSimulationMode;
    public bool simulateInEditor {
        get {
            if(editorSimulationMode == EditorSimulationMode.On) return true;
            else if (editorSimulationMode == EditorSimulationMode.OnWhenTextFieldSelected) return GetSelectedInputFieldRectTransform() != null;
            return false;
        }
    }
    
    public bool visualizeKeyboardAreaInEditor;

    // This happens to be the size of the keyboard/screen height on the iPhone SE 2, but it's roughtly correct for other devices.
    public static float editorSimulationKeyboardScreenHeight => Screen.height * (662f/1334f);

    // When true, we try to get the keyboard screen space area from the device, which allows us to accurately determine how much space the keyboard is taking up.
    // This setting exists because users often don't have much control over what the device reports and there can be cases where you want to pretend the keyboard takes up a certain amount of space.
    // The classic case of this is the keyboard pinging up and down when selection is taken away from a text field and then immediately returned.
    public bool tryUpdateKeyboardAreaRect = true;
    // This is reported with 0,0 as top left rather than Unity's implementation of bottom left on iOS for some reason, so we flip it here.
    Rect correctedDeviceKeyboardScreenRect => new Rect(TouchScreenKeyboard.area.x, 0, TouchScreenKeyboard.area.width, TouchScreenKeyboard.area.height);
    Rect lastCachedDeviceScreenRect;
    Rect lastCachedScreenRect => TouchScreenKeyboard.isSupported ? lastCachedDeviceScreenRect : editorSimulationKeyboardScreenRect;
    
    Rect editorSimulationKeyboardScreenRect => new Rect(0,0,Screen.width,editorSimulationKeyboardScreenHeight);

    
    public Rect nonAnimatedRemainingScreenRect => new Rect(0, nonAnimatedKeyboardScreenRect.height, Screen.width, Screen.height-nonAnimatedKeyboardScreenRect.height);
    public Rect nonAnimatedKeyboardScreenRect => TouchScreenKeyboard.isSupported ? correctedDeviceKeyboardScreenRect : (simulateInEditor ? editorSimulationKeyboardScreenRect : Rect.zero);

    public Rect animatedKeyboardScreenRect => new Rect(lastCachedScreenRect.x, lastCachedScreenRect.y, lastCachedScreenRect.width == 0 ? Screen.width : lastCachedScreenRect.width, lastCachedScreenRect.height * showAmount);
    public Rect animatedRemainingScreenRect => new Rect(0, animatedKeyboardScreenRect.height, Screen.width, Screen.height-animatedKeyboardScreenRect.height);

    public enum VisibilityState {
        FullyHidden,
        Showing,
        FullyShown,
        Hiding,
    }
    VisibilityState _visibilityState = VisibilityState.FullyHidden;
    public VisibilityState visibilityState {
        get => _visibilityState;
        private set {
            if (_visibilityState == value) return;
            var previousVisibilityState = _visibilityState;
            _visibilityState = value;
            if (OnChangeVisibilityState != null) OnChangeVisibilityState(previousVisibilityState, _visibilityState);
        }
    }
    public delegate void OnChangeVisibilityStateDelegate(VisibilityState previousVisibilityState, VisibilityState newVisibilityState);
    public OnChangeVisibilityStateDelegate OnChangeVisibilityState;
    
    float _showAmount;
    public float showAmount {
        get => _showAmount;
        private set {
            if (_showAmount == value) return;
            var previousShowAmount = _showAmount;
            _showAmount = value;
            if (OnChangeShowAmount != null) OnChangeShowAmount(previousShowAmount, _showAmount);
        }
    }
    public delegate void OnChangeShowAmountDelegate(float previousVisibleAmount, float currentVisibleAmount);
    public OnChangeShowAmountDelegate OnChangeShowAmount;


    RectTransform _selectedInputField;
    public RectTransform selectedInputField {
        get => _selectedInputField; 
        private set {
            if (_selectedInputField == value) return;
            var previousSelectedInputField = _selectedInputField;
            _selectedInputField = value;
            if (OnChangeSelectedInputField != null) OnChangeSelectedInputField(previousSelectedInputField, _selectedInputField);
        }
    }
    public delegate void OnChangeSelectedInputFieldDelegate(RectTransform previousInputField, RectTransform currentInputField);
    public OnChangeSelectedInputFieldDelegate OnChangeSelectedInputField;
    
    // float timeWaitingForVirtualKeyboardToAppear = 0;
    float showHideTimer;
    const float showAnimationDuration = 0.475f;
    const float hideAnimationDuration = 0.475f;
    const EasingFunction.Ease easing = EasingFunction.Ease.EaseOutExpo;

    void Update () {
        selectedInputField = GetSelectedInputFieldRectTransform();
        bool visible = TouchScreenKeyboard.isSupported ? correctedDeviceKeyboardScreenRect.height > 0 : simulateInEditor;
        if(tryUpdateKeyboardAreaRect) 
            TryCacheKeyboardAreaRect();
        // Some easing modes don't actually reset to 0/1 when the progress is 0/1, so we force to 0/1 when the timer is in those states
        float newShowAmount;
        if(visible) {
            showHideTimer = showAnimationDuration > 0 ? Mathf.Clamp01(showHideTimer + (Time.unscaledDeltaTime/showAnimationDuration)) : 1;
            newShowAmount = showHideTimer == 1 ? 1 : EasingFunction.GetEasingFunction(easing)(0,1,showHideTimer);
            if(newShowAmount == 1) visibilityState = VisibilityState.FullyShown;
            else visibilityState = VisibilityState.Showing;
        } else {
            showHideTimer = hideAnimationDuration > 0 ? Mathf.Clamp01(showHideTimer - (Time.unscaledDeltaTime/hideAnimationDuration)) : 0;
            newShowAmount = showHideTimer == 0 ? 0 : EasingFunction.GetEasingFunction(easing)(1,0,1-showHideTimer);
            if(newShowAmount == 0) visibilityState = VisibilityState.FullyHidden;
            else visibilityState = VisibilityState.Hiding;
        }
        showAmount = newShowAmount;
    }

    void TryCacheKeyboardAreaRect() {
        if(correctedDeviceKeyboardScreenRect.height > 0) lastCachedDeviceScreenRect = correctedDeviceKeyboardScreenRect;
    }

    public static RectTransform GetSelectedInputFieldRectTransform() {
        return GetSelectedTMPInputField()?.gameObject.GetComponent<RectTransform>() ?? GetSelectedUGUIInputField()?.gameObject.GetComponent<RectTransform>();
    }

    public static TMP_InputField GetSelectedTMPInputField () {
        if(EventSystem.current == null) return null;
        var selectedGO = EventSystem.current.currentSelectedGameObject;
        if (selectedGO == null) return null;
        var inputField = selectedGO.GetComponentInParent<TMP_InputField>();
        return inputField != null && inputField.isFocused ? inputField : null;
    }
    
    public static InputField GetSelectedUGUIInputField () {
        if(EventSystem.current == null) return null;
        var selectedGO = EventSystem.current.currentSelectedGameObject;
        if (selectedGO == null) return null;
        var inputField = selectedGO.GetComponentInParent<InputField>();
        return inputField != null && inputField.isFocused ? inputField : null;
    }

    // public Vector2 targetScreenRectPivot;
    // public Vector2 remainingScreenRectPivot;
    // public static float GetScreenOffsetToShowSelectedInputField (SLayout layout, Rect remainingScreenRect) {
    //     var selectedInputField = GetSelectedInputField();
    //     selectedInputField = FindObjectOfType<TMP_InputField>();
    //     var selectedInputFieldRT = selectedInputField == null ? null : selectedInputField.gameObject.GetComponent<RectTransform>();
    //     if(selectedInputFieldRT != null) {
    //         var inputFieldTextBoundsScreenRect = TextMeshProUtils.GetScreenRectOfTextBounds(selectedInputField.textComponent);
    //         var inputFieldTextScreenRect = selectedInputFieldRT.GetScreenRect();
    //         var screenCenter = inputFieldTextBoundsScreenRect.GetPointFromNormalizedPoint(Instance.targetScreenRectPivot);
    //         var targetScreenCenter = remainingScreenRect.GetPointFromNormalizedPoint(Instance.remainingScreenRectPivot);
    //         var offset = targetScreenCenter.y - screenCenter.y;
    //
    //         var amountLargerThanRect = inputFieldTextBoundsScreenRect.height - inputFieldTextScreenRect.height;
    //         var amountOffScreen = inputFieldTextBoundsScreenRect.height - remainingScreenRect.height;
    //         if (amountOffScreen > 0) offset += amountOffScreen * 0.5f;
    //         if (amountLargerThanRect > 0) offset += amountLargerThanRect * 0.5f;
    //         Debug.Log(offset+ " "+ amountOffScreen+" "+ inputFieldTextBoundsScreenRect+" "+inputFieldTextScreenRect);
    //         return offset;
    //     }
    //     return 0;
    // }
    
    public static float GetScreenOffsetToShowSelectedInputField (SLayout layout, Rect remainingScreenRect) {
        var selectedTMPInputField = GetSelectedTMPInputField();
        var selectedInputFieldRT = selectedTMPInputField == null ? null : selectedTMPInputField.GetRectTransform();
        // var selectedInputFieldRT = GetSelectedInputFieldRectTransform();
        if(selectedInputFieldRT != null && selectedInputFieldRT.IsDescendentOf(layout.transform)) {
            // selectedTMPInputField.ca
            var screenCenter = selectedInputFieldRT.GetScreenRect().center;
            var targetScreenCenter = remainingScreenRect.center;
            return targetScreenCenter.y-screenCenter.y;
        }
        return 0;
    }
    public static float GetScreenOffsetToShowSelectedInputField (SLayout layout) {
        return GetScreenOffsetToShowSelectedInputField(layout, Instance.nonAnimatedRemainingScreenRect);
    }
    
    public static float GetOffsetToShowSelectedInputField (SLayout layout, Rect remainingScreenRect) {
        var screenOffsetY = GetScreenOffsetToShowSelectedInputField(layout, remainingScreenRect);
        return layout.ScreenToSLayoutVector(new Vector2(0,screenOffsetY)).y;
    }
    public static float GetOffsetToShowSelectedInputField (SLayout layout) {
        var screenOffsetY = GetScreenOffsetToShowSelectedInputField(layout);
        return layout.ScreenToSLayoutVector(new Vector2(0,screenOffsetY)).y;
    }

    #if UNITY_EDITOR    
    void OnGUI () {
        if(visualizeKeyboardAreaInEditor && animatedKeyboardScreenRect.height > 0) {
            OnGUIX.BeginColor(Color.green);
            GUI.Box(OnGUIX.ScreenToGUIRect(animatedKeyboardScreenRect), "");
            OnGUIX.EndColor();
            // OnGUIX.BeginColor(Color.red.WithAlpha(0.5f));
            // GUI.Box(animatedRemainingScreenRect, "");
            // OnGUIX.EndColor();
        }
    }
    #endif    
}