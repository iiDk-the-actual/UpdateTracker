using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TransferrableObject))]
public class UnityEventOnGrab : MonoBehaviour
{
	private void Awake()
	{
		TransferrableObject componentInParent = base.GetComponentInParent<TransferrableObject>();
		Behaviour[] behavioursEnabledOnlyWhileHeld = componentInParent.behavioursEnabledOnlyWhileHeld;
		Behaviour[] array = new Behaviour[behavioursEnabledOnlyWhileHeld.Length + 1];
		for (int i = 0; i < behavioursEnabledOnlyWhileHeld.Length; i++)
		{
			array[i] = behavioursEnabledOnlyWhileHeld[i];
		}
		array[behavioursEnabledOnlyWhileHeld.Length] = this;
		componentInParent.behavioursEnabledOnlyWhileHeld = array;
	}

	private void OnEnable()
	{
		UnityEvent unityEvent = this.onGrab;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	private void OnDisable()
	{
		UnityEvent unityEvent = this.onRelease;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	[SerializeField]
	private UnityEvent onGrab;

	[SerializeField]
	private UnityEvent onRelease;
}
