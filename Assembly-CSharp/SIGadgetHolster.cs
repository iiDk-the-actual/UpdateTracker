using System;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameGrabbable))]
[RequireComponent(typeof(GameSnappable))]
public class SIGadgetHolster : SIGadget, I_SIDisruptable
{
	private void Start()
	{
		this.gtPlayer = GTPlayer.Instance;
	}

	public void Disrupt(float disruptTime)
	{
	}

	[SerializeField]
	private Image imageMask;

	public List<SuperInfectionSnapPoint> snapPoints;

	private SIGadgetHolster.State state;

	private GTPlayer gtPlayer;

	private enum State
	{
		Unequipped,
		Equipped
	}
}
