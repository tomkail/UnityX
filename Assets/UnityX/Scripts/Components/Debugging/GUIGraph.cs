using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GUIGraph
{
    #if UNITY_EDITOR
    public static void DrawLayout (float currentValue, Vector2 size, float min = float.MaxValue, float max = float.MinValue, float scale = 1.0f, string tag = "") {
		Rect r = EditorGUILayout.BeginVertical();
        Draw(currentValue, new Rect(r.x, r.y, size.x, size.y), min, max, scale, tag);
		GUILayout.Space(size.y);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
	}
    public static void DrawLayout (float currentValue, float height, float min = float.MaxValue, float max = float.MinValue, float scale = 1.0f, string tag = "") {
		Rect r = EditorGUILayout.BeginVertical();
        Draw(currentValue, new Rect(r.x, r.y, r.width, height), min, max, scale, tag);
		GUILayout.Space(height);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
	}
    #endif

    /// <summary>
    /// Draw an oscilloscope-like graph of a value over time. It automatically caches the samples you feed it to draw a graph.
    /// By default, it will automatically record the high/low water marks to decide on the vertical scale, however you can also
    /// choose to pass in min/max Y values. The scale parameter changes the horizontal scroll speed so you can stretch/squash
    /// the data. By default though it will try to use a single pixel per 30Hz frame which is good for most purposes. 
    /// A scale of 0.1 will be "short" and therefore "fast". A scale of 2.0 will be "long" and therefore "slow". Warning: using
    /// a high scale will cause it to have to draw lots of samples which could get slow.
    /// Pass a tag if you're drawing multiple graphs, so that they can be distinguished internally as well as visually labelled.
    /// Simplest example of usage:
    /// 
    /// void OnGUI()
    /// {
    ///     GUIGraph.Draw(_inputMoveSpeedX, new Rect(10, 10, 150, 150));
    /// }
    /// 
    /// Using a tag (name) to draw multiple graphs:
    /// 
    /// void OnGUI()
    /// {
    ///     GUIGraph.Draw(_inputMoveSpeedX,         new Rect(10, 10, 150, 150),   tag: "move speed");
    ///     GUIGraph.Draw(Input.mouseScrollDelta.x, new Rect(10, 170, 150, 150),  tag: "scroll delta");
    /// }
    /// 
    /// </summary>
    public static void Draw(float currentValue, Rect position, float min = float.MaxValue, float max = float.MinValue, float scale = 1.0f, string tag = "")
    {
        if( _graphsByTag == null )
            _graphsByTag = new Dictionary<string, Graph>();

        Graph graph;
        if( !_graphsByTag.TryGetValue(tag, out graph) )
            _graphsByTag[tag] = graph = new Graph();

        double currentTime = Time.unscaledTime;
#if UNITY_EDITOR
        if( !Application.isPlaying ) {
            currentTime = UnityEditor.EditorApplication.timeSinceStartup;
        }
#endif

        float minimumTimeStep = (1.0f / 60.0f);
        float defaultSampleDuration = (1.0f / 30.0f);

        var secondsWidth = position.width * scale * defaultSampleDuration;
        var earliestTimeBound = currentTime - secondsWidth;

        // Update values only if we're on a new frame
        if (currentTime >= graph.lastSampleTime + minimumTimeStep)
        {
            graph.lastSampleTime = currentTime;

            // Add latest value
            graph.values.Enqueue(new Sample(currentValue, currentTime));

            // Drain values out of range
            while (graph.values.Count > 0 && graph.values.Peek().time < earliestTimeBound)
                graph.values.Dequeue();

            // Record high/low water marks for automatic bounds
            graph.lowWaterMark = Mathf.Min(graph.lowWaterMark, currentValue);
            graph.highWaterMark = Mathf.Max(graph.highWaterMark, currentValue);
        }

        // Choose automatic or manual bounds
        if (min >= float.MaxValue) min = graph.lowWaterMark;
        if (max <= float.MinValue) max = graph.highWaterMark;

        var whiteTex = Texture2D.whiteTexture;

        // Background
        GUI.color = Color.black.WithAlpha(0.9f);
        GUI.DrawTexture(position, whiteTex);

        // y=0
        if( min <= 0 && max >= 0 ) {
            GUI.color = Color.yellow.WithAlpha(0.3f);
            float normY = Mathf.InverseLerp(min, max, 0);
            GUI.DrawTexture(new Rect(position.x, position.yMax - normY * position.height, position.width, 1), whiteTex);
        }

        // High/low water marks
        float labelHeight = 20;
        GUI.color = Color.gray;
        GUI.Label(new Rect(position.x, position.y, position.width, labelHeight), max.ToString());
        GUI.Label(new Rect(position.x, position.yMax - labelHeight, position.width, labelHeight), min.ToString());


        // Tag
        if(!string.IsNullOrWhiteSpace(tag)) {
            GUI.Label(new Rect(position.x, position.yMax - 5, position.width, labelHeight), tag, centeredStyle);
        }

        // Actual samples
        GUI.color = Color.white;

        var prev = Vector2.zero;
        foreach(var sample in graph.values) {

            float xNorm = Mathf.InverseLerp(min, max, sample.val);

            var timeFromStart = sample.time - earliestTimeBound;
            float yNorm = 0.0f;
            if( timeFromStart > 0 )
                yNorm = Mathf.Clamp01((float)((sample.time - earliestTimeBound) / secondsWidth));

            var point = new Vector2(
                position.x + yNorm * position.width, 
                position.yMax - xNorm * position.height
            );

            if (prev == Vector2.zero)
                prev = new Vector2(point.x-1, point.y);

            DrawLine(prev, point, whiteTex);

            prev = point;
        }
    }

    /// <summary>
    /// Draw a square wave of a boolean changing over time. For more information, see description of 
    /// the Draw method for floating point values.
    /// </summary>
    public static void Draw(bool boolValue, Rect position, float scale = 1.0f, string tag = "")
    {
        Draw(boolValue ? 1 : 0, position, min: -0.2f, max: 1.2f, scale: scale, tag: tag);
    }

    static void DrawLine(Vector2 start, Vector2 end, Texture tex)
    {
        float width = Vector2.Distance(start, end);

        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

        GUIUtility.RotateAroundPivot(angle, start);

        GUI.DrawTexture(new Rect(start.x, start.y, width, 1.0f), tex);

        GUI.matrix = Matrix4x4.identity;
    }

    public static void Clear(string tag = "")
    {
        if (_graphsByTag == null) return;

        Graph graph;
        if (_graphsByTag.TryGetValue(tag, out graph))
            graph.ClearValues();
    }

    static GUIStyle centeredStyle {
        get {
            if( _centeredStyle == null ) {
                _centeredStyle = new GUIStyle(GUI.skin.label);
                _centeredStyle.alignment = TextAnchor.UpperCenter;
            }
            return _centeredStyle;
        }
    }
    static GUIStyle _centeredStyle;
        

    struct Sample
    {
        public float val;
        public double time;

        public Sample(float val, double time) {
            this.val = val;
            this.time = time;
        }
    }

    class Graph {
        public float highWaterMark;
        public float lowWaterMark;
        public Queue<Sample> values;
        public double lastSampleTime;

        public Graph() {
            values = new Queue<Sample>();
            lowWaterMark = float.MaxValue;
            highWaterMark = float.MinValue;
        }

        public void ClearValues()
        {
            values.Clear();
            lowWaterMark = float.MaxValue;
            highWaterMark = float.MinValue;
        }
    }
    static Dictionary<string, Graph> _graphsByTag;
}
