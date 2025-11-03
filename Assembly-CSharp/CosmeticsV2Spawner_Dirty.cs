using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Text;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaNetworking.Store;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CosmeticsV2Spawner_Dirty : IDelayedExecListener, ITickSystemTick
{
	public static bool startedAllPartsInstantiated { get; private set; }

	public static bool allPartsInstantiated { get; private set; }

	public static bool completed { get; private set; }

	public bool TickRunning { get; set; }

	void ITickSystemTick.Tick()
	{
		this._shouldTick = false;
		if (CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Count < CosmeticsV2Spawner_Dirty._g_loadOpInfos.Count)
		{
			this._shouldTick = true;
			CosmeticsV2Spawner_Dirty._Step2_UpdateLoadOpStarting();
		}
		if (!this._shouldTick)
		{
			TickSystem<object>.RemoveTickCallback(this);
		}
	}

	void IDelayedExecListener.OnDelayedAction(int contextId)
	{
		if (contextId >= 0 && contextId < 1000000)
		{
			CosmeticsV2Spawner_Dirty._RetryDownload(contextId);
			return;
		}
		if (contextId == -100)
		{
			this._DelayedStatusCheck();
			return;
		}
		if (contextId == -Mathf.Abs("_Step5_InitializeVRRigsAndCosmeticsControllerFinalize".GetHashCode()))
		{
			CosmeticsV2Spawner_Dirty._Step5_InitializeVRRigsAndCosmeticsControllerFinalize();
		}
	}

	public static void StartInstantiatingPrefabs()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (CosmeticsV2Spawner_Dirty.startedAllPartsInstantiated || CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			Debug.LogError("CosmeticsV2Spawner_Dirty.StartInstantiatingPrefabs: All parts already started instantiated. Check `startedAllPartsInstantiated` before calling this.");
			return;
		}
		if (CosmeticsV2Spawner_Dirty._instance == null)
		{
			CosmeticsV2Spawner_Dirty._instance = new CosmeticsV2Spawner_Dirty();
		}
		CosmeticsV2Spawner_Dirty.k_stopwatch.Restart();
		CosmeticsV2Spawner_Dirty.g_gorillaPlayer = Object.FindAnyObjectByType<GTPlayer>();
		foreach (SnowballMaker snowballMaker in CosmeticsV2Spawner_Dirty.g_gorillaPlayer.GetComponentsInChildren<SnowballMaker>(true))
		{
			if (snowballMaker.isLeftHand)
			{
				CosmeticsV2Spawner_Dirty._gSnowballMakerLeft = snowballMaker;
			}
			else
			{
				CosmeticsV2Spawner_Dirty._gSnowballMakerRight = snowballMaker;
			}
		}
		if (!CosmeticsController.hasInstance)
		{
			Debug.LogError("(should never happen) cannot instantiate prefabs before cosmetics controller instance is available.");
			return;
		}
		if (!CosmeticsController.instance.v2_allCosmeticsInfoAssetRef.IsValid())
		{
			Debug.LogError("(should never happen) cannot load prefabs before v2_allCosmeticsInfoAssetRef is loaded.");
			return;
		}
		AllCosmeticsArraySO allCosmeticsArraySO = CosmeticsController.instance.v2_allCosmeticsInfoAssetRef.Asset as AllCosmeticsArraySO;
		if (allCosmeticsArraySO == null)
		{
			Debug.LogError("(should never happen) v2_allCosmeticsInfoAssetRef is valid but null.");
			return;
		}
		Transform[] array;
		string text;
		if (!GTHardCodedBones.TryGetBoneXforms(VRRig.LocalRig, out array, out text))
		{
			Debug.LogError("CosmeticsV2Spawner_Dirty: Error getting bone Transforms from local VRRig: " + text, VRRig.LocalRig);
			return;
		}
		CosmeticsV2Spawner_Dirty._gVRRigDatas.Add(new CosmeticsV2Spawner_Dirty.VRRigData(VRRig.LocalRig, array));
		int num = 0;
		foreach (VRRig vrrig in VRRigCache.Instance.GetAllRigs())
		{
			Transform[] array2;
			if (!GTHardCodedBones.TryGetBoneXforms(vrrig, out array2, out text))
			{
				Debug.LogError("CosmeticsV2Spawner_Dirty: Error getting bone Transforms from cached VRRig: " + text, VRRig.LocalRig);
				return;
			}
			CosmeticsV2Spawner_Dirty._gVRRigDatas.Add(new CosmeticsV2Spawner_Dirty.VRRigData(vrrig, array2));
		}
		CosmeticsV2Spawner_Dirty._gDeactivatedSpawnParent = GlobalDeactivatedSpawnRoot.GetOrCreate();
		GTDelayedExec.Add(CosmeticsV2Spawner_Dirty._instance, 2f, -100);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (GTDirectAssetRef<CosmeticSO> gtdirectAssetRef in allCosmeticsArraySO.sturdyAssetRefs)
		{
			CosmeticInfoV2 info = gtdirectAssetRef.obj.info;
			if (info.hasHoldableParts)
			{
				for (int j = 0; j < CosmeticsV2Spawner_Dirty._gVRRigDatas.Count; j++)
				{
					for (int k = 0; k < info.holdableParts.Length; k++)
					{
						CosmeticPart cosmeticPart = info.holdableParts[k];
						if (!cosmeticPart.prefabAssetRef.RuntimeKeyIsValid())
						{
							if (j == 0)
							{
								GTDev.LogError<string>("Cosmetic " + info.displayName + " has missing object reference in wearable parts, skipping load", null);
							}
						}
						else
						{
							CosmeticsV2Spawner_Dirty.AddEachAttachInfoToLoadOpInfosList(cosmeticPart, k, info, j, ref num2);
						}
					}
				}
			}
			if (info.hasFunctionalParts)
			{
				for (int l = 0; l < CosmeticsV2Spawner_Dirty._gVRRigDatas.Count; l++)
				{
					for (int m = 0; m < info.functionalParts.Length; m++)
					{
						CosmeticPart cosmeticPart2 = info.functionalParts[m];
						if (!cosmeticPart2.prefabAssetRef.RuntimeKeyIsValid())
						{
							if (l == 0)
							{
								GTDev.LogError<string>("Cosmetic " + info.displayName + " has missing object reference in functional parts, skipping load", null);
							}
						}
						else
						{
							CosmeticsV2Spawner_Dirty.AddEachAttachInfoToLoadOpInfosList(cosmeticPart2, m, info, l, ref num2);
						}
					}
				}
			}
			if (info.hasFirstPersonViewParts)
			{
				for (int n = 0; n < info.firstPersonViewParts.Length; n++)
				{
					CosmeticPart cosmeticPart3 = info.firstPersonViewParts[n];
					if (!cosmeticPart3.prefabAssetRef.RuntimeKeyIsValid())
					{
						GTDev.LogError<string>("Cosmetic " + info.displayName + " has missing object reference in first person parts, skipping load", null);
					}
					else
					{
						CosmeticsV2Spawner_Dirty.AddEachAttachInfoToLoadOpInfosList(cosmeticPart3, n, info, num, ref num3);
					}
				}
			}
			if (info.hasLocalRigParts)
			{
				for (int num5 = 0; num5 < info.localRigParts.Length; num5++)
				{
					CosmeticPart cosmeticPart4 = info.localRigParts[num5];
					if (!cosmeticPart4.prefabAssetRef.RuntimeKeyIsValid())
					{
						GTDev.LogError<string>("Cosmetic " + info.displayName + " has missing object reference in local rig parts, skipping load", null);
					}
					else
					{
						CosmeticsV2Spawner_Dirty.AddEachAttachInfoToLoadOpInfosList(cosmeticPart4, num5, info, num, ref num4);
					}
				}
			}
		}
		TickSystem<object>.AddTickCallback(CosmeticsV2Spawner_Dirty._instance);
	}

	private static void AddEachAttachInfoToLoadOpInfosList(CosmeticPart part, int partIndex, CosmeticInfoV2 cosmeticInfo, int vrRigIndex, ref int partCount)
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		for (int i = 0; i < part.attachAnchors.Length; i++)
		{
			CosmeticsV2Spawner_Dirty.LoadOpInfo loadOpInfo = new CosmeticsV2Spawner_Dirty.LoadOpInfo(part.attachAnchors[i], part, partIndex, cosmeticInfo, vrRigIndex);
			CosmeticsV2Spawner_Dirty._g_loadOpInfos.Add(loadOpInfo);
			partCount++;
			if (part.partType == ECosmeticPartType.Holdable && i == 0)
			{
				break;
			}
		}
	}

	private static void _Step2_UpdateLoadOpStarting()
	{
		int num = CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Count - CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted;
		while (CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Count < CosmeticsV2Spawner_Dirty._g_loadOpInfos.Count && num < 1000000)
		{
			num++;
			int count = CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Count;
			CosmeticsV2Spawner_Dirty.LoadOpInfo loadOpInfo = CosmeticsV2Spawner_Dirty._g_loadOpInfos[count];
			try
			{
				loadOpInfo.loadOp = loadOpInfo.part.prefabAssetRef.InstantiateAsync(CosmeticsV2Spawner_Dirty._gDeactivatedSpawnParent, false);
				loadOpInfo.isStarted = true;
				CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Add(loadOpInfo.loadOp, count);
				loadOpInfo.loadOp.Completed += CosmeticsV2Spawner_Dirty._Step3_HandleLoadOpCompleted;
				CosmeticsV2Spawner_Dirty._g_loadOpInfos[count] = loadOpInfo;
			}
			catch (InvalidKeyException ex)
			{
				Debug.LogError("CosmeticsV2Spawner_Dirty: Missing Addressable for " + string.Format("\"{0}\" part index {1}. Skipping. {2}", loadOpInfo.cosmeticInfoV2.displayName, loadOpInfo.partIndex, ex.Message));
				loadOpInfo.isStarted = true;
				loadOpInfo.resultGObj = null;
				CosmeticsV2Spawner_Dirty._g_loadOpInfos[count] = loadOpInfo;
				CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted++;
				num--;
			}
			catch (ArgumentException ex2)
			{
				Debug.LogError("CosmeticsV2Spawner_Dirty: Invalid Addressable key/config for " + string.Format("\"{0}\" part index {1}. Skipping. {2}", loadOpInfo.cosmeticInfoV2.displayName, loadOpInfo.partIndex, ex2.Message));
				loadOpInfo.isStarted = true;
				loadOpInfo.resultGObj = null;
				CosmeticsV2Spawner_Dirty._g_loadOpInfos[count] = loadOpInfo;
				CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted++;
				num--;
			}
		}
	}

	private static void _Step3_HandleLoadOpCompleted(AsyncOperationHandle<GameObject> loadOp)
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		int num;
		if (!CosmeticsV2Spawner_Dirty._g_loadOp_to_index.TryGetValue(loadOp, out num))
		{
			throw new Exception("(this should never happen) could not find LoadOpInfo in `_g_loadOpInfos`.");
		}
		CosmeticsV2Spawner_Dirty.LoadOpInfo loadOpInfo = CosmeticsV2Spawner_Dirty._g_loadOpInfos[num];
		if (loadOp.Status == AsyncOperationStatus.Failed)
		{
			Debug.LogWarning("CosmeticsV2Spawner_Dirty: Failed to load part " + string.Format("\"{0}\" (key: {1}). Skipping.", loadOpInfo.cosmeticInfoV2.displayName, loadOpInfo.part.prefabAssetRef.RuntimeKey));
			CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted++;
			CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Remove(loadOp);
			return;
		}
		CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted++;
		ECosmeticSelectSide ecosmeticSelectSide = loadOpInfo.attachInfo.selectSide;
		string text = loadOpInfo.cosmeticInfoV2.playFabID;
		if (ecosmeticSelectSide != ECosmeticSelectSide.Both)
		{
			string playFabID = loadOpInfo.cosmeticInfoV2.playFabID;
			string text2;
			if (ecosmeticSelectSide != ECosmeticSelectSide.Left)
			{
				if (ecosmeticSelectSide != ECosmeticSelectSide.Right)
				{
					text2 = "";
				}
				else
				{
					text2 = " RIGHT.";
				}
			}
			else
			{
				text2 = " LEFT.";
			}
			text = ZString.Concat<string, string>(playFabID, text2);
		}
		loadOpInfo.resultGObj = loadOp.Result;
		loadOpInfo.resultGObj.SetActive(false);
		Transform transform = loadOpInfo.resultGObj.transform;
		Transform transform2 = transform;
		CosmeticPart[] holdableParts = loadOpInfo.cosmeticInfoV2.holdableParts;
		if (holdableParts != null && holdableParts.Length > 0)
		{
			TransferrableObject componentInChildren = loadOpInfo.resultGObj.GetComponentInChildren<TransferrableObject>(true);
			if (componentInChildren && componentInChildren.gameObject != loadOpInfo.resultGObj)
			{
				transform2 = componentInChildren.transform;
				transform2.gameObject.SetActive(false);
				loadOpInfo.resultGObj.SetActive(true);
			}
		}
		if (loadOpInfo.cosmeticInfoV2.isThrowable)
		{
			SnowballThrowable componentInChildren2 = loadOpInfo.resultGObj.GetComponentInChildren<SnowballThrowable>(true);
			if (componentInChildren2 && componentInChildren2.gameObject != loadOpInfo.resultGObj)
			{
				transform2 = componentInChildren2.transform;
				transform2.gameObject.SetActive(false);
				loadOpInfo.resultGObj.SetActive(true);
			}
		}
		transform2.name = text;
		CosmeticsV2Spawner_Dirty.VRRigData vrrigData = ((loadOpInfo.vrRigIndex != -1) ? CosmeticsV2Spawner_Dirty._gVRRigDatas[loadOpInfo.vrRigIndex] : default(CosmeticsV2Spawner_Dirty.VRRigData));
		Transform transform3;
		switch (loadOpInfo.part.partType)
		{
		case ECosmeticPartType.Holdable:
			transform3 = ((loadOpInfo.attachInfo.parentBone != GTHardCodedBones.EBone.body_AnchorFront_StowSlot) ? vrrigData.parentOfDeactivatedHoldables : vrrigData.boneXforms[(int)loadOpInfo.attachInfo.parentBone]);
			goto IL_02C1;
		case ECosmeticPartType.Functional:
			transform3 = vrrigData.boneXforms[(int)loadOpInfo.attachInfo.parentBone];
			goto IL_02C1;
		case ECosmeticPartType.FirstPerson:
			transform3 = CosmeticsV2Spawner_Dirty.g_gorillaPlayer.CosmeticsHeadTarget;
			goto IL_02C1;
		case ECosmeticPartType.LocalRig:
			transform3 = vrrigData.boneXforms[(int)loadOpInfo.attachInfo.parentBone];
			goto IL_02C1;
		}
		throw new ArgumentOutOfRangeException("partType", "unhandled part type.");
		IL_02C1:
		Transform transform4 = transform3;
		if (transform4)
		{
			transform.SetParent(transform4, false);
			transform.localPosition = loadOpInfo.attachInfo.offset.pos;
			Transform transform5 = transform;
			XformOffset offset = loadOpInfo.attachInfo.offset;
			transform5.localRotation = offset.rot;
			transform.localScale = loadOpInfo.attachInfo.offset.scale;
		}
		else
		{
			Debug.LogError(string.Concat(new string[]
			{
				string.Format("Bone transform not found for cosmetic part type {0}. Cosmetic: ", loadOpInfo.part.partType),
				"\"",
				loadOpInfo.cosmeticInfoV2.displayName,
				"\",",
				string.Format("part: \"{0}\"", loadOpInfo.part.prefabAssetRef.RuntimeKey)
			}));
		}
		switch (loadOpInfo.part.partType)
		{
		case ECosmeticPartType.Holdable:
		{
			vrrigData.vrRig_cosmetics.Add(transform2.gameObject);
			HoldableObject componentInChildren3 = loadOpInfo.resultGObj.GetComponentInChildren<HoldableObject>(true);
			SnowballThrowable snowballThrowable = componentInChildren3 as SnowballThrowable;
			if (snowballThrowable != null)
			{
				CosmeticsV2Spawner_Dirty.AddPartToThrowableLists(loadOpInfo, snowballThrowable);
				goto IL_0617;
			}
			TransferrableObject transferrableObject = componentInChildren3 as TransferrableObject;
			if (transferrableObject == null)
			{
				if (componentInChildren3 != null)
				{
					throw new Exception("Encountered unexpected HoldableObject derived type on cosmetic part: \"" + loadOpInfo.cosmeticInfoV2.displayName + "\"");
				}
				goto IL_0617;
			}
			else
			{
				vrrigData.bdPositions_allObjects.Add(transferrableObject);
				string text3 = loadOpInfo.cosmeticInfoV2.playFabID;
				int[] array;
				if (CosmeticsLegacyV1Info.TryGetBodyDockAllObjectsIndexes(text3, out array))
				{
					if (loadOpInfo.partIndex < array.Length && loadOpInfo.partIndex >= 0)
					{
						transferrableObject.myIndex = array[loadOpInfo.partIndex];
					}
				}
				else if (text3.Length >= 5 && text3[0] == 'L')
				{
					if (text3[1] != 'M')
					{
						throw new Exception("(this should never happen) A TransferrableObject cosmetic added sometime after 2024-06 does not use the expected PlayFabID format where the string starts with \"LM\" and ends with \".\". Path: " + transform2.GetPathQ());
					}
					string text4 = text3;
					text3 = ((text4[text4.Length - 1] == '.') ? text3 : (text3 + "."));
					int num2 = 224;
					transferrableObject.myIndex = num2 + CosmeticIDUtils.PlayFabIdToIndexInCategory(text3);
				}
				else
				{
					transferrableObject.myIndex = -2;
					if (!(text3 == "STICKABLE TARGET"))
					{
						Debug.LogError(string.Concat(new string[]
						{
							"Cosmetic \"",
							loadOpInfo.cosmeticInfoV2.displayName,
							"\" cannot derive `TransferrableObject.myIndex` from playFabId \"",
							text3,
							"\" and so will not be included in `BodyDockPositions.allObjects` array."
						}));
					}
				}
				vrrigData.bdPositions_allObjects_length = math.max(transferrableObject.myIndex + 1, vrrigData.bdPositions_allObjects_length);
				ProjectileWeapon projectileWeapon = transferrableObject as ProjectileWeapon;
				if (projectileWeapon != null && loadOpInfo.cosmeticInfoV2.playFabID == "Slingshot")
				{
					vrrigData.vrRig.projectileWeapon = projectileWeapon;
					goto IL_0617;
				}
				goto IL_0617;
			}
			break;
		}
		case ECosmeticPartType.Functional:
			vrrigData.vrRig_cosmetics.Add(transform2.gameObject);
			goto IL_0617;
		case ECosmeticPartType.FirstPerson:
		case ECosmeticPartType.LocalRig:
			vrrigData.vrRig_override.Add(transform2.gameObject);
			goto IL_0617;
		}
		throw new ArgumentOutOfRangeException("Unexpected ECosmeticPartType value encountered: " + string.Format("{0}, ", loadOpInfo.part.partType) + string.Format("int: {0}.", (int)loadOpInfo.part.partType));
		IL_0617:
		if (loadOpInfo.vrRigIndex > -1)
		{
			CosmeticsV2Spawner_Dirty._gVRRigDatas[loadOpInfo.vrRigIndex] = vrrigData;
		}
		CosmeticRefRegistry cosmeticReferences = CosmeticsV2Spawner_Dirty._gVRRigDatas[loadOpInfo.vrRigIndex].vrRig.cosmeticReferences;
		CosmeticRefTarget[] componentsInChildren = loadOpInfo.resultGObj.GetComponentsInChildren<CosmeticRefTarget>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			cosmeticReferences.Register(componentsInChildren[i].id, componentsInChildren[i].gameObject);
		}
		CosmeticsV2Spawner_Dirty._g_loadOpInfos[num] = loadOpInfo;
		if (CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted < CosmeticsV2Spawner_Dirty._g_loadOpInfos.Count)
		{
			return;
		}
		CosmeticsV2Spawner_Dirty._Step4_PopulateAllArrays();
	}

	private static void _RetryDownload(int loadOpIndex)
	{
		if (loadOpIndex < 0 || loadOpIndex >= CosmeticsV2Spawner_Dirty._g_loadOpInfos.Count)
		{
			Debug.LogError("(should never happen) Unexpected! While trying to recover from a failed download, the value " + string.Format("{0}={1} was out of range of ", "loadOpIndex", loadOpIndex) + string.Format("{0}.Count={1}.", "_g_loadOpInfos", CosmeticsV2Spawner_Dirty._g_loadOpInfos.Count));
			return;
		}
		CosmeticsV2Spawner_Dirty.LoadOpInfo loadOpInfo = CosmeticsV2Spawner_Dirty._g_loadOpInfos[loadOpIndex];
		if (!CosmeticsV2Spawner_Dirty._g_loadOp_to_index.Remove(loadOpInfo.loadOp))
		{
			Debug.LogWarning(string.Concat(new string[]
			{
				"(should never happen) Unexpected! Could not find the loadOp to remove it in the _g_loadOp_to_index. If you see this message then comparison does not work the way I thought and we need a different way to store/retrieve loadOpInfos. Happened while trying to retry failed download prefab part of cosmetic \"",
				loadOpInfo.cosmeticInfoV2.displayName,
				"\" with guid \"",
				loadOpInfo.part.prefabAssetRef.AssetGUID,
				"\"."
			}));
		}
		Debug.Log(string.Concat(new string[]
		{
			"Retrying prefab part of cosmetic \"",
			loadOpInfo.cosmeticInfoV2.displayName,
			"\" with guid \"",
			loadOpInfo.part.prefabAssetRef.AssetGUID,
			"\"."
		}));
		loadOpInfo.loadOp = loadOpInfo.part.prefabAssetRef.InstantiateAsync(CosmeticsV2Spawner_Dirty._gDeactivatedSpawnParent, false);
		CosmeticsV2Spawner_Dirty._g_loadOpInfos[loadOpIndex] = loadOpInfo;
		CosmeticsV2Spawner_Dirty._g_loadOp_to_index[loadOpInfo.loadOp] = loadOpIndex;
		loadOpInfo.loadOp.Completed += CosmeticsV2Spawner_Dirty._Step3_HandleLoadOpCompleted;
	}

	private static void AddPartToThrowableLists(CosmeticsV2Spawner_Dirty.LoadOpInfo loadOpInfo, SnowballThrowable throwable)
	{
		CosmeticsV2Spawner_Dirty.VRRigData vrrigData = CosmeticsV2Spawner_Dirty._gVRRigDatas[loadOpInfo.vrRigIndex];
		EHandedness handednessFromBone = GTHardCodedBones.GetHandednessFromBone(loadOpInfo.attachInfo.parentBone);
		bool flag = vrrigData.vrRig == CosmeticsV2Spawner_Dirty._gVRRigDatas[0].vrRig;
		throwable.SpawnOffset = loadOpInfo.attachInfo.offset;
		switch (handednessFromBone)
		{
		case EHandedness.None:
			throw new ArgumentException(string.Concat(new string[]
			{
				"Encountered throwable cosmetic \"",
				loadOpInfo.cosmeticInfoV2.displayName,
				"\" where handedness ",
				string.Format("could not be determined from bone `{0}`. ", loadOpInfo.attachInfo.parentBone),
				"Path: \"",
				throwable.transform.GetPath(),
				"\""
			}));
		case EHandedness.Left:
			CosmeticsV2Spawner_Dirty.ResizeAndSetAtIndex<GameObject>(vrrigData.bdPositions_leftHandThrowables, throwable.gameObject, throwable.throwableMakerIndex);
			if (flag)
			{
				CosmeticsV2Spawner_Dirty.ResizeAndSetAtIndex<SnowballThrowable>(CosmeticsV2Spawner_Dirty._gSnowballMakerLeft_throwables, throwable, throwable.throwableMakerIndex);
				return;
			}
			break;
		case EHandedness.Right:
			CosmeticsV2Spawner_Dirty.ResizeAndSetAtIndex<GameObject>(vrrigData.bdPositions_rightHandThrowables, throwable.gameObject, throwable.throwableMakerIndex);
			if (flag)
			{
				CosmeticsV2Spawner_Dirty.ResizeAndSetAtIndex<SnowballThrowable>(CosmeticsV2Spawner_Dirty._gSnowballMakerRight_throwables, throwable, throwable.throwableMakerIndex);
				return;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("Unexpected ECosmeticSelectSide value encountered: " + string.Format("{0}, ", handednessFromBone) + string.Format("int: {0}.", (int)handednessFromBone));
		}
	}

	private static void ResizeAndSetAtIndex<T>(List<T> list, T item, int index)
	{
		if (index >= list.Count)
		{
			int num = index - list.Count + 1;
			for (int i = 0; i < num; i++)
			{
				list.Add(default(T));
			}
		}
		list[index] = item;
	}

	private static void _Step4_PopulateAllArrays()
	{
		if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			Debug.LogError("_Step4_PopulateAllArrays: (should never happen) CALLED MORE THAN ONCE!");
			return;
		}
		foreach (CosmeticsV2Spawner_Dirty.LoadOpInfo loadOpInfo in CosmeticsV2Spawner_Dirty._g_loadOpInfos)
		{
			if (!(loadOpInfo.resultGObj == null))
			{
				ISpawnable[] componentsInChildren = loadOpInfo.resultGObj.GetComponentsInChildren<ISpawnable>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					try
					{
						componentsInChildren[i].IsSpawned = true;
						componentsInChildren[i].CosmeticSelectedSide = loadOpInfo.attachInfo.selectSide;
						componentsInChildren[i].OnSpawn(CosmeticsV2Spawner_Dirty._gVRRigDatas[loadOpInfo.vrRigIndex].vrRig);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}
		CosmeticsV2Spawner_Dirty._gSnowballMakerLeft.SetupThrowables(CosmeticsV2Spawner_Dirty._gSnowballMakerLeft_throwables.ToArray());
		CosmeticsV2Spawner_Dirty._gSnowballMakerRight.SetupThrowables(CosmeticsV2Spawner_Dirty._gSnowballMakerRight_throwables.ToArray());
		foreach (CosmeticsV2Spawner_Dirty.VRRigData vrrigData in CosmeticsV2Spawner_Dirty._gVRRigDatas)
		{
			vrrigData.vrRig.cosmetics = vrrigData.vrRig_cosmetics.ToArray();
			vrrigData.vrRig.overrideCosmetics = vrrigData.vrRig_override.ToArray();
			vrrigData.bdPositionsComp.leftHandThrowables = vrrigData.bdPositions_leftHandThrowables.ToArray();
			vrrigData.bdPositionsComp.rightHandThrowables = vrrigData.bdPositions_rightHandThrowables.ToArray();
			vrrigData.bdPositionsComp._allObjects = new TransferrableObject[vrrigData.bdPositions_allObjects_length];
			foreach (TransferrableObject transferrableObject in vrrigData.bdPositions_allObjects)
			{
				if (transferrableObject.myIndex >= 0 && transferrableObject.myIndex < vrrigData.bdPositions_allObjects_length)
				{
					vrrigData.bdPositionsComp._allObjects[transferrableObject.myIndex] = transferrableObject;
				}
			}
		}
		CosmeticsV2Spawner_Dirty.allPartsInstantiated = true;
		GTDelayedExec.Add(CosmeticsV2Spawner_Dirty._instance, 1f, -Mathf.Abs("_Step5_InitializeVRRigsAndCosmeticsControllerFinalize".GetHashCode()));
	}

	private static void _Step5_InitializeVRRigsAndCosmeticsControllerFinalize()
	{
		CosmeticsController.instance.UpdateWardrobeModelsAndButtons();
		try
		{
			Action onPostInstantiateAllPrefabs = CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs;
			if (onPostInstantiateAllPrefabs != null)
			{
				onPostInstantiateAllPrefabs();
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		try
		{
			CosmeticsController.instance.InitializeCosmeticStands();
		}
		catch (Exception ex2)
		{
			Debug.LogException(ex2);
		}
		try
		{
			Action onPostInstantiateAllPrefabs2 = CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2;
			if (onPostInstantiateAllPrefabs2 != null)
			{
				onPostInstantiateAllPrefabs2();
			}
		}
		catch (Exception ex3)
		{
			Debug.LogException(ex3);
		}
		try
		{
			CosmeticsController.instance.UpdateWornCosmetics(false);
		}
		catch (Exception ex4)
		{
			Debug.LogException(ex4);
		}
		foreach (CosmeticsV2Spawner_Dirty.VRRigData vrrigData in CosmeticsV2Spawner_Dirty._gVRRigDatas)
		{
			try
			{
				if (vrrigData.bdPositionsComp.isActiveAndEnabled)
				{
					vrrigData.bdPositionsComp.RefreshTransferrableItems();
				}
			}
			catch (Exception ex5)
			{
				Debug.LogException(ex5, vrrigData.vrRig);
			}
		}
		try
		{
			StoreController.instance.InitalizeCosmeticStands();
		}
		catch (Exception ex6)
		{
			Debug.LogException(ex6);
		}
		CosmeticsV2Spawner_Dirty.completed = true;
		CosmeticsV2Spawner_Dirty.k_stopwatch.Stop();
		Debug.Log("_Step5_InitializeVRRigsAndCosmeticsControllerFinalize" + string.Format(": Done instantiating cosmetics in {0:0.0000} seconds.", (double)CosmeticsV2Spawner_Dirty.k_stopwatch.ElapsedMilliseconds / 1000.0));
	}

	private void _DelayedStatusCheck()
	{
		int count = CosmeticsV2Spawner_Dirty._g_loadOpInfos.Count;
		Debug.Log(ZString.Concat<string, string, string, string, double, string, int, string, int, string>("CosmeticsV2Spawner_Dirty", ".", "_DelayedStatusCheck", ": Load progress ", (double)CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted / (double)count * 100.0, "% (", CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted, "/", count, ")."));
		if (CosmeticsV2Spawner_Dirty._g_loadOpsCountCompleted < count)
		{
			GTDelayedExec.Add(this, 2f, -100);
		}
	}

	private static CosmeticsV2Spawner_Dirty _instance;

	public static Action OnPostInstantiateAllPrefabs;

	public static Action OnPostInstantiateAllPrefabs2;

	[OnEnterPlay_SetNull]
	private static Transform _gDeactivatedSpawnParent;

	[OnEnterPlay_Set(0)]
	private static int _g_loadOpsCountCompleted = 0;

	private const int _k_maxActiveLoadOps = 1000000;

	private const int _k_maxTotalLoadOps = 1000000;

	private const int _k_delayedStatusCheckContextId = -100;

	[OnEnterPlay_Clear]
	private static readonly List<CosmeticsV2Spawner_Dirty.LoadOpInfo> _g_loadOpInfos = new List<CosmeticsV2Spawner_Dirty.LoadOpInfo>(100000);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<AsyncOperationHandle<GameObject>, int> _g_loadOp_to_index = new Dictionary<AsyncOperationHandle<GameObject>, int>(100000);

	[OnEnterPlay_SetNull]
	private static SnowballMaker _gSnowballMakerLeft;

	[OnEnterPlay_Clear]
	private static readonly List<SnowballThrowable> _gSnowballMakerLeft_throwables = new List<SnowballThrowable>(20);

	[OnEnterPlay_SetNull]
	private static SnowballMaker _gSnowballMakerRight;

	[OnEnterPlay_Clear]
	private static readonly List<SnowballThrowable> _gSnowballMakerRight_throwables = new List<SnowballThrowable>(20);

	[OnEnterPlay_SetNull]
	private static GTPlayer g_gorillaPlayer;

	[OnEnterPlay_SetNull]
	private static Transform[] g_allInstantiatedParts;

	private static Stopwatch k_stopwatch = new Stopwatch();

	[OnEnterPlay_Clear]
	private static readonly List<CosmeticsV2Spawner_Dirty.VRRigData> _gVRRigDatas = new List<CosmeticsV2Spawner_Dirty.VRRigData>(11);

	private bool _shouldTick;

	private struct LoadOpInfo
	{
		public LoadOpInfo(CosmeticAttachInfo attachInfo, CosmeticPart part, int partIndex, CosmeticInfoV2 cosmeticInfoV2, int vrRigIndex)
		{
			this.isStarted = false;
			this.loadOp = default(AsyncOperationHandle<GameObject>);
			this.resultGObj = null;
			this.attachInfo = attachInfo;
			this.part = part;
			this.partIndex = partIndex;
			this.cosmeticInfoV2 = cosmeticInfoV2;
			this.vrRigIndex = vrRigIndex;
		}

		public bool isStarted;

		public AsyncOperationHandle<GameObject> loadOp;

		public GameObject resultGObj;

		public readonly CosmeticAttachInfo attachInfo;

		public readonly CosmeticPart part;

		public readonly int partIndex;

		public readonly CosmeticInfoV2 cosmeticInfoV2;

		public readonly int vrRigIndex;
	}

	private struct VRRigData
	{
		public VRRigData(VRRig vrRig, Transform[] boneXforms)
		{
			this.vrRig = vrRig;
			this.boneXforms = boneXforms;
			if (!vrRig.transform.TryFindByPath("./**/Holdables", out this.parentOfDeactivatedHoldables, false))
			{
				Debug.LogError("Could not find parent for deactivated holdables. Falling back to VRRig transform: \"" + vrRig.transform.GetPath() + "\"");
			}
			this.bdPositionsComp = vrRig.GetComponentInChildren<BodyDockPositions>(true);
			this.vrRig_cosmetics = new List<GameObject>(500);
			this.vrRig_override = new List<GameObject>(500);
			this.bdPositions_leftHandThrowables = new List<GameObject>(20);
			this.bdPositions_rightHandThrowables = new List<GameObject>(20);
			this.bdPositions_allObjects = new List<TransferrableObject>(20);
			this.bdPositions_allObjects_length = 0;
		}

		public readonly VRRig vrRig;

		public readonly Transform[] boneXforms;

		public readonly BodyDockPositions bdPositionsComp;

		public readonly List<GameObject> vrRig_cosmetics;

		public readonly List<GameObject> vrRig_override;

		public readonly Transform parentOfDeactivatedHoldables;

		public readonly List<TransferrableObject> bdPositions_allObjects;

		public int bdPositions_allObjects_length;

		public readonly List<GameObject> bdPositions_leftHandThrowables;

		public readonly List<GameObject> bdPositions_rightHandThrowables;
	}
}
