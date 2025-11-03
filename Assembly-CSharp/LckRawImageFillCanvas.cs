using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LckRawImageFillCanvas : UIBehaviour
{
	private new void OnEnable()
	{
		this.UpdateSizeDelta();
	}

	private void Update()
	{
		this.UpdateSizeDelta();
	}

	private void UpdateSizeDelta()
	{
		if (this._rawImage == null || this._rawImage.texture == null)
		{
			return;
		}
		RectTransform rectTransform = this._rawImage.rectTransform;
		Vector2 sizeDelta = ((RectTransform)rectTransform.parent).sizeDelta;
		Vector2 vector = new Vector2((float)this._rawImage.texture.width, (float)this._rawImage.texture.height);
		float num = sizeDelta.x / sizeDelta.y;
		float num2 = vector.x / vector.y;
		float num3 = num / num2;
		Vector2 vector2 = new Vector2(sizeDelta.x, sizeDelta.x / num2);
		Vector2 vector3 = new Vector2(sizeDelta.y * num2, sizeDelta.y);
		switch (this._scaleType)
		{
		case LckRawImageFillCanvas.ScaleType.Fill:
			rectTransform.sizeDelta = ((num3 > 1f) ? vector2 : vector3);
			return;
		case LckRawImageFillCanvas.ScaleType.Inset:
			rectTransform.sizeDelta = ((num3 < 1f) ? vector2 : vector3);
			return;
		case LckRawImageFillCanvas.ScaleType.Stretch:
			rectTransform.sizeDelta = sizeDelta;
			return;
		default:
			return;
		}
	}

	[SerializeField]
	private RawImage _rawImage;

	[SerializeField]
	private LckRawImageFillCanvas.ScaleType _scaleType;

	private enum ScaleType
	{
		Fill,
		Inset,
		Stretch
	}
}
