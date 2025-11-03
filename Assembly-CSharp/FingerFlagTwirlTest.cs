using System;
using UnityEngine;

public class FingerFlagTwirlTest : MonoBehaviour
{
	protected void FixedUpdate()
	{
		this.animTimes += Time.deltaTime * this.rotAnimDurations;
		this.animTimes.x = this.animTimes.x % 1f;
		this.animTimes.y = this.animTimes.y % 1f;
		this.animTimes.z = this.animTimes.z % 1f;
		base.transform.localRotation = Quaternion.Euler(this.rotXAnimCurve.Evaluate(this.animTimes.x) * this.rotAnimAmplitudes.x, this.rotYAnimCurve.Evaluate(this.animTimes.y) * this.rotAnimAmplitudes.y, this.rotZAnimCurve.Evaluate(this.animTimes.z) * this.rotAnimAmplitudes.z);
	}

	public Vector3 rotAnimDurations = new Vector3(0.2f, 0.1f, 0.5f);

	public Vector3 rotAnimAmplitudes = Vector3.one * 360f;

	public AnimationCurve rotXAnimCurve;

	public AnimationCurve rotYAnimCurve;

	public AnimationCurve rotZAnimCurve;

	private Vector3 animTimes = Vector3.zero;
}
