using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WCGL
{
    public class BurstSkinnedMeshRenderer : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] Material material;
        [SerializeField] SkinnedMeshRenderer[] renderers;
#pragma warning restore 649

        BurstSkinnedMeshRendererCore[] cores;

        void Start()
        {
            cores = new BurstSkinnedMeshRendererCore[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                cores[i] = new BurstSkinnedMeshRendererCore(renderers[i]);
            }
        }

        public void UpdateSkinMatrices()
        {
            foreach (var core in cores)
            {
                core.UpdateSkinMatrices();
            }
        }

        public void drawMesh(CommandBuffer command)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var matrix = renderers[i].transform.localToWorldMatrix;
                var materials = renderers[i].sharedMaterials;

                for (int j = 0; j < materials.Length; j++)
                {
                    cores[i].DrawMesh(command, matrix, material, j);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var core in cores)
            {
                core.Dispose();
            }
        }
    }
}