using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class Mole : Tappable
	{
		public event Mole.MoleTapEvent OnTapped;

		public bool IsLeftSideMole { get; set; }

		private void Awake()
		{
			this.currentState = Mole.MoleState.Hidden;
			Vector3 position = base.transform.position;
			this.origin = (this.target = position);
			this.visiblePosition = new Vector3(position.x, position.y + this.positionOffset, position.z);
			this.hiddenPosition = new Vector3(position.x, position.y - this.positionOffset, position.z);
			this.travelTime = this.normalTravelTime;
			this.animCurve = (this.normalAnimCurve = AnimationCurves.EaseInOutQuad);
			this.hitAnimCurve = AnimationCurves.EaseOutBack;
			for (int i = 0; i < this.moleTypes.Length; i++)
			{
				if (this.moleTypes[i].isHazard)
				{
					this.hazardMoles.Add(i);
				}
				else
				{
					this.safeMoles.Add(i);
				}
			}
			this.randomMolePickedIndex = -1;
		}

		public void InvokeUpdate()
		{
			if (this.currentState == Mole.MoleState.Ready)
			{
				return;
			}
			switch (this.currentState)
			{
			case Mole.MoleState.Reset:
			case Mole.MoleState.Hidden:
				this.currentState = Mole.MoleState.Ready;
				break;
			case Mole.MoleState.TransitionToVisible:
			case Mole.MoleState.TransitionToHidden:
			{
				float num = this.animCurve.Evaluate(Mathf.Clamp01((Time.time - this.animStartTime) / this.travelTime));
				base.transform.position = Vector3.Lerp(this.origin, this.target, num);
				if (num >= 1f)
				{
					this.currentState++;
				}
				break;
			}
			}
			if (Time.time - this.currentTime >= this.showMoleDuration && this.currentState > Mole.MoleState.Ready && this.currentState < Mole.MoleState.TransitionToHidden)
			{
				this.HideMole(false);
			}
		}

		public bool CanPickMole()
		{
			return this.currentState == Mole.MoleState.Ready;
		}

		public void ShowMole(float _showMoleDuration, int randomMoleTypeIndex)
		{
			if (randomMoleTypeIndex >= this.moleTypes.Length || randomMoleTypeIndex < 0)
			{
				return;
			}
			this.randomMolePickedIndex = randomMoleTypeIndex;
			for (int i = 0; i < this.moleTypes.Length; i++)
			{
				this.moleTypes[i].gameObject.SetActive(i == randomMoleTypeIndex);
				if (this.moleTypes[i].monkeMoleDefaultMaterial != null)
				{
					this.moleTypes[i].MeshRenderer.material = this.moleTypes[i].monkeMoleDefaultMaterial;
				}
			}
			this.showMoleDuration = _showMoleDuration;
			this.origin = base.transform.position;
			this.target = this.visiblePosition;
			this.animCurve = this.normalAnimCurve;
			this.currentState = Mole.MoleState.TransitionToVisible;
			this.animStartTime = (this.currentTime = Time.time);
			this.travelTime = this.normalTravelTime;
		}

		public void HideMole(bool isHit = false)
		{
			if (this.currentState < Mole.MoleState.TransitionToVisible || this.currentState > Mole.MoleState.Visible)
			{
				return;
			}
			this.origin = base.transform.position;
			this.target = this.hiddenPosition;
			this.animCurve = (isHit ? this.hitAnimCurve : this.normalAnimCurve);
			this.animStartTime = Time.time;
			this.travelTime = (isHit ? this.hitTravelTime : this.normalTravelTime);
			this.currentState = Mole.MoleState.TransitionToHidden;
		}

		public bool CanTap()
		{
			Mole.MoleState moleState = this.currentState;
			return moleState == Mole.MoleState.TransitionToVisible || moleState == Mole.MoleState.Visible;
		}

		public override bool CanTap(bool isLeftHand)
		{
			return this.CanTap();
		}

		public override void OnTapLocal(float tapStrength, float tapTime, PhotonMessageInfoWrapped info)
		{
			if (!this.CanTap())
			{
				return;
			}
			bool flag = info.Sender.ActorNumber == NetworkSystem.Instance.LocalPlayerID;
			bool flag2 = flag && GorillaTagger.Instance.lastLeftTap >= GorillaTagger.Instance.lastRightTap;
			MoleTypes moleTypes = null;
			if (this.randomMolePickedIndex >= 0 && this.randomMolePickedIndex < this.moleTypes.Length)
			{
				moleTypes = this.moleTypes[this.randomMolePickedIndex];
			}
			if (moleTypes != null)
			{
				Mole.MoleTapEvent onTapped = this.OnTapped;
				if (onTapped == null)
				{
					return;
				}
				onTapped(moleTypes, base.transform.position, flag, flag2);
			}
		}

		public void ResetPosition()
		{
			base.transform.position = this.hiddenPosition;
			this.currentState = Mole.MoleState.Reset;
		}

		public int GetMoleTypeIndex(bool useHazardMole)
		{
			if (!useHazardMole)
			{
				return this.safeMoles[Random.Range(0, this.safeMoles.Count)];
			}
			return this.hazardMoles[Random.Range(0, this.hazardMoles.Count)];
		}

		public float positionOffset = 0.2f;

		public MoleTypes[] moleTypes;

		private float showMoleDuration;

		private Vector3 visiblePosition;

		private Vector3 hiddenPosition;

		private float currentTime;

		private float animStartTime;

		private float travelTime;

		private float normalTravelTime = 0.3f;

		private float hitTravelTime = 0.2f;

		private AnimationCurve animCurve;

		private AnimationCurve normalAnimCurve;

		private AnimationCurve hitAnimCurve;

		private Mole.MoleState currentState;

		private Vector3 origin;

		private Vector3 target;

		private int randomMolePickedIndex;

		public CallLimiter rpcCooldown;

		private int moleScore;

		private List<int> safeMoles = new List<int>();

		private List<int> hazardMoles = new List<int>();

		public delegate void MoleTapEvent(MoleTypes moleType, Vector3 position, bool isLocalTap, bool isLeft);

		public enum MoleState
		{
			Reset,
			Ready,
			TransitionToVisible,
			Visible,
			TransitionToHidden,
			Hidden
		}
	}
}
