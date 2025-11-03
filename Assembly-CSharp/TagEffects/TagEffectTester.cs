using System;
using UnityEngine;

namespace TagEffects
{
	public class TagEffectTester : MonoBehaviour, IHandEffectsTrigger
	{
		public bool Static
		{
			get
			{
				return this.isStatic;
			}
		}

		public IHandEffectsTrigger.Mode EffectMode { get; }

		public Transform Transform { get; }

		public VRRig Rig
		{
			get
			{
				return null;
			}
		}

		public bool FingersDown { get; }

		public bool FingersUp { get; }

		public Vector3 Velocity { get; }

		public bool RightHand { get; }

		public float Magnitude { get; }

		public TagEffectPack CosmeticEffectPack { get; }

		public void OnTriggerEntered(IHandEffectsTrigger other)
		{
		}

		public bool InTriggerZone(IHandEffectsTrigger t)
		{
			return false;
		}

		[SerializeField]
		private bool isStatic = true;
	}
}
