using System;
using UnityEngine;

public interface IHoldableObject
{
	GameObject gameObject { get; }

	string name { get; set; }

	bool TwoHanded { get; }

	void OnHover(InteractionPoint pointHovered, GameObject hoveringHand);

	void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand);

	bool OnRelease(DropZone zoneReleased, GameObject releasingHand);

	void DropItemCleanup();
}
