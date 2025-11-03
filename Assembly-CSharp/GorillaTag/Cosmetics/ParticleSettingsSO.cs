using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	[CreateAssetMenu(fileName = "Particle Settings", menuName = "ScriptableObjects/ParticleSettings")]
	public class ParticleSettingsSO : ScriptableObject
	{
		public Color startColor;

		public float startSize;
	}
}
