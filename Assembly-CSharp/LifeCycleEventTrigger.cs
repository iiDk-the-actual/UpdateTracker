using System;
using UnityEngine;
using UnityEngine.Events;

public class LifeCycleEventTrigger : MonoBehaviour
{
	private void Awake()
	{
		UnityEvent onAwake = this._onAwake;
		if (onAwake == null)
		{
			return;
		}
		onAwake.Invoke();
	}

	private void Start()
	{
		UnityEvent onStart = this._onStart;
		if (onStart == null)
		{
			return;
		}
		onStart.Invoke();
	}

	private void OnEnable()
	{
		UnityEvent onEnable = this._onEnable;
		if (onEnable == null)
		{
			return;
		}
		onEnable.Invoke();
	}

	private void OnDisable()
	{
		UnityEvent onDisable = this._onDisable;
		if (onDisable == null)
		{
			return;
		}
		onDisable.Invoke();
	}

	private void OnDestroy()
	{
		UnityEvent onDestroy = this._onDestroy;
		if (onDestroy == null)
		{
			return;
		}
		onDestroy.Invoke();
	}

	[SerializeField]
	private UnityEvent _onAwake;

	[SerializeField]
	private UnityEvent _onStart;

	[SerializeField]
	private UnityEvent _onEnable;

	[SerializeField]
	private UnityEvent _onDisable;

	[SerializeField]
	private UnityEvent _onDestroy;
}
