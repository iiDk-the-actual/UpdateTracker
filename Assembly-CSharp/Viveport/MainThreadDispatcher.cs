using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Viveport
{
	public class MainThreadDispatcher : MonoBehaviour
	{
		private void Awake()
		{
			if (MainThreadDispatcher.instance == null)
			{
				MainThreadDispatcher.instance = this;
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		public void Update()
		{
			Queue<Action> queue = MainThreadDispatcher.actions;
			lock (queue)
			{
				while (MainThreadDispatcher.actions.Count > 0)
				{
					MainThreadDispatcher.actions.Dequeue()();
				}
			}
		}

		public static MainThreadDispatcher Instance()
		{
			if (MainThreadDispatcher.instance == null)
			{
				throw new Exception("Could not find the MainThreadDispatcher GameObject. Please ensure you have added this script to an empty GameObject in your scene.");
			}
			return MainThreadDispatcher.instance;
		}

		private void OnDestroy()
		{
			MainThreadDispatcher.instance = null;
		}

		public void Enqueue(IEnumerator action)
		{
			Queue<Action> queue = MainThreadDispatcher.actions;
			lock (queue)
			{
				MainThreadDispatcher.actions.Enqueue(delegate
				{
					this.StartCoroutine(action);
				});
			}
		}

		public void Enqueue(Action action)
		{
			this.Enqueue(this.ActionWrapper(action));
		}

		public void Enqueue<T1>(Action<T1> action, T1 param1)
		{
			this.Enqueue(this.ActionWrapper<T1>(action, param1));
		}

		public void Enqueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
		{
			this.Enqueue(this.ActionWrapper<T1, T2>(action, param1, param2));
		}

		public void Enqueue<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
		{
			this.Enqueue(this.ActionWrapper<T1, T2, T3>(action, param1, param2, param3));
		}

		public void Enqueue<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			this.Enqueue(this.ActionWrapper<T1, T2, T3, T4>(action, param1, param2, param3, param4));
		}

		private IEnumerator ActionWrapper(Action action)
		{
			action();
			yield return null;
			yield break;
		}

		private IEnumerator ActionWrapper<T1>(Action<T1> action, T1 param1)
		{
			action(param1);
			yield return null;
			yield break;
		}

		private IEnumerator ActionWrapper<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
		{
			action(param1, param2);
			yield return null;
			yield break;
		}

		private IEnumerator ActionWrapper<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
		{
			action(param1, param2, param3);
			yield return null;
			yield break;
		}

		private IEnumerator ActionWrapper<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			action(param1, param2, param3, param4);
			yield return null;
			yield break;
		}

		private static readonly Queue<Action> actions = new Queue<Action>();

		private static MainThreadDispatcher instance = null;
	}
}
