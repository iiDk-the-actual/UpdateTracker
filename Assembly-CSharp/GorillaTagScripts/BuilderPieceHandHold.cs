using System;
using System.Collections.Generic;
using GorillaLocomotion.Gameplay;
using UnityEngine;

namespace GorillaTagScripts
{
	[RequireComponent(typeof(Collider))]
	public class BuilderPieceHandHold : MonoBehaviour, IGorillaGrabable, IBuilderPieceComponent, ITickSystemTick
	{
		private void Initialize()
		{
			if (this.initialized)
			{
				return;
			}
			this.myCollider = base.GetComponent<Collider>();
			this.initialized = true;
		}

		public bool IsHandHoldMoving()
		{
			return this.myPiece.IsPieceMoving();
		}

		public bool MomentaryGrabOnly()
		{
			return this.forceMomentary;
		}

		public virtual bool CanBeGrabbed(GorillaGrabber grabber)
		{
			return this.myPiece.state == BuilderPiece.State.AttachedAndPlaced && (!this.myPiece.GetTable().isTableMutable || grabber.Player.scale < 0.5f);
		}

		public void OnGrabbed(GorillaGrabber grabber, out Transform grabbedTransform, out Vector3 localGrabbedPosition)
		{
			this.Initialize();
			grabbedTransform = base.transform;
			Vector3 position = grabber.transform.position;
			localGrabbedPosition = base.transform.InverseTransformPoint(position);
			this.activeGrabbers.Add(grabber);
			this.isGrabbed = true;
			Vector3 vector;
			grabber.Player.AddHandHold(base.transform, localGrabbedPosition, grabber, grabber.IsRightHand, false, out vector);
		}

		public void OnGrabReleased(GorillaGrabber grabber)
		{
			this.Initialize();
			this.activeGrabbers.Remove(grabber);
			this.isGrabbed = this.activeGrabbers.Count < 1;
			grabber.Player.RemoveHandHold(grabber, grabber.IsRightHand);
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (!this.isGrabbed)
			{
				return;
			}
			foreach (GorillaGrabber gorillaGrabber in this.activeGrabbers)
			{
				if (gorillaGrabber != null && gorillaGrabber.Player.scale > 0.5f)
				{
					this.OnGrabReleased(gorillaGrabber);
				}
			}
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
			if (!this.TickRunning && this.myPiece.GetTable().isTableMutable)
			{
				TickSystem<object>.AddCallbackTarget(this);
			}
		}

		public void OnPieceDeactivate()
		{
			if (this.TickRunning)
			{
				TickSystem<object>.RemoveCallbackTarget(this);
			}
			foreach (GorillaGrabber gorillaGrabber in this.activeGrabbers)
			{
				this.OnGrabReleased(gorillaGrabber);
			}
		}

		string IGorillaGrabable.get_name()
		{
			return base.name;
		}

		private bool initialized;

		private Collider myCollider;

		[SerializeField]
		private bool forceMomentary = true;

		[SerializeField]
		private BuilderPiece myPiece;

		private List<GorillaGrabber> activeGrabbers = new List<GorillaGrabber>(2);

		private bool isGrabbed;
	}
}
