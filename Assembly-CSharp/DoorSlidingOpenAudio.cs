using System;
using UnityEngine;

public class DoorSlidingOpenAudio : MonoBehaviour, IBuildValidation, ITickSystemTick
{
	bool ITickSystemTick.TickRunning { get; set; }

	private void OnEnable()
	{
		TickSystem<object>.AddCallbackTarget(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveCallbackTarget(this);
	}

	public bool BuildValidationCheck()
	{
		if (this.button == null)
		{
			Debug.LogError("reference button missing for doorslidingopenaudio", base.gameObject);
			return false;
		}
		if (this.audioSource == null)
		{
			Debug.LogError("missing audio source on doorslidingopenaudio", base.gameObject);
			return false;
		}
		return true;
	}

	void ITickSystemTick.Tick()
	{
		if (this.button.ghostLab.IsDoorMoving(this.button.forSingleDoor, this.button.buttonIndex))
		{
			if (!this.audioSource.isPlaying)
			{
				this.audioSource.time = 0f;
				this.audioSource.GTPlay();
				return;
			}
		}
		else if (this.audioSource.isPlaying)
		{
			this.audioSource.time = 0f;
			this.audioSource.GTStop();
		}
	}

	public GhostLabButton button;

	public AudioSource audioSource;
}
