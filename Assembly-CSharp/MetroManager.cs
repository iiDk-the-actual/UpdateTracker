using System;
using UnityEngine;

public class MetroManager : MonoBehaviour
{
	private void Update()
	{
		for (int i = 0; i < this._blimps.Length; i++)
		{
			this._blimps[i].Tick();
		}
		for (int j = 0; j < this._spotlights.Length; j++)
		{
			this._spotlights[j].Tick();
		}
	}

	[SerializeField]
	private MetroBlimp[] _blimps = new MetroBlimp[0];

	[SerializeField]
	private MetroSpotlight[] _spotlights = new MetroSpotlight[0];

	[Space]
	[SerializeField]
	private Transform _blimpsRotationAnchor;
}
