using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace ObjectPlacer
{
    public class ObjectPlacer : EditorWindow
    {


        #region Fields

        private const int MIN_VALUE = 1;
        private const int MAX_OBJECTS = 10;
        private const int MAX_LONG_SIDE_OF_NAVMESH = 200;
        private const int MAX_CURRENT_OBJECTS = 50;

        private List<DataBlocks> _dataBlocks = new List<DataBlocks>();
        private List<GameObject> _objects = new List<GameObject>();
        private GameObject _parentGO;
        private Vector2 scrollPosition = Vector2.zero;
        private int _totalObjectsCount = 1;
        private int _maxDistance = 50;
        private bool _randomRotation = true;

        #endregion


        #region DataBlocksClass

        [Serializable]
        public class DataBlocks
        {
            public GameObject GameObj;
            public int How;
            public bool WasBoxCollidder;
        }

        #endregion


        #region Window

        [MenuItem("Tools/Object Placer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ObjectPlacer), false, "Object Placer");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(20);
            GUILayout.Label("Total objects", EditorStyles.boldLabel);
            _totalObjectsCount = EditorGUILayout.IntSlider(_totalObjectsCount, MIN_VALUE, MAX_OBJECTS);
            GUILayout.Label("Longest side of NavMesh (approx)", EditorStyles.boldLabel);
            _maxDistance = EditorGUILayout.IntSlider(_maxDistance, MIN_VALUE, MAX_LONG_SIDE_OF_NAVMESH);
            _randomRotation = EditorGUILayout.Toggle($"Y random rotation", _randomRotation);
            GUILayout.Space(20);

            if (_dataBlocks.Count < _totalObjectsCount)
                _dataBlocks.Add(new DataBlocks());
            else if (_dataBlocks.Count > _totalObjectsCount)
                _dataBlocks.RemoveAt(_dataBlocks.Count - 1);

            for (int i = 0; i < _totalObjectsCount; i++)
            {
                GUILayout.BeginVertical("Box");
                _dataBlocks[i].GameObj = EditorGUILayout.ObjectField("Game object", _dataBlocks[i].GameObj, typeof(GameObject), true) as GameObject;
                _dataBlocks[i].How = EditorGUILayout.IntSlider("Current object count", _dataBlocks[i].How, 0, MAX_CURRENT_OBJECTS);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Place"))
                CreateObjects();

            GUILayout.Space(10);
            if (GUILayout.Button("Remove all objects"))
                RemoveAllObjects();

            EditorGUILayout.EndScrollView();
        }

        #endregion


        #region Methods

        private void CreateObjects()
        {
            _parentGO = GameObject.Find("CreatedObjects");
            if (_parentGO == null)
                _parentGO = new GameObject { name = "CreatedObjects" };

            for (int i = 0; i < _totalObjectsCount; i++)
            {
                var block = _dataBlocks[i];
                for (int b = 0; b < block.How;)
                {
                    Vector3 point = _objects.Count == 0 ? default : _objects[_objects.Count - 1].gameObject.transform.position;
                    var randomPoint = point + Random.insideUnitSphere * _maxDistance;

                    Quaternion rot = _randomRotation == false ? Quaternion.identity :
                        new Quaternion(0, Random.Range(0, 360), 0, Random.Range(0, 360));

                    if (NavMesh.SamplePosition(randomPoint, out var hit, 1f, NavMesh.AllAreas))
                    {
                        var placedObj = Instantiate(block.GameObj, randomPoint, rot) as GameObject;
                        placedObj.name = block.GameObj.name;
                        placedObj.transform.SetParent(_parentGO.transform);

                        block.WasBoxCollidder = GetBoxCollider(placedObj, out var currentObjCollider);

                        if (CheckCollisions(currentObjCollider, hit.position))
                        {
                            placedObj.transform.position = hit.position;
                            _objects.Add(placedObj);
                            b++;
                        }
                        else
                        {
                            DestroyImmediate(placedObj, true);
                        }
                    }
                }
            }
            FinishPlacing();
        }

        /// <summary>
        /// Checking for the distance between objects. Helps to prevent collision of objects when placing
        /// </summary>
        private bool CheckCollisions(BoxCollider currentObjCollider, Vector3 newPosition)
        {
            if (_objects != null)
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i] != null)
                    {
                        var pastObjCollider = _objects[i].gameObject.GetComponent<BoxCollider>();
                        if (pastObjCollider == null)
                            GetBoxCollider(_objects[i].gameObject, out pastObjCollider);
                        var currentDistance = Vector3.Distance(_objects[i].transform.position, newPosition);
                        var minDistance = currentObjCollider.bounds.extents + pastObjCollider.bounds.extents;

                        if (minDistance.x > currentDistance && minDistance.y > currentDistance)
                            return false;
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Check was or not BoxCollider on incoming game object and returns BoxCollider
        /// </summary>
        private bool GetBoxCollider(GameObject incomingGO, out BoxCollider box)
        {
            bool wasBoxCollider;
            box = incomingGO.GetComponent<BoxCollider>();
            if (box == null)
            {
                wasBoxCollider = false;
                box = incomingGO.AddComponent<BoxCollider>();
            }
            else
            {
                wasBoxCollider = true; ;
            }

            return wasBoxCollider;
        }

        /// <summary>
        /// Delete a BoxCollider object that didn't have one before
        /// </summary>
        private void FinishPlacing()
        {
            if (_objects == null) return;
            if (_dataBlocks == null) return;

            foreach (var block in _dataBlocks)
            {
                if (block.WasBoxCollidder) continue;

                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i] == null) continue;
                    if (_objects[i].name.Equals(block.GameObj.name))
                    {
                        var bc = _objects[i].GetComponent<BoxCollider>();
                        if (bc != null)
                            DestroyImmediate(bc);
                    }
                }
            }
            _objects.Clear();
        }

        /// <summary>
        /// Delete main game object with placed objects
        /// </summary>
        private void RemoveAllObjects()
        {
            var mainGO = GameObject.Find("CreatedObjects");
            if (mainGO != null)
            {
                DestroyImmediate(mainGO);
            }
        }

        #endregion


    }
}