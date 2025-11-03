using System;
using System.Collections.Generic;
using UnityEngine;

public static class CrittersGrabberSharedData
{
	public static void Initialize()
	{
		if (CrittersGrabberSharedData.initialized)
		{
			return;
		}
		CrittersGrabberSharedData.initialized = true;
		CrittersGrabberSharedData.enteredCritterActor = new List<CrittersActor>();
		CrittersGrabberSharedData.triggerCollidersToCheck = new List<CapsuleCollider>();
		CrittersGrabberSharedData.heldActor = new List<CrittersActor>();
		CrittersGrabberSharedData.actorGrabbers = new List<CrittersActorGrabber>();
	}

	public static void AddEnteredActor(CrittersActor actor)
	{
		CrittersGrabberSharedData.Initialize();
		if (CrittersGrabberSharedData.enteredCritterActor.Contains(actor))
		{
			return;
		}
		CrittersGrabberSharedData.enteredCritterActor.Add(actor);
	}

	public static void RemoveEnteredActor(CrittersActor actor)
	{
		CrittersGrabberSharedData.Initialize();
		if (!CrittersGrabberSharedData.enteredCritterActor.Contains(actor))
		{
			return;
		}
		CrittersGrabberSharedData.enteredCritterActor.Remove(actor);
	}

	public static void AddTrigger(CapsuleCollider trigger)
	{
		CrittersGrabberSharedData.Initialize();
		if (CrittersGrabberSharedData.triggerCollidersToCheck.Contains(trigger))
		{
			return;
		}
		CrittersGrabberSharedData.triggerCollidersToCheck.Add(trigger);
	}

	public static void RemoveTrigger(CapsuleCollider trigger)
	{
		CrittersGrabberSharedData.Initialize();
		if (!CrittersGrabberSharedData.triggerCollidersToCheck.Contains(trigger))
		{
			return;
		}
		CrittersGrabberSharedData.triggerCollidersToCheck.Remove(trigger);
	}

	public static void AddActorGrabber(CrittersActorGrabber grabber)
	{
		CrittersGrabberSharedData.Initialize();
		if (CrittersGrabberSharedData.actorGrabbers.Contains(grabber))
		{
			return;
		}
		CrittersGrabberSharedData.actorGrabbers.Add(grabber);
	}

	public static void RemoveActorGrabber(CrittersActorGrabber grabber)
	{
		CrittersGrabberSharedData.Initialize();
		if (!CrittersGrabberSharedData.actorGrabbers.Contains(grabber))
		{
			return;
		}
		CrittersGrabberSharedData.actorGrabbers.Remove(grabber);
	}

	public static void DisableEmptyGrabberJoints()
	{
		CrittersGrabberSharedData.Initialize();
		for (int i = 0; i < CrittersGrabberSharedData.actorGrabbers.Count; i++)
		{
			if (CrittersGrabberSharedData.actorGrabbers[i].grabber != null && CrittersGrabberSharedData.actorGrabbers[i].actorsStillPresent.Count == 0)
			{
				for (int j = 0; j < CrittersGrabberSharedData.actorGrabbers[i].grabber.grabbedActors.Count; j++)
				{
					CrittersGrabberSharedData.actorGrabbers[i].grabber.grabbedActors[j].DisconnectJoint();
				}
			}
		}
	}

	public static List<CrittersActor> enteredCritterActor;

	public static List<CapsuleCollider> triggerCollidersToCheck;

	public static List<CrittersActor> heldActor;

	public static List<CrittersActorGrabber> actorGrabbers;

	private static bool initialized;
}
