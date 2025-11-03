using System;
using GorillaExtensions;
using UnityEngine;

public class CosmeticButton : GorillaPressableButton
{
	public bool Initialized { get; private set; }

	public void Awake()
	{
		this.startingPos = base.transform.localPosition;
		this.Initialized = true;
	}

	public override void UpdateColor()
	{
		if (!base.enabled)
		{
			this.buttonRenderer.material = this.disabledMaterial;
			this.SetOffText(this.myText != null, false, false);
		}
		else if (this.isOn)
		{
			this.buttonRenderer.material = this.pressedMaterial;
			this.SetOnText(this.myText.IsNotNull(), false, false);
		}
		else
		{
			this.buttonRenderer.material = this.unpressedMaterial;
			this.SetOffText(this.myText != null, false, false);
		}
		this.UpdatePosition();
	}

	public virtual void UpdatePosition()
	{
		Vector3 vector = this.startingPos;
		if (!base.enabled)
		{
			vector += this.disabledOffset;
		}
		else if (this.isOn)
		{
			vector += this.pressedOffset;
		}
		this.posOffset = base.transform.position;
		base.transform.localPosition = vector;
		this.posOffset = base.transform.position - this.posOffset;
		if (this.myText != null)
		{
			this.myText.transform.position += this.posOffset;
		}
		if (this.myTmpText != null)
		{
			this.myTmpText.transform.position += this.posOffset;
		}
		if (this.myTmpText2 != null)
		{
			this.myTmpText2.transform.position += this.posOffset;
		}
	}

	[SerializeField]
	private Vector3 pressedOffset = new Vector3(0f, 0f, 0.1f);

	[SerializeField]
	private Material disabledMaterial;

	[SerializeField]
	private Vector3 disabledOffset = new Vector3(0f, 0f, 0.1f);

	private Vector3 startingPos;

	protected Vector3 posOffset;
}
