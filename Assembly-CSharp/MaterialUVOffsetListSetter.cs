using System;
using System.Collections.Generic;
using GorillaTag.Rendering;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialUVOffsetListSetter : MonoBehaviour, IBuildValidation
{
	private void Awake()
	{
		this.matPropertyBlock = new MaterialPropertyBlock();
		this.meshRenderer = base.GetComponent<MeshRenderer>();
		this.meshRenderer.GetPropertyBlock(this.matPropertyBlock);
	}

	public void SetUVOffset(int listIndex)
	{
		if (listIndex >= this.uvOffsetList.Count || listIndex < 0)
		{
			Debug.LogError("Invalid uv offset list index provided.");
			return;
		}
		if (this.matPropertyBlock == null || this.meshRenderer == null)
		{
			Debug.LogError("MaterialUVOffsetListSetter settings are incorrect somehow, please fix", base.gameObject);
			this.Awake();
			return;
		}
		Vector2 vector = this.uvOffsetList[listIndex];
		this.matPropertyBlock.SetVector(ShaderProps._BaseMap_ST, new Vector4(1f, 1f, vector.x, vector.y));
		this.meshRenderer.SetPropertyBlock(this.matPropertyBlock);
	}

	public bool BuildValidationCheck()
	{
		if (base.GetComponent<MeshRenderer>() == null)
		{
			Debug.LogError("missing a mesh renderer for the materialuvoffsetlistsetter", base.gameObject);
			return false;
		}
		if (base.GetComponentInParent<EdMeshCombinerMono>() != null && base.GetComponentInParent<EdDoNotMeshCombine>() == null)
		{
			Debug.LogError("the meshrenderer is going to getcombined, that will likely cause issues for the materialuvoffsetlistsetter", base.gameObject);
			return false;
		}
		return true;
	}

	[SerializeField]
	private List<Vector2> uvOffsetList = new List<Vector2>();

	private MeshRenderer meshRenderer;

	private MaterialPropertyBlock matPropertyBlock;
}
