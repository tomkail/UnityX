using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// [InitializeOnLoad]
public static class GameLayersClassGenerator {
    public const string className = "Assets/GameLayers.cs";
    public const string classTemplate = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n/// <summary>\n/// Convenience accessor to (cached) named layer masks used within the game\n/// </summary>\npublic static class GameLayers {{{0}\n\tpublic class Layer {{\n\t\tpublic string name;\n\t\tpublic int layer {{\n\t\t\tget {{\n\t\t\t\treturn CachedLayer(name, ref _layer);\n\t\t\t}}\n\t\t}}\n\t\tpublic int mask {{\n\t\tget {{\n\t\t\treturn CachedMask(name, ref _mask);\n\t\t\t}}\n\t\t}}\n\t\tint _layer;\n\t\tint _mask;\n\t\tpublic Layer (string name) {{\n\t\t\tthis.name = name;\n\t\t}}\n\t\tstatic int CachedLayer(string name, ref int cachedLayerIndex) {{\n\t\t\tif( cachedLayerIndex == 0 ) cachedLayerIndex = LayerMask.NameToLayer(name);\n\t\t\treturn cachedLayerIndex;\n\t\t}}\n\t\tstatic int CachedMask(string name, ref int cachedMaskIndex) {{\n\t\t\tif( cachedMaskIndex == 0 ) cachedMaskIndex = LayerMask.NameToLayer(name);\n\t\t\treturn 1 << CachedLayer(name, ref cachedMaskIndex);\n\t\t}}\n\t}}}}";

    static GameLayersClassGenerator () {
        // CreateGameLayersClass();
    }

    static void CreateGameLayersClass () {
        var filePath = className;
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();

        for(int i=0;i<=31;i++) {
            var layerN = LayerMask.LayerToName(i); //get the name of the layer
            if(layerN.Length > 0) {
                stringBuilder.AppendLine();
                stringBuilder.Append("\tpublic static Layer ");
                var name = SanitizeIdentifier(ScriptAssetCreator.ToCamelCase(layerN));
                if(name == "default") stringBuilder.Append("@");
                stringBuilder.Append(name);
                stringBuilder.Append(" = new Layer(\"");
                stringBuilder.Append(layerN);
                stringBuilder.Append("\");");
            }
        }
        stringBuilder.AppendLine();

        var text = string.Format(classTemplate, stringBuilder.ToString());
        
        bool requiresBuild = false;
        if(File.Exists(filePath)) {
            var fileText = File.ReadAllText(filePath);
            if(fileText != text) {
                File.Delete(filePath);
                requiresBuild = true;
            }
        } else {
            requiresBuild = true;
        }
        
        if(requiresBuild) ScriptAssetCreator.CreateNewFile(filePath, text);
    }

    // Ensures the generated member name is a valid C# identifier: strips any
    // characters that aren't letters/digits/underscore, and prefixes an
    // underscore if the result is empty or starts with a digit (e.g. a layer
    // named "2D Collider" -> "2dCollider" -> "_2dCollider").
    static string SanitizeIdentifier (string name) {
        if(string.IsNullOrEmpty(name)) return "_";
        StringBuilder sb = new StringBuilder(name.Length);
        foreach(char c in name) {
            if(char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
        }
        if(sb.Length == 0 || char.IsDigit(sb[0])) sb.Insert(0, '_');
        return sb.ToString();
    }
}