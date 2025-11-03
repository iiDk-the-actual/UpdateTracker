using System;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTag
{
	[CreateAssetMenu(fileName = "MaterialDatasSO", menuName = "Gorilla Tag/MaterialDatasSO")]
	public class MaterialDatasSO : ScriptableObject
	{
		public List<GTPlayer.MaterialData> datas;

		public List<HashWrapper> surfaceEffects;
	}
}
