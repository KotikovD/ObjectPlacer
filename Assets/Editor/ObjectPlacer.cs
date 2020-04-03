using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace FPS_Kotikov_D.Editor
{
    public class ObjectPlacer : EditorWindow
    {


        #region Fields

        const int MINVALUE = 1;
        const int MAXVALUE = 100;
        const int MAXOBJECTS = 10;

        private static int _howTotalObjects = 1;
        private int _minDistance = 5;
        private int _maxDistance = 50;
        private List<DataBlocks> _dataBlocks = new List<DataBlocks>();
        private List<GameObject> _objects = new List<GameObject>();
        private List<bool> _objectsGroups = new List<bool>();
        private GameObject _parentGO;
        private bool _randomRotation = true;
        #endregion


        #region DataBlocks

        private class DataBlocks
        {
            private GameObject _objectForPlace;
            private int _howEachObjects;

            public GameObject ObjectForPlace
            {
                get { return _objectForPlace; }
                set { _objectForPlace = value; }
            }

            public int HowEachObjects
            {
                get { return _howEachObjects; }
                set { _howEachObjects = value; }
            }

            public void CreateBlock()
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Prefub", EditorStyles.boldLabel);
                _objectForPlace = EditorGUILayout.ObjectField("Game object", _objectForPlace, typeof(GameObject), true) as GameObject;
                _howEachObjects = EditorGUILayout.IntField("How objects place?", _howEachObjects);
                GUILayout.EndVertical();
                GUILayout.Space(20);

            }

        }

        #endregion


        #region Window&GUIMetodths

        [MenuItem("MyEditorScripts/Object Placer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ObjectPlacer));
        }

        private void OnGUI()
        {

            GUILayout.Space(20);
            _howTotalObjects = EditorGUILayout.IntSlider("How total objects?", _howTotalObjects, MINVALUE, MAXOBJECTS);
            _minDistance = EditorGUILayout.IntSlider("Min Distance", _minDistance, MINVALUE, MAXVALUE);
            _maxDistance = EditorGUILayout.IntSlider("Max Distance", _maxDistance, MINVALUE, MAXVALUE);
            _randomRotation = EditorGUILayout.Toggle($"RandomRotation", _randomRotation);
            GUILayout.Space(20);

            for (int i = 0; i < _howTotalObjects; i++)
            {
                _dataBlocks.Add(new DataBlocks());
                _dataBlocks[i].CreateBlock();
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Place"))
                CreateObjects();



            GUILayout.Space(20);
            for (int z = 0; z < _howTotalObjects; z++)
            {
                if (_objectsGroups.Count < _howTotalObjects)
                    _objectsGroups.Add(false);

                var name = _dataBlocks[z].ObjectForPlace == null ? "Prefub name" : _dataBlocks[z].ObjectForPlace.name;
                _objectsGroups[z] = EditorGUILayout.Toggle($"{name}", _objectsGroups[z]);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Remove selected"))
                RemoveGroup();
            GUILayout.Space(10);
            if (GUILayout.Button("Remove ALL"))
                RemoveAllObjects();
        }

        #endregion


        #region Metodths


        private void CreateObjects()
        {
            _parentGO = GameObject.Find("CreatedObjects");
            if (_parentGO == null)
                _parentGO = new GameObject { name = "CreatedObjects" };

            for (int a = 0; a < _howTotalObjects; a++)
            {
                var block = _dataBlocks[a];
                for (int i = 0; i < block.HowEachObjects;)
                {
                    var dis = Random.Range(_minDistance, _maxDistance);
                    var randomPoint = Random.insideUnitSphere * dis;

                    Quaternion rot = _randomRotation == false ?
                        Quaternion.identity :
                        new Quaternion(0, Random.Range(0, 360), 0, Random.Range(0, 360));

                    if (NavMesh.SamplePosition(randomPoint, out var hit, dis, NavMesh.AllAreas))
                    {
                        var obj = Instantiate(block.ObjectForPlace, hit.position, rot) as GameObject;
                        obj.name = block.ObjectForPlace.name;
                        obj.transform.parent = _parentGO.transform;
                        _objects.Add(obj);
                        i++;
                    }
                }
            }
        }

        private void RemoveAllObjects()
        {
            DestroyImmediate(GameObject.Find("CreatedObjects"));
        }

        private void RemoveGroup()
        {
            for (int i = 0; i < _howTotalObjects; i++)
            {
                if (_objectsGroups[i] == true)
                    for (int a = 0; a < _dataBlocks[i].HowEachObjects; a++)
                    {
                        DestroyImmediate(GameObject.Find(_dataBlocks[i].ObjectForPlace.name));
                    }
            }
        }

        #endregion


    }
}


