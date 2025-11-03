using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaExtensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaNetworking.Store
{
	public class BundleManager : MonoBehaviour
	{
		private IEnumerable GetStoreBundles()
		{
			List<StoreBundleData> list = new List<StoreBundleData>();
			list.Add(this.nullBundleData);
			list.AddRange(this._bundleScriptableObjects);
			return list;
		}

		public void Awake()
		{
			if (BundleManager.instance == null)
			{
				BundleManager.instance = this;
				return;
			}
			if (BundleManager.instance != this)
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}

		private void Start()
		{
			this.GenerateBundleDictionaries();
			this.Initialize();
		}

		private void Initialize()
		{
			foreach (StoreBundle storeBundle in this._storeBundles)
			{
				storeBundle.InitializebundleStands();
			}
		}

		private void ValidateBundleData()
		{
			foreach (StoreBundle storeBundle in this._storeBundles)
			{
				storeBundle.ValidateBundleData();
			}
		}

		private void SpawnBundleStands()
		{
			foreach (StoreBundle storeBundle in this._storeBundles)
			{
				foreach (BundleStand bundleStand in storeBundle.bundleStands)
				{
					if (bundleStand != null)
					{
						Object.DestroyImmediate(bundleStand.gameObject);
					}
				}
			}
			this._spawnedBundleStands.Clear();
			this.storeBundlesById.Clear();
			this.storeBundlesBySKU.Clear();
			this._storeBundles.Clear();
			this._bundleScriptableObjects.Clear();
			BundleStand[] array = Object.FindObjectsByType<BundleStand>(FindObjectsSortMode.None);
			for (int i = 0; i < array.Length; i++)
			{
				Object.DestroyImmediate(array[i].gameObject);
			}
			for (int j = 0; j < this.BundleStands.Count; j++)
			{
				if (this.BundleStands[j].spawnLocation == null)
				{
					Debug.LogError("No spawn location set for Bundle Stand " + j.ToString());
				}
				else if (this.BundleStands[j].bundleStand == null)
				{
					Debug.LogError("No Bundle Stand set for Bundle Stand " + j.ToString());
				}
			}
			this.GenerateAllStoreBundleReferences();
			if (!this._bundleScriptableObjects.Contains(this.tryOnBundleButton1))
			{
				this.tryOnBundleButton1 = this.nullBundleData;
			}
			if (!this._bundleScriptableObjects.Contains(this.tryOnBundleButton2))
			{
				this.tryOnBundleButton2 = this.nullBundleData;
			}
			if (!this._bundleScriptableObjects.Contains(this.tryOnBundleButton3))
			{
				this.tryOnBundleButton3 = this.nullBundleData;
			}
			if (!this._bundleScriptableObjects.Contains(this.tryOnBundleButton4))
			{
				this.tryOnBundleButton4 = this.nullBundleData;
			}
			if (!this._bundleScriptableObjects.Contains(this.tryOnBundleButton5))
			{
				this.tryOnBundleButton4 = this.nullBundleData;
			}
		}

		public void ClearEverything()
		{
			foreach (StoreBundle storeBundle in this._storeBundles)
			{
				foreach (BundleStand bundleStand in storeBundle.bundleStands)
				{
					if (bundleStand != null)
					{
						Object.DestroyImmediate(bundleStand.gameObject);
					}
				}
			}
			this._spawnedBundleStands.Clear();
			this.storeBundlesById.Clear();
			this.storeBundlesBySKU.Clear();
			this._storeBundles.Clear();
			this._bundleScriptableObjects.Clear();
			this.tryOnBundleButton1 = this.nullBundleData;
			this.tryOnBundleButton2 = this.nullBundleData;
			this.tryOnBundleButton3 = this.nullBundleData;
			this.tryOnBundleButton4 = this.nullBundleData;
			this.tryOnBundleButton5 = this.nullBundleData;
			BundleStand[] array = Object.FindObjectsByType<BundleStand>(FindObjectsSortMode.None);
			for (int i = 0; i < array.Length; i++)
			{
				Object.DestroyImmediate(array[i].gameObject);
			}
		}

		public void GenerateAllStoreBundleReferences()
		{
		}

		private void AddNewBundleStand(BundleStand bundleStand)
		{
			foreach (StoreBundle storeBundle in this._storeBundles)
			{
				if (storeBundle.playfabBundleID == bundleStand._bundleDataReference.playfabBundleID)
				{
					storeBundle.bundleStands.Add(bundleStand);
					return;
				}
			}
			StoreBundle storeBundle2 = new StoreBundle(bundleStand._bundleDataReference);
			storeBundle2.bundleStands.Add(bundleStand);
			this._storeBundles.Add(storeBundle2);
		}

		public void GenerateBundleDictionaries()
		{
			this.storeBundlesById.Clear();
			this.storeBundlesBySKU.Clear();
			foreach (StoreBundle storeBundle in this._storeBundles)
			{
				this.storeBundlesById.Add(storeBundle.playfabBundleID, storeBundle);
				this.storeBundlesBySKU.Add(storeBundle.bundleSKU, storeBundle);
			}
		}

		public void BundlePurchaseButtonPressed(string playFabItemName)
		{
			CosmeticsController.instance.PurchaseBundle(this.storeBundlesById[playFabItemName]);
		}

		public void FixBundles()
		{
			this._storeBundles.Clear();
			for (int i = this._spawnedBundleStands.Count - 1; i >= 0; i--)
			{
				if (this._spawnedBundleStands[i].bundleStand == null)
				{
					this._spawnedBundleStands.RemoveAt(i);
				}
			}
			BundleStand[] array = Object.FindObjectsByType<BundleStand>(FindObjectsSortMode.None);
			for (int j = 0; j < array.Length; j++)
			{
				BundleStand bundle = array[j];
				if (this._spawnedBundleStands.Any((SpawnedBundle x) => x.spawnLocationPath == bundle.transform.parent.gameObject.GetPath(3)))
				{
					SpawnedBundle spawnedBundle = this._spawnedBundleStands.First((SpawnedBundle x) => x.spawnLocationPath == bundle.transform.parent.gameObject.GetPath(3));
					if (spawnedBundle != null && spawnedBundle.bundleStand != bundle)
					{
						Object.DestroyImmediate(spawnedBundle.bundleStand.gameObject);
						spawnedBundle.bundleStand = bundle;
					}
				}
				else
				{
					this._spawnedBundleStands.Add(new SpawnedBundle
					{
						spawnLocationPath = bundle.transform.parent.gameObject.GetPath(3),
						bundleStand = bundle
					});
				}
			}
			this.GenerateAllStoreBundleReferences();
		}

		public StoreBundleData[] GetTryOnButtons()
		{
			return new StoreBundleData[] { this.tryOnBundleButton1, this.tryOnBundleButton2, this.tryOnBundleButton3, this.tryOnBundleButton4, this.tryOnBundleButton5 };
		}

		public void NotifyBundleOfErrorByPlayFabID(string ItemId)
		{
			StoreBundle storeBundle;
			if (this.storeBundlesById.TryGetValue(ItemId, out storeBundle))
			{
				foreach (BundleStand bundleStand in storeBundle.bundleStands)
				{
					bundleStand.ErrorHappened();
				}
			}
		}

		public void NotifyBundleOfErrorBySKU(string ItemSKU)
		{
			StoreBundle storeBundle;
			if (this.storeBundlesBySKU.TryGetValue(ItemSKU, out storeBundle))
			{
				foreach (BundleStand bundleStand in storeBundle.bundleStands)
				{
					bundleStand.ErrorHappened();
				}
			}
		}

		public void MarkBundleOwnedByPlayFabID(string ItemId)
		{
			if (this.storeBundlesById.ContainsKey(ItemId))
			{
				this.storeBundlesById[ItemId].isOwned = true;
				foreach (BundleStand bundleStand in this.storeBundlesById[ItemId].bundleStands)
				{
					bundleStand.NotifyAlreadyOwn();
				}
			}
		}

		public void MarkBundleOwnedBySKU(string SKU)
		{
			if (this.storeBundlesBySKU.ContainsKey(SKU))
			{
				this.storeBundlesBySKU[SKU].isOwned = true;
				foreach (BundleStand bundleStand in this.storeBundlesBySKU[SKU].bundleStands)
				{
					bundleStand.NotifyAlreadyOwn();
				}
			}
		}

		public void CheckIfBundlesOwned()
		{
			foreach (StoreBundle storeBundle in this.storeBundlesById.Values)
			{
				if (storeBundle.isOwned)
				{
					foreach (BundleStand bundleStand in storeBundle.bundleStands)
					{
						bundleStand.NotifyAlreadyOwn();
					}
				}
			}
		}

		public void PressTryOnBundleButton(TryOnBundleButton pressedTryOnBundleButton, bool isLeftHand)
		{
			if (this._tryOnBundlesStand.IsNotNull())
			{
				this._tryOnBundlesStand.PressTryOnBundleButton(pressedTryOnBundleButton, isLeftHand);
			}
		}

		public void PressPurchaseTryOnBundleButton()
		{
			this._tryOnBundlesStand.PurchaseButtonPressed();
		}

		public void UpdateBundlePrice(string productSku, string productFormattedPrice)
		{
			if (this.storeBundlesBySKU.ContainsKey(productSku))
			{
				this.storeBundlesBySKU[productSku].TryUpdatePrice(productFormattedPrice);
			}
		}

		public void CheckForNoPriceBundlesAndDefaultPrice()
		{
			foreach (KeyValuePair<string, StoreBundle> keyValuePair in this.storeBundlesBySKU)
			{
				string text;
				StoreBundle storeBundle;
				keyValuePair.Deconstruct(out text, out storeBundle);
				StoreBundle storeBundle2 = storeBundle;
				if (!storeBundle2.HasPrice)
				{
					storeBundle2.TryUpdatePrice(null);
				}
			}
		}

		public static volatile BundleManager instance;

		[FormerlySerializedAs("_TryOnBundlesStand")]
		public TryOnBundlesStand _tryOnBundlesStand;

		[SerializeField]
		private StoreBundleData nullBundleData;

		private List<StoreBundleData> _bundleScriptableObjects = new List<StoreBundleData>();

		[SerializeField]
		private List<StoreBundle> _storeBundles = new List<StoreBundle>();

		[FormerlySerializedAs("_SpawnedBundleStands")]
		[SerializeField]
		private List<SpawnedBundle> _spawnedBundleStands = new List<SpawnedBundle>();

		public Dictionary<string, StoreBundle> storeBundlesById = new Dictionary<string, StoreBundle>();

		public Dictionary<string, StoreBundle> storeBundlesBySKU = new Dictionary<string, StoreBundle>();

		[Header("Enable Advanced Search window in your settings to easily see all bundle prefabs")]
		[SerializeField]
		private List<BundleManager.BundleStandSpawn> BundleStands = new List<BundleManager.BundleStandSpawn>();

		[SerializeField]
		private StoreBundleData tryOnBundleButton1;

		[SerializeField]
		private StoreBundleData tryOnBundleButton2;

		[SerializeField]
		private StoreBundleData tryOnBundleButton3;

		[SerializeField]
		private StoreBundleData tryOnBundleButton4;

		[SerializeField]
		private StoreBundleData tryOnBundleButton5;

		[Serializable]
		public class BundleStandSpawn
		{
			private static IEnumerable GetEndCapSpawnPoints()
			{
				return from x in Object.FindObjectsByType<EndCapSpawnPoint>(FindObjectsSortMode.None)
					select new ValueDropdownItem(string.Concat(new string[]
					{
						x.transform.parent.parent.name,
						"/",
						x.transform.parent.name,
						"/",
						x.name
					}), x);
			}

			public EndCapSpawnPoint spawnLocation;

			public BundleStand bundleStand;
		}
	}
}
