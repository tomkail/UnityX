using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class VirtualKeyboardManager : MonoSingleton<VirtualKeyboardManager> {
    // This is reported with 0,0 as top left rather than Unity's implementation of bottom left on iOS for some reason, so we flip it here.
    Rect correctedDeviceKeyboardScreenRect => new Rect(TouchScreenKeyboard.area.x, 0, TouchScreenKeyboard.area.width, TouchScreenKeyboard.area.height);
    
    public enum EditorSimulationMode {
        Off,
        On,
        OnWhenTextFieldSelected
    }
    public EditorSimulationMode editorSimulationMode;
    public bool simulateInEditor {
        get {
            if(editorSimulationMode == EditorSimulationMode.On) return true;
            else if(editorSimulationMode == EditorSimulationMode.OnWhenTextFieldSelected) {
                if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null) {
                    if(EventSystem.current.currentSelectedGameObject.GetComponentInParent<TMPro.TMP_InputField>() != null) return true;
                    if(EventSystem.current.currentSelectedGameObject.GetComponentInParent<UnityEngine.UI.InputField>() != null) return true;
                }
            }
            return false;
        }
    }
    
    [System.Flags]
    public enum EditorSimulationFlags {
        VisualizeKeyboardArea
    }
    public EditorSimulationFlags editorSimulationFlags;

    // This happens to be the size of the keyboard/screen height on the iPhone SE 2, but it's roughtly correct for other devices.
    public static float editorSimulationKeyboardScreenHeight => Screen.height * (662f/1334f);
    
    Rect lastCachedDeviceScreenRect;
    Rect lastCachedScreenRect => TouchScreenKeyboard.isSupported ? lastCachedDeviceScreenRect : editorSimulationKeyboardScreenRect;
    
    Rect editorSimulationKeyboardScreenRect => new Rect(0,0,Screen.width,editorSimulationKeyboardScreenHeight);

    
    public Rect nonAnimatedRemainingScreenRect => new Rect(0, nonAnimatedKeyboardScreenRect.height, Screen.width, Screen.height-nonAnimatedKeyboardScreenRect.height);
    public Rect nonAnimatedKeyboardScreenRect => TouchScreenKeyboard.isSupported ? correctedDeviceKeyboardScreenRect : (simulateInEditor ? editorSimulationKeyboardScreenRect : Rect.zero);

    public Rect animatedKeyboardScreenRect => new Rect(lastCachedScreenRect.x, lastCachedScreenRect.y, lastCachedScreenRect.width == 0 ? Screen.width : lastCachedScreenRect.width, lastCachedScreenRect.height * showAmount);
    public Rect animatedRemainingScreenRect => new Rect(0, animatedKeyboardScreenRect.height, Screen.width, Screen.height-animatedKeyboardScreenRect.height);

    float _showAmount = 0;
    public float showAmount => _showAmount;
    // float timeWaitingForVirtualKeyboardToAppear = 0;
    float timer = 0;
    const float showAnimationDuration = 0.475f;
    const float hideAnimationDuration = 0.475f;
    const EasingFunction.Ease easing = EasingFunction.Ease.EaseOutExpo;

    void Update () {
        var visible = TouchScreenKeyboard.isSupported ? correctedDeviceKeyboardScreenRect.height > 0 : simulateInEditor;
        if(correctedDeviceKeyboardScreenRect.height > 0) lastCachedDeviceScreenRect = correctedDeviceKeyboardScreenRect;
        // Some easing modes don't actually reset to 0/1 when the progress is 0/1, so we force to 0/1 when the timer is in those states
        if(visible) {
            timer = showAnimationDuration > 0 ? Mathf.Clamp01(timer + (Time.unscaledDeltaTime/showAnimationDuration)) : 1;
            _showAmount = timer == 1 ? 1 : EasingFunction.GetEasingFunction(easing)(0,1,timer);
        } else {
            timer = hideAnimationDuration > 0 ? Mathf.Clamp01(timer - (Time.unscaledDeltaTime/hideAnimationDuration)) : 0;
            _showAmount = timer == 0 ? 0 : EasingFunction.GetEasingFunction(easing)(1,0,1-timer);
        }
    }

    public static TMPro.TMP_InputField GetSelectedInputField () {
        var selectedGO = EventSystem.current.currentSelectedGameObject;
        return selectedGO == null ? null : selectedGO.GetComponentInParent<TMPro.TMP_InputField>();
    }
    public static float GetScreenOffsetToShowSelectedInputField (SLayout layout) {
        var selectedInputFieldRT = GetSelectedInputField()?.gameObject.GetComponent<RectTransform>();
        if(selectedInputFieldRT != null && selectedInputFieldRT.IsDescendentOf(layout.transform)) {
            var screenCenter = selectedInputFieldRT.GetScreenRect().center;
            var targetScreenCenter = VirtualKeyboardManager.Instance.nonAnimatedRemainingScreenRect.center;
            return targetScreenCenter.y-screenCenter.y;
        }
        return 0;
    }
    public static float GetOffsetToShowSelectedInputField (SLayout layout) {
        var screenOffsetY = GetScreenOffsetToShowSelectedInputField(layout);
        return layout.ScreenToSLayoutVector(new Vector2(0,screenOffsetY)).y;
    }

    void OnGUI () {
        if(editorSimulationFlags.HasFlag(EditorSimulationFlags.VisualizeKeyboardArea) && animatedKeyboardScreenRect.height > 0) {
            OnGUIX.BeginColor(Color.green);
            GUI.Box(OnGUIX.ScreenToGUIRect(animatedKeyboardScreenRect), "");
            OnGUIX.EndColor();
            // OnGUIX.BeginColor(Color.red.WithAlpha(0.5f));
            // GUI.Box(animatedRemainingScreenRect, "");
            // OnGUIX.EndColor();
        }
    }
}