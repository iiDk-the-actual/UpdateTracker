using System;
using GorillaLocomotion;
using UnityEngine;

public class RaceCheckpoint : MonoBehaviour
{
	public void Init(RaceCheckpointManager manager, int index)
	{
		this.manager = manager;
		this.checkpointIndex = index;
		this.SetIsCorrectCheckpoint(index == 0);
	}

	public void SetIsCorrectCheckpoint(bool isCorrect)
	{
		this.isCorrect = isCorrect;
		this.banner.sharedMaterial = (isCorrect ? this.activeCheckpointMat : this.wrongCheckpointMat);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other != GTPlayer.Instance.headCollider)
		{
			return;
		}
		if (this.isCorrect)
		{
			this.manager.OnCheckpointReached(this.checkpointIndex, this.checkpointSound);
			return;
		}
		this.wrongCheckpointSound.Play();
	}

	[SerializeField]
	private MeshRenderer banner;

	[SerializeField]
	private Material activeCheckpointMat;

	[SerializeField]
	private Material wrongCheckpointMat;

	[SerializeField]
	private SoundBankPlayer checkpointSound;

	[SerializeField]
	private SoundBankPlayer wrongCheckpointSound;

	private RaceCheckpointManager manager;

	private int checkpointIndex;

	private bool isCorrect;
}
