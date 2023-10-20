#if UNITY_EDITOR
public class EnforceDecendentGameObjectPropertiesPostProcessor : UnityEditor.AssetModificationProcessor {
	static string[] OnWillSaveAssets (string[] paths) {
		EnforceDecendentGameObjectProperties.EnforcePropertiesAll();
		return paths;
	}
}
#endif