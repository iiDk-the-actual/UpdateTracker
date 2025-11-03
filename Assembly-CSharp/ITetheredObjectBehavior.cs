using System;
using UnityEngine;

internal interface ITetheredObjectBehavior
{
	void DbgClear();

	void EnableDistanceConstraints(bool v, float playerScale);

	void EnableDynamics(bool enable, bool collider, bool kinematic);

	bool IsEnabled();

	void ReParent();

	bool ReturnStep();

	void TriggerEnter(Collider other, ref Vector3 force, ref Vector3 collisionPt, ref bool transferOwnership);
}
