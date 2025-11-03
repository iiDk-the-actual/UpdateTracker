using System;
using System.Collections.Generic;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GorillaNetworking.Store
{
	public class HeadModel_CosmeticStand : HeadModel
	{
		private string mountID
		{
			get
			{
				return "Mount_" + this.bustType.ToString();
			}
		}

		public void LoadCosmeticParts(CosmeticSO cosmeticInfo, bool forRightSide = false)
		{
			this.ClearManuallySpawnedCosmeticParts();
			this.ClearCosmetics();
			if (cosmeticInfo == null)
			{
				Debug.LogWarning("Dynamic Cosmetics - LoadWardRobeParts -  No Cosmetic Info");
				return;
			}
			Debug.Log("Dynamic Cosmetics - Loading Wardrobe Parts for " + cosmeticInfo.info.playFabID);
			this.HandleLoadCosmeticParts(cosmeticInfo, forRightSide);
		}

		private void ResetMannequinSkin()
		{
			this.mannequin.GetComponent<SkinnedMeshRenderer>();
			SkinnedMeshRenderer skinnedMeshRenderer;
			if (this.mannequin.TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer))
			{
				Material[] array = new Material[] { this.defaultMannequinBody, this.defaultMannequinChest, this.defaultMannequinFace };
				skinnedMeshRenderer.sharedMaterials = array;
				return;
			}
			MeshRenderer meshRenderer;
			if (this.mannequin.TryGetComponent<MeshRenderer>(out meshRenderer))
			{
				Material[] array2 = new Material[] { this.defaultMannequinBody, this.defaultMannequinChest, this.defaultMannequinFace };
				meshRenderer.sharedMaterials = array2;
			}
		}

		private void HandleLoadCosmeticParts(CosmeticSO cosmeticInfo, bool forRightSide)
		{
			if (cosmeticInfo.info.category == CosmeticsController.CosmeticCategory.Set && !cosmeticInfo.info.hasStoreParts)
			{
				foreach (CosmeticSO cosmeticSO in cosmeticInfo.info.setCosmetics)
				{
					this.HandleLoadCosmeticParts(cosmeticSO, forRightSide);
				}
				return;
			}
			CosmeticPart[] array;
			if (cosmeticInfo.info.storeParts.Length != 0)
			{
				array = cosmeticInfo.info.storeParts;
			}
			else
			{
				if (cosmeticInfo.info.category == CosmeticsController.CosmeticCategory.Fur)
				{
					CosmeticPart[] array2 = cosmeticInfo.info.functionalParts;
					int i = 0;
					if (i < array2.Length)
					{
						CosmeticPart cosmeticPart = array2[i];
						GameObject gameObject = this.LoadAndInstantiatePrefab(cosmeticPart.prefabAssetRef, base.transform);
						gameObject.GetComponent<GorillaSkinToggle>().ApplyToMannequin(this.mannequin, false);
						Object.DestroyImmediate(gameObject);
						return;
					}
				}
				array = cosmeticInfo.info.wardrobeParts;
			}
			foreach (CosmeticPart cosmeticPart2 in array)
			{
				foreach (CosmeticAttachInfo cosmeticAttachInfo in cosmeticPart2.attachAnchors)
				{
					if ((!forRightSide || !(cosmeticAttachInfo.selectSide == ECosmeticSelectSide.Left)) && (forRightSide || !(cosmeticAttachInfo.selectSide == ECosmeticSelectSide.Right)))
					{
						HeadModel._CosmeticPartLoadInfo cosmeticPartLoadInfo = new HeadModel._CosmeticPartLoadInfo
						{
							playFabId = cosmeticInfo.info.playFabID,
							prefabAssetRef = cosmeticPart2.prefabAssetRef,
							attachInfo = cosmeticAttachInfo,
							xform = null
						};
						GameObject gameObject2 = this.LoadAndInstantiatePrefab(cosmeticPart2.prefabAssetRef, base.transform);
						cosmeticPartLoadInfo.xform = gameObject2.transform;
						this._manuallySpawnedCosmeticParts.Add(gameObject2);
						gameObject2.SetActive(true);
						switch (this.bustType)
						{
						case HeadModel_CosmeticStand.BustType.Disabled:
							this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
							break;
						case HeadModel_CosmeticStand.BustType.GorillaHead:
						case HeadModel_CosmeticStand.BustType.GorillaTorso:
						case HeadModel_CosmeticStand.BustType.GorillaTorsoPost:
						case HeadModel_CosmeticStand.BustType.GuitarStand:
						case HeadModel_CosmeticStand.BustType.JewelryBox:
						case HeadModel_CosmeticStand.BustType.Table:
						case HeadModel_CosmeticStand.BustType.PinDisplay:
						case HeadModel_CosmeticStand.BustType.TagEffectDisplay:
							this.PositionWardRobeItems(gameObject2, cosmeticPartLoadInfo);
							break;
						case HeadModel_CosmeticStand.BustType.GorillaMannequin:
							this._manuallySpawnedCosmeticParts.Remove(gameObject2);
							Object.DestroyImmediate(gameObject2);
							break;
						default:
							this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
							break;
						}
					}
				}
			}
		}

		public void LoadCosmeticPartsV2(string playFabId, bool forRightSide = false)
		{
			this.ClearManuallySpawnedCosmeticParts();
			this.ClearCosmetics();
			CosmeticInfoV2 cosmeticInfoV;
			if (!CosmeticsController.instance.TryGetCosmeticInfoV2(playFabId, out cosmeticInfoV))
			{
				if (!(playFabId == "null") && !(playFabId == "NOTHING") && !(playFabId == "Slingshot"))
				{
					Debug.LogError("HeadModel.playFabId: Cosmetic id \"" + playFabId + "\" not found in `CosmeticsController`.", this);
				}
				return;
			}
			this.HandleLoadingAllPieces(playFabId, forRightSide, cosmeticInfoV);
		}

		private void HandleLoadingAllPieces(string playFabId, bool forRightSide, CosmeticInfoV2 cosmeticInfo)
		{
			CosmeticPart[] array;
			if (cosmeticInfo.storeParts.Length != 0)
			{
				array = cosmeticInfo.storeParts;
			}
			else
			{
				if (cosmeticInfo.category == CosmeticsController.CosmeticCategory.Fur)
				{
					this.HandleLoadingFur(playFabId, forRightSide, cosmeticInfo);
					return;
				}
				if (cosmeticInfo.category == CosmeticsController.CosmeticCategory.Set)
				{
					foreach (CosmeticSO cosmeticSO in cosmeticInfo.setCosmetics)
					{
						this.HandleLoadingAllPieces(playFabId, forRightSide, cosmeticSO.info);
					}
					return;
				}
				array = cosmeticInfo.wardrobeParts;
			}
			foreach (CosmeticPart cosmeticPart in array)
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
						cosmeticPartLoadInfo.loadOp.Completed += this._HandleLoadCosmeticPartsV2;
						this._loadOp_to_partInfoIndex[cosmeticPartLoadInfo.loadOp] = this._currentPartLoadInfos.Count;
						this._currentPartLoadInfos.Add(cosmeticPartLoadInfo);
					}
				}
			}
		}

		private void _HandleLoadCosmeticPartsV2(AsyncOperationHandle<GameObject> loadOp)
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
			this._manuallySpawnedCosmeticParts.Add(cosmeticPartLoadInfo.xform.gameObject);
			switch (this.bustType)
			{
			case HeadModel_CosmeticStand.BustType.Disabled:
				this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.GorillaHead:
				this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.GorillaTorso:
				this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.GorillaTorsoPost:
				this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.GorillaMannequin:
				this._manuallySpawnedCosmeticParts.Remove(cosmeticPartLoadInfo.xform.gameObject);
				Object.DestroyImmediate(cosmeticPartLoadInfo.xform.gameObject);
				break;
			case HeadModel_CosmeticStand.BustType.GuitarStand:
				this.PositionWardRobeItems(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.JewelryBox:
				this.PositionWardRobeItems(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.Table:
				this.PositionWardRobeItems(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.PinDisplay:
				this.PositionWardRobeItems(cosmeticPartLoadInfo);
				break;
			case HeadModel_CosmeticStand.BustType.TagEffectDisplay:
				this.PositionWardRobeItems(cosmeticPartLoadInfo);
				break;
			default:
				this.PositionWithWardRobeOffsets(cosmeticPartLoadInfo);
				break;
			}
			cosmeticPartLoadInfo.xform.gameObject.SetActive(true);
		}

		private void HandleLoadingFur(string playFabId, bool forRightSide, CosmeticInfoV2 cosmeticInfo)
		{
			foreach (CosmeticPart cosmeticPart in cosmeticInfo.functionalParts)
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
						cosmeticPartLoadInfo.loadOp.Completed += this._HandleLoadCosmeticPartsV2Fur;
						this._loadOp_to_partInfoIndex[cosmeticPartLoadInfo.loadOp] = this._currentPartLoadInfos.Count;
						this._currentPartLoadInfos.Add(cosmeticPartLoadInfo);
					}
				}
			}
		}

		private void _HandleLoadCosmeticPartsV2Fur(AsyncOperationHandle<GameObject> loadOp)
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
			cosmeticPartLoadInfo.xform.GetComponent<GorillaSkinToggle>().ApplyToMannequin(this.mannequin, false);
			Object.DestroyImmediate(cosmeticPartLoadInfo.xform.gameObject);
		}

		public void SetStandType(HeadModel_CosmeticStand.BustType newBustType)
		{
			this.bustType = newBustType;
		}

		private void PositionWardRobeItems(GameObject instantiateEdObject, HeadModel._CosmeticPartLoadInfo partLoadInfo)
		{
			Transform transform = instantiateEdObject.transform.FindChildRecursive(this.mountID);
			if (transform != null)
			{
				Debug.Log("Dynamic Cosmetics - Mount Found: " + this.mountID);
				instantiateEdObject.transform.position = base.transform.position;
				instantiateEdObject.transform.rotation = base.transform.rotation;
				instantiateEdObject.transform.localPosition = transform.localPosition;
				instantiateEdObject.transform.localRotation = transform.localRotation;
				return;
			}
			HeadModel_CosmeticStand.BustType bustType = this.bustType;
			if (bustType - HeadModel_CosmeticStand.BustType.GuitarStand <= 2 || bustType == HeadModel_CosmeticStand.BustType.TagEffectDisplay)
			{
				instantiateEdObject.transform.position = base.transform.position;
				instantiateEdObject.transform.rotation = base.transform.rotation;
				return;
			}
			this.PositionWithWardRobeOffsets(partLoadInfo);
		}

		private void PositionWardRobeItems(HeadModel._CosmeticPartLoadInfo partLoadInfo)
		{
			Transform transform = partLoadInfo.xform.FindChildRecursive(this.mountID);
			if (transform != null)
			{
				Debug.Log("Dynamic Cosmetics - Mount Found: " + this.mountID);
				partLoadInfo.xform.position = base.transform.position;
				partLoadInfo.xform.rotation = base.transform.rotation;
				partLoadInfo.xform.localPosition = transform.localPosition;
				partLoadInfo.xform.localRotation = transform.localRotation;
				return;
			}
			HeadModel_CosmeticStand.BustType bustType = this.bustType;
			if (bustType - HeadModel_CosmeticStand.BustType.GuitarStand <= 2 || bustType == HeadModel_CosmeticStand.BustType.TagEffectDisplay)
			{
				partLoadInfo.xform.position = base.transform.position;
				partLoadInfo.xform.rotation = base.transform.rotation;
				return;
			}
			this.PositionWithWardRobeOffsets(partLoadInfo);
		}

		private void PositionWithWardRobeOffsets(HeadModel._CosmeticPartLoadInfo partLoadInfo)
		{
			Debug.Log("Dynamic Cosmetics - Mount Not Found: " + this.mountID);
			partLoadInfo.xform.localPosition = partLoadInfo.attachInfo.offset.pos;
			partLoadInfo.xform.localRotation = partLoadInfo.attachInfo.offset.rot;
			partLoadInfo.xform.localScale = partLoadInfo.attachInfo.offset.scale;
		}

		public void ClearManuallySpawnedCosmeticParts()
		{
			foreach (GameObject gameObject in this._manuallySpawnedCosmeticParts)
			{
				Object.DestroyImmediate(gameObject);
			}
			this._manuallySpawnedCosmeticParts.Clear();
		}

		public void ClearCosmetics()
		{
			this.ResetMannequinSkin();
			for (int i = base.transform.childCount - 1; i >= 0; i--)
			{
				Object.DestroyImmediate(base.transform.GetChild(i).gameObject);
			}
		}

		private GameObject LoadAndInstantiatePrefab(GTAssetRef<GameObject> prefabAssetRef, Transform parent)
		{
			return null;
		}

		public void UpdateCosmeticsMountPositions(CosmeticSO findCosmeticInAllCosmeticsArraySO)
		{
		}

		public HeadModel_CosmeticStand.BustType bustType = HeadModel_CosmeticStand.BustType.JewelryBox;

		[SerializeField]
		private List<GameObject> _manuallySpawnedCosmeticParts = new List<GameObject>();

		public GameObject mannequin;

		public Material defaultMannequinFace;

		public Material defaultMannequinChest;

		public Material defaultMannequinBody;

		[DebugReadout]
		protected new readonly List<HeadModel._CosmeticPartLoadInfo> _currentPartLoadInfos = new List<HeadModel._CosmeticPartLoadInfo>(1);

		[DebugReadout]
		private readonly Dictionary<AsyncOperationHandle, int> _loadOp_to_partInfoIndex = new Dictionary<AsyncOperationHandle, int>(1);

		public enum BustType
		{
			Disabled,
			GorillaHead,
			GorillaTorso,
			GorillaTorsoPost,
			GorillaMannequin,
			GuitarStand,
			JewelryBox,
			Table,
			PinDisplay,
			TagEffectDisplay
		}
	}
}
