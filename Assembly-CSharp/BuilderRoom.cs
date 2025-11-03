using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderRoom : MonoBehaviour
{
	public List<GameObject> disableColliderRoots;

	public List<GameObject> disableRenderRoots;

	public List<GameObject> disableGameObjectsForScene;

	public List<GameObject> disableObjectsForPersistent;

	public List<MeshRenderer> disabledRenderersForPersistent;

	public List<Collider> disabledCollidersForScene;
}
