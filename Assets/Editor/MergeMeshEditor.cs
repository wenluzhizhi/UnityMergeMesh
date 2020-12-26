using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace MergeMesh
{

    public class MergeItem
    {
        public Material useMaterial;
        public List<MeshFilter> ListMeshFilter = new List<MeshFilter>();
        public List<int> ListSubIndex = new List<int>();
        public string Key = "";
        public MergeItem(string Key, Material mas)
        {
            this.Key = Key;
            this.useMaterial = mas;
        }

        public bool IsSameItem(MergeItem item)
        {
            bool isSame = false;
            if (this.Key.Equals(item.Key))
            {
                isSame = true;
            }
            return isSame;
        }

    }


    public class MergeMeshEditor : MonoBehaviour
    {

        public static Dictionary<string, MergeItem> dictItems = new Dictionary<string, MergeItem>();
        [MenuItem("Tools/MergeMesh")]
        public static void MergeMesh()
        {
            GameObject go = Selection.activeGameObject;
            Debug.Log(go);
            traverse(go.transform);
            CopyMesh();
        }

        public static void CopyMesh()
        {
            Debug.Log(dictItems.Count);
            string maPath = Application.dataPath + "/Materials";
            if (!Directory.Exists(maPath))
            {
                Directory.CreateDirectory(maPath);
            }
            string mePath = Application.dataPath + "/Meshes";
            if (!Directory.Exists(mePath))
            {
                Directory.CreateDirectory(mePath);
            }
            int index = 0;
            GameObject go = new GameObject("NewFbx");
            foreach (KeyValuePair<string, MergeItem> kv in dictItems)
            {

                Transform first = (new GameObject("df_" + index)).transform;
                first.gameObject.AddComponent<MeshRenderer>();
                first.gameObject.AddComponent<MeshFilter>();
                Mesh newMesh = new Mesh();
                Material m = Instantiate(kv.Value.useMaterial) as Material;
                AssetDatabase.CreateAsset(m, "Assets/Materials/" + m.name + ".mat");
                first.gameObject.GetComponent<MeshRenderer>().material = m;

                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                List<CombineInstance> listC = new List<CombineInstance>();
                for (int i = 0; i < kv.Value.ListMeshFilter.Count; i++)
                {
                    MeshFilter msr1 = kv.Value.ListMeshFilter[i];
                    CombineInstance c = new CombineInstance();
                    c.mesh = msr1.sharedMesh;
                    c.subMeshIndex = kv.Value.ListSubIndex[i];
                    c.transform = msr1.transform.localToWorldMatrix;
                    listC.Add(c);
                }
                first.gameObject.GetComponent<MeshFilter>().mesh = newMesh;
                try
                {
                    newMesh.CombineMeshes(listC.ToArray());
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.ToString());
                }
                AssetDatabase.CreateAsset(newMesh, "Assets/Meshes/" + m.name + ".asset");
                first.SetParent(go.transform);
                index++;
            }
        }


        static void traverse(Transform parent)
        {
            int count = parent.childCount;
            if (count <= 0)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                Transform child = parent.GetChild(i);
                Debug.Log(child.gameObject.name);
                MeshFilter msr = child.gameObject.GetComponent<MeshFilter>();
                if (msr != null)
                {

                    Material[] mas = msr.gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    for (int j = 0; j < mas.Length; j++)
                    {
                        Material m = mas[j];
                        if (!dictItems.ContainsKey(m.name))
                        {
                            MergeItem item = new MergeItem(m.name, m);
                            item.ListMeshFilter.Add(msr);
                            item.ListSubIndex.Add(j);
                            dictItems.Add(m.name, item);
                        }
                        else
                        {
                            MergeItem item = dictItems[m.name];
                            item.ListMeshFilter.Add(msr);
                            item.ListSubIndex.Add(j);
                        }
                    }
                }
                traverse(child);
            }
        }
    }
}
