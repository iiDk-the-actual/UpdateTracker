using System;
using System.Collections.Generic;
using System.Linq;
using BoingKit;
using UnityEngine;

public class LengthStiffnessComparison : MonoBehaviour
{
	private void Start()
	{
		this.m_timer = 0f;
	}

	private void FixedUpdate()
	{
		BoingBones[] components = this.BonesA.GetComponents<BoingBones>();
		BoingBones[] components2 = this.BonesB.GetComponents<BoingBones>();
		Transform[] array = new Transform[]
		{
			this.BonesA.transform,
			this.BonesB.transform
		};
		IEnumerable<BoingBones> enumerable = components.Concat(components2);
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num = 0.5f * this.Run;
		this.m_timer += fixedDeltaTime;
		if (this.m_timer > this.Period + this.Rest)
		{
			this.m_timer = Mathf.Repeat(this.m_timer, this.Period + this.Rest);
			foreach (Transform transform in array)
			{
				Vector3 position = transform.position;
				position.z = -num;
				transform.position = position;
			}
			foreach (BoingBones boingBones in enumerable)
			{
				boingBones.Reboot();
			}
		}
		float num2 = Mathf.Min(1f, this.m_timer * MathUtil.InvSafe(this.Period));
		float num3 = 1f - Mathf.Pow(1f - num2, 6f);
		foreach (Transform transform2 in array)
		{
			Vector3 position2 = transform2.position;
			position2.z = Mathf.Lerp(-num, num, num3);
			transform2.position = position2;
			transform2.rotation = Quaternion.AngleAxis(this.Tilt * (1f - num3), Vector3.right);
		}
	}

	public float Run = 11f;

	public float Tilt = 15f;

	public float Period = 3f;

	public float Rest = 3f;

	public Transform BonesA;

	public Transform BonesB;

	private float m_timer;
}
