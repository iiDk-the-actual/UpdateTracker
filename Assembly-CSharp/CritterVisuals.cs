using System;
using GorillaExtensions;
using UnityEngine;

public class CritterVisuals : MonoBehaviour
{
	public CritterAppearance Appearance
	{
		get
		{
			return this._appearance;
		}
	}

	public void SetAppearance(CritterAppearance appearance)
	{
		this._appearance = appearance;
		float num = this._appearance.size.ClampSafe(0.25f, 1.5f);
		this.bodyRoot.localScale = new Vector3(num, num, num);
		if (!string.IsNullOrEmpty(appearance.hatName))
		{
			foreach (GameObject gameObject in this.hats)
			{
				gameObject.SetActive(gameObject.name == this._appearance.hatName);
			}
			this.hatRoot.gameObject.SetActive(true);
			return;
		}
		this.hatRoot.gameObject.SetActive(false);
	}

	public void ApplyMesh(Mesh newMesh)
	{
		this.myMeshFilter.sharedMesh = newMesh;
	}

	public void ApplyMaterial(Material mat)
	{
		this.myRenderer.sharedMaterial = mat;
	}

	public int critterType;

	[Header("Visuals")]
	public Transform bodyRoot;

	public MeshRenderer myRenderer;

	public MeshFilter myMeshFilter;

	public Transform hatRoot;

	public GameObject[] hats;

	private CritterAppearance _appearance;
}
