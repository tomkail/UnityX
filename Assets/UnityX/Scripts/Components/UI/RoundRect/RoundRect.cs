using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

/// <summary>
/// Procedurally draw round rects with a fill and an outline with simple geometry
/// and with the curves drawn with antialiasing in the pixel shader.
/// 
/// The basic approach is:
///   - The shader only knows how to draw outlines using a radial gradient.
///   - It expects to draw a curve of a certain thickness within a 0,0 to 1,1 UV space
///   - For the straight edges, it simply UVs with 0,0 to 0,1.
///   - For solid fill regions (e.g. the interior) it UVs all with 0,0.
///   - Therefore, mostly we can consider a 9-slice style set of quads, each with their
///     own set of UVs
/// 
/// It draws one set of geometry for the outline and one set of geometry for the fill.
/// Originally it used to draw both with two different colours, but this isn't possible
/// without different materials since you can only pass one colour into the vertex data.
/// 
/// The biggest edge (haha) case we have to cover is when the outline is larger than
/// the radius. In this case, we don't just draw a 9-slice. Instead, we draw the 8 slices
/// around the edge, and then draw an additional 4 quad frame within the centre slice,
/// and a final quad within the centre.
/// 
/// The geometry from the outline and fill can overlap, this is necessary in case you're
/// using semi-transparent colours.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
public class RoundRect : Graphic
{
    public float cornerRadius {
        get => _cornerRadius;
        set {
            _cornerRadius = value;
            _roundRectOutlineParamsDirty = true;
            SetVerticesDirty();
        }
    }
    [SerializeField] float _cornerRadius = 10.0f;

    public Color fillColor {
        get => _fillColor;
        set {
            _fillColor = value;
            SetVerticesDirty();
        }
    }
    [SerializeField] Color _fillColor = Color.white;

    public Color outlineColor {
        get => _outlineColor;
        set {
            _outlineColor = value;
            SetVerticesDirty();
        }
    }
    [SerializeField] Color _outlineColor = Color.black;

    public enum OutlineMode {
        Inner,
        Center,
        Outer
    };
    public OutlineMode outlineMode {
        get => _outlineMode;
        set {
            _outlineMode = value;
            _roundRectOutlineParamsDirty = true;
            SetVerticesDirty();
        }
    }
    [SerializeField] OutlineMode _outlineMode = OutlineMode.Inner;

    public float antiAliasWidth {
        get => _antiAliasWidth;
        set {
            _antiAliasWidth = value;
            _roundRectOutlineParamsDirty = true;
            SetVerticesDirty();
        }
    }
    [Range(0, 4)]
    [SerializeField] float _antiAliasWidth = 1.2f;

    public float outlineWidth {
        get => _outlineWidth;
        set {
            _outlineWidth = value;
            _roundRectOutlineParamsDirty = true;
            SetVerticesDirty();
        }
    }
    [SerializeField] float _outlineWidth = 2.0f;

    // cornerRadius, but constrained by the size of the rect
    float RenderedRadius(float radius, Vector2 rectSize) {
        if( radius < 0 ) {
            radius = 0;
        }
        if( radius*2 > rectSize.y ) {
            radius = rectSize.y/2;
        }
        if( radius*2 > rectSize.x ) {
            radius = rectSize.x/2;
        }
        return radius;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        _roundRectOutlineParamsDirty = true;
    }
    
    // Stolen from Unity's Image class source
    protected override void OnCanvasHierarchyChanged()
    {
        base.OnCanvasHierarchyChanged();

        if (canvas != null) {
            SetVerticesDirty();
            SetLayoutDirty();
        }
    }

    protected override void OnDisable () {
        base.OnDisable();
        _roundRectParamsOutline = default(RoundRectParams);
        _roundRectOutlineParamsDirty = true;
    }

    protected override void Start()
    {
        base.Start();
        SetupMaterialIfNecessary();
    }

    void SetupMaterialIfNecessary()
    {
        if( material == null || _sharedMaterial == null || material != _sharedMaterial ) {
            if( _sharedMaterial == null ) {
                var shader = Resources.Load<Shader>("RoundRectShader");
                _sharedMaterial = new Material(shader);
                _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                _sharedMaterial.name = "RoundRectShader";
            }
            material = _sharedMaterial;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        SetupMaterialIfNecessary();

        if( _outlineWidth < 0 ) _outlineWidth = 0;
        if( _cornerRadius < 0 ) _cornerRadius = 0;

        _roundRectOutlineParamsDirty = true;
    }
#endif

    struct RoundRectOuterGeom {
        public Rect rect;
        public float radius;
    }
    RoundRectOuterGeom CalcOuterGeom(float radius, float sizeAdjust)
    {
        Vector2 corner1 = Vector2.zero;
        Vector2 corner2 = Vector2.zero;

        var rect = rectTransform.rect;
        var sizeAdjustVec = sizeAdjust*Vector2.one;
        rect.min -= sizeAdjustVec;
        rect.max += sizeAdjustVec;

        corner1.x = -rect.width * rectTransform.pivot.x;
        corner1.y = -rect.height * rectTransform.pivot.y;
        corner2.x = rect.width * (1.0f - rectTransform.pivot.x);
        corner2.y = rect.height * (1.0f - rectTransform.pivot.y);

        float renderedRadius = RenderedRadius(radius, rect.size);

        return new RoundRectOuterGeom {
            rect = rect,
            radius = renderedRadius
        };
    }

    [SerializeField] bool _debugBool;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        var rt = rectTransform;
        if( rt == null ) return;

        var tintedFillColor = _fillColor * color;
        var tintedOutlineColor = _outlineColor * color;
        if( tintedFillColor.a == 0 && tintedOutlineColor.a == 0 ) return;

        // Outline Mode: Default is Inner.
        // When outline's edge is right on top of fill, we
        // push the fill in/out slightly so that they don't
        // have a weird blend in the antialiased edge
        float antiAliasWidth = this.antiAliasWidth;

        // e.g. if cornerRadius == 0
        if( _outlineMode == OutlineMode.Inner && antiAliasWidth > cornerRadius )
            antiAliasWidth = cornerRadius;
            
        float outlineSizeAdjust = 0;
        float fillSizeAdjust = -Min(1.5f*antiAliasWidth, outlineWidth);
        float outlineOuterRadius = cornerRadius;
        float fillRadius = cornerRadius - 0.5f*antiAliasWidth;

        if( _outlineMode == OutlineMode.Center ) {
            outlineSizeAdjust = 0.5f*outlineWidth;
            fillSizeAdjust = 0;
            fillRadius = cornerRadius;
            outlineOuterRadius = cornerRadius + 0.5f*outlineWidth;
        } else if( _outlineMode == OutlineMode.Outer ) {
            outlineSizeAdjust = outlineWidth;
            fillSizeAdjust = antiAliasWidth;
            fillRadius = cornerRadius +0.5f*antiAliasWidth;
            outlineOuterRadius = cornerRadius + outlineWidth;
        }

        var outlineGeom = CalcOuterGeom(outlineOuterRadius, outlineSizeAdjust);
        var fillGeom = CalcOuterGeom(fillRadius, fillSizeAdjust);

        // Calculate NORMALISED to the quad that contains the corner of the outer outline:
        //  - Where the inner edge of the outline begins
        //  - width of antialising gradient, normalised to corner quad size
        if( _roundRectOutlineParamsDirty ) {

            float antiAliasWidthForCanvas = 0.0f;
            if( canvas != null ) {
                var rootCanvasWidth = canvas.rootCanvas.pixelRect.width;
                var canvasRT = (RectTransform)canvas.rootCanvas.transform;
                antiAliasWidthForCanvas = antiAliasWidth * canvasRT.rect.width / rootCanvasWidth;
            }

            var renderedOutlineOuterRadius = RenderedRadius(outlineOuterRadius, outlineGeom.rect.size);
            float antiAliasGradientWidth = 0;
            float outlineWidthNorm = 0;
            if( renderedOutlineOuterRadius > 0 ) {
                antiAliasGradientWidth = antiAliasWidthForCanvas / renderedOutlineOuterRadius;
                outlineWidthNorm = _outlineWidth / renderedOutlineOuterRadius;
            }

            float pushOut = 0;
            if( _outlineWidth < 1 ) {
                pushOut = 1.5f*antiAliasGradientWidth * (1.0f- InverseLerp(0, 1, _outlineWidth));
            }

            _roundRectParamsOutline = new RoundRectParams
            {
                outlineStartNorm = 1.0f - outlineWidthNorm + pushOut - 0.5f* antiAliasGradientWidth,
                antialiasWidth = antiAliasGradientWidth
            };

            _roundRectOutlineParamsDirty = false;
        }

        var roundRectParamsFill = new RoundRectParams {
            outlineStartNorm = -1,
            antialiasWidth = _roundRectParamsOutline.antialiasWidth
        };

        vh.Clear();

        // Create outer part of fill geometry (it has a round rect outer shape,
        // the hard inner rect will be added below)
        if( tintedFillColor.a > 0 )
            MakeRoundRectOutlineGeometry(vh, fillGeom, roundRectParamsFill, tintedFillColor);

        // Create outline geometry
        if( tintedOutlineColor.a > 0 )
            MakeRoundRectOutlineGeometry(vh, outlineGeom, _roundRectParamsOutline, tintedOutlineColor);

        // Of 9-slice, innerRect is the middle slice.
        // This is only for the outer corner radius though.
        // If there is a very thick outline (thicker than the radius)
        // Then we will have another inner inner rect (insideOutlineRect)
        var innerRect = fillGeom.rect;
        innerRect.min += fillGeom.radius * Vector2.one;
        innerRect.max -= fillGeom.radius * Vector2.one;

        var hardFillParams = new RoundRectParams {
            outlineStartNorm = -1,
            antialiasWidth = 0
        };

        // Inner quad for fill
        // We render this even if the outline overlaps it fully, since
        // the outline might be drawn in a transparent colour
        if( innerRect.size.x > 0 && innerRect.size.y > 0 && tintedFillColor.a > 0 ) {
            MakeHardQuad(vh, innerRect, hardFillParams, tintedFillColor);
        }

        // Cases:
        //  - Outline thicker than corner radius:
        //      - Need concentric rects, built out of 4 outer quads
        //        and 1 inner quad, assuming all are non zero size
        //  - Outline thinner than corner radius:
        //      - Just need single inner quad, if it's non zero width/height
        if( outlineWidth >= outlineGeom.radius + 1.5f*antiAliasWidth && tintedOutlineColor.a > 0 ) {
            var innerOutlineWidthPx = outlineWidth - outlineGeom.radius - 1.5f*antiAliasWidth;

            // Outline is very thick - thicker than the corner radius.
            // So this is the actual inner rect within the outline
            var insideOutlineRect = innerRect;
            insideOutlineRect.min += innerOutlineWidthPx * Vector2.one;
            insideOutlineRect.max -= innerOutlineWidthPx * Vector2.one;

            // Concentric quads
            if( insideOutlineRect.size.x > 0 && insideOutlineRect.size.y > 0 ) {
                
                // Outer band that's the extension of the outline
                // Bottom
                MakeHardQuad(vh, new Rect(innerRect.x, innerRect.y, innerRect.width, innerOutlineWidthPx), hardFillParams, tintedOutlineColor);

                // Top
                MakeHardQuad(vh, new Rect(innerRect.x, innerRect.yMax-innerOutlineWidthPx, innerRect.width, innerOutlineWidthPx), hardFillParams, tintedOutlineColor);

                // Left
                MakeHardQuad(vh, new Rect(innerRect.x, innerRect.y+innerOutlineWidthPx, innerOutlineWidthPx, innerRect.height-2*innerOutlineWidthPx), hardFillParams, tintedOutlineColor);

                // Right
                MakeHardQuad(vh, new Rect(innerRect.xMax-innerOutlineWidthPx, innerRect.y+innerOutlineWidthPx, innerOutlineWidthPx, innerRect.height-2*innerOutlineWidthPx), hardFillParams, tintedOutlineColor);
            }

            // Outline width encompases entire shape
            else if( innerRect.size.x > 0 && innerRect.size.y > 0) {
                MakeHardQuad(vh, innerRect, hardFillParams, tintedOutlineColor);
            }
        }

        
    }



    void MakeRoundRectOutlineGeometry(VertexHelper vh, RoundRectOuterGeom geom, RoundRectParams roundRectParams, Color color)
    {
        // Corners
        if( geom.radius > 0 ) {

            // Bottom left
            AddCorner(vh, geom.rect.min, new Vector2(geom.radius, geom.radius), roundRectParams, color, false);

            // Bottom right
            AddCorner(vh, new Vector2(geom.rect.xMax, geom.rect.yMin), new Vector2(-geom.radius, geom.radius), roundRectParams, color, true);

            // Top left
            AddCorner(vh, new Vector2(geom.rect.xMin, geom.rect.yMax), new Vector2(geom.radius, -geom.radius), roundRectParams, color, true);

            // Top right
            AddCorner(vh, new Vector2(geom.rect.xMax, geom.rect.yMax), new Vector2(-geom.radius, -geom.radius), roundRectParams, color, false);
        }

        // Edges
        float edgeWidth = geom.rect.width - 2*geom.radius;
        if( edgeWidth > 0 ) {

            // Top edge
            AddEdge(vh, 
                new Vector2(geom.rect.xMin + geom.radius, geom.rect.yMax), 
                new Vector2(edgeWidth, -geom.radius),
                roundRectParams,
                color,
                false);

            // Bottom edge
            AddEdge(vh, 
                new Vector2(geom.rect.xMin + geom.radius + edgeWidth,  geom.rect.yMin), 
                new Vector2(-edgeWidth, geom.radius),
                roundRectParams,
                color,
                false);
        }

        float edgeHeight = geom.rect.height - 2*geom.radius;
        if( edgeHeight > 0 ) {

            // Left edge
            AddEdge(vh, 
                new Vector2(geom.rect.xMin, geom.rect.yMin + geom.radius), 
                new Vector2(geom.radius, edgeHeight),
                roundRectParams,
                color,
                true);

            // Right
            AddEdge(vh, 
                new Vector2(geom.rect.xMax, geom.rect.yMax - geom.radius), 
                new Vector2(-geom.radius, -edgeHeight),
                roundRectParams,
                color,
                true);
        }
    }


    void AddCorner(VertexHelper vh, Vector2 corner, Vector2 toCurveOrigin, RoundRectParams roundRectParams, Color color, bool flipWinding)
    {
        var vert = UIVertex.simpleVert;
        vert.color = color;
        int baseIdx = vh.currentVertCount;

        vert.position = new Vector2(corner.x, corner.y);
        vert.uv0 = RoundRectUV0(1, 1, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(corner.x, corner.y + toCurveOrigin.y);
        vert.uv0 = RoundRectUV0(1, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(corner.x + toCurveOrigin.x, corner.y + toCurveOrigin.y);
        vert.uv0 = RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(corner.x + toCurveOrigin.x, corner.y);
        vert.uv0 = RoundRectUV0(0, 1, roundRectParams);
        vh.AddVert(vert);

        if( flipWinding ) {
            vh.AddTriangle(baseIdx+2, baseIdx+1, baseIdx+0);
            vh.AddTriangle(baseIdx+0, baseIdx+3, baseIdx+2);
        } else {
            vh.AddTriangle(baseIdx+0, baseIdx+1, baseIdx+2);
            vh.AddTriangle(baseIdx+2, baseIdx+3, baseIdx+0);
        }
    }

    void AddEdge(VertexHelper vh, Vector2 onEdgeLeft, Vector2 opEdgeCorner, RoundRectParams roundRectParams, Color color, bool isSideEdge)
    {
        var vert = UIVertex.simpleVert;
        vert.color = color;
        int baseIdx = vh.currentVertCount;

        vert.position = new Vector2(onEdgeLeft.x, onEdgeLeft.y);
        vert.uv0 = isSideEdge ? RoundRectUV0(0, 1, roundRectParams) : RoundRectUV0(0, 1, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(onEdgeLeft.x, onEdgeLeft.y + opEdgeCorner.y);
        vert.uv0 = isSideEdge ? RoundRectUV0(0, 1, roundRectParams) : RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(onEdgeLeft.x + opEdgeCorner.x, onEdgeLeft.y + opEdgeCorner.y);
        vert.uv0 = isSideEdge ? RoundRectUV0(0, 0, roundRectParams) : RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(onEdgeLeft.x + opEdgeCorner.x, onEdgeLeft.y);
        vert.uv0 = isSideEdge ? RoundRectUV0(0, 0, roundRectParams) : RoundRectUV0(0, 1, roundRectParams);
        vh.AddVert(vert);

        if( isSideEdge ) {
            vh.AddTriangle(baseIdx+0, baseIdx+1, baseIdx+2);
            vh.AddTriangle(baseIdx+2, baseIdx+3, baseIdx+0);
        } else {
            vh.AddTriangle(baseIdx+2, baseIdx+1, baseIdx+0);
            vh.AddTriangle(baseIdx+0, baseIdx+3, baseIdx+2);
        }
    }

    // TODO: Add a bunch of quads in case outline thickness > corner radius?
    // Or be clever about texture coordinates??
    void AddMiddle(VertexHelper vh, Vector2 corner1, Vector2 corner2, RoundRectParams roundRectParams, Color color, float renderedRadius)
    {
        var vert = UIVertex.simpleVert;
        vert.color = color;
        int baseIdx = vh.currentVertCount;

        vert.position = new Vector2(corner1.x+renderedRadius, corner1.y+renderedRadius);
        vert.uv0 = RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(corner1.x+renderedRadius, corner2.y-renderedRadius);
        vert.uv0 = RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(corner2.x-renderedRadius, corner2.y-renderedRadius);
        vert.uv0 = RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(corner2.x-renderedRadius, corner1.y+renderedRadius);
        vert.uv0 = RoundRectUV0(0, 0, roundRectParams);
        vh.AddVert(vert);

        vh.AddTriangle(baseIdx+2, baseIdx+1, baseIdx+0);
        vh.AddTriangle(baseIdx+0, baseIdx+3, baseIdx+2);
    }


    void MakeHardQuad(VertexHelper vh, Rect rect, RoundRectParams fillRoundRectParams, Color color)
    {
        var vert = UIVertex.simpleVert;
        vert.color = color;
        int baseIdx = vh.currentVertCount;

        vert.position = new Vector2(rect.xMin, rect.yMin);
        vert.uv0 = RoundRectUV0(0, 0, fillRoundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(rect.xMin, rect.yMax);
        vert.uv0 = RoundRectUV0(0, 0, fillRoundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(rect.xMax, rect.yMax);
        vert.uv0 = RoundRectUV0(0, 0, fillRoundRectParams);
        vh.AddVert(vert);

        vert.position = new Vector2(rect.xMax, rect.yMin);
        vert.uv0 = RoundRectUV0(0, 0, fillRoundRectParams);
        vh.AddVert(vert);

        vh.AddTriangle(baseIdx+2, baseIdx+1, baseIdx+0);
        vh.AddTriangle(baseIdx+0, baseIdx+3, baseIdx+2);
    }
    

    // In UV0 we pack:
    //  - x, y: The normalised coord for the procedurally drawn radial gradient, though
    //          it's used for both the corners and the linear sections (which are just stretched)
    //  - z:    The normalised inner start radius for the outline (outer is implicitly 1.0)
    //  - w:    The falloff gradient for antialiasing
    Vector4 RoundRectUV0(float x, float y, RoundRectParams roundRectParams) {
        return new Vector4(
            x, y, roundRectParams.outlineStartNorm, roundRectParams.antialiasWidth
        );
    }


    struct RoundRectParams {
        public float outlineStartNorm;
        
        public float antialiasWidth;
    }

    RoundRectParams _roundRectParamsOutline;
    bool _roundRectOutlineParamsDirty = true;

    static Material _sharedMaterial = null;
}