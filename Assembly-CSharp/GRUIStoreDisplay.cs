using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GRUIStoreDisplay : MonoBehaviour
{
	public void Awake()
	{
	}

	public void OnEnable()
	{
		this.RefreshUI();
	}

	public void OnDisable()
	{
	}

	public void Setup(int playerActorId, GhostReactor reactor)
	{
		this.reactor = reactor;
		this.toolProgressionManager = reactor.toolProgression;
		this.playerActorId = playerActorId;
		this.RefreshUI();
		this.toolProgressionManager.OnProgressionUpdated += this.onProgressionUpdated;
	}

	private void onProgressionUpdated()
	{
		this.RefreshUI();
	}

	private void RefreshUI()
	{
		this.RefreshItemInfo();
	}

	public void OnBuy(int playerActorNumber)
	{
		if (playerActorNumber != this.playerActorId)
		{
			return;
		}
		if (GRPlayer.Get(this.playerActorId) == null)
		{
			return;
		}
		if (!this.CanLocalPlayerPurchaseItem())
		{
			if (this.scanner != null)
			{
				UnityEvent onFailed = this.scanner.onFailed;
				if (onFailed == null)
				{
					return;
				}
				onFailed.Invoke();
			}
			return;
		}
		if (this.scanner != null)
		{
			UnityEvent onSucceeded = this.scanner.onSucceeded;
			if (onSucceeded != null)
			{
				onSucceeded.Invoke();
			}
		}
		bool flag;
		if (!this.reactor.grManager.DebugIsToolStationHacked() && (!this.toolProgressionManager.IsPartUnlocked(this.slot.PurchaseID, out flag) || !flag))
		{
			if (this.slot.drillUpgradeLevel == ProgressionManager.DrillUpgradeLevel.Base)
			{
				if (ProgressionManager.Instance.GetShinyRocksTotal() >= 2500)
				{
					ProgressionManager.Instance.PurchaseDrillUpgrade(ProgressionManager.DrillUpgradeLevel.Base);
					return;
				}
			}
			else
			{
				this.toolProgressionManager.AttemptToUnlockPart(this.slot.PurchaseID);
			}
		}
	}

	private bool CanLocalPlayerPurchaseItem()
	{
		return this.slot.canAfford;
	}

	public void RefreshItemInfo()
	{
		bool flag = true;
		if (this.toolProgressionManager != null)
		{
			GRToolProgressionManager.ToolProgressionMetaData partMetadata = this.toolProgressionManager.GetPartMetadata(this.slot.PurchaseID);
			if (partMetadata == null)
			{
				this.slot.Name.text = "ERROR";
				return;
			}
			string text = "ERROR";
			string text2 = "";
			Color white = Color.white;
			bool flag2 = true;
			int num = 10000;
			int num2;
			this.toolProgressionManager.GetPlayerShiftCredit(out num2);
			int numberOfResearchPoints = this.toolProgressionManager.GetNumberOfResearchPoints();
			this.slot.canAfford = false;
			this.slot.purchaseText = "LOCKED";
			if (this.slot.Description != null)
			{
				this.slot.Description.text = partMetadata.description;
			}
			bool flag3;
			if (this.toolProgressionManager.IsPartUnlocked(this.slot.PurchaseID, out flag3))
			{
				if (flag3)
				{
					if (this.slot.drillUpgradeLevel != ProgressionManager.DrillUpgradeLevel.None)
					{
						this.slot.Price.color = this.colorCanBuyCredits;
						this.slot.Price.fontSize = ((text.Length <= 8) ? 2.25f : 1.6f);
						this.slot.canAfford = true;
						this.slot.purchaseText = "Purchased";
						text = this.slot.purchaseText;
						this.slot.Price.text = text;
						return;
					}
					if (this.toolProgressionManager.GetShiftCreditCost(this.slot.PurchaseID, out num))
					{
						text = string.Format("⑭ {0}", num);
					}
					bool flag4 = num2 >= num;
					this.slot.Name.text = partMetadata.name;
					this.slot.Name.color = (flag ? this.colorSelectedItem : this.colorUnselectedItem);
					this.slot.Price.text = text;
					this.slot.Price.color = (flag4 ? this.colorCanBuyCredits : this.colorCantBuy);
					this.slot.Price.fontSize = ((text.Length <= 8) ? 2.25f : 1.6f);
					this.slot.canAfford = flag4;
					if (flag4)
					{
						this.slot.purchaseText = string.Format("BUY FOR\n⑭ {0}", num);
						return;
					}
					this.slot.purchaseText = string.Format("NEED\n⑭ {0}", num);
					return;
				}
				else
				{
					this.slot.Name.text = partMetadata.name;
					this.slot.Name.color = (flag ? this.colorUnresearchedItem : this.colorUnselectedUnresearchedItem);
					flag2 = true;
					GRToolProgressionTree.EmployeeLevelRequirement employeeLevelRequirement;
					if (this.toolProgressionManager.GetPartUnlockEmployeeRequiredLevel(this.slot.PurchaseID, out employeeLevelRequirement) && this.toolProgressionManager.GetCurrentEmployeeLevel() < employeeLevelRequirement)
					{
						this.toolProgressionManager.GetEmployeeLevelDisplayName(employeeLevelRequirement);
						text2 += string.Format("⑱ {0}\n", employeeLevelRequirement);
						flag2 = false;
					}
					this.cachedRequiredPartsList.Clear();
					if (this.toolProgressionManager.GetPartUnlockRequiredParentParts(this.slot.PurchaseID, out this.cachedRequiredPartsList))
					{
						foreach (GRToolProgressionManager.ToolParts toolParts in this.cachedRequiredPartsList)
						{
							bool flag5 = false;
							GRToolProgressionManager.ToolProgressionMetaData partMetadata2 = this.toolProgressionManager.GetPartMetadata(toolParts);
							if (partMetadata2 == null)
							{
								text2 += "⑱ ERROR\n";
								flag2 = false;
							}
							else if (!this.toolProgressionManager.IsPartUnlocked(toolParts, out flag5) || !flag5)
							{
								text2 = text2 + "⑱ " + partMetadata2.name + "\n";
								flag2 = false;
							}
						}
					}
					if (!flag2)
					{
						this.slot.Price.text = text2;
						this.slot.Price.color = this.colorCantBuy;
						this.slot.Price.fontSize = ((text2.Length <= 8) ? 2.25f : 1.6f);
						this.slot.canAfford = false;
						this.slot.purchaseText = "LOCKED";
						return;
					}
					if (this.slot.drillUpgradeLevel == ProgressionManager.DrillUpgradeLevel.Base)
					{
						this.slot.Price.color = this.colorCanBuyCredits;
						this.slot.Price.fontSize = ((text.Length <= 8) ? 2.25f : 1.6f);
						this.slot.canAfford = true;
						this.slot.purchaseText = string.Format("Cost {0}⑯ Shiny Rocks", 2500);
						text = this.slot.purchaseText;
						this.slot.Price.text = text;
						return;
					}
					if (this.toolProgressionManager.GetPartUnlockJuiceCost(this.slot.PurchaseID, out num))
					{
						text = string.Format("⑮ {0}", num);
					}
					bool flag4 = numberOfResearchPoints >= num;
					this.slot.Price.text = text;
					this.slot.Price.color = (flag4 ? this.colorCanBuyJuice : this.colorCantBuy);
					this.slot.Price.fontSize = ((text.Length <= 8) ? 2.25f : 1.6f);
					this.slot.canAfford = flag4;
					if (flag4)
					{
						this.slot.purchaseText = string.Format("RESEARCH\n⑮ {0}", num);
						return;
					}
					this.slot.purchaseText = string.Format("NEED\n⑮ {0}", num);
				}
			}
		}
	}

	public IDCardScanner scanner;

	public GRUIStoreDisplay.GRPurchaseSlot slot;

	private GhostReactor reactor;

	private GRToolProgressionManager toolProgressionManager;

	private int playerActorId;

	private Color colorPurchaseButtonCanAfford = GRToolUpgradePurchaseStationFull.ColorFromRGB32(0, 0, 0);

	private Color colorCanBuyCredits = GRToolUpgradePurchaseStationFull.ColorFromRGB32(140, 229, 37);

	private Color colorCanBuyJuice = GRToolUpgradePurchaseStationFull.ColorFromRGB32(232, 65, 255);

	private Color colorCantBuy = GRToolUpgradePurchaseStationFull.ColorFromRGB32(140, 38, 38);

	private Color colorSelectedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(251, 240, 229);

	private Color colorUnselectedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(147, 145, 140);

	private Color colorUnresearchedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(230, 19, 17);

	private Color colorUnselectedUnresearchedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(133, 11, 10);

	private List<GRToolProgressionManager.ToolParts> cachedRequiredPartsList = new List<GRToolProgressionManager.ToolParts>(5);

	[Serializable]
	public class GRPurchaseSlot
	{
		public TMP_Text Name;

		public TMP_Text Price;

		public TMP_Text Description;

		public GRToolProgressionManager.ToolParts PurchaseID;

		[NonSerialized]
		public Material overrideMaterial;

		[NonSerialized]
		public bool canAfford;

		[NonSerialized]
		public string purchaseText = "";

		public ProgressionManager.DrillUpgradeLevel drillUpgradeLevel;
	}
}
