using System;
using UnityEngine;

public abstract class CustomMapsBehaviourBase
{
	public abstract bool CanExecute();

	public abstract void Execute();

	public abstract void NetExecute();

	public abstract void ResetBehavior();

	public abstract bool CanContinueExecuting();

	public abstract void OnTriggerEnter(Collider otherCollider);
}
