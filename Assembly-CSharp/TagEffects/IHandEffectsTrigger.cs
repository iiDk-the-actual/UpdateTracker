using System;
using UnityEngine;

namespace TagEffects
{
	public interface IHandEffectsTrigger
	{
		IHandEffectsTrigger.Mode EffectMode { get; }

		Transform Transform { get; }

		VRRig Rig { get; }

		bool FingersDown { get; }

		bool FingersUp { get; }

		Vector3 Velocity { get; }

		bool RightHand { get; }

		TagEffectPack CosmeticEffectPack { get; }

		bool Static { get; }

		void OnTriggerEntered(IHandEffectsTrigger other);

		bool InTriggerZone(IHandEffectsTrigger t);

		public enum Mode
		{
			HighFive,
			FistBump,
			Tag3P,
			Tag1P,
			HighFive_And_FistBump
		}
	}
}
