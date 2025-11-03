using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GRMetalEnergyGate : MonoBehaviour
{
	private void OnEnable()
	{
		this.tool.OnEnergyChange += this.OnEnergyChange;
		this.gameEntity.OnStateChanged += this.OnEntityStateChanged;
	}

	private void OnDisable()
	{
		if (this.tool != null)
		{
			this.tool.OnEnergyChange -= this.OnEnergyChange;
		}
		if (this.gameEntity != null)
		{
			this.gameEntity.OnStateChanged -= this.OnEntityStateChanged;
		}
	}

	private void OnEnergyChange(GRTool tool, int energyChange, GameEntityId chargingEntityId)
	{
		GameEntity gameEntity = this.gameEntity.manager.GetGameEntity(chargingEntityId);
		GRPlayer grplayer = null;
		if (gameEntity != null)
		{
			grplayer = GRPlayer.Get(gameEntity.heldByActorNumber);
		}
		if (grplayer != null)
		{
			grplayer.IncrementCoresSpentPlayer(energyChange);
		}
		if (this.state == GRMetalEnergyGate.State.Closed && tool.energy >= tool.GetEnergyMax())
		{
			if (grplayer != null)
			{
				grplayer.IncrementGatesUnlocked(1);
			}
			this.SetState(GRMetalEnergyGate.State.Open);
			if (this.gameEntity.IsAuthority())
			{
				this.gameEntity.RequestState(this.gameEntity.id, 1L);
			}
		}
	}

	private void OnEntityStateChanged(long prevState, long nextState)
	{
		if (!this.gameEntity.IsAuthority())
		{
			this.SetState((GRMetalEnergyGate.State)nextState);
		}
	}

	public void SetState(GRMetalEnergyGate.State newState)
	{
		if (this.state != newState)
		{
			this.state = newState;
			GRMetalEnergyGate.State state = this.state;
			if (state != GRMetalEnergyGate.State.Closed)
			{
				if (state == GRMetalEnergyGate.State.Open)
				{
					this.audioSource.PlayOneShot(this.doorOpenClip);
					for (int i = 0; i < this.enableObjectsOnOpen.Count; i++)
					{
						this.enableObjectsOnOpen[i].gameObject.SetActive(true);
					}
					for (int j = 0; j < this.disableObjectsOnOpen.Count; j++)
					{
						this.disableObjectsOnOpen[j].gameObject.SetActive(false);
					}
				}
			}
			else
			{
				this.audioSource.PlayOneShot(this.doorCloseClip);
				for (int k = 0; k < this.enableObjectsOnOpen.Count; k++)
				{
					this.enableObjectsOnOpen[k].gameObject.SetActive(false);
				}
				for (int l = 0; l < this.disableObjectsOnOpen.Count; l++)
				{
					this.disableObjectsOnOpen[l].gameObject.SetActive(true);
				}
			}
			if (this.doorAnimationCoroutine == null)
			{
				this.doorAnimationCoroutine = base.StartCoroutine(this.UpdateDoorAnimation());
			}
		}
	}

	public void OpenGate()
	{
		this.SetState(GRMetalEnergyGate.State.Open);
	}

	public void CloseGate()
	{
		this.SetState(GRMetalEnergyGate.State.Closed);
	}

	private IEnumerator UpdateDoorAnimation()
	{
		while ((this.state == GRMetalEnergyGate.State.Open && this.openProgress < 1f) || (this.state == GRMetalEnergyGate.State.Closed && this.openProgress > 0f))
		{
			GRMetalEnergyGate.State state = this.state;
			if (state != GRMetalEnergyGate.State.Closed)
			{
				if (state == GRMetalEnergyGate.State.Open)
				{
					this.openProgress = Mathf.MoveTowards(this.openProgress, 1f, Time.deltaTime / this.doorOpenTime);
					float num = this.doorOpenCurve.Evaluate(this.openProgress);
					this.upperDoor.doorTransform.localPosition = Vector3.Lerp(this.upperDoor.doorClosedPosition.localPosition, this.upperDoor.doorOpenPosition.localPosition, num);
					this.lowerDoor.doorTransform.localPosition = Vector3.Lerp(this.lowerDoor.doorClosedPosition.localPosition, this.lowerDoor.doorOpenPosition.localPosition, num);
				}
			}
			else
			{
				this.openProgress = Mathf.MoveTowards(this.openProgress, 0f, Time.deltaTime / this.doorOpenTime);
				float num2 = this.doorCloseCurve.Evaluate(this.openProgress);
				this.upperDoor.doorTransform.localPosition = Vector3.Lerp(this.upperDoor.doorClosedPosition.localPosition, this.upperDoor.doorOpenPosition.localPosition, num2);
				this.lowerDoor.doorTransform.localPosition = Vector3.Lerp(this.lowerDoor.doorClosedPosition.localPosition, this.lowerDoor.doorOpenPosition.localPosition, num2);
			}
			yield return null;
		}
		this.doorAnimationCoroutine = null;
		yield break;
	}

	[SerializeField]
	public GRMetalEnergyGate.DoorParams upperDoor;

	[SerializeField]
	public GRMetalEnergyGate.DoorParams lowerDoor;

	[SerializeField]
	private float doorOpenTime = 1.5f;

	[SerializeField]
	private float doorCloseTime = 1.5f;

	[SerializeField]
	private AnimationCurve doorOpenCurve;

	[SerializeField]
	private AnimationCurve doorCloseCurve;

	[SerializeField]
	private AudioClip doorOpenClip;

	[SerializeField]
	private AudioClip doorCloseClip;

	[SerializeField]
	private List<Transform> enableObjectsOnOpen = new List<Transform>();

	[SerializeField]
	private List<Transform> disableObjectsOnOpen = new List<Transform>();

	[SerializeField]
	private GRTool tool;

	[SerializeField]
	private GameEntity gameEntity;

	[SerializeField]
	private AudioSource audioSource;

	public GRMetalEnergyGate.State state;

	private float openProgress;

	private Coroutine doorAnimationCoroutine;

	public enum State
	{
		Closed,
		Open
	}

	[Serializable]
	public struct DoorParams
	{
		public Transform doorTransform;

		public Transform doorClosedPosition;

		public Transform doorOpenPosition;
	}
}
