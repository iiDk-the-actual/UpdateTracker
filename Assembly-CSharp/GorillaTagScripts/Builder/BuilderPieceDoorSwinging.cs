using System;
using BoingKit;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.Builder
{
	public class BuilderPieceDoorSwinging : MonoBehaviour, IBuilderPieceComponent, IBuilderPieceFunctional
	{
		private void Awake()
		{
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.doorHoldTriggers)
			{
				builderSmallMonkeTrigger.onTriggerFirstEntered += this.OnHoldTriggerEntered;
				builderSmallMonkeTrigger.onTriggerLastExited += this.OnHoldTriggerExited;
			}
			this.frontTrigger.TriggeredEvent.AddListener(new UnityAction(this.OnFrontTriggerEntered));
			this.backTrigger.TriggeredEvent.AddListener(new UnityAction(this.OnBackTriggerEntered));
		}

		private void OnDestroy()
		{
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.doorHoldTriggers)
			{
				builderSmallMonkeTrigger.onTriggerFirstEntered -= this.OnHoldTriggerEntered;
				builderSmallMonkeTrigger.onTriggerLastExited -= this.OnHoldTriggerExited;
			}
			this.frontTrigger.TriggeredEvent.RemoveListener(new UnityAction(this.OnFrontTriggerEntered));
			this.backTrigger.TriggeredEvent.RemoveListener(new UnityAction(this.OnBackTriggerEntered));
		}

		private void OnFrontTriggerEntered()
		{
			if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
			{
				if (NetworkSystem.Instance.IsMasterClient)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 7, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 7);
			}
		}

		private void OnBackTriggerEntered()
		{
			if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
			{
				if (NetworkSystem.Instance.IsMasterClient)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 3, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 3);
			}
		}

		private void OnHoldTriggerEntered()
		{
			this.peopleInHoldOpenVolume = true;
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			BuilderPieceDoorSwinging.SwingingDoorState swingingDoorState = this.currentState;
			if (swingingDoorState != BuilderPieceDoorSwinging.SwingingDoorState.Closed)
			{
				if (swingingDoorState == BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut)
				{
					this.openSound.Play();
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 4, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				if (swingingDoorState != BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn)
				{
					return;
				}
				this.openSound.Play();
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 8, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
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
			if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn && !this.peopleInHoldOpenVolume)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 5, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				return;
			}
			if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut && !this.peopleInHoldOpenVolume)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
			}
		}

		private void SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState value)
		{
			bool flag = this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed;
			bool flag2 = value == BuilderPieceDoorSwinging.SwingingDoorState.Closed;
			this.currentState = value;
			if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
			{
				this.frontTrigger.enabled = true;
				this.backTrigger.enabled = true;
			}
			else
			{
				this.frontTrigger.enabled = false;
				this.backTrigger.enabled = false;
			}
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
			case BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn:
				if (Mathf.Abs(this.doorSpring.Value) < 1f && Mathf.Abs(this.doorSpring.Velocity) < this.doorClosedVelocityMag)
				{
					this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.Closed);
					return;
				}
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenIn:
				if (Time.time - this.tLastOpened > this.timeUntilDoorCloses)
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
						BuilderPieceDoorSwinging.SwingingDoorState swingingDoorState = ((this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.OpenIn) ? BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn : BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut);
						this.checkHoldTriggersTime = (double)(Time.time + this.checkHoldTriggersDelay);
						this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, (byte)swingingDoorState, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
						return;
					}
					BuilderPieceDoorSwinging.SwingingDoorState swingingDoorState2 = ((this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.OpenIn) ? BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn : BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut);
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, (byte)swingingDoorState2, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningOut:
				if (Mathf.Abs(this.doorSpring.Value) > 89f)
				{
					this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.OpenOut);
					return;
				}
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn:
				if ((double)Time.time > this.checkHoldTriggersTime)
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
						this.checkHoldTriggersTime = (double)(Time.time + this.checkHoldTriggersDelay);
						return;
					}
					BuilderPieceDoorSwinging.SwingingDoorState swingingDoorState3 = ((this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn) ? BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn : BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut);
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, (byte)swingingDoorState3, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				}
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningIn:
				if (Mathf.Abs(this.doorSpring.Value) > 89f)
				{
					this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.OpenIn);
					return;
				}
				break;
			default:
				return;
			}
		}

		private void UpdateDoorState()
		{
			switch (this.currentState)
			{
			case BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn:
				if (Mathf.Abs(this.doorSpring.Value) < 1f && Mathf.Abs(this.doorSpring.Velocity) < this.doorClosedVelocityMag)
				{
					this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.Closed);
				}
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenIn:
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningOut:
				if (Mathf.Abs(this.doorSpring.Value) > 89f)
				{
					this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.OpenOut);
					return;
				}
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningIn:
				if (Mathf.Abs(this.doorSpring.Value) > 89f)
				{
					this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.OpenIn);
					return;
				}
				break;
			default:
				return;
			}
		}

		private void CloseDoor()
		{
			switch (this.currentState)
			{
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut:
				this.closeSound.Play();
				this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut);
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn:
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningIn:
				break;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenIn:
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn:
				this.closeSound.Play();
				this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn);
				return;
			default:
				return;
			}
		}

		private void OpenDoor(bool openIn)
		{
			if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
			{
				this.tLastOpened = Time.time;
				this.openSound.Play();
				this.SetDoorState(openIn ? BuilderPieceDoorSwinging.SwingingDoorState.OpeningIn : BuilderPieceDoorSwinging.SwingingDoorState.OpeningOut);
			}
		}

		private void UpdateDoorAnimation()
		{
			switch (this.currentState)
			{
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningOut:
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut:
				this.doorSpring.TrackDampingRatio(-90f, 3.1415927f * this.doorOpenSpeed, 1f, Time.deltaTime);
				this.doorTransform.localRotation = Quaternion.Euler(this.rotateAxis * this.doorSpring.Value);
				if (this.isDoubleDoor && this.doorTransformB != null)
				{
					this.doorTransformB.localRotation = Quaternion.Euler(this.rotateAxisB * this.doorSpring.Value);
					return;
				}
				return;
			case BuilderPieceDoorSwinging.SwingingDoorState.OpenIn:
			case BuilderPieceDoorSwinging.SwingingDoorState.OpeningIn:
			case BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn:
				this.doorSpring.TrackDampingRatio(90f, 3.1415927f * this.doorOpenSpeed, 1f, Time.deltaTime);
				this.doorTransform.localRotation = Quaternion.Euler(this.rotateAxis * this.doorSpring.Value);
				if (this.isDoubleDoor && this.doorTransformB != null)
				{
					this.doorTransformB.localRotation = Quaternion.Euler(this.rotateAxisB * this.doorSpring.Value);
					return;
				}
				return;
			}
			this.doorSpring.TrackDampingRatio(0f, 3.1415927f * this.doorCloseSpeed, this.dampingRatio, Time.deltaTime);
			this.doorTransform.localRotation = Quaternion.Euler(this.rotateAxis * this.doorSpring.Value);
			if (this.isDoubleDoor && this.doorTransformB != null)
			{
				this.doorTransformB.localRotation = Quaternion.Euler(this.rotateAxisB * this.doorSpring.Value);
			}
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
			this.tLastOpened = 0f;
			this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.Closed);
			this.doorSpring.Reset();
			Collider[] array = this.triggerVolumes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
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
			this.SetDoorState(BuilderPieceDoorSwinging.SwingingDoorState.Closed);
			this.doorSpring.Reset();
			this.doorTransform.localRotation = Quaternion.Euler(this.rotateAxis * this.doorSpring.Value);
			if (this.isDoubleDoor && this.doorTransformB != null)
			{
				this.doorTransformB.localRotation = Quaternion.Euler(this.rotateAxisB * this.doorSpring.Value);
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
				if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.OpenOut || this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenOut)
				{
					this.CloseDoor();
				}
				break;
			case 3:
				if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
				{
					this.OpenDoor(false);
				}
				break;
			case 4:
				if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.ClosingOut)
				{
					this.openSound.Play();
				}
				break;
			case 5:
				if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.OpenIn || this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.HeldOpenIn)
				{
					this.CloseDoor();
				}
				break;
			case 7:
				if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
				{
					this.OpenDoor(true);
				}
				break;
			case 8:
				if (this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.ClosingIn)
				{
					this.openSound.Play();
				}
				break;
			}
			this.SetDoorState((BuilderPieceDoorSwinging.SwingingDoorState)newState);
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (this.IsStateValid(newState) && instigator != null && (newState == 7 || newState == 3) && this.currentState == BuilderPieceDoorSwinging.SwingingDoorState.Closed)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, newState, instigator.GetPlayerRef(), timeStamp);
			}
		}

		public bool IsStateValid(byte state)
		{
			return state <= 8;
		}

		public void FunctionalPieceUpdate()
		{
			if (this.myPiece != null && this.myPiece.state == BuilderPiece.State.AttachedAndPlaced)
			{
				if (!NetworkSystem.Instance.InRoom && this.currentState != BuilderPieceDoorSwinging.SwingingDoorState.Closed)
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

		[SerializeField]
		private Transform doorTransform;

		[SerializeField]
		private Collider[] triggerVolumes;

		[SerializeField]
		private BuilderSmallMonkeTrigger[] doorHoldTriggers;

		[SerializeField]
		private BuilderSmallHandTrigger frontTrigger;

		[SerializeField]
		private BuilderSmallHandTrigger backTrigger;

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

		[SerializeField]
		private float doorClosedVelocityMag = 30f;

		[SerializeField]
		private float dampingRatio = 0.5f;

		[Header("Double Door Settings")]
		[SerializeField]
		private bool isDoubleDoor;

		[SerializeField]
		private Vector3 rotateAxisB = Vector3.down;

		[SerializeField]
		private Transform doorTransformB;

		private BuilderPieceDoorSwinging.SwingingDoorState currentState;

		private float tLastOpened;

		private FloatSpring doorSpring;

		private bool peopleInHoldOpenVolume;

		private double checkHoldTriggersTime;

		private float checkHoldTriggersDelay = 3f;

		private int pushDirection = 1;

		private enum SwingingDoorState
		{
			Closed,
			ClosingOut,
			OpenOut,
			OpeningOut,
			HeldOpenOut,
			ClosingIn,
			OpenIn,
			OpeningIn,
			HeldOpenIn
		}
	}
}
