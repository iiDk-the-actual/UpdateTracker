using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	public void UpdateProgress(float newFill)
	{
		bool flag = newFill > 1f;
		this._fillAmount = Mathf.Clamp(newFill, 0f, 1f);
		this.fillImage.fillAmount = this._fillAmount;
		if (this.useColors)
		{
			if (flag)
			{
				this.fillImage.color = this.overCapacity;
				return;
			}
			if (Mathf.Approximately(this._fillAmount, 1f))
			{
				this.fillImage.color = this.atCapacity;
				return;
			}
			this.fillImage.color = this.underCapacity;
		}
	}

	[SerializeField]
	private Image fillImage;

	[SerializeField]
	private bool useColors;

	[SerializeField]
	private Color underCapacity = Color.green;

	[SerializeField]
	private Color overCapacity = Color.red;

	[SerializeField]
	private Color atCapacity = Color.yellow;

	private float _fillAmount;
}
