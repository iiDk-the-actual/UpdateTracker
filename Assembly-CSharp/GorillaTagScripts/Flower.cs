using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	public class Flower : MonoBehaviour
	{
		public bool IsWatered { get; private set; }

		private void Awake()
		{
			this.shouldUpdateVisuals = true;
			this.anim = base.GetComponent<Animator>();
			this.timer = base.GetComponent<GorillaTimer>();
			this.perchPoint = base.GetComponent<BeePerchPoint>();
			this.timer.onTimerStopped.AddListener(new UnityAction<GorillaTimer>(this.HandleOnFlowerTimerEnded));
			this.currentState = Flower.FlowerState.None;
			this.wateredFx = this.wateredFx.GetComponent<ParticleSystem>();
			this.IsWatered = false;
			this.meshRenderer = base.GetComponent<SkinnedMeshRenderer>();
			this.meshRenderer.enabled = false;
			this.anim.enabled = false;
		}

		private void OnDestroy()
		{
			this.timer.onTimerStopped.RemoveListener(new UnityAction<GorillaTimer>(this.HandleOnFlowerTimerEnded));
		}

		public void WaterFlower(bool isWatered = false)
		{
			this.IsWatered = isWatered;
			switch (this.currentState)
			{
			case Flower.FlowerState.None:
				this.UpdateFlowerState(Flower.FlowerState.Healthy, false, true);
				return;
			case Flower.FlowerState.Healthy:
				if (!isWatered)
				{
					this.UpdateFlowerState(Flower.FlowerState.Middle, false, true);
					return;
				}
				break;
			case Flower.FlowerState.Middle:
				if (isWatered)
				{
					this.UpdateFlowerState(Flower.FlowerState.Healthy, true, true);
					return;
				}
				this.UpdateFlowerState(Flower.FlowerState.Wilted, false, true);
				return;
			case Flower.FlowerState.Wilted:
				if (isWatered)
				{
					this.UpdateFlowerState(Flower.FlowerState.Middle, true, true);
				}
				break;
			default:
				return;
			}
		}

		public void UpdateFlowerState(Flower.FlowerState newState, bool isWatered = false, bool updateVisual = true)
		{
			if (FlowersManager.Instance.IsMine)
			{
				this.timer.RestartTimer();
			}
			this.ChangeState(newState);
			if (this.perchPoint)
			{
				this.perchPoint.enabled = this.currentState == Flower.FlowerState.Healthy;
			}
			if (updateVisual)
			{
				this.LocalUpdateFlowers(newState, isWatered);
			}
		}

		private void LocalUpdateFlowers(Flower.FlowerState state, bool isWatered = false)
		{
			GameObject[] array = this.meshStates;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			if (!this.shouldUpdateVisuals)
			{
				this.meshStates[(int)this.currentState].SetActive(true);
				return;
			}
			if (isWatered && this.wateredFx)
			{
				this.wateredFx.Play();
			}
			this.meshRenderer.enabled = true;
			this.anim.enabled = true;
			switch (state)
			{
			case Flower.FlowerState.Healthy:
				this.anim.SetTrigger(Flower.middle_to_healthy);
				return;
			case Flower.FlowerState.Middle:
				if (this.lastState == Flower.FlowerState.Wilted)
				{
					this.anim.SetTrigger(Flower.wilted_to_middle);
					return;
				}
				this.anim.SetTrigger(Flower.healthy_to_middle);
				return;
			case Flower.FlowerState.Wilted:
				this.anim.SetTrigger(Flower.middle_to_wilted);
				return;
			default:
				return;
			}
		}

		private void HandleOnFlowerTimerEnded(GorillaTimer _timer)
		{
			if (!FlowersManager.Instance.IsMine)
			{
				return;
			}
			if (this.timer == _timer)
			{
				this.WaterFlower(false);
			}
		}

		private void ChangeState(Flower.FlowerState state)
		{
			this.lastState = this.currentState;
			this.currentState = state;
		}

		public Flower.FlowerState GetCurrentState()
		{
			return this.currentState;
		}

		public void OnAnimationIsDone(int state)
		{
			if (this.meshRenderer.enabled)
			{
				for (int i = 0; i < this.meshStates.Length; i++)
				{
					bool flag = i == (int)this.currentState;
					this.meshStates[i].SetActive(flag);
				}
				this.anim.enabled = false;
				this.meshRenderer.enabled = false;
			}
		}

		public void UpdateVisuals(bool enable)
		{
			this.shouldUpdateVisuals = enable;
			this.meshStatesGameObject.SetActive(enable);
		}

		public void AnimCatch()
		{
			if (this.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				this.OnAnimationIsDone(0);
			}
		}

		private Animator anim;

		private SkinnedMeshRenderer meshRenderer;

		[HideInInspector]
		public GorillaTimer timer;

		private BeePerchPoint perchPoint;

		public ParticleSystem wateredFx;

		public ParticleSystem sparkleFx;

		public GameObject meshStatesGameObject;

		public GameObject[] meshStates;

		private static readonly int healthy_to_middle = Animator.StringToHash("healthy_to_middle");

		private static readonly int middle_to_healthy = Animator.StringToHash("middle_to_healthy");

		private static readonly int wilted_to_middle = Animator.StringToHash("wilted_to_middle");

		private static readonly int middle_to_wilted = Animator.StringToHash("middle_to_wilted");

		private Flower.FlowerState currentState;

		private string id;

		private bool shouldUpdateVisuals;

		private Flower.FlowerState lastState;

		public enum FlowerState
		{
			None = -1,
			Healthy,
			Middle,
			Wilted
		}
	}
}
