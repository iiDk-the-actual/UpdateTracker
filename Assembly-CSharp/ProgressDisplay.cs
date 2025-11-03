using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressDisplay : MonoBehaviour
{
	private void Reset()
	{
		this.root = base.gameObject;
	}

	public void SetVisible(bool visible)
	{
		this.root.SetActive(visible);
	}

	public void SetProgress(int progress, int total)
	{
		if (this.text)
		{
			if (total < this.largestNumberToShow)
			{
				this.text.text = ((progress >= total) ? string.Format("{0}", total) : string.Format("{0}/{1}", progress, total));
				this.SetTextVisible(true);
			}
			else
			{
				this.SetTextVisible(false);
			}
		}
		this.progressImage.fillAmount = (float)progress / (float)total;
	}

	public void SetProgress(float progress)
	{
		this.progressImage.fillAmount = progress;
	}

	private void SetTextVisible(bool visible)
	{
		if (this.text.gameObject.activeSelf == visible)
		{
			return;
		}
		this.text.gameObject.SetActive(visible);
	}

	[SerializeField]
	private GameObject root;

	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private int largestNumberToShow = 99;
}
