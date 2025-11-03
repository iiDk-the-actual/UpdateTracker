using System;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag
{
	public class TemporaryCosmeticUnlocksEnableDisable : MonoBehaviour
	{
		private void Awake()
		{
			if (this.m_wardrobe.IsNull() || this.m_cosmeticAreaTrigger.IsNull())
			{
				Debug.LogError("TemporaryCosmeticUnlocksEnableDisable: reference is null, disabling self");
				base.enabled = false;
			}
			if (CosmeticsController.instance.IsNull() || !this.m_wardrobe.WardrobeButtonsInitialized())
			{
				base.enabled = false;
				this.m_timer = new TickSystemTimer(0.05f, new Action(this.CheckWardrobeRady));
				this.m_timer.Start();
			}
		}

		private void OnEnable()
		{
			bool tempUnlocksEnabled = PlayerCosmeticsSystem.TempUnlocksEnabled;
			this.m_wardrobe.UseTemporarySet = tempUnlocksEnabled;
			this.m_cosmeticAreaTrigger.SetActive(tempUnlocksEnabled);
		}

		private void CheckWardrobeRady()
		{
			if (CosmeticsController.instance.IsNotNull() && this.m_wardrobe.WardrobeButtonsInitialized())
			{
				this.m_timer.Stop();
				this.m_timer = null;
				base.enabled = true;
				return;
			}
			this.m_timer.Start();
		}

		[SerializeField]
		private CosmeticWardrobe m_wardrobe;

		[SerializeField]
		private GameObject m_cosmeticAreaTrigger;

		private TickSystemTimer m_timer;
	}
}
