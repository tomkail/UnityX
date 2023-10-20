using System.Collections.Generic;
using UnityEngine;

public static class PrimitiveHelper {
    static Dictionary<PrimitiveType, Mesh> primitiveMeshes = new();
 
     public static GameObject CreatePrimitive(PrimitiveType type, bool withCollider) {
         if (withCollider) { return GameObject.CreatePrimitive(type); }
 
         GameObject gameObject = new GameObject(type.ToString());
         MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
         meshFilter.sharedMesh = GetPrimitiveMesh(type);
         gameObject.AddComponent<MeshRenderer>();
 
         return gameObject;
     }
 
     public static Mesh GetPrimitiveMesh(PrimitiveType type) {
         if (!primitiveMeshes.ContainsKey(type)) {
             CreatePrimitiveMesh(type);
         }
 
         return primitiveMeshes[type];
     }

     static Mesh CreatePrimitiveMesh(PrimitiveType type) {
         GameObject gameObject = GameObject.CreatePrimitive(type);
         Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
         if(Application.isPlaying) Object.Destroy(gameObject);
         else Object.DestroyImmediate(gameObject);
 
         primitiveMeshes[type] = mesh;
         return mesh;
     }
 }