using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;

namespace WCGL
{
    class BurstSkinnedMeshRendererCore : IDisposable
    {
        SkinnedMeshRenderer renderer;
        Mesh mesh;
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        NativeArray<Matrix4x4> skinMatrices, boneMatrices, bindposes;
        Matrix4x4[] skinMatricesManaged;
        JobHandle handle;

        [BurstCompile]
        struct UpdateSkinmatricesJob : IJob
        {
            [WriteOnly] public NativeArray<Matrix4x4> skinMatrices;
            [ReadOnly] public NativeArray<Matrix4x4> boneMatrices, bindposes;
            [ReadOnly] public Matrix4x4 rootMatrix;

            void IJob.Execute()
            {
                for (int i = 0; i < skinMatrices.Length; i++)
                {
                    skinMatrices[i] = rootMatrix * boneMatrices[i] * bindposes[i];
                }
            }
        }

        public BurstSkinnedMeshRendererCore(SkinnedMeshRenderer skinnedMeshRenderer,
            Vector3[] vertices = null, Vector3[] normals = null, (int[] indices, MeshTopology topology)[] indices = null)
        {
            void initBoneWeight(Mesh mesh)
            {
                var weights = mesh.boneWeights;
                var dstWeights = new Vector4[weights.Length];
                var dstIndices = new Vector4[weights.Length];

                for (int i = 0; i < weights.Length; i++)
                {
                    var w = weights[i];
                    dstWeights[i] = new Vector4(w.weight0, w.weight1, w.weight2, w.weight3);
                    dstIndices[i] = new Vector4(w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3);
                }

                mesh.SetUVs(6, dstWeights);
                mesh.SetUVs(7, dstIndices);
            }

            renderer = skinnedMeshRenderer;
            mesh = GameObject.Instantiate(skinnedMeshRenderer.sharedMesh);

            if (vertices != null) { mesh.SetVertices(vertices); }
            if (normals != null) { mesh.SetNormals(normals); }

            if (indices != null)
            {
                for (int i = 0; i < indices.Length; i++)
                {
                    mesh.SetIndices(indices[i].indices, indices[i].topology, i);
                }
            }

            initBoneWeight(mesh);
            mesh.UploadMeshData(true);

            int length = skinnedMeshRenderer.bones.Length;
            skinMatrices = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
            boneMatrices = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
            bindposes = new NativeArray<Matrix4x4>(mesh.bindposes, Allocator.Persistent);
            skinMatricesManaged = new Matrix4x4[length];
        }

        public void UpdateSkinMatrices()
        {
            var _rootMatrix = renderer.transform.localToWorldMatrix;
            var bones = renderer.bones;

            for (int i = 0; i < bones.Length; i++)
            {
                boneMatrices[i] = bones[i].localToWorldMatrix;
            }

            var job = new UpdateSkinmatricesJob()
            {
                skinMatrices = this.skinMatrices,
                boneMatrices = this.boneMatrices,
                bindposes = this.bindposes,
                rootMatrix = _rootMatrix
            };

            handle = job.Schedule();
        }

        public void DrawMesh(CommandBuffer commandBuffer, in Matrix4x4 matrix, Material material, int subMeshIndex)
        {
            handle.Complete();

            skinMatrices.CopyTo(skinMatricesManaged);
            materialPropertyBlock.SetMatrixArray("_SkinMatrices", skinMatricesManaged);

            commandBuffer.DrawMesh(mesh, matrix, material, subMeshIndex, -1, materialPropertyBlock);
        }

        public virtual void Dispose()
        {
            skinMatrices.Dispose();
            boneMatrices.Dispose();
            bindposes.Dispose();
        }
    }
}
