using System;
using System.Collections.Generic;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class PropHuntPropZone : MonoBehaviour, IDelayedExecListener
{
	private void Awake()
	{
		this.hasBoxCollider = base.TryGetComponent<BoxCollider>(out this.boxCollider);
	}

	private void OnEnable()
	{
		GorillaPropHuntGameManager.RegisterPropZone(this);
	}

	private void OnDisable()
	{
		this.DestroyDecoys();
		GorillaPropHuntGameManager.UnregisterPropZone(this);
	}

	public void DestroyDecoys()
	{
		foreach (PropPlacementRB propPlacementRB in this.propPlacementRBs)
		{
			if (propPlacementRB != null)
			{
				PropHuntPools.ReturnDecoyProp(propPlacementRB);
			}
		}
		this.propPlacementRBs.Clear();
	}

	public void OnRoundStart()
	{
		if (!PropHuntPools.IsReady)
		{
			Debug.LogError("ERROR!!!  PropHuntPropZone: (this should never happen) props not ready to be spawned so aborting. you should only be calling this if `PropHuntPools.IsReady` is true or from the callback `PropHuntPools.OnReady`.");
		}
		this.CreateDecoys(GorillaPropHuntGameManager.instance.GetSeed());
	}

	public void CreateDecoys(int seed)
	{
		this.DestroyDecoys();
		SRand srand = new SRand(seed + this.seedOffset);
		for (int i = 0; i < this.numProps; i++)
		{
			PropPlacementRB propPlacementRB;
			if (!PropHuntPools.TryGetDecoyProp(GorillaPropHuntGameManager.instance.GetCosmeticId(srand.NextUInt()), out propPlacementRB))
			{
				return;
			}
			Vector3 vector2;
			if (this.hasBoxCollider)
			{
				Vector3 vector = new Vector3(srand.NextFloat(-this.boxCollider.size.x, this.boxCollider.size.x) / 2f, srand.NextFloat(-this.boxCollider.size.y, this.boxCollider.size.y) / 2f, srand.NextFloat(-this.boxCollider.size.z, this.boxCollider.size.z) / 2f);
				vector2 = base.transform.TransformPoint(vector);
			}
			else
			{
				vector2 = base.transform.position + srand.NextPointInsideSphere(this.radius);
			}
			propPlacementRB.gameObject.SetActive(false);
			propPlacementRB.transform.SetParent(null, false);
			propPlacementRB.transform.position = vector2;
			propPlacementRB.transform.rotation = Quaternion.Euler(srand.NextFloat(360f), srand.NextFloat(360f), srand.NextFloat(360f));
			propPlacementRB._placingProp.SetActive(false);
			propPlacementRB._placingProp.transform.SetParent(null, false);
			this.propPlacementRBs.Add(propPlacementRB);
		}
		for (int j = 0; j < this.propPlacementRBs.Count; j++)
		{
			this.propPlacementRBs[j].gameObject.SetActive(true);
		}
		GTDelayedExec.Add(this, this.m_simDurationBeforeFreeze, 0);
	}

	public void OnDelayedAction(int contextId)
	{
		for (int i = 0; i < this.propPlacementRBs.Count; i++)
		{
			PropPlacementRB propPlacementRB = this.propPlacementRBs[i];
			propPlacementRB.gameObject.SetActive(false);
			Transform transform = propPlacementRB.transform;
			GameObject placingProp = propPlacementRB._placingProp;
			placingProp.transform.SetPositionAndRotation(transform.position, transform.rotation);
			placingProp.SetActive(true);
		}
	}

	private PropPlacementRB _GetOrCreatePropPlacementObj_NoPool()
	{
		PropPlacementRB propPlacementRB;
		if (this.nextUnusedPropPlacement < this.propPlacementRBs.Count)
		{
			propPlacementRB = this.propPlacementRBs[this.nextUnusedPropPlacement];
		}
		else
		{
			propPlacementRB = Object.Instantiate<PropPlacementRB>(this.propPlacementPrefab, base.transform);
			this.propPlacementRBs.Add(propPlacementRB);
		}
		this.nextUnusedPropPlacement++;
		return propPlacementRB;
	}

	private void SpawnProp_NoPool(GTAssetRef<GameObject> item, Vector3 pos, Quaternion rot, CosmeticSO debugCosmeticSO)
	{
		this._GetOrCreatePropPlacementObj_NoPool().PlaceProp_NoPool(this, item, pos, rot, debugCosmeticSO);
	}

	private const string preLog = "PropHuntPropZone: ";

	private const string preLogEd = "(editor only log) PropHuntPropZone: ";

	private const string preLogBeta = "(beta only log) PropHuntPropZone: ";

	private const string preErr = "ERROR!!!  PropHuntPropZone: ";

	private const string preErrEd = "ERROR!!!  (editor only log) PropHuntPropZone: ";

	private const string preErrBeta = "ERROR!!!  (beta only log) PropHuntPropZone: ";

	private const bool _k__GT_PROP_HUNT__USE_POOLING__ = true;

	[SerializeField]
	private PropPlacementRB propPlacementPrefab;

	[SerializeField]
	private int seedOffset;

	[SerializeField]
	private float radius = 1f;

	[SerializeField]
	private int numProps = 10;

	[SerializeField]
	private float m_simDurationBeforeFreeze = 2f;

	private BoxCollider boxCollider;

	private bool hasBoxCollider;

	private int nextUnusedPropPlacement;

	private readonly List<PropPlacementRB> propPlacementRBs = new List<PropPlacementRB>(64);
}
