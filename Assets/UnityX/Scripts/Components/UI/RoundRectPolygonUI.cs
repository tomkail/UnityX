using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityX.Geometry;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class RoundRectPolygonUI : UIBehaviour, ILayoutElement {
	RectTransform rectTransform {
        get {
            return (RectTransform)transform;
        }
    }
	[SerializeField]
	UIPolygon polygon = null;
	[SerializeField]
	AdvancedUILineRenderer lineRenderer = null;
	public float rounding = 1;
	public float degreesPerPoint = 3;
	public float outlineWidth;
	public Color color;
	public Color outlineColor;
	
	void OnDrawGizmos () {
		if(!Application.isPlaying) RenderFromRect();
	}
	public Vector2 RenderFromRect () {
		var verts = rectTransform.rect.GetVertices();
		polygon.color = color;
		polygon.polygon = new Polygon(Polygon.GetSmoothed(verts, rounding, degreesPerPoint).ToArray());
		
		// verts.Add(verts.First());
		var x = polygon.polygon.vertices.ToList();
		x.Add(x.First());
		lineRenderer.pointsToDraw = AdvancedUILineRendererPoint.GetLineRendererPoints(x.ToArray(), outlineWidth, outlineColor);

		var sizeDelta = new Vector2(polygon.preferredWidth, polygon.preferredHeight);
		SetDirty();
		return sizeDelta;
	}



    protected override void OnRectTransformDimensionsChange () {
        RenderFromRect();
        base.OnRectTransformDimensionsChange();
    }


    // [SerializeField] 
    private float m_MinWidth = -1;
    // [SerializeField] 
    private float m_MinHeight = -1;
    // [SerializeField] 
    private float m_PreferredWidth = -1;
    // [SerializeField] 
    private float m_PreferredHeight = -1;
    // [SerializeField] 
    private float m_FlexibleWidth = -1;
    // [SerializeField] 
    private float m_FlexibleHeight = -1;
    // [SerializeField] 
    private int m_LayoutPriority = 1;

    public virtual void CalculateLayoutInputHorizontal() {
        m_PreferredWidth = polygon.preferredWidth;
    }

    public virtual void CalculateLayoutInputVertical() {
        m_PreferredHeight = polygon.preferredHeight;
    }
    
    public virtual float minWidth { get { return m_MinWidth; } }
    public virtual float minHeight { get { return m_MinHeight; } }
    public virtual float preferredWidth { get { return m_PreferredWidth; } }
    public virtual float preferredHeight { get { return m_PreferredHeight; } }

    /// <summary>
    /// The extra relative width this layout element should be allocated if there is additional available space.
    /// </summary>
    public virtual float flexibleWidth { get { return m_FlexibleWidth; } }

    /// <summary>
    /// The extra relative height this layout element should be allocated if there is additional available space.
    /// </summary>
    public virtual float flexibleHeight { get { return m_FlexibleHeight; } }

    /// <summary>
    /// The Priority of layout this element has.
    /// </summary>
    public virtual int layoutPriority { get { return m_LayoutPriority; } }

    

    protected override void OnTransformParentChanged()
    {
        SetDirty();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetDirty();
    }

    protected override void OnBeforeTransformParentChanged()
    {
        SetDirty();
    }

    /// <summary>
    /// Mark the LayoutElement as dirty.
    /// </summary>
    /// <remarks>
    /// This will make the auto layout system process this element on the next layout pass. This method should be called by the LayoutElement whenever a change is made that potentially affects the layout.
    /// </remarks>
    protected void SetDirty()
    {
        if (!IsActive())
            return;
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }

    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }

    #endif
}
