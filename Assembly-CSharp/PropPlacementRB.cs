using System;
using System.Collections.Generic;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class PropPlacementRB : MonoBehaviour, IDelayedExecListener
{
	protected void OnDestroy()
	{
		if (this._placingProp != null)
		{
			Object.Destroy(this._placingProp);
		}
	}

	public void PlaceProp_NoPool(PropHuntPropZone parentZone, GTAssetRef<GameObject> propRef, Vector3 pos, Quaternion rot, CosmeticSO debugCosmeticSO)
	{
		if (this._isInstantiatingAsync)
		{
			Debug.LogError("ERROR!!!  PropPlacementRB: Tried to place (spawn) prop while one was already being placed.");
			return;
		}
		this._parentZone = parentZone;
		MeshCollider[] colliders = this._colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].gameObject.SetActive(false);
		}
		base.transform.position = pos;
		base.transform.rotation = rot;
		base.gameObject.SetActive(false);
		this._isInstantiatingAsync = true;
		propRef.InstantiateAsync(null, false).Completed += this.OnPropLoaded_NoPool;
	}

	public void OnPropLoaded_NoPool(AsyncOperationHandle<GameObject> handle)
	{
		this._isInstantiatingAsync = false;
		this._placingProp = handle.Result;
		this._placingProp.transform.position = base.transform.position;
		this._placingProp.transform.rotation = base.transform.rotation;
		this.m_rb.linearVelocity = Vector3.zero;
		this.m_rb.angularVelocity = Vector3.zero;
		CosmeticSO cosmeticSO = null;
		if (!PropPlacementRB.TryPrepPropTemplate(this, this._placingProp, cosmeticSO))
		{
			this.DestroyProp_NoPool();
			return;
		}
		this._placingProp.SetActive(false);
		base.gameObject.SetActive(true);
		GTDelayedExec.Add(this, 2f, 0);
	}

	public static bool TryPrepPropTemplate(PropPlacementRB rb, GameObject rendererGobj, CosmeticSO _debugCosmeticSO)
	{
		rb._isInstantiatingAsync = false;
		rb._placingProp = rendererGobj;
		rb._placingProp.transform.position = rb.transform.position;
		rb._placingProp.transform.rotation = rb.transform.rotation;
		rb.m_rb.linearVelocity = Vector3.zero;
		rb.m_rb.angularVelocity = Vector3.zero;
		bool flag = false;
		MeshFilter[] componentsInChildren = rendererGobj.GetComponentsInChildren<MeshFilter>(true);
		List<MeshCollider> list;
		bool flag2;
		using (ListPool<MeshCollider>.Get(out list))
		{
			list.Capacity = math.max(list.Capacity, 8);
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				Mesh sharedMesh = meshFilter.sharedMesh;
				if (!(sharedMesh == null) && sharedMesh.isReadable)
				{
					flag = true;
					MeshCollider meshCollider = new GameObject(meshFilter.name + "__PropHuntDecoy_Collider")
					{
						transform = 
						{
							parent = rb.transform
						},
						layer = 30
					}.AddComponent<MeshCollider>();
					meshCollider.convex = true;
					meshCollider.transform.position = meshFilter.transform.position;
					meshCollider.transform.rotation = meshFilter.transform.rotation;
					meshCollider.sharedMesh = meshFilter.sharedMesh;
					list.Add(meshCollider);
				}
			}
			rb._colliders = list.ToArray();
			if (!flag)
			{
				flag2 = false;
			}
			else
			{
				Transform[] componentsInChildren2 = rendererGobj.GetComponentsInChildren<Transform>(true);
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].gameObject.isStatic = true;
				}
				flag2 = true;
			}
		}
		return flag2;
	}

	void IDelayedExecListener.OnDelayedAction(int contextId)
	{
		this.OnPropFell();
	}

	private void OnPropFell()
	{
		if (this._placingProp == null)
		{
			return;
		}
		this._placingProp.transform.position = base.transform.position;
		this._placingProp.transform.rotation = base.transform.rotation;
		this._placingProp.SetActive(true);
		base.gameObject.SetActive(false);
	}

	public void DestroyProp_NoPool()
	{
		if (this._placingProp != null)
		{
			Object.Destroy(this._placingProp);
			this._placingProp = null;
		}
	}

	[FormerlySerializedAs("rb")]
	[SerializeField]
	private Rigidbody m_rb;

	[FormerlySerializedAs("simDurationBeforeFreeze")]
	[SerializeField]
	private float m_simDurationBeforeFreeze;

	private PropHuntPropZone _parentZone;

	[SerializeField]
	internal GameObject _placingProp;

	[SerializeField]
	private MeshCollider[] _colliders;

	private bool _isInstantiatingAsync;
}
