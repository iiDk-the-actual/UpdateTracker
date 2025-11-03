using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SIUIProgressBar : MonoBehaviour
{
	public void UpdateFillPercent(float percentFull)
	{
		float num = this.backgroundImage.rectTransform.sizeDelta.x * (1f - 2f * this.borderPercent / 100f);
		float num2 = num * Mathf.Min(1f, percentFull);
		float num3 = -(num - num2) / 2f * this.progressImage.rectTransform.localScale.x;
		this.progressImage.rectTransform.sizeDelta = new Vector2(num2, this.progressImage.rectTransform.sizeDelta.y);
		this.progressImage.rectTransform.localPosition = new Vector3(num3, this.progressImage.rectTransform.localPosition.y, this.progressImage.rectTransform.localPosition.z);
	}

	public Image backgroundImage;

	public Image progressImage;

	public float borderPercent;

	public TextMeshProUGUI progressText;
}
