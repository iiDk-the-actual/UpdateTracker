using System;
using System.Collections.Generic;
using Cysharp.Text;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeadModel : MonoBehaviour, IDelayedExecListener
{
	protected void Awake()
	{
		this.RefreshRenderer();
	}

	protected void RefreshRenderer()
	{
		this._mannequinRenderer = base.GetComponentInChildren<Renderer>(true);
	}

	public void SetCosmeticActive(string playFabId, bool forRightSide = false)
	{
		this._ClearCurrent();
		this._AddPreviewCosmetic(playFabId, forRightSide);
	}

	public void SetCosmeticActiveArray(string[] playFabIds, bool[] forRightSideArray)
	{
		this._ClearCurrent();
		for (int i = 0; i < playFabIds.Length; i++)
		{
			this._AddPreviewCosmetic(playFabIds[i], forRightSideArray[i]);
		}
	}

	private void _AddPreviewCosmetic(string playFabId, bool forRightSide)
	{
		CosmeticInfoV2 cosmeticInfoV;
		if (!CosmeticsController.instance.TryGetCosmeticInfoV2(playFabId, out cosmeticInfoV))
		{
			if (!(playFabId == "null") && !(playFabId == "NOTHING") && !(playFabId == "Slingshot"))
			{
				Debug.LogError(ZString.Concat<string, string, string>("HeadModel._AddPreviewCosmetic: Cosmetic id \"", playFabId, "\" not found in `CosmeticsController`."), this);
			}
			return;
		}
		if (cosmeticInfoV.hideWardrobeMannequin)
		{
			if (this._mannequinRenderer.IsNull())
			{
				this.RefreshRenderer();
			}
			if (this._mannequinRenderer.IsNotNull())
			{
				this._mannequinRenderer.enabled = false;
			}
		}
		foreach (CosmeticPart cosmeticPart in cosmeticInfoV.wardrobeParts)
		{
			if (!cosmeticPart.prefabAssetRef.RuntimeKeyIsValid())
			{
				GTDev.LogError<string>("Cosmetic " + cosmeticInfoV.displayName + " has missing object reference in wardrobe parts, skipping load", null);
			}
			else
			{
				foreach (CosmeticAttachInfo cosmeticAttachInfo in cosmeticPart.attachAnchors)
				{
					if ((!forRightSide || !(cosmeticAttachInfo.selectSide == ECosmeticSelectSide.Left)) && (forRightSide || !(cosmeticAttachInfo.selectSide == ECosmeticSelectSide.Right)))
					{
						HeadModel._CosmeticPartLoadInfo cosmeticPartLoadInfo = new HeadModel._CosmeticPartLoadInfo
						{
							playFabId = playFabId,
							prefabAssetRef = cosmeticPart.prefabAssetRef,
							attachInfo = cosmeticAttachInfo,
							loadOp = cosmeticPart.prefabAssetRef.InstantiateAsync(base.transform, false),
							xform = null
						};
						cosmeticPartLoadInfo.loadOp.Completed += this._HandleLoadOpOnCompleted;
						this._loadOp_to_partInfoIndex[cosmeticPartLoadInfo.loadOp] = this._currentPartLoadInfos.Count;
						this._currentPartLoadInfos.Add(cosmeticPartLoadInfo);
					}
				}
			}
		}
	}

	private void _HandleLoadOpOnCompleted(AsyncOperationHandle<GameObject> loadOp)
	{
		int num;
		if (!this._loadOp_to_partInfoIndex.TryGetValue(loadOp, out num))
		{
			if (loadOp.Status == AsyncOperationStatus.Succeeded && loadOp.Result)
			{
				Object.Destroy(loadOp.Result);
			}
			return;
		}
		HeadModel._CosmeticPartLoadInfo cosmeticPartLoadInfo = this._currentPartLoadInfos[num];
		if (loadOp.Status == AsyncOperationStatus.Failed)
		{
			Debug.Log("HeadModel: Failed to load a part for cosmetic \"" + cosmeticPartLoadInfo.playFabId + "\"! Waiting for 10 seconds before trying again.", this);
			GTDelayedExec.Add(this, 10f, num);
			return;
		}
		cosmeticPartLoadInfo.xform = loadOp.Result.transform;
		cosmeticPartLoadInfo.xform.localPosition = cosmeticPartLoadInfo.attachInfo.offset.pos;
		cosmeticPartLoadInfo.xform.localRotation = cosmeticPartLoadInfo.attachInfo.offset.rot;
		cosmeticPartLoadInfo.xform.localScale = cosmeticPartLoadInfo.attachInfo.offset.scale;
		cosmeticPartLoadInfo.xform.gameObject.SetActive(true);
	}

	void IDelayedExecListener.OnDelayedAction(int partLoadInfosIndex)
	{
		if (partLoadInfosIndex < 0 || partLoadInfosIndex >= this._currentPartLoadInfos.Count)
		{
			return;
		}
		HeadModel._CosmeticPartLoadInfo cosmeticPartLoadInfo = this._currentPartLoadInfos[partLoadInfosIndex];
		if (cosmeticPartLoadInfo.loadOp.Status != AsyncOperationStatus.Failed)
		{
			return;
		}
		cosmeticPartLoadInfo.loadOp.Completed += this._HandleLoadOpOnCompleted;
		cosmeticPartLoadInfo.loadOp = cosmeticPartLoadInfo.prefabAssetRef.InstantiateAsync(base.transform, false);
		this._loadOp_to_partInfoIndex[cosmeticPartLoadInfo.loadOp] = partLoadInfosIndex;
	}

	protected void _ClearCurrent()
	{
		for (int i = 0; i < this._currentPartLoadInfos.Count; i++)
		{
			Object.Destroy(this._currentPartLoadInfos[i].loadOp.Result);
		}
		this._EnsureCapacityAndClear<AsyncOperationHandle, int>(this._loadOp_to_partInfoIndex);
		this._EnsureCapacityAndClear<HeadModel._CosmeticPartLoadInfo>(this._currentPartLoadInfos);
		if (this._mannequinRenderer.IsNull())
		{
			this.RefreshRenderer();
		}
		this._mannequinRenderer.enabled = true;
	}

	private void _EnsureCapacityAndClear<T>(List<T> list)
	{
		if (list.Count > list.Capacity)
		{
			list.Capacity = list.Count;
		}
		list.Clear();
	}

	private void _EnsureCapacityAndClear<T1, T2>(Dictionary<T1, T2> dict)
	{
		dict.EnsureCapacity(dict.Count);
		dict.Clear();
	}

	[DebugReadout]
	protected readonly List<HeadModel._CosmeticPartLoadInfo> _currentPartLoadInfos = new List<HeadModel._CosmeticPartLoadInfo>(1);

	[DebugReadout]
	private readonly Dictionary<AsyncOperationHandle, int> _loadOp_to_partInfoIndex = new Dictionary<AsyncOperationHandle, int>(1);

	private Renderer _mannequinRenderer;

	public GameObject[] cosmetics;

	protected struct _CosmeticPartLoadInfo
	{
		public string playFabId;

		public GTAssetRef<GameObject> prefabAssetRef;

		public CosmeticAttachInfo attachInfo;

		public AsyncOperationHandle<GameObject> loadOp;

		public Transform xform;
	}
}
