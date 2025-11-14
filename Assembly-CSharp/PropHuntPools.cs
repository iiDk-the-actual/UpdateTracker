using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using PlayFab;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class PropHuntPools
{
	public static PropHuntPools.EState State
	{
		get
		{
			return PropHuntPools._state;
		}
	}

	public static bool IsReady
	{
		get
		{
			return PropHuntPools.State == PropHuntPools.EState.Ready;
		}
	}

	public static string[] AllPropCosmeticIds
	{
		get
		{
			return PropHuntPools._allPropCosmeticIds;
		}
	}

	public static void StartInitializingPropsList(AllCosmeticsArraySO allCosmeticsArraySO, CosmeticSO fallbackCosmeticSO)
	{
		if (PropHuntPools._state != PropHuntPools.EState.None)
		{
			return;
		}
		PropHuntPools._ResetPool<PropPlacementRB>(PropHuntPools._cosmeticId_to_decoyTemplate, PropHuntPools._cosmeticId_to_inactiveDecoys, PropHuntPools._activeDecoy_to_cosmeticId);
		PropHuntPools._ResetPool<PropHuntGrabbableProp>(PropHuntPools._cosmeticId_to_grabbableTemplate, PropHuntPools._cosmeticId_to_inactiveGrabbables, PropHuntPools._activeGrabbable_to_cosmeticId);
		PropHuntPools._ResetPool<PropHuntTaggableProp>(PropHuntPools._cosmeticId_to_taggableTemplate, PropHuntPools._cosmeticId_to_inactiveTaggables, PropHuntPools._activeTaggable_to_cosmeticId);
		PropHuntPools._propCosmeticIdsWaitingToLoad.Clear();
		PropHuntPools._allCosmeticsArraySO = allCosmeticsArraySO;
		PropHuntPools._CreateInactivePropsParent(ref PropHuntPools._decoyTemplatesParent, "_decoyTemplatesParent");
		PropHuntPools._CreateInactivePropsParent(ref PropHuntPools._decoyInactivePropsParent, "_decoyInactivePropsParent");
		PropHuntPools._CreateInactivePropsParent(ref PropHuntPools._grabbableTemplatesParent, "_grabbableTemplatesParent");
		PropHuntPools._CreateInactivePropsParent(ref PropHuntPools._grabbableInactivePropsParent, "_grabbableInactivePropsParent");
		PropHuntPools._CreateInactivePropsParent(ref PropHuntPools._taggableTemplatesParent, "_taggableTemplatesParent");
		PropHuntPools._CreateInactivePropsParent(ref PropHuntPools._taggableInactivePropsParent, "_taggableInactivePropsParent");
		PropHuntPools._state = PropHuntPools.EState.WaitingForTitleData;
		PropHuntPools._fallbackProp_cosmeticSO = fallbackCosmeticSO;
		PropHuntPools_Callbacks.instance.ListenForZoneChanged();
		PlayFabTitleDataCache.Instance.GetTitleData("PropHuntProps", new Action<string>(PropHuntPools._HandleOnTitleDataPropsListLoaded), delegate(PlayFabError e)
		{
			Debug.LogError("ERROR!!!  PropHuntPools: Falling back to interal list because title data with \"PropHuntProps\" failed to load. Error: " + ((e != null) ? e.ToString() : null));
			PropHuntPools._HandleOnTitleDataPropsListLoaded("\r\nLMAOY.\r\nLMAKL.\r\nLHABU.\r\nLFAEA.\r\nLMAHO.\r\nLMACQ.\r\nLFAFI.\r\nLHADA.\r\nLMALH.\r\nLFAFA.\r\nLHAED.\r\nLHAHD.\r\nLMAQW.\r\nLFAAW.\r\nLHAFS.\r\nLFABK.\r\nLMAMC.\r\nLMAOX.\r\nLHAFP.\r\nLHACO.\r\nLMAIG.\r\nLMAQG.\r\nLFADC.\r\nLMALL.\r\nLFACA.\r\nLMALO.\r\nLHAFK.\r\nLHAGK.\r\nLMAMI.\r\nLBAAP.\r\nLHAIB.\r\nLHACD.\r\nLFACF.\r\nLMAED.\r\nLMACL.\r\nLHABP.\r\nLMAER.\r\nLMAMR.\r\nLHAFD.\r\nLHACH.\r\nLHADQ.\r\nLMAOP.\r\nLMALY.\r\nLMAHD.\r\nLHAAZ.\r\nLHAGJ.\r\nLMACI.\r\nLFAAO.\r\nLHAEX.\r\nLFAFW.\r\nLMAFR.\r\nLFABC.\r\nLMAKR.\r\nLHAAH.\r\nLHABD.\r\nLHAAB.\r\nLMAKJ.\r\nLHABB.\r\nLHAEN.\r\nLMAGK.\r\nLMAGZ.\r\nLHADR.\r\nLHAFB.\r\nLMAOD.\r\nLHAHT.\r\nLFAFQ.\r\nLFAFP.\r\nLMAOQ.\r\nLMALZ.\r\nLBAAV.\r\nLHAFM.\r\nLHACT.\r\nLFACN.\r\nLHAGH.\r\nLMAJH.\r\nLHADG.\r\nLMAPH.\r\nLHAIU.\r\nLHADW.\r\nLHACJ.\r\nLMAFH.\r\nLHACP.\r\nLFABJ.\r\nLMAQE.\r\nLHAAE.\r\nLMAGW.\r\nLHAFV.\r\nLMABS.\r\nLMADN.\r\nLHAAR.\r\nLMAOL.\r\nLMABM.\r\nLHAEK.\r\nLHAJG.\r\nLFACD.\r\nLHAIE.\r\nLMAPQ.\r\nLFAEZ.\r\nLFABE.\r\nLMAJI.\r\nLFAAU.\r\nLHACX.\r\nLHADP.\r\nLMAGI.\r\nLMAAK.\r\nLFAAJ.\r\nLMACP.\r\nLFADR.\r\nLMAEK.\r\nLMACT.\r\nLMAJS.\r\nLMADH.\r\nLMALI.\r\nLMAEG.\r\nLMAKE.\r\nLMALE.\r\nLHACW.\r\nLMAGS.\r\nLFAEM.\r\nLHAEA.\r\nLHAFU.\r\nLHADL.\r\nLMAGR.\r\nLHAAD.\r\nLHAAU.\r\nLMAFD.\r\nLHAIF.\r\nLHACZ.\r\nLHAIJ.\r\nLHABV.\r\nLMAEM.\r\nLMAKF.\r\nLHAII.\r\nLMALU.\r\nLFAAF.\r\nLHAHR.\r\nLHABJ.\r\nLMAGJ.\r\nLMAHA.\r\nLHAHA.\r\nLFACE.\r\nLMAQR.\r\nLMABK.\r\nLHAGQ.\r\nLHABQ.\r\nLHAEQ.\r\nLMAKD.\r\nLFAHA.\r\nLHAGS.\r\nLMAFF.\r\nLMACW.\r\n");
		}, false);
	}

	private static void _ResetPool<T>(Dictionary<string, T> cosmeticId_to_propTemplate, Dictionary<string, Queue<T>> cosmeticId_to_inactiveProps, Dictionary<T, string> activeProp_to_cosmeticId) where T : Component
	{
		foreach (T t in cosmeticId_to_propTemplate.Values)
		{
			if (t != null)
			{
				Object.Destroy(t);
			}
		}
		cosmeticId_to_propTemplate.Clear();
		foreach (Queue<T> queue in cosmeticId_to_inactiveProps.Values)
		{
			foreach (T t2 in queue)
			{
				if (t2 != null)
				{
					Object.Destroy(t2.gameObject);
				}
			}
		}
		cosmeticId_to_inactiveProps.Clear();
		foreach (T t3 in activeProp_to_cosmeticId.Keys)
		{
			if (t3 != null)
			{
				Object.Destroy(t3.gameObject);
			}
		}
		activeProp_to_cosmeticId.Clear();
	}

	private static void _CreateInactivePropsParent(ref Transform _inactivePropsParent, string name)
	{
		if (_inactivePropsParent != null)
		{
			Object.Destroy(_inactivePropsParent.gameObject);
		}
		_inactivePropsParent = new GameObject("__PropHunt_-_" + name + "__").transform;
		_inactivePropsParent.gameObject.SetActive(false);
		Object.DontDestroyOnLoad(_inactivePropsParent);
		_inactivePropsParent.gameObject.isStatic = true;
	}

	private static void _HandleOnTitleDataPropsListLoaded(string titleDataPropsString)
	{
		PropHuntPools._allPropCosmeticIds = titleDataPropsString.Split(PropHuntPools._g_ph_titleDataSeparators, StringSplitOptions.RemoveEmptyEntries);
		PropHuntPools._propCosmeticIdsWaitingToLoad.UnionWith(PropHuntPools._allPropCosmeticIds);
		string playFabID = PropHuntPools._fallbackProp_cosmeticSO.info.playFabID;
		PropHuntPools._propCosmeticIdsWaitingToLoad.Add(playFabID);
		int num = 0;
		PropHuntPools._propCosmeticIds_uniqueArray = new string[PropHuntPools._propCosmeticIdsWaitingToLoad.Count];
		foreach (string text in PropHuntPools._propCosmeticIdsWaitingToLoad)
		{
			int num2 = 0;
			for (int i = 0; i < PropHuntPools._allPropCosmeticIds.Length; i++)
			{
				num2 += ((text == PropHuntPools._allPropCosmeticIds[i]) ? 1 : 0);
			}
			PropHuntPools._cosmeticId_to_decoyInitialCount[text] = num2 * 10;
			PropHuntPools._propCosmeticIds_uniqueArray[num] = text;
			num++;
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = PropHuntPools._fallbackProp_cosmeticSO.info.wardrobeParts[0].prefabAssetRef.InstantiateAsync(PropHuntPools._decoyTemplatesParent, false);
		asyncOperationHandle.WaitForCompletion();
		PropHuntPools._HandleOnPropTemplateLoaded(asyncOperationHandle, playFabID, PropHuntPools._fallbackProp_cosmeticSO);
		PropHuntPools._state_isTitleDataLoaded = true;
		PropHuntPools._state_hasLocalPlayerVisitedBayou |= VRRigCache.Instance != null && VRRigCache.Instance.localRig != null && VRRigCache.Instance.localRig.Rig.zoneEntity.currentZone == GTZone.bayou;
		if (PropHuntPools._state_hasLocalPlayerVisitedBayou)
		{
			PropHuntPools.StartCreatingPools();
			return;
		}
		PropHuntPools._state = PropHuntPools.EState.WaitingForLocalPlayerToVisitBayou;
	}

	internal static void OnLocalPlayerEnteredBayou()
	{
		if (!PropHuntPools._state_hasLocalPlayerVisitedBayou)
		{
			PropHuntPools._state_hasLocalPlayerVisitedBayou = true;
			if (PropHuntPools._state_isTitleDataLoaded)
			{
				PropHuntPools.StartCreatingPools();
			}
		}
	}

	internal static void StartCreatingPools()
	{
		PropHuntPools._state = PropHuntPools.EState.SpawningProps;
		for (int i = 0; i < PropHuntPools._propCosmeticIds_uniqueArray.Length; i++)
		{
			string cosmeticId = PropHuntPools._propCosmeticIds_uniqueArray[i];
			CosmeticSO cosmeticSO = PropHuntPools._allCosmeticsArraySO.SearchForCosmeticSO(cosmeticId);
			PropHuntPools.propCosmeticId_to_cosmeticSO[cosmeticId] = cosmeticSO;
			if (cosmeticSO == null)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"ERROR!!!  PropHuntPools: CosmeticId \"",
					cosmeticId,
					"\" from title data does not exist in AllCosmeticsArraySO. Using backup \"",
					PropHuntPools._fallbackProp_cosmeticSO.name,
					"\" instead."
				}));
				cosmeticSO = PropHuntPools._fallbackProp_cosmeticSO;
			}
			else
			{
				CosmeticPart[] wardrobeParts = cosmeticSO.info.wardrobeParts;
				if (wardrobeParts == null || wardrobeParts.Length <= 0)
				{
					Debug.LogError(string.Concat(new string[]
					{
						"ERROR!!!  PropHuntPools: Prop \"",
						cosmeticSO.name,
						"\" has no wardrobeParts. Using backup \"",
						PropHuntPools._fallbackProp_cosmeticSO.name,
						"\" instead."
					}));
					cosmeticSO = PropHuntPools._fallbackProp_cosmeticSO;
				}
			}
			GTAssetRef<GameObject> prefabAssetRef = cosmeticSO.info.wardrobeParts[0].prefabAssetRef;
			Queue<PropPlacementRB> queue;
			if (!PropHuntPools._cosmeticId_to_inactiveDecoys.TryGetValue(cosmeticId, out queue))
			{
				queue = new Queue<PropPlacementRB>(10);
				PropHuntPools._cosmeticId_to_inactiveDecoys[cosmeticId] = queue;
				prefabAssetRef.InstantiateAsync(PropHuntPools._taggableTemplatesParent, false).Completed += delegate(AsyncOperationHandle<GameObject> handle)
				{
					PropHuntPools._HandleOnPropTemplateLoaded(handle, cosmeticId, cosmeticSO);
				};
			}
		}
	}

	private static void _HandleOnPropTemplateLoaded(AsyncOperationHandle<GameObject> handle, string cosmeticId, CosmeticSO cosmeticSO)
	{
		bool flag = cosmeticSO == PropHuntPools._fallbackProp_cosmeticSO;
		if (handle.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError("ERROR!!!  PropHuntPools: " + string.Format("Failed to load asset for pooling: {0} with error: {1}", cosmeticSO.name, handle.OperationException), cosmeticSO);
			return;
		}
		GameObject gameObject = handle.Result;
		if (gameObject == null)
		{
			Debug.LogError("ERROR!!!  PropHuntPools: (should never happen) Failed to load asset prop from CosmeticSO \"" + cosmeticSO.name + "\" for pooling but while async op was successful, the resulting GameObject was null!", cosmeticSO);
			return;
		}
		gameObject.SetActive(false);
		gameObject.name = "PropHunt_Prop" + cosmeticSO.name;
		gameObject.layer = 14;
		PropHuntPools._temp_meshFilters.Clear();
		foreach (Component component in gameObject.GetComponentsInChildren<Component>(true))
		{
			Transform transform = component as Transform;
			if (transform != null)
			{
				transform.gameObject.isStatic = false;
			}
			else
			{
				MeshRenderer meshRenderer = component as MeshRenderer;
				if (meshRenderer != null)
				{
					if (meshRenderer.enabled)
					{
						MeshFilter component2 = meshRenderer.GetComponent<MeshFilter>();
						if (component2 != null)
						{
							PropHuntPools._temp_meshFilters.Add(component2);
						}
					}
					else
					{
						Object.Destroy(meshRenderer);
					}
				}
				else
				{
					Object.Destroy(component);
				}
			}
		}
		if (PropHuntPools._temp_meshFilters.Count == 0)
		{
			gameObject = PropHuntPools._fallbackPrefabInstance;
		}
		List<Transform> list = new List<Transform>(gameObject.GetComponentsInChildren<Transform>(true));
		list.Sort((Transform a, Transform b) => -a.GetDepth().CompareTo(b.GetDepth()));
		Transform transform2 = gameObject.transform;
		for (int j = 0; j < list.Count; j++)
		{
			Transform transform3 = list[j];
			if (transform3.childCount == 0 && !(transform3 == transform2))
			{
				Component[] components = transform3.GetComponents<Component>();
				int num = 0;
				for (int k = 0; k < components.Length; k++)
				{
					int num2 = num;
					Component component3 = components[k];
					num = num2 + ((component3 is Transform || component3 is MeshRenderer || component3 is MeshFilter) ? 0 : 1);
				}
				if (num == 0)
				{
					Object.Destroy(transform3.gameObject);
				}
			}
		}
		if (flag && PropHuntPools._fallbackPrefabInstance == null)
		{
			PropHuntPools._fallbackPrefabInstance = Object.Instantiate<GameObject>(gameObject);
		}
		if (!PropHuntPools._cosmeticId_to_decoyTemplate.ContainsKey(cosmeticId))
		{
			PropPlacementRB propPlacementRB = Object.Instantiate<PropPlacementRB>(GorillaPropHuntGameManager.instance.PropDecoyPrefab, PropHuntPools._decoyTemplatesParent);
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, PropHuntPools._decoyTemplatesParent);
			propPlacementRB.name = "__PropHuntPoolProp_Decoy_TEMPLATE__" + cosmeticSO.name + "__";
			PropPlacementRB.TryPrepPropTemplate(propPlacementRB, gameObject2, cosmeticSO);
			propPlacementRB.gameObject.SetActive(false);
			gameObject2.transform.SetParent(propPlacementRB.transform);
			PropHuntPools._cosmeticId_to_decoyTemplate[cosmeticId] = propPlacementRB;
			int num3 = PropHuntPools._cosmeticId_to_decoyInitialCount[cosmeticId];
			Queue<PropPlacementRB> queue = new Queue<PropPlacementRB>(num3);
			string text = "__PropHuntPoolProp_Decoy__" + cosmeticSO.name + "__";
			for (int l = 0; l < num3; l++)
			{
				PropPlacementRB propPlacementRB2 = Object.Instantiate<PropPlacementRB>(propPlacementRB, PropHuntPools._decoyInactivePropsParent);
				propPlacementRB2.name = text;
				queue.Enqueue(propPlacementRB2);
			}
			PropHuntPools._cosmeticId_to_inactiveDecoys[cosmeticId] = queue;
		}
		if (!PropHuntPools._cosmeticId_to_grabbableTemplate.ContainsKey(cosmeticId))
		{
			GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject, PropHuntPools._grabbableTemplatesParent);
			List<MeshCollider> list2 = new List<MeshCollider>();
			List<InteractionPoint> list3 = new List<InteractionPoint>();
			PropHuntGrabbableProp propHuntGrabbableProp;
			PropHuntTaggableProp propHuntTaggableProp;
			PropHuntHandFollower.TryPrepPropTemplate(gameObject3, true, cosmeticSO, list2, list3, out propHuntGrabbableProp, out propHuntTaggableProp);
			propHuntGrabbableProp.name = "GrabbableProp_Template_" + cosmeticSO.name;
			PropHuntPools._cosmeticId_to_grabbableTemplate[cosmeticId] = propHuntGrabbableProp;
			Queue<PropHuntGrabbableProp> queue2 = new Queue<PropHuntGrabbableProp>(1);
			string text2 = "__PropHuntPoolProp_Grabbable__" + cosmeticSO.name + "__";
			for (int m = 0; m < 1; m++)
			{
				GameObject gameObject4 = Object.Instantiate<GameObject>(propHuntGrabbableProp.gameObject, PropHuntPools._grabbableInactivePropsParent);
				gameObject4.name = text2;
				queue2.Enqueue(gameObject4.GetComponent<PropHuntGrabbableProp>());
			}
			PropHuntPools._cosmeticId_to_inactiveGrabbables[cosmeticId] = queue2;
		}
		if (!PropHuntPools._cosmeticId_to_taggableTemplate.ContainsKey(cosmeticId))
		{
			GameObject gameObject5 = gameObject;
			List<MeshCollider> list4 = new List<MeshCollider>();
			List<InteractionPoint> list5 = new List<InteractionPoint>();
			PropHuntGrabbableProp propHuntGrabbableProp2;
			PropHuntTaggableProp propHuntTaggableProp2;
			PropHuntHandFollower.TryPrepPropTemplate(gameObject5, false, cosmeticSO, list4, list5, out propHuntGrabbableProp2, out propHuntTaggableProp2);
			propHuntTaggableProp2.name = "__PropHuntPoolProp_Taggable_TEMPLATE__" + cosmeticSO.name + "__";
			PropHuntPools._cosmeticId_to_taggableTemplate[cosmeticId] = propHuntTaggableProp2;
			Queue<PropHuntTaggableProp> queue3 = new Queue<PropHuntTaggableProp>(2);
			string text3 = "__PropHuntPoolProp_Taggable__" + cosmeticSO.name + "__";
			for (int n = 0; n < 2; n++)
			{
				GameObject gameObject6 = Object.Instantiate<GameObject>(propHuntTaggableProp2.gameObject, PropHuntPools._taggableInactivePropsParent);
				gameObject6.name = text3;
				queue3.Enqueue(gameObject6.GetComponent<PropHuntTaggableProp>());
			}
			PropHuntPools._cosmeticId_to_inactiveTaggables[cosmeticId] = queue3;
		}
		PropHuntPools._propCosmeticIdsWaitingToLoad.Remove(cosmeticId);
		if (PropHuntPools._propCosmeticIdsWaitingToLoad.Count == 0)
		{
			PropHuntPools._state = PropHuntPools.EState.Ready;
			Action onReady = PropHuntPools.OnReady;
			if (onReady == null)
			{
				return;
			}
			onReady();
		}
	}

	public static bool TryGetDecoyProp(string cosmeticId, out PropPlacementRB out_prop)
	{
		if (!PropHuntPools.IsReady)
		{
			Debug.LogError("ERROR!!!  PropHuntPools:  TryGetDecoyProp: Cannot get because `PropHuntPools.IsReady` is not true yet!");
			out_prop = null;
			return false;
		}
		Queue<PropPlacementRB> queue;
		if (!PropHuntPools._cosmeticId_to_inactiveDecoys.TryGetValue(cosmeticId, out queue))
		{
			Debug.LogError("ERROR!!!  PropHuntPools: (should never happen) Prop does not exist with cosmeticId \"" + cosmeticId + "\"!");
			out_prop = null;
			return false;
		}
		if (queue.Count > 0)
		{
			out_prop = queue.Dequeue();
			out_prop.transform.SetParent(null);
			out_prop.gameObject.SetActive(true);
			PropHuntPools._activeDecoy_to_cosmeticId[out_prop] = cosmeticId;
			return true;
		}
		PropPlacementRB propPlacementRB;
		if (PropHuntPools._cosmeticId_to_decoyTemplate.TryGetValue(cosmeticId, out propPlacementRB))
		{
			Dictionary<string, int> cosmeticId_to_decoyInitialCount = PropHuntPools._cosmeticId_to_decoyInitialCount;
			int num = cosmeticId_to_decoyInitialCount[cosmeticId] + 1;
			cosmeticId_to_decoyInitialCount[cosmeticId] = num;
			int num2 = num;
			PropHuntPools._debug_decoyMaxCountPerProp = Mathf.Max(PropHuntPools._debug_decoyMaxCountPerProp, num2);
			out_prop = Object.Instantiate<PropPlacementRB>(propPlacementRB);
			PropHuntPools._activeDecoy_to_cosmeticId[out_prop] = cosmeticId;
			return true;
		}
		out_prop = null;
		return false;
	}

	public static bool TryGetTaggableProp(string cosmeticId, out PropHuntTaggableProp out_prop)
	{
		if (!PropHuntPools.IsReady)
		{
			Debug.LogError("ERROR!!!  PropHuntPools: TryGetTaggableProp: Cannot get because `PropHuntPools.IsReady` is not true yet!");
			out_prop = null;
			return false;
		}
		Queue<PropHuntTaggableProp> queue;
		if (PropHuntPools._cosmeticId_to_inactiveTaggables.TryGetValue(cosmeticId, out queue))
		{
			if (queue.Count > 0)
			{
				out_prop = queue.Dequeue();
				out_prop.transform.SetParent(null);
				out_prop.gameObject.SetActive(true);
				PropHuntPools._activeTaggable_to_cosmeticId[out_prop] = cosmeticId;
				return true;
			}
			PropHuntTaggableProp propHuntTaggableProp;
			if (PropHuntPools._cosmeticId_to_taggableTemplate.TryGetValue(cosmeticId, out propHuntTaggableProp))
			{
				PropHuntPools._debug_decoyMaxCountPerProp = ((PropHuntPools._debug_decoyMaxCountPerProp >= queue.Count + 1) ? PropHuntPools._debug_decoyMaxCountPerProp : ((int)((double)queue.Count * 1.5)));
				out_prop = Object.Instantiate<PropHuntTaggableProp>(propHuntTaggableProp);
				PropHuntPools._activeTaggable_to_cosmeticId[out_prop] = cosmeticId;
				return true;
			}
		}
		Debug.LogError("ERROR!!!  PropHuntPools: Prop does not exist with cosmeticId \"" + cosmeticId + "\"!");
		out_prop = null;
		return false;
	}

	public static bool TryGetGrabbableProp(string cosmeticId, out PropHuntGrabbableProp out_prop)
	{
		if (!PropHuntPools.IsReady)
		{
			Debug.LogError("ERROR!!!  PropHuntPools:  TryGetGrabbableProp: Called before pools are ready.");
			out_prop = null;
			return false;
		}
		Queue<PropHuntGrabbableProp> queue;
		if (PropHuntPools._cosmeticId_to_inactiveGrabbables.TryGetValue(cosmeticId, out queue) && queue.Count > 0)
		{
			out_prop = queue.Dequeue();
			out_prop.transform.SetParent(null);
			out_prop.gameObject.SetActive(true);
			PropHuntPools._activeGrabbable_to_cosmeticId[out_prop] = cosmeticId;
			return true;
		}
		PropHuntGrabbableProp propHuntGrabbableProp;
		if (PropHuntPools._cosmeticId_to_grabbableTemplate.TryGetValue(cosmeticId, out propHuntGrabbableProp))
		{
			out_prop = Object.Instantiate<PropHuntGrabbableProp>(propHuntGrabbableProp);
			PropHuntPools._activeGrabbable_to_cosmeticId[out_prop] = cosmeticId;
			return true;
		}
		Debug.LogError("ERROR!!!  PropHuntPools: Prop does not exist with cosmeticId \"" + cosmeticId + "\"!");
		out_prop = null;
		return false;
	}

	public static void ReturnDecoyProp(PropPlacementRB prop)
	{
		if (prop == null)
		{
			Debug.LogError("ERROR!!!  PropHuntPools: Tried to return a prop but it was null!");
			return;
		}
		string text;
		Queue<PropPlacementRB> queue;
		if (PropHuntPools._activeDecoy_to_cosmeticId.TryGetValue(prop, out text) && PropHuntPools._cosmeticId_to_inactiveDecoys.TryGetValue(text, out queue))
		{
			prop.gameObject.SetActive(false);
			prop.transform.SetParent(PropHuntPools._grabbableInactivePropsParent);
			queue.Enqueue(prop);
			PropHuntPools._activeDecoy_to_cosmeticId.Remove(prop);
		}
	}

	public static void ReturnTaggableProp(PropHuntTaggableProp prop)
	{
		if (prop == null)
		{
			Debug.LogError("ERROR!!!  PropHuntPools: Tried to return a prop but it was null!");
			return;
		}
		string text;
		Queue<PropHuntTaggableProp> queue;
		if (PropHuntPools._activeTaggable_to_cosmeticId.TryGetValue(prop, out text) && PropHuntPools._cosmeticId_to_inactiveTaggables.TryGetValue(text, out queue))
		{
			prop.gameObject.SetActive(false);
			prop.transform.SetParent(PropHuntPools._grabbableInactivePropsParent);
			queue.Enqueue(prop);
			PropHuntPools._activeTaggable_to_cosmeticId.Remove(prop);
		}
	}

	public static void ReturnGrabbableProp(PropHuntGrabbableProp prop)
	{
		if (prop == null)
		{
			return;
		}
		string text;
		Queue<PropHuntGrabbableProp> queue;
		if (PropHuntPools._activeGrabbable_to_cosmeticId.TryGetValue(prop, out text) && PropHuntPools._cosmeticId_to_inactiveGrabbables.TryGetValue(text, out queue))
		{
			prop.gameObject.SetActive(false);
			prop.transform.SetParent(PropHuntPools._grabbableInactivePropsParent);
			queue.Enqueue(prop);
			PropHuntPools._activeGrabbable_to_cosmeticId.Remove(prop);
		}
	}

	private const string preLog = "PropHuntPools: ";

	private const string preLogEd = "(editor only log) PropHuntPools: ";

	private const string preLogBeta = "(beta only log) PropHuntPools: ";

	private const string preErr = "ERROR!!!  PropHuntPools: ";

	private const string preErrEd = "ERROR!!!  (editor only log) PropHuntPools: ";

	private const string preErrBeta = "ERROR!!!  (beta only log) PropHuntPools: ";

	private const string _k_titleDataKey = "PropHuntProps";

	private const bool _k__GT_PROP_HUNT__USE_POOLING__ = true;

	[OnEnterPlay_Set(PropHuntPools.EState.None)]
	private static PropHuntPools.EState _state;

	[OnEnterPlay_Set(false)]
	private static bool _state_isTitleDataLoaded;

	[OnEnterPlay_Set(false)]
	private static bool _state_hasLocalPlayerVisitedBayou;

	public static Action OnReady;

	[OnEnterPlay_SetNull]
	private static AllCosmeticsArraySO _allCosmeticsArraySO;

	private static readonly string[] _g_ph_titleDataSeparators = new string[] { "\"", " ", "\n" };

	[OnEnterPlay_SetNull]
	private static string[] _allPropCosmeticIds;

	[OnEnterPlay_SetNull]
	private static CosmeticSO _fallbackProp_cosmeticSO;

	public static Dictionary<string, CosmeticSO> propCosmeticId_to_cosmeticSO = new Dictionary<string, CosmeticSO>(256);

	[OnEnterPlay_Clear]
	private static readonly HashSet<string> _propCosmeticIdsWaitingToLoad = new HashSet<string>(256);

	[OnEnterPlay_SetNull]
	private static string[] _propCosmeticIds_uniqueArray;

	[OnEnterPlay_SetNull]
	private static GameObject _fallbackPrefabInstance;

	private const int _k_decoyInitialCountPerPropLine = 10;

	[OnEnterPlay_SetNull]
	private static Transform _decoyTemplatesParent;

	[OnEnterPlay_SetNull]
	private static Transform _decoyInactivePropsParent;

	[OnEnterPlay_Set(0)]
	private static int _debug_decoyMaxCountPerProp;

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, int> _cosmeticId_to_decoyInitialCount = new Dictionary<string, int>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, PropPlacementRB> _cosmeticId_to_decoyTemplate = new Dictionary<string, PropPlacementRB>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, Queue<PropPlacementRB>> _cosmeticId_to_inactiveDecoys = new Dictionary<string, Queue<PropPlacementRB>>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<PropPlacementRB, string> _activeDecoy_to_cosmeticId = new Dictionary<PropPlacementRB, string>(256);

	private const int _k_initialCountPerTaggable = 2;

	[OnEnterPlay_SetNull]
	private static Transform _taggableTemplatesParent;

	[OnEnterPlay_SetNull]
	private static Transform _taggableInactivePropsParent;

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, PropHuntTaggableProp> _cosmeticId_to_taggableTemplate = new Dictionary<string, PropHuntTaggableProp>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, Queue<PropHuntTaggableProp>> _cosmeticId_to_inactiveTaggables = new Dictionary<string, Queue<PropHuntTaggableProp>>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<PropHuntTaggableProp, string> _activeTaggable_to_cosmeticId = new Dictionary<PropHuntTaggableProp, string>(256);

	private const int _k_initialCountPerFollower = 1;

	[OnEnterPlay_SetNull]
	private static Transform _grabbableTemplatesParent;

	[OnEnterPlay_SetNull]
	private static Transform _grabbableInactivePropsParent;

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, PropHuntGrabbableProp> _cosmeticId_to_grabbableTemplate = new Dictionary<string, PropHuntGrabbableProp>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<string, Queue<PropHuntGrabbableProp>> _cosmeticId_to_inactiveGrabbables = new Dictionary<string, Queue<PropHuntGrabbableProp>>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<PropHuntGrabbableProp, string> _activeGrabbable_to_cosmeticId = new Dictionary<PropHuntGrabbableProp, string>(256);

	[OnEnterPlay_Clear]
	private static readonly List<MeshFilter> _temp_meshFilters = new List<MeshFilter>(8);

	public enum EState
	{
		None,
		WaitingForTitleData,
		WaitingForLocalPlayerToVisitBayou,
		SpawningProps,
		Ready
	}
}
