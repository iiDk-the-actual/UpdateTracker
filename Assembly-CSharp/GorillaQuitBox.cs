using System;
using UnityEngine;

public class GorillaQuitBox : GorillaTriggerBox
{
	private void Start()
	{
	}

	public override void OnBoxTriggered()
	{
		Debug.Log("quitbox hit! hopefully you expected this to happen!");
		Application.Quit();
	}
}
