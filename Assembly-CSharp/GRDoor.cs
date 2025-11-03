using System;
using UnityEngine;

[Serializable]
public class GRDoor
{
	public void Setup()
	{
		this.doorState = GRDoor.DoorState.Closed;
	}

	public void SetDoorState(GRDoor.DoorState newState)
	{
		if (newState == this.doorState)
		{
			return;
		}
		this.doorState = newState;
		if (this.doorState == GRDoor.DoorState.Closed)
		{
			this.animation.clip = this.closeAnim;
			this.animation.Play();
			this.closeDoorSound.Play(null);
			return;
		}
		this.animation.clip = this.openAnim;
		this.animation.Play();
		this.openDoorSound.Play(null);
	}

	public GRDoor.DoorState doorState;

	public Animation animation;

	public AnimationClip openAnim;

	public AnimationClip closeAnim;

	public AbilitySound openDoorSound;

	public AbilitySound closeDoorSound;

	public enum DoorState
	{
		Closed,
		Open
	}
}
