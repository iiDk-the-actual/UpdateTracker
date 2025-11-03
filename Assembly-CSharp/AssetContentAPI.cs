using System;
using UnityEngine;

public class AssetContentAPI : ScriptableObject
{
	public string bundleName;

	public LazyLoadReference<TextAsset> bundleFile;

	public Object[] assets = new Object[0];
}
