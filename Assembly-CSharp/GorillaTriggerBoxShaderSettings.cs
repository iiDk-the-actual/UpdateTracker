using System;
using GorillaTag.Rendering;
using UnityEngine;

public class GorillaTriggerBoxShaderSettings : GorillaTriggerBox
{
	private void Awake()
	{
		if (this.sameSceneSettingsRef != null)
		{
			this.settings = this.sameSceneSettingsRef;
			return;
		}
		this.settingsRef.TryResolve<ZoneShaderSettings>(out this.settings);
	}

	public override void OnBoxTriggered()
	{
		if (this.settings == null)
		{
			if (this.sameSceneSettingsRef != null)
			{
				this.settings = this.sameSceneSettingsRef;
			}
			else
			{
				this.settingsRef.TryResolve<ZoneShaderSettings>(out this.settings);
			}
		}
		if (this.settings != null)
		{
			this.settings.BecomeActiveInstance(false);
			return;
		}
		ZoneShaderSettings.ActivateDefaultSettings();
	}

	[SerializeField]
	private XSceneRef settingsRef;

	[SerializeField]
	private ZoneShaderSettings sameSceneSettingsRef;

	private ZoneShaderSettings settings;
}
