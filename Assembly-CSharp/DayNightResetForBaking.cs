using System;
using UnityEngine;

public class DayNightResetForBaking : MonoBehaviour
{
	public void SetMaterialsForBaking()
	{
		foreach (Material material in this.dayNightManager.dayNightSupportedMaterials)
		{
			if (material != null)
			{
				material.shader = this.dayNightManager.standard;
			}
			else
			{
				Debug.LogError("a material is missing from day night supported materials in the daynightmanager! something might have gotten deleted inappropriately, or an entry should be manually removed.", base.gameObject);
			}
		}
		foreach (Material material2 in this.dayNightManager.dayNightSupportedMaterialsCutout)
		{
			if (material2 != null)
			{
				material2.shader = this.dayNightManager.standardCutout;
			}
			else
			{
				Debug.LogError("a material is missing from day night supported materials cutout in the daynightmanager! something might have gotten deleted inappropriately, or an entry should be manually removed.", base.gameObject);
			}
		}
	}

	public void SetMaterialsForGame()
	{
		foreach (Material material in this.dayNightManager.dayNightSupportedMaterials)
		{
			if (material != null)
			{
				material.shader = this.dayNightManager.gorillaUnlit;
			}
			else
			{
				Debug.LogError("a material is missing from day night supported materials in the daynightmanager! something might have gotten deleted inappropriately, or an entry should be manually removed.", base.gameObject);
			}
		}
		foreach (Material material2 in this.dayNightManager.dayNightSupportedMaterialsCutout)
		{
			if (material2 != null)
			{
				material2.shader = this.dayNightManager.gorillaUnlitCutout;
			}
			else
			{
				Debug.LogError("a material is missing from day night supported materialsc cutout in the daynightmanager! something might have gotten deleted inappropriately, or an entry should be manually removed.", base.gameObject);
			}
		}
	}

	public BetterDayNightManager dayNightManager;
}
