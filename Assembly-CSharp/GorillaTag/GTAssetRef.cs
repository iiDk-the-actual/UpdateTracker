using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GorillaTag
{
	[Serializable]
	public class GTAssetRef<TObject> : AssetReferenceT<TObject> where TObject : Object
	{
		public GTAssetRef(string guid)
			: base(guid)
		{
		}
	}
}
