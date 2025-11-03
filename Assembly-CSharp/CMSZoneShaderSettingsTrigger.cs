using System;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaTag.Rendering;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CMSZoneShaderSettingsTrigger : MonoBehaviour
{
	public void OnEnable()
	{
		if (this.activateOnEnable)
		{
			this.ActivateShaderSettings();
		}
	}

	public void CopySettings(ZoneShaderTriggerSettings triggerSettings)
	{
		base.gameObject.layer = UnityLayer.GorillaBoundary.ToLayerIndex();
		this.activateOnEnable = triggerSettings.activateOnEnable;
		if (triggerSettings.activationType == ZoneShaderTriggerSettings.ActivationType.ActivateCustomMapDefaults)
		{
			this.activateCustomMapDefaults = true;
			return;
		}
		GameObject zoneShaderSettingsObject = triggerSettings.zoneShaderSettingsObject;
		if (zoneShaderSettingsObject.IsNotNull())
		{
			this.shaderSettingsObject = zoneShaderSettingsObject;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other == GTPlayer.Instance.bodyCollider)
		{
			this.ActivateShaderSettings();
		}
	}

	private void ActivateShaderSettings()
	{
		if (this.activateCustomMapDefaults)
		{
			CustomMapManager.ActivateDefaultZoneShaderSettings();
			return;
		}
		if (this.shaderSettingsObject.IsNotNull())
		{
			ZoneShaderSettings component = this.shaderSettingsObject.GetComponent<ZoneShaderSettings>();
			if (component.IsNotNull())
			{
				component.BecomeActiveInstance(false);
			}
		}
	}

	public GameObject shaderSettingsObject;

	public bool activateCustomMapDefaults;

	public bool activateOnEnable;
}
