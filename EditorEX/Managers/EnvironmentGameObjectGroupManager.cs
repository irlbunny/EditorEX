using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorEX.Managers
{
    internal class EnvironmentGameObjectGroupManager
    {
        private readonly GameObject _environmentGameObject;
        private readonly Dictionary<string, GameObject[]> _groups = new();

        public EnvironmentGameObjectGroupManager()
        {
            _environmentGameObject = GameObject.Find("Environment");
        }

        public GameObject[] Get(string id)
        {
            if (!_groups.ContainsKey(id))
                throw new Exception($"Group with ID \"{id}\" doesn't exist!");

            return _groups[id];
        }

        public void Add<T>(string id) where T : MonoBehaviour
        {
            if (_groups.ContainsKey(id))
                throw new Exception($"Group with ID \"{id}\" already exists!");

            var gameObjects = new List<GameObject>();
            foreach (var monoBehaviour in _environmentGameObject.GetComponentsInChildren<T>())
            {
                if (monoBehaviour.gameObject.activeSelf)
                    gameObjects.Add(monoBehaviour.gameObject);
            }

            _groups.Add(id, gameObjects.ToArray());
        }

        public void Remove(string id)
        {
            if (!_groups.ContainsKey(id))
                throw new Exception($"Group with ID \"{id}\" doesn't exist!");

            _groups.Remove(id);
        }

        public void SetVisible(string id, bool visible)
        {
            if (!_groups.ContainsKey(id))
                throw new Exception($"Group with ID \"{id}\" doesn't exist!");

            var gameObjects = _groups[id];
            for (var i = 0; i < gameObjects.Length; i++)
                gameObjects[i].SetActive(visible);
        }
    }
}
