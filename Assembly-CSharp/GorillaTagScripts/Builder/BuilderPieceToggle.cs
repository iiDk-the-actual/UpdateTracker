using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.Builder
{
	public class BuilderPieceToggle : MonoBehaviour, IBuilderPieceFunctional, IBuilderPieceComponent, IBuilderTappable
	{
		private void Awake()
		{
			this.colliders.Clear();
			if (this.toggleType == BuilderPieceToggle.ToggleType.OnTriggerEnter)
			{
				foreach (BuilderSmallHandTrigger builderSmallHandTrigger in this.handTriggers)
				{
					builderSmallHandTrigger.TriggeredEvent.AddListener(new UnityAction(this.OnHandTriggerEntered));
					Collider component = builderSmallHandTrigger.GetComponent<Collider>();
					if (component != null)
					{
						this.colliders.Add(component);
					}
				}
				foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.bodyTriggers)
				{
					builderSmallMonkeTrigger.onPlayerEnteredTrigger += this.OnBodyTriggerEntered;
					Collider component2 = builderSmallMonkeTrigger.GetComponent<Collider>();
					if (component2 != null)
					{
						this.colliders.Add(component2);
					}
				}
			}
		}

		private void OnDestroy()
		{
			foreach (BuilderSmallHandTrigger builderSmallHandTrigger in this.handTriggers)
			{
				if (!(builderSmallHandTrigger == null))
				{
					builderSmallHandTrigger.TriggeredEvent.RemoveListener(new UnityAction(this.OnHandTriggerEntered));
				}
			}
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.bodyTriggers)
			{
				if (!(builderSmallMonkeTrigger == null))
				{
					builderSmallMonkeTrigger.onPlayerEnteredTrigger -= this.OnBodyTriggerEntered;
				}
			}
		}

		private bool CanTap()
		{
			return (!this.onlySmallMonkeTaps || !this.myPiece.GetTable().isTableMutable || (double)VRRigCache.Instance.localRig.Rig.scaleFactor <= 0.99) && this.toggleType == BuilderPieceToggle.ToggleType.OnTap && this.myPiece.state == BuilderPiece.State.AttachedAndPlaced;
		}

		public void OnTapLocal(float tapStrength)
		{
			if (!this.CanTap())
			{
				Debug.Log("BuilderPieceToggle Can't Tap");
				return;
			}
			Debug.Log("Tap Local");
			this.ToggleStateRequest();
		}

		private bool CanTrigger()
		{
			return this.toggleType == BuilderPieceToggle.ToggleType.OnTriggerEnter && this.myPiece.state == BuilderPiece.State.AttachedAndPlaced;
		}

		private void OnHandTriggerEntered()
		{
			if (this.CanTrigger())
			{
				this.ToggleStateRequest();
				return;
			}
			Debug.Log("BuilderPieceToggle Can't Trigger");
		}

		private void OnBodyTriggerEntered(int playerNumber)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(playerNumber);
			if (player == null)
			{
				return;
			}
			if (this.CanTrigger())
			{
				this.ToggleStateMaster(player.GetPlayerRef());
				return;
			}
			Debug.Log("BuilderPieceToggle Can't Trigger");
		}

		private void ToggleStateRequest()
		{
			if (!NetworkSystem.Instance.InRoom)
			{
				return;
			}
			BuilderPieceToggle.ToggleStates toggleStates = ((this.toggleState == BuilderPieceToggle.ToggleStates.Off) ? BuilderPieceToggle.ToggleStates.On : BuilderPieceToggle.ToggleStates.Off);
			Debug.Log("BuilderPieceToggle" + string.Format(" Requesting state {0}", toggleStates));
			this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, (byte)toggleStates);
		}

		private void ToggleStateMaster(Player instigator)
		{
			BuilderPieceToggle.ToggleStates toggleStates = ((this.toggleState == BuilderPieceToggle.ToggleStates.Off) ? BuilderPieceToggle.ToggleStates.On : BuilderPieceToggle.ToggleStates.Off);
			Debug.Log("BuilderPieceToggle" + string.Format(" Set Master state {0}", toggleStates));
			this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, (byte)toggleStates, instigator, NetworkSystem.Instance.ServerTimestamp);
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!this.IsStateValid(newState))
			{
				Debug.Log("BuilderPieceToggle State Invalid");
				return;
			}
			Debug.Log("BuilderPieceToggle" + string.Format(" State Changed {0}", newState));
			if ((BuilderPieceToggle.ToggleStates)newState != this.toggleState)
			{
				if (newState == 1)
				{
					Debug.Log("BuilderPieceToggle Toggled On");
					UnityEvent toggledOn = this.ToggledOn;
					if (toggledOn != null)
					{
						toggledOn.Invoke();
					}
				}
				else
				{
					Debug.Log("BuilderPieceToggle Toggled Off");
					this.ToggledOff.Invoke();
				}
			}
			this.toggleState = (BuilderPieceToggle.ToggleStates)newState;
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (!this.IsStateValid(newState) || instigator == null)
			{
				Debug.Log("BuilderPieceToggle State Invalid or Player Null");
				return;
			}
			Debug.Log("BuilderPieceToggle" + string.Format(" State Request {0}", newState));
			if (newState != (byte)this.toggleState)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, newState, instigator.GetPlayerRef(), timeStamp);
				return;
			}
			Debug.Log("BuilderPieceToggle Same State");
		}

		public bool IsStateValid(byte state)
		{
			Debug.Log(string.Format("Is State Valid? {0}", state));
			return state <= 1;
		}

		public void FunctionalPieceUpdate()
		{
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
		}

		public void OnPieceDestroy()
		{
		}

		public void OnPiecePlacementDeserialized()
		{
		}

		public void OnPieceActivate()
		{
			foreach (Collider collider in this.colliders)
			{
				collider.enabled = true;
			}
		}

		public void OnPieceDeactivate()
		{
			this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
			foreach (Collider collider in this.colliders)
			{
				collider.enabled = false;
			}
		}

		[SerializeField]
		protected BuilderPiece myPiece;

		[SerializeField]
		private BuilderPieceToggle.ToggleType toggleType;

		public bool onlySmallMonkeTaps;

		[SerializeField]
		private BuilderSmallHandTrigger[] handTriggers;

		[SerializeField]
		private BuilderSmallMonkeTrigger[] bodyTriggers;

		[SerializeField]
		protected UnityEvent ToggledOn;

		[SerializeField]
		protected UnityEvent ToggledOff;

		private List<Collider> colliders = new List<Collider>(5);

		private BuilderPieceToggle.ToggleStates toggleState;

		[Serializable]
		private enum ToggleType
		{
			OnTap,
			OnTriggerEnter
		}

		private enum ToggleStates
		{
			Off,
			On
		}
	}
}
