using System;
using UnityEngine;
using UnityEngine.Events;

namespace PerformanceSystems
{
	public interface ILod
	{
		int CurrentLod { get; }

		Vector3 Position { get; }

		float[] LodRanges { get; }

		UnityEvent[] OnLodRangeEvents { get; }

		UnityEvent OnCulledEvent { get; }

		void UpdateLod(Vector3 refPos);
	}
}
