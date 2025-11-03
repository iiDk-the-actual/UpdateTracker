using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FortuneTellerButton : GorillaPressableButton
{
	public void Awake()
	{
		this.startingPos = base.transform.localPosition;
	}

	public override void ButtonActivation()
	{
		this.PressButtonUpdate();
	}

	public void PressButtonUpdate()
	{
		if (this.pressTime != 0f)
		{
			return;
		}
		base.transform.localPosition = this.startingPos + this.pressedOffset;
		this.buttonRenderer.material = this.pressedMaterial;
		this.pressTime = Time.time;
		base.StartCoroutine(this.<PressButtonUpdate>g__ButtonColorUpdate_Local|6_0());
	}

	[CompilerGenerated]
	private IEnumerator <PressButtonUpdate>g__ButtonColorUpdate_Local|6_0()
	{
		yield return new WaitForSeconds(this.durationPressed);
		if (this.pressTime != 0f && Time.time > this.durationPressed + this.pressTime)
		{
			base.transform.localPosition = this.startingPos;
			this.buttonRenderer.material = this.unpressedMaterial;
			this.pressTime = 0f;
		}
		yield break;
	}

	[SerializeField]
	private float durationPressed = 0.25f;

	[SerializeField]
	private Vector3 pressedOffset = new Vector3(0f, 0f, 0.1f);

	private float pressTime;

	private Vector3 startingPos;
}
