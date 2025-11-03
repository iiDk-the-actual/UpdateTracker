using System;
using UnityEngine;

public class CosmeticCategoryButton : CosmeticButton
{
	public void SetIcon(Sprite sprite)
	{
		this.equippedLeftIcon.enabled = false;
		this.equippedRightIcon.enabled = false;
		this.equippedIcon.enabled = sprite != null;
		this.equippedIcon.sprite = sprite;
	}

	public void SetDualIcon(Sprite leftSprite, Sprite rightSprite)
	{
		this.equippedLeftIcon.enabled = leftSprite != null;
		this.equippedRightIcon.enabled = rightSprite != null;
		this.equippedIcon.enabled = false;
		this.equippedLeftIcon.sprite = leftSprite;
		this.equippedRightIcon.sprite = rightSprite;
	}

	public override void UpdatePosition()
	{
		base.UpdatePosition();
		if (this.equippedIcon != null)
		{
			this.equippedIcon.transform.position += this.posOffset;
		}
		if (this.equippedLeftIcon != null)
		{
			this.equippedLeftIcon.transform.position += this.posOffset;
		}
		if (this.equippedRightIcon != null)
		{
			this.equippedRightIcon.transform.position += this.posOffset;
		}
	}

	[SerializeField]
	private SpriteRenderer equippedIcon;

	[SerializeField]
	private SpriteRenderer equippedLeftIcon;

	[SerializeField]
	private SpriteRenderer equippedRightIcon;
}
