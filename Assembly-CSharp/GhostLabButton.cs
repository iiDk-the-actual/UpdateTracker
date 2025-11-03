using System;
using UnityEngine;

public class GhostLabButton : GorillaPressableButton, IBuildValidation
{
	public bool BuildValidationCheck()
	{
		if (this.ghostLab == null)
		{
			Debug.LogError("ghostlab is missing", this);
			return false;
		}
		return true;
	}

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		this.ghostLab.DoorButtonPress(this.buttonIndex, this.forSingleDoor);
	}

	public GhostLab ghostLab;

	public int buttonIndex;

	public bool forSingleDoor;
}
