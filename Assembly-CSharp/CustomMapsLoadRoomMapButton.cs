using System;
using System.Collections;
using GorillaTagScripts.VirtualStumpCustomMaps;
using UnityEngine;

public class CustomMapsLoadRoomMapButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		base.ButtonActivation();
		base.StartCoroutine(this.ButtonPressed_Local());
		if (CustomMapManager.CanLoadRoomMap())
		{
			CustomMapManager.ApproveAndLoadRoomMap();
		}
	}

	private IEnumerator ButtonPressed_Local()
	{
		this.isOn = true;
		this.UpdateColor();
		yield return new WaitForSeconds(this.pressedTime);
		this.isOn = false;
		this.UpdateColor();
		yield break;
	}

	[SerializeField]
	private float pressedTime = 0.2f;
}
