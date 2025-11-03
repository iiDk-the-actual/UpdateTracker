using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GorillaTurning : GorillaTriggerBox
{
	private void Awake()
	{
	}

	public Material redMaterial;

	public Material blueMaterial;

	public Material greenMaterial;

	public Material transparentBlueMaterial;

	public Material transparentRedMaterial;

	public Material transparentGreenMaterial;

	public MeshRenderer smoothTurnBox;

	public MeshRenderer snapTurnBox;

	public MeshRenderer noTurnBox;

	public GorillaSnapTurn snapTurn;

	public string currentChoice;

	public float currentSpeed;
}
