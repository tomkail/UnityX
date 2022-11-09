using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ImageX
{
	public static Rect GetCroppedRect (this Image image) {
		var sprite = image.sprite;
		var rect = image.rectTransform.rect;
		return new Rect(
				rect.x + (sprite.textureRect.x / sprite.rect.width) * rect.width,
				rect.y + (sprite.textureRect.y / sprite.rect.height) * rect.height,
				(sprite.textureRect.width / sprite.rect.width) * rect.width,
				(sprite.textureRect.height / sprite.rect.height) * rect.height);
	}
	public static void GetTightLocalCorners(this Image image, Vector3[] fourCornersArray)
	{
		if (fourCornersArray == null || fourCornersArray.Length < 4)
		{
			Debug.LogError("Calling GetLocalCorners with an array that is null or has less than 4 elements.");
			return;
		}

		Rect tmpRect = image.GetCroppedRect();
		float x0 = tmpRect.x;
		float y0 = tmpRect.y;
		float x1 = tmpRect.xMax;
		float y1 = tmpRect.yMax;

		fourCornersArray[0] = new Vector3(x0, y0, 0f);
		fourCornersArray[1] = new Vector3(x0, y1, 0f);
		fourCornersArray[2] = new Vector3(x1, y1, 0f);
		fourCornersArray[3] = new Vector3(x1, y0, 0f);
	}

	public static void GetTightWorldCorners(this Image image, Vector3[] fourCornersArray)
	{
		if (fourCornersArray == null || fourCornersArray.Length < 4)
		{
			Debug.LogError("Calling GetWorldCorners with an array that is null or has less than 4 elements.");
			return;
		}

		image.GetTightLocalCorners(fourCornersArray);

		Matrix4x4 mat = image.transform.localToWorldMatrix;
		for (int i = 0; i < 4; i++)
			fourCornersArray[i] = mat.MultiplyPoint(fourCornersArray[i]);
	}
}
