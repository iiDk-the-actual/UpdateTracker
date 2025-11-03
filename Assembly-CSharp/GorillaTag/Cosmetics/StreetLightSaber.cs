using System;
using System.Collections.Generic;
using GorillaLocomotion.Climbing;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class StreetLightSaber : MonoBehaviour
	{
		private StreetLightSaber.State CurrentState
		{
			get
			{
				return StreetLightSaber.values[this.currentIndex];
			}
		}

		private void Awake()
		{
			foreach (StreetLightSaber.StaffStates staffStates in this.allStates)
			{
				this.allStatesDict[staffStates.state] = staffStates;
			}
			this.currentIndex = 0;
			this.autoSwitchEnabledTime = 0f;
			this.hashId = Shader.PropertyToID(this.shaderColorProperty);
			Material[] sharedMaterials = this.meshRenderer.sharedMaterials;
			this.instancedMaterial = new Material(sharedMaterials[this.materialIndex]);
			sharedMaterials[this.materialIndex] = this.instancedMaterial;
			this.meshRenderer.sharedMaterials = sharedMaterials;
		}

		private void Update()
		{
			if (this.autoSwitch && Time.time - this.autoSwitchEnabledTime > this.autoSwitchTimer)
			{
				this.UpdateStateAuto();
			}
		}

		private void OnDestroy()
		{
			this.allStatesDict.Clear();
		}

		private void OnEnable()
		{
			this.ForceSwitchTo(StreetLightSaber.State.Off);
		}

		public void UpdateStateManual()
		{
			int num = (this.currentIndex + 1) % StreetLightSaber.values.Length;
			this.SwitchState(num);
		}

		private void UpdateStateAuto()
		{
			StreetLightSaber.State state = ((this.CurrentState == StreetLightSaber.State.Green) ? StreetLightSaber.State.Red : StreetLightSaber.State.Green);
			int num = Array.IndexOf<StreetLightSaber.State>(StreetLightSaber.values, state);
			this.SwitchState(num);
			this.autoSwitchEnabledTime = Time.time;
		}

		public void EnableAutoSwitch(bool enable)
		{
			this.autoSwitch = enable;
		}

		public void ResetStaff()
		{
			this.ForceSwitchTo(StreetLightSaber.State.Off);
		}

		public void HitReceived(Vector3 contact)
		{
			if (this.velocityTracker != null && this.velocityTracker.GetLatestVelocity(true).magnitude >= this.minHitVelocityThreshold)
			{
				StreetLightSaber.StaffStates staffStates = this.allStatesDict[this.CurrentState];
				if (staffStates == null)
				{
					return;
				}
				staffStates.OnSuccessfulHit.Invoke(contact);
			}
		}

		private void SwitchState(int newIndex)
		{
			if (newIndex == this.currentIndex)
			{
				return;
			}
			StreetLightSaber.State currentState = this.CurrentState;
			StreetLightSaber.State state = StreetLightSaber.values[newIndex];
			StreetLightSaber.StaffStates staffStates;
			if (this.allStatesDict.TryGetValue(currentState, out staffStates))
			{
				UnityEvent onExitState = staffStates.onExitState;
				if (onExitState != null)
				{
					onExitState.Invoke();
				}
			}
			this.currentIndex = newIndex;
			StreetLightSaber.StaffStates staffStates2;
			if (this.allStatesDict.TryGetValue(state, out staffStates2))
			{
				UnityEvent onEnterState = staffStates2.onEnterState;
				if (onEnterState != null)
				{
					onEnterState.Invoke();
				}
				if (this.trailRenderer != null)
				{
					this.trailRenderer.startColor = staffStates2.color;
				}
				if (this.meshRenderer != null)
				{
					this.instancedMaterial.SetColor(this.hashId, staffStates2.color);
				}
			}
		}

		private void ForceSwitchTo(StreetLightSaber.State targetState)
		{
			int num = Array.IndexOf<StreetLightSaber.State>(StreetLightSaber.values, targetState);
			if (num >= 0)
			{
				this.SwitchState(num);
			}
		}

		[SerializeField]
		private float autoSwitchTimer = 5f;

		[SerializeField]
		private TrailRenderer trailRenderer;

		[SerializeField]
		private Renderer meshRenderer;

		[SerializeField]
		private string shaderColorProperty;

		[SerializeField]
		private int materialIndex;

		[SerializeField]
		private GorillaVelocityTracker velocityTracker;

		[SerializeField]
		private float minHitVelocityThreshold;

		private static readonly StreetLightSaber.State[] values = (StreetLightSaber.State[])Enum.GetValues(typeof(StreetLightSaber.State));

		[Space]
		[Header("Staff State Settings")]
		public StreetLightSaber.StaffStates[] allStates = new StreetLightSaber.StaffStates[0];

		private int currentIndex;

		private Dictionary<StreetLightSaber.State, StreetLightSaber.StaffStates> allStatesDict = new Dictionary<StreetLightSaber.State, StreetLightSaber.StaffStates>();

		private bool autoSwitch;

		private float autoSwitchEnabledTime;

		private int hashId;

		private Material instancedMaterial;

		[Serializable]
		public class StaffStates
		{
			public StreetLightSaber.State state;

			public Color color;

			public UnityEvent onEnterState;

			public UnityEvent onExitState;

			public UnityEvent<Vector3> OnSuccessfulHit;
		}

		public enum State
		{
			Off,
			Green,
			Red
		}
	}
}
