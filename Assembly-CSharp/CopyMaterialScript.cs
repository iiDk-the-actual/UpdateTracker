using System;
using UnityEngine;

public class CopyMaterialScript : MonoBehaviour
{
	private void Start()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (this.sourceToCopyMaterialFrom.material != this.mySkinnedMeshRenderer.material)
		{
			this.mySkinnedMeshRenderer.material = this.sourceToCopyMaterialFrom.material;
		}
	}

	public SkinnedMeshRenderer sourceToCopyMaterialFrom;

	public SkinnedMeshRenderer mySkinnedMeshRenderer;
}
