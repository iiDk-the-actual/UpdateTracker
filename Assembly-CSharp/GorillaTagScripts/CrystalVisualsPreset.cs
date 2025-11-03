using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GorillaTagScripts
{
	[CreateAssetMenu(fileName = "CrystalVisualsPreset", menuName = "ScriptableObjects/CrystalVisualsPreset", order = 0)]
	public class CrystalVisualsPreset : ScriptableObject
	{
		public override int GetHashCode()
		{
			return new ValueTuple<CrystalVisualsPreset.VisualState, CrystalVisualsPreset.VisualState>(this.stateA, this.stateB).GetHashCode();
		}

		[Conditional("UNITY_EDITOR")]
		private void Save()
		{
		}

		public CrystalVisualsPreset.VisualState stateA;

		public CrystalVisualsPreset.VisualState stateB;

		[Serializable]
		public struct VisualState
		{
			public override int GetHashCode()
			{
				int num = CrystalVisualsPreset.VisualState.<GetHashCode>g__GetColorHash|2_0(this.albedo);
				int num2 = CrystalVisualsPreset.VisualState.<GetHashCode>g__GetColorHash|2_0(this.emission);
				return new ValueTuple<int, int>(num, num2).GetHashCode();
			}

			[CompilerGenerated]
			internal static int <GetHashCode>g__GetColorHash|2_0(Color c)
			{
				return new ValueTuple<float, float, float>(c.r, c.g, c.b).GetHashCode();
			}

			[ColorUsage(false, false)]
			public Color albedo;

			[ColorUsage(false, false)]
			public Color emission;
		}
	}
}
