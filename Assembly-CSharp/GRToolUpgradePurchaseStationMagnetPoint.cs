using System;
using UnityEngine;

public class GRToolUpgradePurchaseStationMagnetPoint : MonoBehaviour
{
	[Tooltip("Drag in the child transform that marks where the tool should attach. This MUST be a direct child of the entity, and not buried in the hierarchy.")]
	public Transform magnetAttachTransform;
}
