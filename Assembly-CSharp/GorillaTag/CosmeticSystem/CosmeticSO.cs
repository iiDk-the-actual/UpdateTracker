using System;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[CreateAssetMenu(fileName = "Untitled_CosmeticSO", menuName = "- Gorilla Tag/CosmeticSO", order = 0)]
	public class CosmeticSO : ScriptableObject
	{
		private bool ShowPropHuntWeight()
		{
			return true;
		}

		public void OnEnable()
		{
			this.info.debugCosmeticSOName = base.name;
		}

		public CosmeticInfoV2 info = new CosmeticInfoV2("UNNAMED");

		public int propHuntWeight = 1;
	}
}
