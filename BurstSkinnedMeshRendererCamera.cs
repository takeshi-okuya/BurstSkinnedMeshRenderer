using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WCGL
{
    [RequireComponent(typeof(Camera))]
    public class BurstSkinnedMeshRendererCamera : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] BurstSkinnedMeshRenderer[] renderers;
#pragma warning restore 649

        CommandBuffer commandBuffer;

        void Start()
        {
            commandBuffer = new CommandBuffer();
            GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
        }

        private void OnPreRender()
        {
            foreach (var renderer in renderers)
            {
                if (renderer.isActiveAndEnabled) { renderer.UpdateSkinMatrices(); }
            }

            commandBuffer.Clear();
            foreach (var renderer in renderers)
            {
                if (renderer.isActiveAndEnabled) { renderer.drawMesh(commandBuffer); }
            }
        }
    }
}
