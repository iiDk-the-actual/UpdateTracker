using System;
using TMPro;
using UnityEngine;

namespace GorillaTagScripts.UI.ModIO
{
	public class VirtualStumpTeleportingHUD : MonoBehaviour
	{
		public void Initialize(bool isEntering)
		{
			this.isEnteringVirtualStump = isEntering;
			if (isEntering)
			{
				string text;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("VIRT_STUMP_HUD_ENTERING", out text, this.enteringVirtualStumpString))
				{
					Debug.LogError("[LOCALIZATION::VIRT_STUMP_TELEPORT_HUD] Failed to retrieve key [VIRT_STUMP_HUD_ENTERING] for locale [" + LocalisationManager.CurrentLanguage.LocaleName + "]");
				}
				this.teleportingStatusText.text = text;
				this.teleportingStatusText.gameObject.SetActive(true);
				return;
			}
			string text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("VIRT_STUMP_HUD_LEAVING", out text2, this.leavingVirtualStumpString))
			{
				Debug.LogError("[LOCALIZATION::VIRT_STUMP_TELEPORT_HUD] Failed to retrieve key [VIRT_STUMP_HUD_LEAVING] for locale [" + LocalisationManager.CurrentLanguage.LocaleName + "]");
			}
			this.teleportingStatusText.text = text2;
			this.teleportingStatusText.gameObject.SetActive(true);
		}

		private void Update()
		{
			if (Time.time - this.lastTextUpdateTime > this.textUpdateInterval)
			{
				this.lastTextUpdateTime = Time.time;
				this.IncrementProgressDots();
				this.teleportingStatusText.text = (this.isEnteringVirtualStump ? this.enteringVirtualStumpString : this.leavingVirtualStumpString);
				for (int i = 0; i < this.numProgressDots; i++)
				{
					TMP_Text tmp_Text = this.teleportingStatusText;
					tmp_Text.text += ".";
				}
			}
		}

		private void IncrementProgressDots()
		{
			this.numProgressDots++;
			if (this.numProgressDots > this.maxNumProgressDots)
			{
				this.numProgressDots = 0;
			}
		}

		private const string VIRT_STUMP_HUD_ENTERING_KEY = "VIRT_STUMP_HUD_ENTERING";

		private const string VIRT_STUMP_HUD_LEAVING_KEY = "VIRT_STUMP_HUD_LEAVING";

		[SerializeField]
		private string enteringVirtualStumpString = "Now Entering the Virtual Stump";

		[SerializeField]
		private string leavingVirtualStumpString = "Now Leaving the Virtual Stump";

		[SerializeField]
		private TMP_Text teleportingStatusText;

		[SerializeField]
		private int maxNumProgressDots = 3;

		[SerializeField]
		private float textUpdateInterval = 0.5f;

		private float lastTextUpdateTime;

		private int numProgressDots;

		private bool isEnteringVirtualStump;
	}
}
