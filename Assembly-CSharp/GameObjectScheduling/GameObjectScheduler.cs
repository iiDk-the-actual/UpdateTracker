using System;
using System.Collections;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

namespace GameObjectScheduling
{
	public class GameObjectScheduler : MonoBehaviour
	{
		private void Start()
		{
			this.schedule.Validate();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				list.Add(base.transform.GetChild(i).gameObject);
			}
			this.scheduledGameObject = list.ToArray();
			for (int j = 0; j < this.scheduledGameObject.Length; j++)
			{
				this.scheduledGameObject[j].SetActive(false);
			}
			this.dispatcher = base.GetComponent<GameObjectSchedulerEventDispatcher>();
			this.monitor = base.StartCoroutine(this.MonitorTime());
		}

		private void OnEnable()
		{
			if (this.monitor == null && this.scheduledGameObject != null)
			{
				this.monitor = base.StartCoroutine(this.MonitorTime());
			}
		}

		private void OnDisable()
		{
			if (this.monitor != null)
			{
				base.StopCoroutine(this.monitor);
			}
			this.monitor = null;
		}

		private IEnumerator MonitorTime()
		{
			while (GorillaComputer.instance == null || GorillaComputer.instance.startupMillis == 0L)
			{
				yield return null;
			}
			bool previousState = this.getActiveState();
			for (int i = 0; i < this.scheduledGameObject.Length; i++)
			{
				this.scheduledGameObject[i].SetActive(previousState);
			}
			for (;;)
			{
				yield return new WaitForSeconds(60f);
				bool activeState = this.getActiveState();
				if (previousState != activeState)
				{
					this.changeActiveState(activeState);
					previousState = activeState;
				}
			}
			yield break;
		}

		private bool getActiveState()
		{
			this.currentNodeIndex = this.schedule.GetCurrentNodeIndex(this.getServerTime(), 0);
			bool flag;
			if (this.currentNodeIndex == -1)
			{
				flag = this.schedule.InitialState;
			}
			else if (this.currentNodeIndex < this.schedule.Nodes.Length)
			{
				flag = this.schedule.Nodes[this.currentNodeIndex].ActiveState;
			}
			else
			{
				flag = this.schedule.Nodes[this.schedule.Nodes.Length - 1].ActiveState;
			}
			return flag;
		}

		private DateTime getServerTime()
		{
			return GorillaComputer.instance.GetServerTime();
		}

		private void changeActiveState(bool state)
		{
			if (state)
			{
				for (int i = 0; i < this.scheduledGameObject.Length; i++)
				{
					this.scheduledGameObject[i].SetActive(true);
				}
				if (this.dispatcher != null && this.dispatcher.OnScheduledActivation != null)
				{
					this.dispatcher.OnScheduledActivation.Invoke();
					return;
				}
			}
			else
			{
				if (this.dispatcher != null && this.dispatcher.OnScheduledDeactivation != null)
				{
					this.dispatcher.OnScheduledActivation.Invoke();
					return;
				}
				for (int j = 0; j < this.scheduledGameObject.Length; j++)
				{
					this.scheduledGameObject[j].SetActive(false);
				}
			}
		}

		[SerializeField]
		private GameObjectSchedule schedule;

		private GameObject[] scheduledGameObject;

		private GameObjectSchedulerEventDispatcher dispatcher;

		private int currentNodeIndex = -1;

		private Coroutine monitor;
	}
}
