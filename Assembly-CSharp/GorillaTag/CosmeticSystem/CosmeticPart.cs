using System;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticPart
	{
		public GTAssetRef<GameObject> prefabAssetRef;

		[Tooltip("Determines how the cosmetic part will be attached to the player.")]
		public CosmeticAttachInfo[] attachAnchors;

		[NonSerialized]
		public ECosmeticPartType partType;
	}
}
