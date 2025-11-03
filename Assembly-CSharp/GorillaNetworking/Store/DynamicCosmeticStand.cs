using System;
using GorillaExtensions;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GT_CustomMapSupportRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GorillaNetworking.Store
{
	public class DynamicCosmeticStand : MonoBehaviour, iFlagForBaking
	{
		public virtual void SetForBaking()
		{
			this.GorillaHeadModel.SetActive(true);
			this.GorillaTorsoModel.SetActive(true);
			this.GorillaTorsoPostModel.SetActive(true);
			this.GorillaMannequinModel.SetActive(true);
			this.JeweleryBoxModel.SetActive(true);
			this.root.SetActive(true);
			this.DisplayHeadModel.gameObject.SetActive(false);
		}

		public void OnEnable()
		{
			this.addToCartTextTMP.gameObject.SetActive(true);
			this.slotPriceTextTMP.gameObject.SetActive(true);
		}

		public void OnDisable()
		{
			this.addToCartTextTMP.gameObject.SetActive(false);
			this.slotPriceTextTMP.gameObject.SetActive(false);
		}

		public virtual void SetForGame()
		{
			this.DisplayHeadModel.gameObject.SetActive(true);
			this.SetStandType(this.DisplayHeadModel.bustType);
		}

		public string thisCosmeticName
		{
			get
			{
				return this._thisCosmeticName;
			}
			set
			{
				this._thisCosmeticName = value;
			}
		}

		public void InitializeCosmetic()
		{
			this.thisCosmeticItem = CosmeticsController.instance.allCosmetics.Find((CosmeticsController.CosmeticItem x) => this.thisCosmeticName == x.displayName || this.thisCosmeticName == x.overrideDisplayName || this.thisCosmeticName == x.itemName);
			if (this.slotPriceText != null)
			{
				this.slotPriceText.text = this.thisCosmeticItem.itemCategory.ToString().ToUpper() + " " + this.thisCosmeticItem.cost.ToString();
			}
			if (this.slotPriceTextTMP != null)
			{
				this.slotPriceTextTMP.text = this.thisCosmeticItem.itemCategory.ToString().ToUpper() + " " + this.thisCosmeticItem.cost.ToString();
			}
		}

		public void SpawnItemOntoStand(string PlayFabID)
		{
			this.ClearCosmetics();
			if (PlayFabID.IsNullOrEmpty())
			{
				GTDev.LogWarning<string>("ManuallyInitialize: PlayFabID is null or empty for " + this.StandName, null);
				return;
			}
			if (StoreController.instance.IsNotNull() && Application.isPlaying)
			{
				StoreController.instance.RemoveStandFromPlayFabIDDictionary(this);
			}
			this.thisCosmeticName = PlayFabID;
			if (this.thisCosmeticName.Length == 5)
			{
				this.thisCosmeticName += ".";
			}
			if (Application.isPlaying)
			{
				this.DisplayHeadModel.LoadCosmeticPartsV2(this.thisCosmeticName, false);
			}
			else
			{
				this.DisplayHeadModel.LoadCosmeticParts(StoreController.FindCosmeticInAllCosmeticsArraySO(this.thisCosmeticName), false);
			}
			if (StoreController.instance.IsNotNull() && Application.isPlaying)
			{
				StoreController.instance.AddStandToPlayfabIDDictionary(this);
			}
		}

		public void ClearCosmetics()
		{
			this.thisCosmeticName = "";
			this.DisplayHeadModel.ClearManuallySpawnedCosmeticParts();
			this.DisplayHeadModel.ClearCosmetics();
		}

		public void SetStandType(HeadModel_CosmeticStand.BustType newBustType)
		{
			this.DisplayHeadModel.SetStandType(newBustType);
			this.GorillaHeadModel.SetActive(false);
			this.GorillaTorsoModel.SetActive(false);
			this.GorillaTorsoPostModel.SetActive(false);
			this.GorillaMannequinModel.SetActive(false);
			this.GuitarStandModel.SetActive(false);
			this.JeweleryBoxModel.SetActive(false);
			this.AddToCartButton.gameObject.SetActive(true);
			Text text = this.slotPriceText;
			if (text != null)
			{
				text.gameObject.SetActive(true);
			}
			TMP_Text tmp_Text = this.slotPriceTextTMP;
			if (tmp_Text != null)
			{
				tmp_Text.gameObject.SetActive(true);
			}
			Text text2 = this.addToCartText;
			if (text2 != null)
			{
				text2.gameObject.SetActive(true);
			}
			TMP_Text tmp_Text2 = this.addToCartTextTMP;
			if (tmp_Text2 != null)
			{
				tmp_Text2.gameObject.SetActive(true);
			}
			switch (newBustType)
			{
			case HeadModel_CosmeticStand.BustType.Disabled:
			{
				this.ClearCosmetics();
				this.thisCosmeticName = "";
				this.AddToCartButton.gameObject.SetActive(false);
				Text text3 = this.slotPriceText;
				if (text3 != null)
				{
					text3.gameObject.SetActive(false);
				}
				TMP_Text tmp_Text3 = this.slotPriceTextTMP;
				if (tmp_Text3 != null)
				{
					tmp_Text3.gameObject.SetActive(false);
				}
				Text text4 = this.addToCartText;
				if (text4 != null)
				{
					text4.gameObject.SetActive(false);
				}
				TMP_Text tmp_Text4 = this.addToCartTextTMP;
				if (tmp_Text4 != null)
				{
					tmp_Text4.gameObject.SetActive(false);
				}
				this.DisplayHeadModel.transform.localPosition = Vector3.zero;
				this.DisplayHeadModel.transform.localRotation = Quaternion.identity;
				this.root.SetActive(false);
				break;
			}
			case HeadModel_CosmeticStand.BustType.GorillaHead:
				this.root.SetActive(true);
				this.GorillaHeadModel.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.GorillaHeadModel.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.GorillaHeadModel.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.GorillaTorso:
				this.root.SetActive(true);
				this.GorillaTorsoModel.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.GorillaTorsoModel.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.GorillaTorsoModel.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.GorillaTorsoPost:
				this.root.SetActive(true);
				this.GorillaTorsoPostModel.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.GorillaTorsoPostModel.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.GorillaTorsoPostModel.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.GorillaMannequin:
				this.root.SetActive(true);
				this.GorillaMannequinModel.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.GorillaMannequinModel.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.GorillaMannequinModel.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.GuitarStand:
				this.root.SetActive(true);
				this.GuitarStandModel.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.GuitarStandMount.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.GuitarStandMount.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.JewelryBox:
				this.root.SetActive(true);
				this.JeweleryBoxModel.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.JeweleryBoxMount.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.JeweleryBoxMount.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.Table:
				this.root.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.TableMount.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.TableMount.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.PinDisplay:
				this.root.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = this.PinDisplayMount.transform.localPosition;
				this.DisplayHeadModel.transform.localRotation = this.PinDisplayMount.transform.localRotation;
				break;
			case HeadModel_CosmeticStand.BustType.TagEffectDisplay:
				this.root.SetActive(true);
				break;
			default:
				this.root.SetActive(true);
				this.DisplayHeadModel.transform.localPosition = Vector3.zero;
				this.DisplayHeadModel.transform.localRotation = Quaternion.identity;
				break;
			}
			this.SpawnItemOntoStand(this.thisCosmeticName);
		}

		public void CopyChildsName()
		{
			foreach (DynamicCosmeticStand dynamicCosmeticStand in base.gameObject.GetComponentsInChildren<DynamicCosmeticStand>(true))
			{
				if (dynamicCosmeticStand != this)
				{
					this.StandName = dynamicCosmeticStand.StandName;
				}
			}
		}

		public void PressCosmeticStandButton()
		{
			this.searchIndex = CosmeticsController.instance.currentCart.IndexOf(this.thisCosmeticItem);
			if (this.searchIndex != -1)
			{
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.cart_item_remove, this.thisCosmeticItem);
				CosmeticsController.instance.currentCart.RemoveAt(this.searchIndex);
				foreach (DynamicCosmeticStand dynamicCosmeticStand in StoreController.instance.StandsByPlayfabID[this.thisCosmeticItem.itemName])
				{
					dynamicCosmeticStand.AddToCartButton.isOn = false;
					dynamicCosmeticStand.AddToCartButton.UpdateColor();
				}
				for (int i = 0; i < 16; i++)
				{
					if (this.thisCosmeticItem.itemName == CosmeticsController.instance.tryOnSet.items[i].itemName)
					{
						CosmeticsController.instance.tryOnSet.items[i] = CosmeticsController.instance.nullItem;
					}
				}
			}
			else
			{
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.cart_item_add, this.thisCosmeticItem);
				CosmeticsController.instance.currentCart.Insert(0, this.thisCosmeticItem);
				foreach (DynamicCosmeticStand dynamicCosmeticStand2 in StoreController.instance.StandsByPlayfabID[this.thisCosmeticName])
				{
					dynamicCosmeticStand2.AddToCartButton.isOn = true;
					dynamicCosmeticStand2.AddToCartButton.UpdateColor();
				}
				if (CosmeticsController.instance.currentCart.Count > CosmeticsController.instance.numFittingRoomButtons)
				{
					foreach (DynamicCosmeticStand dynamicCosmeticStand3 in StoreController.instance.StandsByPlayfabID[CosmeticsController.instance.currentCart[CosmeticsController.instance.numFittingRoomButtons].itemName])
					{
						dynamicCosmeticStand3.AddToCartButton.isOn = false;
						dynamicCosmeticStand3.AddToCartButton.UpdateColor();
					}
					CosmeticsController.instance.currentCart.RemoveAt(CosmeticsController.instance.numFittingRoomButtons);
				}
			}
			CosmeticsController.instance.UpdateShoppingCart();
		}

		public void SetStandTypeString(string bustTypeString)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(bustTypeString);
			if (num <= 1590453963U)
			{
				if (num <= 1121133049U)
				{
					if (num != 214514339U)
					{
						if (num == 1121133049U)
						{
							if (bustTypeString == "GuitarStand")
							{
								this.SetStandType(HeadModel_CosmeticStand.BustType.GuitarStand);
								return;
							}
						}
					}
					else if (bustTypeString == "GorillaHead")
					{
						this.SetStandType(HeadModel_CosmeticStand.BustType.GorillaHead);
						return;
					}
				}
				else if (num != 1364530810U)
				{
					if (num != 1520673798U)
					{
						if (num == 1590453963U)
						{
							if (bustTypeString == "GorillaMannequin")
							{
								this.SetStandType(HeadModel_CosmeticStand.BustType.GorillaMannequin);
								return;
							}
						}
					}
					else if (bustTypeString == "JewelryBox")
					{
						this.SetStandType(HeadModel_CosmeticStand.BustType.JewelryBox);
						return;
					}
				}
				else if (bustTypeString == "PinDisplay")
				{
					this.SetStandType(HeadModel_CosmeticStand.BustType.PinDisplay);
					return;
				}
			}
			else if (num <= 2111326094U)
			{
				if (num != 1952506660U)
				{
					if (num == 2111326094U)
					{
						if (bustTypeString == "GorillaTorsoPost")
						{
							this.SetStandType(HeadModel_CosmeticStand.BustType.GorillaTorsoPost);
							return;
						}
					}
				}
				else if (bustTypeString == "GorillaTorso")
				{
					this.SetStandType(HeadModel_CosmeticStand.BustType.GorillaTorso);
					return;
				}
			}
			else if (num != 3217987877U)
			{
				if (num != 3607948159U)
				{
					if (num == 3845287012U)
					{
						if (bustTypeString == "TagEffectDisplay")
						{
							this.SetStandType(HeadModel_CosmeticStand.BustType.TagEffectDisplay);
							return;
						}
					}
				}
				else if (bustTypeString == "Table")
				{
					this.SetStandType(HeadModel_CosmeticStand.BustType.Table);
					return;
				}
			}
			else if (bustTypeString == "Disabled")
			{
				this.SetStandType(HeadModel_CosmeticStand.BustType.Disabled);
				return;
			}
			this.SetStandType(HeadModel_CosmeticStand.BustType.Table);
		}

		public void UpdateCosmeticsMountPositions()
		{
			this.DisplayHeadModel.UpdateCosmeticsMountPositions(StoreController.FindCosmeticInAllCosmeticsArraySO(this.thisCosmeticName));
		}

		public void InitializeForCustomMapCosmeticItem(GTObjectPlaceholder.ECustomMapCosmeticItem cosmeticItemSlot, Scene scene)
		{
			this.StandName = "CustomMapCosmeticItemStand-" + cosmeticItemSlot.ToString();
			this.customMapScene = scene;
			this.ClearCosmetics();
			CustomMapCosmeticItem customMapCosmeticItem;
			if (CosmeticsController.instance.customMapCosmeticsData.TryGetItem(cosmeticItemSlot, out customMapCosmeticItem))
			{
				this.thisCosmeticName = customMapCosmeticItem.playFabID;
				this.SetStandType(customMapCosmeticItem.bustType);
				this.InitializeCosmetic();
			}
		}

		public bool IsFromCustomMapScene(Scene scene)
		{
			return this.customMapScene == scene;
		}

		public HeadModel_CosmeticStand DisplayHeadModel;

		public GorillaPressableButton AddToCartButton;

		[HideInInspector]
		public Text slotPriceText;

		[HideInInspector]
		public Text addToCartText;

		public TMP_Text slotPriceTextTMP;

		public TMP_Text addToCartTextTMP;

		private CosmeticsController.CosmeticItem thisCosmeticItem;

		[FormerlySerializedAs("StandID")]
		public string StandName;

		public string _thisCosmeticName = "";

		public GameObject GorillaHeadModel;

		public GameObject GorillaTorsoModel;

		public GameObject GorillaTorsoPostModel;

		public GameObject GorillaMannequinModel;

		public GameObject GuitarStandModel;

		public GameObject GuitarStandMount;

		public GameObject JeweleryBoxModel;

		public GameObject JeweleryBoxMount;

		public GameObject TableMount;

		[FormerlySerializedAs("PinDisplayMounnt")]
		[FormerlySerializedAs("PinDisplayMountn")]
		public GameObject PinDisplayMount;

		public GameObject root;

		public GameObject TagEffectDisplayMount;

		public GameObject TageEffectDisplayModel;

		private Scene customMapScene;

		private int searchIndex;
	}
}
