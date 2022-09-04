using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [ExecuteAlways]
// public class Test : MonoSingleton<Test> {
//     public bool defaultVisible;
//     public bool edges;
//     public bool center;
//     public static bool Edges => instance.edges;
//     public static bool Center => instance.center;
//     [PreviewTexture(256)]
//     public Texture2D inputTexture;
//     [PreviewTexture(256)]
//     public Texture2D outputTexture;
//     [PreviewTexture(256)]
//     public Texture2D outputTexture2;
//     void Update() {
//         Vector2Int ArrayIndexToGridPoint (int arrayIndex, int width){
//             return new Vector2Int(arrayIndex%width, Mathf.FloorToInt((float)arrayIndex/width));
//         }
//         Vector2Int visibilityMapSize = new Vector2Int(inputTexture.width, inputTexture.height);
//         bool[] visibilityMap = new bool[visibilityMapSize.x * visibilityMapSize.y];
//         for(int i = 0; i < visibilityMapSize.x*visibilityMapSize.y; i++) {
//             var gridPoint = ArrayIndexToGridPoint(i, visibilityMapSize.x);
//             visibilityMap[i] = inputTexture.GetPixel(gridPoint.x, gridPoint.y).r > 0.5f;
//         }
//         var colors = UpscaleTools.UpscaleBoolMap(visibilityMapSize, visibilityMap, defaultVisible);
//         outputTexture = new Texture2D(visibilityMapSize.x*4,visibilityMapSize.y*4);
//         outputTexture.SetPixels(colors);
//         outputTexture.filterMode = FilterMode.Point;
//         outputTexture.Apply();
        
        
//         visibilityMapSize = new Vector2Int(outputTexture.width, outputTexture.height);
//         visibilityMap = new bool[visibilityMapSize.x * visibilityMapSize.y];
//         for(int i = 0; i < visibilityMapSize.x*visibilityMapSize.y; i++) {
//             var gridPoint = ArrayIndexToGridPoint(i, visibilityMapSize.x);
//             visibilityMap[i] = outputTexture.GetPixel(gridPoint.x, gridPoint.y).r > 0.5f;
//         }
//         colors = UpscaleTools.UpscaleBoolMap(visibilityMapSize, visibilityMap, defaultVisible);
//         outputTexture2 = new Texture2D(visibilityMapSize.x*4,visibilityMapSize.y*4);
//         outputTexture2.SetPixels(colors);
//         outputTexture2.filterMode = FilterMode.Point;
//         outputTexture2.Apply();
//     }
// }
public static class UpscaleTools {
    // Upscales maps of booleans, idea nabbed here https://technology.riotgames.com/news/story-fog-and-war

    
    public static Color[] UpscaleBoolMap (Vector2Int sourceMapSize, bool[] sourceMap, bool defaultIsTrue) {
        var b = Color.black;
        var w = Color.white;
        var g = Color.grey;

        Vector2Int colorMapSize = new Vector2Int(sourceMapSize.x * 4, sourceMapSize.y * 4);
        Color[] colorMap = new Color[colorMapSize.x * colorMapSize.y];
        
        bool GetValueAtGridPoint(int x, int y) {
            return sourceMap[GridPointToArrayIndex(x, y, sourceMapSize.x)];
        }

        bool IsOnVisibilityMap(int x, int y) {
            return true;
            // return (x >= 0 && x < sourceMapSize.x && y >= 0 && y < sourceMapSize.y);
        }
        
        for (int x = 0; x < sourceMapSize.x; x++) {
            for (int y = 0; y < sourceMapSize.y; y++) {
                var thisPix = GetValueAtGridPoint(x,y);
                
                if(x == 0 && y == 0) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*4, y*4, 4, 4);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(x == 0 && y == sourceMapSize.y-1) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*4, y*4, 4, 4);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(x == sourceMapSize.x-1 && y == 0) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*4, y*4, 4, 4);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(x == sourceMapSize.x-1 && y == sourceMapSize.y-1) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*4, y*4, 4, 4);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(x == 0 && y > 0 && y < sourceMapSize.y-1) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*2, y*4, 2, 4);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(x == sourceMapSize.x-1 && y > 0 && y < sourceMapSize.y-1) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(2 + x*4, y*4, 2, 4);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(y == 0 && x > 0 && x < sourceMapSize.x-1) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*4, y*2, 4, 2);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }
                if(y == sourceMapSize.y-1 && x > 0 && x < sourceMapSize.x-1) {
                    var c = thisPix ? w : b;
                    var edgePointRect = new RectInt(x*4, 2 + y*4, 4, 2);
                    SetMapValues(colorMap, colorMapSize.x, edgePointRect, c);
                }

                var fill = x < sourceMapSize.x-1 && y < sourceMapSize.y-1;
                var pointRect = new RectInt(2 + x*4, 2 + y*4, 4, 4);

                var rightPix = IsOnVisibilityMap(x+1,y) ? GetValueAtGridPoint(x+1,y) : thisPix;
                var downPix = IsOnVisibilityMap(x,y+1) ? GetValueAtGridPoint(x,y+1) : thisPix;
                var cornerPix = IsOnVisibilityMap(x+1,y+1) ? GetValueAtGridPoint(x+1,y+1) : thisPix;
                int cell = thisPix ? 0 : 1;
                cell += rightPix ? 0 : 2;
                cell += downPix ? 0 : 4;
                cell += cornerPix ? 0 : 8;

                if(cell == 0) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,w,w,
                        w,w,w,w,
                        w,w,w,w,
                        w,w,w,w,
                    });
                } else if(cell == 1) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,g,w,w,
                        g,w,w,w,
                        w,w,w,w,
                        w,w,w,w,
                    });
                } else if(cell == 2) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,g,b,
                        w,w,w,g,
                        w,w,w,w,
                        w,w,w,w,
                    });
                } else if(cell == 3) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,b,b,b,
                        b,b,b,b,
                        w,w,w,w,
                        w,w,w,w,
                    });
                } else if(cell == 4) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,w,w,
                        w,w,w,w,
                        g,w,w,w,
                        b,g,w,w,
                    });
                } else if(cell == 5) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,b,w,w,
                        b,b,w,w,
                        b,b,w,w,
                        b,b,w,w,
                    });
                } else if(cell == 6) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,g,b,
                        w,w,w,g,
                        g,w,w,w,
                        b,g,w,w,
                    });
                } else if(cell == 7) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,b,b,b,
                        b,b,b,b,
                        b,b,b,g,
                        b,b,g,w,
                    });
                } else if(cell == 8) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,w,w,
                        w,w,w,w,
                        w,w,w,g,
                        w,w,g,b,
                    });
                } else if(cell == 9) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,g,w,w,
                        g,w,w,w,
                        w,w,w,g,
                        w,w,g,b,
                    });
                } else if(cell == 10) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,b,b,
                        w,w,b,b,
                        w,w,b,b,
                        w,w,b,b,
                    });
                } else if(cell == 11) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,b,b,b,
                        b,b,b,b,
                        g,b,b,b,
                        w,g,b,b,
                    });
                } else if(cell == 12) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,w,w,w,
                        w,w,w,w,
                        b,b,b,b,
                        b,b,b,b,
                    });
                } else if(cell == 13) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,b,g,w,
                        b,b,b,g,
                        b,b,b,b,
                        b,b,b,b,
                    });
                } else if(cell == 14) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        w,g,b,b,
                        g,b,b,b,
                        b,b,b,b,
                        b,b,b,b,
                    });
                } else if(cell == 15) {
                    SetMapValues(colorMap, colorMapSize.x, pointRect, new Color[16] {
                        b,b,b,b,
                        b,b,b,b,
                        b,b,b,b,
                        b,b,b,b,
                    });
                }
            }
        }
        return colorMap;
    }


    static int GridPointToArrayIndex (int x, int y, int width){
        return y * width + x;
    }

    static void SetMapValues(Color[] map, int maxWidth, RectInt pointRect, Color[] vals) {
        Debug.Assert(vals.Length == pointRect.width * pointRect.height);
        int i = 0;

        for(int y = 0; y < pointRect.height; y++) {
            for(int x = 0; x < pointRect.width; x++) {
                Vector2Int gridPoint = new Vector2Int(pointRect.x+x, pointRect.y+y);
                var index = GridPointToArrayIndex(gridPoint.x, gridPoint.y, maxWidth);
                map[index] = vals[i];
                i++;
            }
        }
    }
    static void SetMapValues(Color[] map, int maxWidth, RectInt pointRect, Color val) {
        int i = 0;
        for(int y = 0; y < pointRect.height; y++) {
            for(int x = 0; x < pointRect.width; x++) {
                Vector2Int gridPoint = new Vector2Int(pointRect.x+x, pointRect.y+y);
                var index = GridPointToArrayIndex(gridPoint.x, gridPoint.y, maxWidth);
                map[index] = val;
                i++;
            }
        }
    }
}