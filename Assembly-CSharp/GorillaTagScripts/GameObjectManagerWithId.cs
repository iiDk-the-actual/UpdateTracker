using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class GameObjectManagerWithId : MonoBehaviour
	{
		private void Awake()
		{
			Transform[] componentsInChildren = this.objectsContainer.GetComponentsInChildren<Transform>(false);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameObjectManagerWithId.gameObjectData gameObjectData = new GameObjectManagerWithId.gameObjectData();
				gameObjectData.transform = componentsInChildren[i];
				gameObjectData.id = this.zone.ToString() + i.ToString();
				this.objectData.Add(gameObjectData);
			}
		}

		private void OnDestroy()
		{
			this.objectData.Clear();
		}

		public void ReceiveEvent(string id, Transform _transform)
		{
			foreach (GameObjectManagerWithId.gameObjectData gameObjectData in this.objectData)
			{
				if (gameObjectData.id == id)
				{
					gameObjectData.isMatched = true;
					gameObjectData.followTransform = _transform;
				}
			}
		}

		private void Update()
		{
			foreach (GameObjectManagerWithId.gameObjectData gameObjectData in this.objectData)
			{
				if (gameObjectData.isMatched)
				{
					gameObjectData.transform.transform.position = gameObjectData.followTransform.position;
					gameObjectData.transform.transform.rotation = gameObjectData.followTransform.rotation;
				}
			}
		}

		public GameObject objectsContainer;

		public GTZone zone;

		private readonly List<GameObjectManagerWithId.gameObjectData> objectData = new List<GameObjectManagerWithId.gameObjectData>();

		private class gameObjectData
		{
			public Transform transform;

			public Transform followTransform;

			public string id;

			public bool isMatched;
		}
	}
}
