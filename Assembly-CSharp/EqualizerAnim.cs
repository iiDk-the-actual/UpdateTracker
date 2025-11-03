using System;
using System.Collections.Generic;
using UnityEngine;

public class EqualizerAnim : MonoBehaviour
{
	private void Start()
	{
		this.inputColorHash = Shader.PropertyToID(this.inputColorProperty);
	}

	private void Update()
	{
		if (EqualizerAnim.thisFrame == Time.frameCount)
		{
			if (EqualizerAnim.materialsUpdatedThisFrame.Contains(this.material))
			{
				return;
			}
		}
		else
		{
			EqualizerAnim.thisFrame = Time.frameCount;
			EqualizerAnim.materialsUpdatedThisFrame.Clear();
		}
		float num = Time.time % this.loopDuration;
		this.material.SetColor(this.inputColorHash, new Color(this.redCurve.Evaluate(num), this.greenCurve.Evaluate(num), this.blueCurve.Evaluate(num)));
		EqualizerAnim.materialsUpdatedThisFrame.Add(this.material);
	}

	[SerializeField]
	private AnimationCurve redCurve;

	[SerializeField]
	private AnimationCurve greenCurve;

	[SerializeField]
	private AnimationCurve blueCurve;

	[SerializeField]
	private float loopDuration;

	[SerializeField]
	private Material material;

	[SerializeField]
	private string inputColorProperty;

	private int inputColorHash;

	private static int thisFrame;

	private static HashSet<Material> materialsUpdatedThisFrame = new HashSet<Material>();
}
