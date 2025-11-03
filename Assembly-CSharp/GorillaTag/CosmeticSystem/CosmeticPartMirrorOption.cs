using System;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticPartMirrorOption
	{
		public ECosmeticPartMirrorAxis axis;

		[Tooltip("This will multiply the local scale for the selected axis by -1.")]
		public bool negativeScale;
	}
}
