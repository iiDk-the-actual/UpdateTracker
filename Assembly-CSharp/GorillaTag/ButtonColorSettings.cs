using System;
using UnityEngine;

namespace GorillaTag
{
	[CreateAssetMenu(fileName = "GorillaButtonColorSettings", menuName = "ScriptableObjects/GorillaButtonColorSettings", order = 0)]
	public class ButtonColorSettings : ScriptableObject
	{
		public Color UnpressedColor;

		public Color PressedColor;

		[Tooltip("Optional\nThe time the change will be in effect")]
		public float PressedTime;
	}
}
