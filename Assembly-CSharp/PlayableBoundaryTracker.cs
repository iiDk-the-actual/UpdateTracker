using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayableBoundaryTracker : MonoBehaviour
{
	public float signedDistanceToBoundary { get; private set; }

	public float prevSignedDistanceToBoundary { get; private set; }

	public float timeSinceCrossingBorder { get; private set; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInsideZone()
	{
		return Mathf.Sign(this.signedDistanceToBoundary) < 0f;
	}

	public void UpdateSignedDistanceToBoundary(float newDistance, float elapsed)
	{
		this.prevSignedDistanceToBoundary = this.signedDistanceToBoundary;
		this.signedDistanceToBoundary = newDistance;
		if ((int)Mathf.Sign(this.prevSignedDistanceToBoundary) != (int)Mathf.Sign(this.signedDistanceToBoundary))
		{
			this.timeSinceCrossingBorder = 0f;
			return;
		}
		this.timeSinceCrossingBorder += elapsed;
	}

	internal void ResetValues()
	{
		this.timeSinceCrossingBorder = 0f;
	}

	public float radius = 1f;
}
