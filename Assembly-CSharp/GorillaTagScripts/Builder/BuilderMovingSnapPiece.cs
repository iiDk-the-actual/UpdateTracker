using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderMovingSnapPiece : MonoBehaviour, IBuilderPieceComponent, IBuilderPieceFunctional
	{
		private void Awake()
		{
			this.myPiece = base.GetComponent<BuilderPiece>();
			if (this.myPiece == null)
			{
				Debug.LogWarning("Missing BuilderPiece component " + base.gameObject.name);
			}
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				builderMovingPart.myPiece = this.myPiece;
			}
		}

		public int GetTimeOffset()
		{
			if (this.myPiece.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return 0;
			}
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				if (!builderMovingPart.IsAnchoredToTable())
				{
					return builderMovingPart.GetTimeOffsetMS();
				}
			}
			return 0;
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
		}

		public void OnPieceDestroy()
		{
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				builderMovingPart.OnPieceDestroy();
			}
		}

		public void OnPiecePlacementDeserialized()
		{
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				builderMovingPart.InitMovingGrid();
				builderMovingPart.SetMoving(false);
				if (this.myPiece.functionalPieceState == 0 && !builderMovingPart.IsAnchoredToTable())
				{
					this.currentPauseNode = builderMovingPart.GetStartNode();
				}
			}
			this.moving = false;
			if (!this.activated)
			{
				BuilderTable table = this.myPiece.GetTable();
				table.RegisterFunctionalPiece(this);
				table.RegisterFunctionalPieceFixedUpdate(this);
				this.activated = true;
			}
			this.OnStateChanged(this.myPiece.functionalPieceState, NetworkSystem.Instance.MasterClient, this.myPiece.activatedTimeStamp);
		}

		public void OnPieceActivate()
		{
			BuilderTable table = this.myPiece.GetTable();
			if (table.GetTableState() != BuilderTable.TableState.Ready && table.GetTableState() != BuilderTable.TableState.ExecuteQueuedCommands)
			{
				return;
			}
			if (!this.activated)
			{
				table.RegisterFunctionalPiece(this);
				table.RegisterFunctionalPieceFixedUpdate(this);
				this.activated = true;
			}
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				builderMovingPart.InitMovingGrid();
				if (!builderMovingPart.IsAnchoredToTable())
				{
					int num = 0;
					foreach (BuilderAttachGridPlane builderAttachGridPlane in builderMovingPart.myGridPlanes)
					{
						num += builderAttachGridPlane.GetChildCount();
					}
					if (num <= 5)
					{
						this.currentPauseNode = builderMovingPart.GetStartNode();
						if (this.myPiece.functionalPieceState > 0 && (int)this.myPiece.functionalPieceState < BuilderMovingPart.NUM_PAUSE_NODES * 2 + 1)
						{
							this.currentPauseNode = this.myPiece.functionalPieceState - 1;
						}
						this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.MasterClient, this.myPiece.activatedTimeStamp);
					}
					else
					{
						this.currentPauseNode = builderMovingPart.GetStartNode();
						if (this.myPiece.functionalPieceState > 0 && (int)this.myPiece.functionalPieceState < BuilderMovingPart.NUM_PAUSE_NODES * 2 + 1)
						{
							this.currentPauseNode = this.myPiece.functionalPieceState - 1;
						}
						this.myPiece.SetFunctionalPieceState(this.currentPauseNode + 1, NetworkSystem.Instance.MasterClient, this.myPiece.activatedTimeStamp);
					}
				}
			}
		}

		public void OnPieceDeactivate()
		{
			BuilderTable table = this.myPiece.GetTable();
			table.UnregisterFunctionalPiece(this);
			table.UnregisterFunctionalPieceFixedUpdate(this);
			this.myPiece.functionalPieceState = 0;
			this.moving = false;
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				builderMovingPart.SetMoving(false);
			}
			this.activated = false;
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!this.IsStateValid(newState))
			{
				return;
			}
			if (this.myPiece.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return;
			}
			if (!this.activated)
			{
				return;
			}
			if (newState == 0 && !this.moving)
			{
				this.moving = true;
				if (this.startMovingFX != null)
				{
					ObjectPools.instance.Instantiate(this.startMovingFX, base.transform.position, true);
				}
				using (List<BuilderMovingPart>.Enumerator enumerator = this.MovingParts.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BuilderMovingPart builderMovingPart = enumerator.Current;
						if (!builderMovingPart.IsAnchoredToTable())
						{
							builderMovingPart.ActivateAtNode(this.currentPauseNode, timeStamp);
							this.currentPauseNode = builderMovingPart.GetStartNode();
						}
					}
					return;
				}
			}
			if (this.moving && this.stopMovingFX != null)
			{
				ObjectPools.instance.Instantiate(this.stopMovingFX, base.transform.position, true);
			}
			this.moving = false;
			this.currentPauseNode = newState - 1;
			foreach (BuilderMovingPart builderMovingPart2 in this.MovingParts)
			{
				if (!builderMovingPart2.IsAnchoredToTable())
				{
					builderMovingPart2.PauseMovement(this.currentPauseNode);
				}
			}
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
		}

		public bool IsStateValid(byte state)
		{
			return (int)state <= BuilderMovingPart.NUM_PAUSE_NODES * 2 + 1;
		}

		public void FunctionalPieceUpdate()
		{
			this.UpdateMaster();
		}

		public void FunctionalPieceFixedUpdate()
		{
			if (!this.moving)
			{
				return;
			}
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				if (!builderMovingPart.IsAnchoredToTable())
				{
					builderMovingPart.UpdateMovingGrid();
				}
			}
		}

		private void UpdateMaster()
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.myPiece.GetTable();
			foreach (BuilderMovingPart builderMovingPart in this.MovingParts)
			{
				if (!builderMovingPart.IsAnchoredToTable())
				{
					int num = 0;
					foreach (BuilderAttachGridPlane builderAttachGridPlane in builderMovingPart.myGridPlanes)
					{
						num += builderAttachGridPlane.GetChildCount();
					}
					bool flag = num <= 5;
					if (flag && !this.moving)
					{
						table.builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 0, PhotonNetwork.MasterClient, NetworkSystem.Instance.ServerTimestamp);
					}
					if (!flag && this.moving)
					{
						byte b = builderMovingPart.GetNearestNode() + 1;
						table.builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, b, PhotonNetwork.MasterClient, NetworkSystem.Instance.ServerTimestamp);
					}
				}
			}
		}

		public List<BuilderMovingPart> MovingParts;

		public BuilderPiece myPiece;

		public const int MAX_MOVING_CHILDREN = 5;

		[SerializeField]
		private GameObject startMovingFX;

		[SerializeField]
		private GameObject stopMovingFX;

		private bool activated;

		private bool moving;

		private const byte MOVING_STATE = 0;

		private byte currentPauseNode;
	}
}
