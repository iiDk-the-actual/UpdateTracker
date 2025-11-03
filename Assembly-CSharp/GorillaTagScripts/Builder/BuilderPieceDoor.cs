using System;
using BoingKit;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.Builder
{
	public class BuilderPieceDoor : MonoBehaviour, IBuilderPieceComponent, IBuilderPieceFunctional
	{
		private void Awake()
		{
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.doorHoldTriggers)
			{
				builderSmallMonkeTrigger.onTriggerFirstEntered += this.OnHoldTriggerEntered;
				builderSmallMonkeTrigger.onTriggerLastExited += this.OnHoldTriggerExited;
			}
			BuilderSmallHandTrigger[] array2 = this.doorButtonTriggers;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].TriggeredEvent.AddListener(new UnityAction(this.OnDoorButtonTriggered));
			}
		}

		private void OnDestroy()
		{
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.doorHoldTriggers)
			{
				builderSmallMonkeTrigger.onTriggerFirstEntered -= this.OnHoldTriggerEntered;
				builderSmallMonkeTrigger.onTriggerLastExited -= this.OnHoldTriggerExited;
			}
			BuilderSmallHandTrigger[] array2 = this.doorButtonTriggers;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].TriggeredEvent.RemoveListener(new UnityAction(this.OnDoorButtonTriggered));
			}
		}

		private void SetDoorState(BuilderPieceDoor.DoorState value)
		{
			bool flag = this.currentState == BuilderPieceDoor.DoorState.Closed || (this.currentState == BuilderPieceDoor.DoorState.Open && this.IsToggled);
			bool flag2 = value == BuilderPieceDoor.DoorState.Closed || (value == BuilderPieceDoor.DoorState.Open && this.IsToggled);
			this.currentState = value;
			if (flag != flag2)
			{
				if (flag2)
				{
					this.myPiece.GetTable().UnregisterFunctionalPiece(this);
					return;
				}
				this.myPiece.GetTable().RegisterFunctionalPiece(this);
			}
		}

		private void UpdateDoorStateMaster()
		{
			switch (this.currentState)
			{
			case BuilderPieceDoor.DoorState.Closing:
				if (this.doorSpring.Value < 1f)
				{
					this.doorSpring.Reset();
					this.doorTransform.localRotation = Quaternion.identity;
					if (this.isDoubleDoor && this.doorTransformB != null)
					{
						this.doorTransformB.localRotation = Quaternion.identity;
					}
					this.SetDoorState(BuilderPieceDoor.DoorState.Closed);
					return;
				}
				break;
			case BuilderPieceDoor.DoorState.Open:
				if (!this.IsToggled && Time.time - this.tLastOpened > this.timeUntilDoorCloses)
				{
					this.peopleInHoldOpenVolume = false;
					foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.doorHoldTriggers)
					{
						builderSmallMonkeTrigger.ValidateOverlappingColliders();
						if (builderSmallMonkeTrigger.overlapCount > 0)
						{
							this.peopleInHoldOpenVolume = true;
							break;
						}
					}
					if (this.peopleInHoldOpenVolume)
					{
						this.CheckHoldTriggersTime = (double)(Time.time + this.checkHoldTriggersDelay);
						this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 4, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
						return;
					}
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				break;
			case BuilderPieceDoor.DoorState.Opening:
				if (this.doorSpring.Value > 89f)
				{
					this.SetDoorState(BuilderPieceDoor.DoorState.Open);
					return;
				}
				break;
			case BuilderPieceDoor.DoorState.HeldOpen:
				if (!this.IsToggled && (double)Time.time > this.CheckHoldTriggersTime)
				{
					foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger2 in this.doorHoldTriggers)
					{
						builderSmallMonkeTrigger2.ValidateOverlappingColliders();
						if (builderSmallMonkeTrigger2.overlapCount > 0)
						{
							this.peopleInHoldOpenVolume = true;
							break;
						}
					}
					if (this.peopleInHoldOpenVolume)
					{
						this.CheckHoldTriggersTime = (double)(Time.time + this.checkHoldTriggersDelay);
						return;
					}
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				}
				break;
			default:
				return;
			}
		}

		private void UpdateDoorState()
		{
			BuilderPieceDoor.DoorState doorState = this.currentState;
			if (doorState != BuilderPieceDoor.DoorState.Closing)
			{
				if (doorState == BuilderPieceDoor.DoorState.Opening && this.doorSpring.Value > 89f)
				{
					this.SetDoorState(BuilderPieceDoor.DoorState.Open);
					return;
				}
			}
			else if (this.doorSpring.Value < 1f)
			{
				this.doorSpring.Reset();
				this.doorTransform.localRotation = Quaternion.identity;
				if (this.isDoubleDoor && this.doorTransformB != null)
				{
					this.doorTransformB.localRotation = Quaternion.identity;
				}
				this.SetDoorState(BuilderPieceDoor.DoorState.Closed);
			}
		}

		private void CloseDoor()
		{
			switch (this.currentState)
			{
			case BuilderPieceDoor.DoorState.Closed:
			case BuilderPieceDoor.DoorState.Closing:
			case BuilderPieceDoor.DoorState.Opening:
				return;
			case BuilderPieceDoor.DoorState.Open:
			case BuilderPieceDoor.DoorState.HeldOpen:
				this.closeSound.Play();
				this.SetDoorState(BuilderPieceDoor.DoorState.Closing);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private void OpenDoor()
		{
			BuilderPieceDoor.DoorState doorState = this.currentState;
			if (doorState == BuilderPieceDoor.DoorState.Closed)
			{
				this.tLastOpened = Time.time;
				this.openSound.Play();
				this.SetDoorState(BuilderPieceDoor.DoorState.Opening);
				return;
			}
			if (doorState - BuilderPieceDoor.DoorState.Closing > 3)
			{
				throw new ArgumentOutOfRangeException();
			}
		}

		private void UpdateDoorAnimation()
		{
			BuilderPieceDoor.DoorState doorState = this.currentState;
			if (doorState > BuilderPieceDoor.DoorState.Closing && doorState - BuilderPieceDoor.DoorState.Open <= 2)
			{
				this.doorSpring.TrackDampingRatio(90f, 3.1415927f * this.doorOpenSpeed, 1f, Time.deltaTime);
				this.doorTransform.localRotation = Quaternion.Euler(this.rotateAxis * this.doorSpring.Value);
				if (this.isDoubleDoor && this.doorTransformB != null)
				{
					this.doorTransformB.localRotation = Quaternion.Euler(this.rotateAxisB * this.doorSpring.Value);
					return;
				}
			}
			else
			{
				this.doorSpring.TrackDampingRatio(0f, 3.1415927f * this.doorCloseSpeed, 1f, Time.deltaTime);
				this.doorTransform.localRotation = Quaternion.Euler(this.rotateAxis * this.doorSpring.Value);
				if (this.isDoubleDoor && this.doorTransformB != null)
				{
					this.doorTransformB.localRotation = Quaternion.Euler(this.rotateAxisB * this.doorSpring.Value);
				}
			}
		}

		private void OnDoorButtonTriggered()
		{
			BuilderPieceDoor.DoorState doorState = this.currentState;
			if (doorState != BuilderPieceDoor.DoorState.Closed)
			{
				if (doorState != BuilderPieceDoor.DoorState.Open)
				{
					return;
				}
				if (this.IsToggled)
				{
					if (NetworkSystem.Instance.IsMasterClient)
					{
						this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
						return;
					}
					this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 1);
				}
				return;
			}
			else
			{
				if (NetworkSystem.Instance.IsMasterClient)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 3, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 3);
				return;
			}
		}

		private void OnHoldTriggerEntered()
		{
			this.peopleInHoldOpenVolume = true;
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			BuilderPieceDoor.DoorState doorState = this.currentState;
			if (doorState != BuilderPieceDoor.DoorState.Closed)
			{
				if (doorState != BuilderPieceDoor.DoorState.Closing)
				{
					return;
				}
				if (!this.IsToggled)
				{
					this.openSound.Play();
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 4, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				}
			}
			else if (this.isAutomatic)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 3, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				return;
			}
		}

		private void OnHoldTriggerExited()
		{
			this.peopleInHoldOpenVolume = false;
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.doorHoldTriggers)
			{
				builderSmallMonkeTrigger.ValidateOverlappingColliders();
				if (builderSmallMonkeTrigger.overlapCount > 0)
				{
					this.peopleInHoldOpenVolume = true;
					break;
				}
			}
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (this.currentState == BuilderPieceDoor.DoorState.HeldOpen && !this.peopleInHoldOpenVolume)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
			}
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
			this.tLastOpened = 0f;
			this.SetDoorState(BuilderPieceDoor.DoorState.Closed);
			this.doorSpring.Reset();
			this.doorTransform.localRotation = Quaternion.identity;
			if (this.isDoubleDoor && this.doorTransformB != null)
			{
				this.doorTransformB.localRotation = Quaternion.identity;
			}
			Collider[] array = this.triggerVolumes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			if (this.lineRenderers != null)
			{
				LineRenderer[] array2 = this.lineRenderers;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].widthMultiplier = this.myPiece.GetScale();
				}
			}
		}

		public void OnPieceDestroy()
		{
		}

		public void OnPiecePlacementDeserialized()
		{
		}

		public void OnPieceActivate()
		{
			Collider[] array = this.triggerVolumes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}

		public void OnPieceDeactivate()
		{
			Collider[] array = this.triggerVolumes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			this.myPiece.functionalPieceState = 0;
			this.SetDoorState(BuilderPieceDoor.DoorState.Closed);
			this.doorSpring.Reset();
			this.doorTransform.localRotation = Quaternion.identity;
			if (this.isDoubleDoor && this.doorTransformB != null)
			{
				this.doorTransformB.localRotation = Quaternion.identity;
			}
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (this.IsStateValid(newState) && instigator != null && this.currentState != (BuilderPieceDoor.DoorState)newState)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, newState, instigator.GetPlayerRef(), timeStamp);
			}
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!this.IsStateValid(newState))
			{
				return;
			}
			switch (newState)
			{
			case 1:
				if (this.currentState == BuilderPieceDoor.DoorState.Open || this.currentState == BuilderPieceDoor.DoorState.HeldOpen)
				{
					this.CloseDoor();
				}
				break;
			case 3:
				if (this.currentState == BuilderPieceDoor.DoorState.Closed)
				{
					this.OpenDoor();
				}
				break;
			case 4:
				if (this.currentState == BuilderPieceDoor.DoorState.Closing)
				{
					this.openSound.Play();
				}
				break;
			}
			this.SetDoorState((BuilderPieceDoor.DoorState)newState);
		}

		public bool IsStateValid(byte state)
		{
			return state < 5;
		}

		public void FunctionalPieceUpdate()
		{
			if (this.myPiece != null && this.myPiece.state == BuilderPiece.State.AttachedAndPlaced)
			{
				if (!NetworkSystem.Instance.InRoom && this.currentState != BuilderPieceDoor.DoorState.Closed)
				{
					this.CloseDoor();
				}
				else if (NetworkSystem.Instance.IsMasterClient)
				{
					this.UpdateDoorStateMaster();
				}
				else
				{
					this.UpdateDoorState();
				}
				this.UpdateDoorAnimation();
			}
		}

		[SerializeField]
		private BuilderPiece myPiece;

		[SerializeField]
		private Vector3 rotateAxis = Vector3.up;

		[Tooltip("True if the door stays open until the button is triggered again")]
		[SerializeField]
		private bool IsToggled;

		[Tooltip("True if the door opens when players enter the Keep Open Trigger")]
		[SerializeField]
		private bool isAutomatic;

		[SerializeField]
		private Transform doorTransform;

		[SerializeField]
		private Collider[] triggerVolumes;

		[SerializeField]
		private BuilderSmallHandTrigger[] doorButtonTriggers;

		[SerializeField]
		private BuilderSmallMonkeTrigger[] doorHoldTriggers;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private SoundBankPlayer openSound;

		[SerializeField]
		private SoundBankPlayer closeSound;

		[SerializeField]
		private float doorOpenSpeed = 1f;

		[SerializeField]
		private float doorCloseSpeed = 1f;

		[SerializeField]
		[Range(1.5f, 10f)]
		private float timeUntilDoorCloses = 3f;

		[Header("Double Door Settings")]
		[SerializeField]
		private bool isDoubleDoor;

		[SerializeField]
		private Vector3 rotateAxisB = Vector3.down;

		[SerializeField]
		private Transform doorTransformB;

		[SerializeField]
		private LineRenderer[] lineRenderers;

		private BuilderPieceDoor.DoorState currentState;

		private float tLastOpened;

		private FloatSpring doorSpring;

		private bool peopleInHoldOpenVolume;

		private double CheckHoldTriggersTime;

		private float checkHoldTriggersDelay = 3f;

		public enum DoorState
		{
			Closed,
			Closing,
			Open,
			Opening,
			HeldOpen
		}
	}
}
