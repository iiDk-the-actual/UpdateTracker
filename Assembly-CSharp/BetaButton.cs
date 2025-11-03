using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BetaButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		base.ButtonActivation();
		this.count++;
		base.StartCoroutine(this.ButtonColorUpdate());
		if (this.count >= 10)
		{
			this.betaParent.SetActive(false);
			PlayerPrefs.SetString("CheckedBox2", "true");
			PlayerPrefs.Save();
		}
	}

	private IEnumerator ButtonColorUpdate()
	{
		this.buttonRenderer.material = this.pressedMaterial;
		yield return new WaitForSeconds(this.buttonFadeTime);
		this.buttonRenderer.material = this.unpressedMaterial;
		yield break;
	}

	public GameObject betaParent;

	public int count;

	public float buttonFadeTime = 0.25f;

	public Text messageText;
}
