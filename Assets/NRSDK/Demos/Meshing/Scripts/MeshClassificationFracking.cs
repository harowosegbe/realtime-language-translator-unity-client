using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace NRKernal.NRExamples
{
    public class MeshClassificationFracking : MonoBehaviour, IMeshInfoProcessor
    {
        /// <summary>
        /// key: Mesh block Identifier
        /// value: Classified MeshFilters for this block
        /// </summary>
        Dictionary<ulong, Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>> m_ClassifiedMeshFilters = new Dictionary<ulong, Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>>();
        /// <summary>
        /// key: Mesh block Identifier
        /// valuse: Classified Meshes for this block
        /// </summary>
        Dictionary<ulong, Dictionary<NRMeshingVertexSemanticLabel, Mesh>> m_ClassifiedMeshes = new Dictionary<ulong, Dictionary<NRMeshingVertexSemanticLabel, Mesh>>();

        /// <summary>
        /// List for all classified meshfilter prefabs
        /// </summary>
        [SerializeField]
        private LabelMeshFilterPair[] m_ClassifiedMeshFilterPrefabs;

        private NRMeshingVertexSemanticLabel[] m_AvailableLabels;
        public NRMeshingVertexSemanticLabel[] AvailableLabels
        {
            get
            {
                if (m_AvailableLabels == null)
                {
                    m_AvailableLabels = new NRMeshingVertexSemanticLabel[m_ClassifiedMeshFilterPrefabs.Length];
                    for (int i = 0; i < m_ClassifiedMeshFilterPrefabs.Length; ++i)
                    {
                        m_AvailableLabels[i] = m_ClassifiedMeshFilterPrefabs[i].label;
                    }
                }
                return m_AvailableLabels;
            }
        }
        private void Awake()
        {
            NRDebugger.Info($"[{this.GetType()}] Awake");
        }

        public void ClearMeshInfo()
        {
            NRDebugger.Info($"[{this.GetType()}] {nameof(ClearMeshInfo)} ");

            ulong[] identifiers = m_ClassifiedMeshFilters.Keys.ToArray();
            foreach (var identifier in identifiers)
            {
                HandleDeletedBlock(identifier);
            }
        }

        public void UpdateMeshInfo(ulong identifier, NRMeshInfo meshInfo)
        {
            NRDebugger.Debug($"[{nameof(MeshClassificationFracking)}] {nameof(UpdateMeshInfo)} Begin");
            string sample_UpdateMeshInfo = $"{nameof(UpdateMeshInfo)}";
            profile.BeginProfile(sample_UpdateMeshInfo);

            NRMeshingBlockState meshingBlockState = meshInfo.state;
            Mesh mesh = meshInfo.baseMesh;
            NRMeshingVertexSemanticLabel[] labels = meshInfo.labels;

            UnityEngine.Debug.Assert(mesh.vertexCount == labels.Length);

            switch (meshingBlockState)
            {
                case NRMeshingBlockState.NR_MESHING_BLOCK_STATE_NEW:
                case NRMeshingBlockState.NR_MESHING_BLOCK_STATE_UPDATED:
                    HandleUpdateBlock(identifier, mesh, labels);
                    break;
                case NRMeshingBlockState.NR_MESHING_BLOCK_STATE_DELETED:
                    HandleDeletedBlock(identifier);
                    break;
                case NRMeshingBlockState.NR_MESHING_BLOCK_STATE_UNCHANGED:
                default:
                    break;
            }
            profile.EndProfile(sample_UpdateMeshInfo);
            NRDebugger.Info($"[{nameof(MeshClassificationFracking)}] {nameof(UpdateMeshInfo)} End");

        }

        private void HandleDeletedBlock(ulong identifier)
        {
            string sample_HandleDeletedBlock = $"{nameof(HandleDeletedBlock)}";
            profile.BeginProfile(sample_HandleDeletedBlock);

            if (m_ClassifiedMeshFilters.TryGetValue(identifier, out var filters))
            {
                foreach (var kvpair in filters)
                {
                    if (kvpair.Value != null)
                    {
                        Destroy(kvpair.Value.gameObject);
                    }
                }
                m_ClassifiedMeshFilters.Remove(identifier);
            }
            profile.EndProfile(sample_HandleDeletedBlock);

        }

        private void HandleUpdateBlock(ulong identifier, Mesh baseMesh, NRMeshingVertexSemanticLabel[] labels)
        {
            string sample_HandleUpdateBlock = $"{nameof(HandleUpdateBlock)}";
            profile.BeginProfile(sample_HandleUpdateBlock);

            if (!m_ClassifiedMeshFilters.TryGetValue(identifier, out Dictionary<NRMeshingVertexSemanticLabel, MeshFilter> filters))
            {
                filters = new Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>();
                for (int i = 0; i < m_ClassifiedMeshFilterPrefabs.Length; ++i)
                {
                    var pair = m_ClassifiedMeshFilterPrefabs[i];
                    filters.Add(pair.label, pair.meshFilter == null ? null : Instantiate(pair.meshFilter));
                }
                m_ClassifiedMeshFilters.Add(identifier, filters);
            }

            if (!m_ClassifiedMeshes.TryGetValue(identifier, out Dictionary<NRMeshingVertexSemanticLabel, Mesh> classifiedMeshes))
            {
                classifiedMeshes = new Dictionary<NRMeshingVertexSemanticLabel, Mesh>();
                for (int i = 0; i < AvailableLabels.Length; ++i)
                {
                    var label = AvailableLabels[i];
                    classifiedMeshes.Add(label, new Mesh());
                }

                m_ClassifiedMeshes.Add(identifier, classifiedMeshes);
            }

            ExtractClassifiedMesh(baseMesh, labels, classifiedMeshes);

            for (int i = 0; i < AvailableLabels.Length; ++i)
            {
                var label = AvailableLabels[i];
                if (filters.TryGetValue(label, out var classifiedMeshFilter))
                {
                    if (classifiedMeshes.TryGetValue(label, out var mesh))
                    {
                        classifiedMeshFilter.sharedMesh = mesh;

                        MeshCollider meshCollider = classifiedMeshFilter.GetComponent<MeshCollider>();
                        if (meshCollider != null)
                        {
                            meshCollider.sharedMesh = mesh;
                        }
                    }
                }
            }

            profile.EndProfile(sample_HandleUpdateBlock);
        }

        private static List<int> s_BaseTriangles = new List<int>();
        private static Dictionary<NRMeshingVertexSemanticLabel, List<int>> s_LabelClassifiedTrianglesDict = new Dictionary<NRMeshingVertexSemanticLabel, List<int>>();

        static FunctionTimeProfile profile = new FunctionTimeProfile(nameof(MeshClassificationFracking));
        private void ExtractClassifiedMesh(Mesh baseMesh, NRMeshingVertexSemanticLabel[] labels, Dictionary<NRMeshingVertexSemanticLabel, Mesh> classifiedMeshes)
        {
            string sample_ExtractClassifiedMesh = nameof(ExtractClassifiedMesh);
            profile.BeginProfile(sample_ExtractClassifiedMesh);

            baseMesh.GetIndices(s_BaseTriangles, 0);

            // Renew s_LabelClassifiedTrianglesDict data
            s_LabelClassifiedTrianglesDict.Clear();
            for (int i = 0; i < AvailableLabels.Length; ++i)
            {
                var label = AvailableLabels[i];
                var classifiedTriangles = new List<int>();
                classifiedTriangles.Capacity = s_BaseTriangles.Count;
                s_LabelClassifiedTrianglesDict.Add(label, classifiedTriangles);
            }

            // Iterate through each triangle face and assign it to the list of triangle faces for a certain label.
            for (int i = 0; i < s_BaseTriangles.Count / 3; i++)
            {
                int idx_0 = s_BaseTriangles[i * 3];
                int idx_1 = s_BaseTriangles[i * 3 + 1];
                int idx_2 = s_BaseTriangles[i * 3 + 2];

                NRMeshingVertexSemanticLabel[] sl = new NRMeshingVertexSemanticLabel[] {
                    labels[idx_0],
                    labels[idx_1],
                    labels[idx_2]
                };

                // If this triangle has a vertex with label A, this triangle belongs to the lable A
                NRMeshingVertexSemanticLabel[] selectedLabels = RemoveDuplicateLabels(sl);

                for (int j = 0; j < selectedLabels.Length; ++j)
                {
                    try
                    {
                        NRMeshingVertexSemanticLabel selectedLabel = selectedLabels[j];
                        var classifiedTriangles = s_LabelClassifiedTrianglesDict[selectedLabel];
                        classifiedTriangles.Add(idx_0);
                        classifiedTriangles.Add(idx_1);
                        classifiedTriangles.Add(idx_2);
                    }
                    catch (System.Exception ex)
                    {
                        NRDebugger.Warning($"[{nameof(MeshClassificationFracking)}] {ex.Message}");
                    }
                }
            }

            profile.BeginProfile("ComputeWireframeMeshData");
            // Setup classified mesh for each label
            for (int i = 0; i < AvailableLabels.Length; ++i)
            {
                try
                {
                    NRMeshingVertexSemanticLabel selectedLabel = AvailableLabels[i];
                    var classifiedTriangles = s_LabelClassifiedTrianglesDict[selectedLabel];
                    Mesh classifiedMesh = classifiedMeshes[selectedLabel];
                    WirframeMeshComputor.ComputeWireframeMeshData(baseMesh.vertices, baseMesh.normals, classifiedTriangles, classifiedMesh);
                }
                catch (System.Exception ex)
                {
                    NRDebugger.Warning($"[{nameof(MeshClassificationFracking)}] {ex.Message}");
                }
            }
            profile.EndProfile("ComputeWireframeMeshData");

            profile.EndProfile(sample_ExtractClassifiedMesh);
        }

        static Dictionary<NRMeshingVertexSemanticLabel, int> s_labelCountDict = new Dictionary<NRMeshingVertexSemanticLabel, int>();
        private static NRMeshingVertexSemanticLabel SelectMostMatchingLabelForTriangle(NRMeshingVertexSemanticLabel[] triangleLabels)
        {
            s_labelCountDict.Clear();
            int maxCount = 0;
            NRMeshingVertexSemanticLabel selectedLabel = triangleLabels[0];

            for (int i = 0; i < triangleLabels.Length; ++i)
            {
                var label = triangleLabels[i];
                if (!s_labelCountDict.TryGetValue(label, out var count))
                {
                    s_labelCountDict[label] = 1;
                    if (maxCount < 1)
                    {
                        maxCount = 1;
                        selectedLabel = label;
                    }
                }
                else
                {
                    s_labelCountDict[label] = count + 1;
                    if (maxCount < count + 1)
                    {
                        maxCount = count + 1;
                        selectedLabel = label;
                    }
                }
            }
            return selectedLabel;
        }

        static HashSet<NRMeshingVertexSemanticLabel> s_TempHashSet = new HashSet<NRMeshingVertexSemanticLabel>();
        private static NRMeshingVertexSemanticLabel[] RemoveDuplicateLabels(NRMeshingVertexSemanticLabel[] triangleLabels)
        {
            s_TempHashSet.Clear();
            for (int i = 0; i < triangleLabels.Length; ++i)
            {
                var label = triangleLabels[i];
                if ((int)label == 9 || (int)label == 3)
                {
                    NRDebugger.Warning($"[{nameof(MeshClassificationFracking)}] {nameof(RemoveDuplicateLabels)} find invalid label {label}");
                }
                else
                {
                    s_TempHashSet.Add(label);

                }
            }
            return s_TempHashSet.ToArray();
        }


        class FunctionTimeProfile
        {
            Dictionary<string, Stopwatch> samplerDict = new Dictionary<string, Stopwatch>();
            private string m_Domain;
            public FunctionTimeProfile(string domain)
            {
                m_Domain = domain;
            }

            public void BeginProfile(string samplerName)
            {
                if (!samplerDict.TryGetValue($"{m_Domain}.{samplerName}", out var stopwatch))
                {
                    stopwatch = new Stopwatch();
                    samplerDict.Add($"{m_Domain}.{samplerName}", stopwatch);
                }
                stopwatch.Restart();
            }

            public void EndProfile(string samplerName)
            {
                if (samplerDict.TryGetValue($"{m_Domain}.{samplerName}", out var stopwatch))
                {
                    NRDebugger.Info($"{m_Domain}.{samplerName} takes {stopwatch.Elapsed.TotalMilliseconds}ms");
                }
            }
        }
    }

    [System.Serializable]
    public class LabelMeshFilterPair
    {
        public NRMeshingVertexSemanticLabel label;
        public MeshFilter meshFilter;
    }
}