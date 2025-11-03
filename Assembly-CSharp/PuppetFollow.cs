using System;
using UnityEngine;

public class PuppetFollow : MonoBehaviour
{
	private void FixedUpdate()
	{
		base.transform.position = this.sourceTarget.position - this.sourceBase.position + this.puppetBase.position;
		base.transform.localRotation = this.sourceTarget.localRotation;
	}

	public Transform sourceTarget;

	public Transform sourceBase;

	public Transform puppetBase;
}
