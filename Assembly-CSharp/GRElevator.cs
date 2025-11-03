using System;
using System.Collections.Generic;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class GRElevator : MonoBehaviour
{
	private void OnEnable()
	{
		GRElevatorManager.RegisterElevator(this);
		this.ambientAudio.clip = this.ambientLoopClip;
		this.ambientAudio.Play();
	}

	private void OnDisable()
	{
		GRElevatorManager.DeregisterElevator(this);
	}

	private void Awake()
	{
		this.typeButtonDict = new Dictionary<GRElevator.ButtonType, GRElevatorButton>();
		for (int i = 0; i < this.elevatorButtons.Count; i++)
		{
			this.typeButtonDict.TryAdd(this.elevatorButtons[i].buttonType, this.elevatorButtons[i]);
		}
		this.travelDistance = (this.openTargetTop.position - this.closedTargetTop.position).magnitude;
		this.doorOpenSpeed = this.travelDistance / this.openTravelDuration;
		this.doorCloseSpeed = this.travelDistance / this.closeTravelDuration;
		this.state = GRElevator.ElevatorState.DoorClosed;
		this.UpdateLocalState(this.state);
	}

	public void PressButton(int type)
	{
		GRElevatorManager.ElevatorButtonPressed((GRElevator.ButtonType)type, this.location);
	}

	public void PressButtonVisuals(GRElevator.ButtonType type)
	{
		this.typeButtonDict[type].Pressed();
	}

	public void PlayDing()
	{
		this.ambientAudio.PlayOneShot(this.dingClip);
	}

	public void PlayButtonPress()
	{
		this.buttonBank.Play();
	}

	public void PlayElevatorMoving()
	{
		if (this.ambientAudio.isPlaying && this.ambientAudio.clip == this.travellingLoopClip)
		{
			return;
		}
		this.ambientAudio.clip = this.travellingLoopClip;
		this.ambientAudio.loop = true;
		this.ambientAudio.time = 0f;
		this.ambientAudio.Play();
	}

	public void PlayElevatorStopped()
	{
		if (this.ambientAudio.isPlaying && this.ambientAudio.clip == this.ambientLoopClip)
		{
			return;
		}
		this.ambientAudio.clip = this.ambientLoopClip;
		this.ambientAudio.loop = true;
		this.ambientAudio.time = 0f;
		this.ambientAudio.Play();
	}

	public void PlayElevatorMusic(float time = 0f)
	{
		if (this.musicAudio.isPlaying)
		{
			return;
		}
		this.musicAudio.time = time;
		this.musicAudio.Play();
	}

	public void PlayDoorOpenBegin()
	{
		this.doorAudio.clip = this.doorOpenClip;
		this.doorAudio.time = 0f;
		this.doorAudio.Play();
	}

	public void PlayDoorCloseBegin()
	{
		this.doorAudio.clip = this.doorCloseClip;
		this.doorAudio.time = 0f;
		this.doorAudio.Play();
	}

	public void PlayDoorOpenTravel()
	{
		this.doorAudio.time = this.adjustedOffsetTime + this.openBeginDuration;
	}

	public void PlayDoorCloseTravel()
	{
		this.doorAudio.time = this.adjustedOffsetTime + this.closeBeginDuration;
	}

	public bool DoorsFullyClosed()
	{
		return (this.upperDoor.position - this.closedTargetTop.position).sqrMagnitude < 0.0001f;
	}

	public bool DoorsFullyOpen()
	{
		return (this.upperDoor.position - this.openTargetTop.position).sqrMagnitude < 0.0001f;
	}

	public void UpdateLocalState(GRElevator.ElevatorState newState)
	{
		if (newState == this.state)
		{
			return;
		}
		this.state = newState;
		switch (newState)
		{
		case GRElevator.ElevatorState.DoorBeginClosing:
			if (this.DoorsFullyClosed())
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorClosed);
				return;
			}
			this.doorMoveBeginTime = Time.time;
			this.SetDoorClosedBeginTime();
			this.PlayDoorCloseBegin();
			return;
		case GRElevator.ElevatorState.DoorMovingClosing:
			this.PlayDoorCloseTravel();
			return;
		case GRElevator.ElevatorState.DoorEndClosing:
		case GRElevator.ElevatorState.DoorEndOpening:
			break;
		case GRElevator.ElevatorState.DoorClosed:
			this.upperDoor.position = this.closedTargetTop.position;
			this.lowerDoor.position = this.closedTargetBottom.position;
			return;
		case GRElevator.ElevatorState.DoorBeginOpening:
			if (this.DoorsFullyOpen())
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorOpen);
				return;
			}
			this.doorMoveBeginTime = Time.time;
			this.SetDoorOpenBeginTime();
			this.PlayDoorOpenBegin();
			return;
		case GRElevator.ElevatorState.DoorMovingOpening:
			this.PlayDoorOpenTravel();
			return;
		case GRElevator.ElevatorState.DoorOpen:
			this.upperDoor.position = this.openTargetTop.position;
			this.lowerDoor.position = this.openTargetBottom.position;
			break;
		default:
			return;
		}
	}

	public void UpdateRemoteState(GRElevator.ElevatorState remoteNewState)
	{
		if (GRElevator.StateIsOpeningState(remoteNewState) && GRElevator.StateIsClosingState(this.state))
		{
			this.UpdateLocalState(GRElevator.ElevatorState.DoorBeginOpening);
			return;
		}
		if (GRElevator.StateIsClosingState(remoteNewState) && GRElevator.StateIsOpeningState(this.state))
		{
			this.UpdateLocalState(GRElevator.ElevatorState.DoorBeginClosing);
		}
	}

	public void SetDoorOpenBeginTime()
	{
		float num = (this.travelDistance - (this.upperDoor.position - this.openTargetTop.position).magnitude) / this.travelDistance;
		this.adjustedOffsetTime = num * this.openTravelDuration;
	}

	public void SetDoorClosedBeginTime()
	{
		float num = (this.travelDistance - (this.upperDoor.position - this.closedTargetTop.position).magnitude) / this.travelDistance;
		this.adjustedOffsetTime = num * this.closeTravelDuration;
	}

	public static bool StateIsOpeningState(GRElevator.ElevatorState checkState)
	{
		return checkState == GRElevator.ElevatorState.DoorMovingOpening || checkState == GRElevator.ElevatorState.DoorBeginOpening || checkState == GRElevator.ElevatorState.DoorEndOpening || checkState == GRElevator.ElevatorState.DoorOpen;
	}

	public static bool StateIsClosingState(GRElevator.ElevatorState checkState)
	{
		return checkState == GRElevator.ElevatorState.DoorMovingClosing || checkState == GRElevator.ElevatorState.DoorBeginClosing || checkState == GRElevator.ElevatorState.DoorEndClosing || checkState == GRElevator.ElevatorState.DoorClosed;
	}

	public bool DoorIsOpening()
	{
		return GRElevator.StateIsOpeningState(this.state);
	}

	public bool DoorIsClosing()
	{
		return GRElevator.StateIsClosingState(this.state);
	}

	public void PhysicalElevatorUpdate()
	{
		switch (this.state)
		{
		case GRElevator.ElevatorState.DoorBeginClosing:
			if (Time.time > this.doorMoveBeginTime + this.closeBeginDuration)
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorMovingClosing);
			}
			break;
		case GRElevator.ElevatorState.DoorMovingClosing:
			if (Time.time > this.doorMoveBeginTime - this.adjustedOffsetTime + this.closeBeginDuration + this.closeTravelDuration)
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorEndClosing);
			}
			break;
		case GRElevator.ElevatorState.DoorEndClosing:
			if (Time.time > this.doorMoveBeginTime - this.adjustedOffsetTime + this.closeBeginDuration + this.closeTravelDuration + this.closeEndDuration)
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorClosed);
			}
			break;
		case GRElevator.ElevatorState.DoorBeginOpening:
			if (Time.time > this.doorMoveBeginTime + this.openBeginDuration)
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorMovingOpening);
			}
			break;
		case GRElevator.ElevatorState.DoorMovingOpening:
			if (Time.time > this.doorMoveBeginTime - this.adjustedOffsetTime + this.openBeginDuration + this.openTravelDuration)
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorEndOpening);
			}
			break;
		case GRElevator.ElevatorState.DoorEndOpening:
			if (Time.time > this.doorMoveBeginTime - this.adjustedOffsetTime + this.openBeginDuration + this.openTravelDuration + this.openEndDuration)
			{
				this.UpdateLocalState(GRElevator.ElevatorState.DoorOpen);
			}
			break;
		}
		GRElevator.ElevatorState elevatorState = this.state;
		Transform transform;
		Transform transform2;
		float num;
		if (elevatorState != GRElevator.ElevatorState.DoorMovingClosing)
		{
			if (elevatorState == GRElevator.ElevatorState.DoorMovingOpening)
			{
				transform = this.openTargetTop;
				transform2 = this.openTargetBottom;
				num = this.doorOpenSpeed;
			}
			else
			{
				transform = this.upperDoor;
				transform2 = this.lowerDoor;
				num = 1f;
			}
		}
		else
		{
			transform = this.closedTargetTop;
			transform2 = this.closedTargetBottom;
			num = this.doorCloseSpeed;
		}
		this.upperDoor.position = Vector3.MoveTowards(this.upperDoor.position, transform.position, Time.deltaTime * num);
		this.lowerDoor.position = Vector3.MoveTowards(this.lowerDoor.position, transform2.position, Time.deltaTime * num);
	}

	public GRElevatorManager.ElevatorLocation location;

	public Transform upperDoor;

	public Transform lowerDoor;

	public Transform closedTargetTop;

	public Transform closedTargetBottom;

	public Transform openTargetTop;

	public Transform openTargetBottom;

	public TextMeshPro outerText;

	public TextMeshPro innerText;

	public List<GRElevatorButton> elevatorButtons;

	private Dictionary<GRElevator.ButtonType, GRElevatorButton> typeButtonDict;

	public GorillaFriendCollider friendCollider;

	public GorillaNetworkJoinTrigger joinTrigger;

	public SoundBankPlayer buttonBank;

	public AudioSource doorAudio;

	public AudioSource ambientAudio;

	public AudioSource musicAudio;

	public AudioClip travellingLoopClip;

	public AudioClip ambientLoopClip;

	public AudioClip dingClip;

	public AudioClip doorOpenClip;

	public AudioClip doorCloseClip;

	public float adjustedOffsetTime;

	public float doorMoveBeginTime;

	public float doorOpenSpeed = 0.5f;

	public float doorCloseSpeed = 0.5f;

	public float closeBeginDuration;

	public float closeTravelDuration;

	public float closeEndDuration;

	public float openBeginDuration;

	public float openTravelDuration;

	public float openEndDuration;

	public float travelDistance;

	public GRElevator.ElevatorState state;

	public GameObject collidersAndVisuals;

	public enum ElevatorState
	{
		DoorBeginClosing,
		DoorMovingClosing,
		DoorEndClosing,
		DoorClosed,
		DoorBeginOpening,
		DoorMovingOpening,
		DoorEndOpening,
		DoorOpen,
		None
	}

	[Serializable]
	public enum ButtonType
	{
		Stump = 1,
		City,
		GhostReactor,
		Open,
		Close,
		Summon,
		MonkeBlocks,
		Count
	}
}
