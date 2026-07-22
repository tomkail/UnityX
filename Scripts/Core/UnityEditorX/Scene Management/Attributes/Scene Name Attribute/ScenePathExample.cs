using UnityEngine;

public class ScenePathExample : MonoBehaviour
{
	[ScenePathAttribute]
	public string sceneName;
    [ScenePathAttribute(ScenePathAttribute.SceneFindMethod.EnabledInBuild)]
    public string sceneName2;
}