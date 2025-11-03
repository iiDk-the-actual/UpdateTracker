using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class MaterialCycler : MonoBehaviour
{
	private void Awake()
	{
		this.materialCyclerNetworked = base.GetComponent<MaterialCyclerNetworked>();
		this.SetMaterials();
	}

	private void OnEnable()
	{
		if (this.materialCyclerNetworked != null)
		{
			this.materialCyclerNetworked.OnSynchronize += this.MaterialCyclerNetworked_OnSynchronize;
		}
	}

	private void OnDisable()
	{
		if (this.materialCyclerNetworked != null)
		{
			this.materialCyclerNetworked.OnSynchronize -= this.MaterialCyclerNetworked_OnSynchronize;
		}
	}

	private void MaterialCyclerNetworked_OnSynchronize(int idx, int3 rgb)
	{
		if (idx < 0 || idx >= this.materials.Length)
		{
			return;
		}
		this.index = idx;
		for (int i = 0; i < this.renderers.Length; i++)
		{
			this.renderers[i].material = this.materials[this.index].Materials[i];
			this.renderers[i].material.SetColor(this.setColorTarget, new Color((float)rgb.x / 9f, (float)rgb.y / 9f, (float)rgb.z / 9f));
		}
		this.reset.Invoke(new Vector3(this.renderers[0].material.color.r, this.renderers[0].material.color.g, this.renderers[0].material.color.b));
	}

	private void SetMaterials()
	{
		for (int i = 0; i < this.renderers.Length; i++)
		{
			if (this.materials[this.index].Materials.Length > i)
			{
				this.renderers[i].material = this.materials[this.index].Materials[i];
			}
			else
			{
				this.renderers[i].material = null;
			}
		}
		this.reset.Invoke(new Vector3(this.renderers[0].material.color.r, this.renderers[0].material.color.g, this.renderers[0].material.color.b));
	}

	public void NextMaterial()
	{
		this.index = (this.index + 1) % this.materials.Length;
		this.SetMaterials();
		this.SetDirty();
	}

	private void SetDirty()
	{
		if (this.materialCyclerNetworked == null)
		{
			return;
		}
		this.synchTime = Time.time + this.materialCyclerNetworked.SyncTimeOut;
		if (this.crDirty == null)
		{
			this.crDirty = base.StartCoroutine(this.timeOutDirty());
		}
	}

	private IEnumerator timeOutDirty()
	{
		while (this.synchTime > Time.time)
		{
			yield return null;
		}
		this.synchronize();
		this.crDirty = null;
		yield break;
	}

	private void synchronize()
	{
		this.materialCyclerNetworked.Synchronize(this.index, this.renderers[0].material.color);
	}

	public void SetColor(Vector3 rgb)
	{
		for (int i = 0; i < this.renderers.Length; i++)
		{
			this.renderers[i].material.SetColor(this.setColorTarget, new Color(rgb.x, rgb.y, rgb.z));
		}
		this.SetDirty();
	}

	[SerializeField]
	private MaterialCycler.MaterialPack[] materials;

	[SerializeField]
	private Renderer[] renderers;

	private int index;

	[SerializeField]
	private string setColorTarget = "_BaseColor";

	[SerializeField]
	private UnityEvent<Vector3> reset;

	private Coroutine crDirty;

	private float synchTime;

	private MaterialCyclerNetworked materialCyclerNetworked;

	[Serializable]
	private class MaterialPack
	{
		public Material[] Materials
		{
			get
			{
				return this.materials;
			}
		}

		[SerializeField]
		private Material[] materials;
	}
}
