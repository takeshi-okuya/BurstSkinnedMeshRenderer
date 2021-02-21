float4x4 _SkinMatrices[1020];

struct Bone
{
    float4 weights : TEXCOORD6;
    float4 indices : TEXCOORD7;
};

float4x4 GenerateSkinMatrix(in Bone bone)
{
    float4x4 skinMatrix = bone.weights.x * _SkinMatrices[bone.indices.x]
                        + bone.weights.y * _SkinMatrices[bone.indices.y]
                        + bone.weights.z * _SkinMatrices[bone.indices.z]
                        + bone.weights.w * _SkinMatrices[bone.indices.w];

    return skinMatrix;
}

void TransformSkin(in Bone bone, inout float4 vertex)
{
    float4x4 skinMatrix = GenerateSkinMatrix(bone);
    vertex = mul(skinMatrix, vertex);
}

float4 TransformSkin(in Bone bone, inout float4 vertex, inout float3 normal)
{
    float4x4 skinMatrix = GenerateSkinMatrix(bone);
    vertex = mul(skinMatrix, vertex);
    normal = mul((float3x3)skinMatrix, normal);
}
