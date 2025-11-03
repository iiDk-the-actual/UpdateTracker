using System;
using UnityEngine;

[Serializable]
public class CrittersAnim
{
	public bool IsModified()
	{
		return (this.squashAmount != null && this.squashAmount.length > 1) || (this.forwardOffset != null && this.forwardOffset.length > 1) || (this.horizontalOffset != null && this.horizontalOffset.length > 1) || (this.verticalOffset != null && this.verticalOffset.length > 1);
	}

	public static bool IsModified(CrittersAnim anim)
	{
		return anim != null && anim.IsModified();
	}

	public AnimationCurve squashAmount;

	public AnimationCurve forwardOffset;

	public AnimationCurve horizontalOffset;

	public AnimationCurve verticalOffset;

	public float playSpeed;
}
