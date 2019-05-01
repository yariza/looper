Shader "Unlit/Mesh"
{
    Properties
    {
        _DistanceThreshold("Distance Threshold", Range(0, 1)) = 0.1
    }
    CGINCLUDE

    #pragma multi_compile __ FLIP

    #include "UnityCG.cginc"

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float4 color : TEXCOORD0;
        float3 localPos : TEXCOORD1;
        float3 worldPos : TEXCOORD2;
    };

    StructuredBuffer<float4> _ParticlePositionBuffer;
    StructuredBuffer<uint> _MeshIndicesBuffer;
    float4x4 _ModelMat;
    float4x4 _BoundingBoxMat;

    float _DistanceThreshold;

    float4 UnpackColor(uint c)
    {
        return float4(
            (float)((c >> 16) & 0xFF) / 255.0,
            (float)((c >> 8) & 0xFF) / 255.0,
            (float)(c & 0xFF) / 255.0,
            1.0
        );
    }

    v2f vert (uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
    {
        uint id = instance_id * 3 + vertex_id;
        uint index = _MeshIndicesBuffer[id];

        float4 particle = _ParticlePositionBuffer[index];
        float4 p = float4(particle.xyz, 1.0);

        #if !FLIP
        p.x = -p.x;
        #endif
        v2f o;

        float4 c = UnpackColor(asuint(particle.w));

        o.localPos = p.xyz;
        p = float4(mul(_ModelMat, p).xyz, 1.0);
        o.worldPos = p.xyz;

        p = UnityWorldToClipPos(p.xyz);

        o.vertex = p;
        o.color = c;
        return o;
    }

    [maxvertexcount(3)]
    void geom (triangle v2f input[3], inout TriangleStream<v2f> outputStream)
    {
        float3 box0 = mul(_BoundingBoxMat, float4(input[0].worldPos, 1.0)).xyz;
        float3 box1 = mul(_BoundingBoxMat, float4(input[1].worldPos, 1.0)).xyz;
        float3 box2 = mul(_BoundingBoxMat, float4(input[2].worldPos, 1.0)).xyz;

        if (any(box0 < (-0.5).xxx) || any(box0 > (0.5).xxx) ||
            any(box1 < (-0.5).xxx) || any(box1 > (0.5).xxx) ||
            any(box2 < (-0.5).xxx) || any(box2 > (0.5).xxx))
        {
            return;
        }

        float3 pos0 = input[0].localPos;
        float3 pos1 = input[1].localPos;
        float3 pos2 = input[2].localPos;

        // float x0 = pos0.x / pos0.z;
        // float x1 = pos1.x / pos1.z;
        // float x2 = pos2.x / pos2.z;

        // float d01 = pos0 - pos1;
        // float d12 = pos1 - pos2;
        // float d02 = pos0 - pos2;

        // float d = max(dot(d01, d01), max(dot(d12, d12), dot(d02, d02)));
        float dz = max(abs(pos0.z - pos1.z), max(abs(pos1.z - pos2.z), abs(pos0.z - pos2.z)));
        // float dx = max(abs(x0 - x1), max(abs(x1 - x2), abs(x0 - x2)));

        if (dz > _DistanceThreshold)
        {
            return;
        }

        // input[0].color = input[1].color = input[2].color = float4(
        //     _BoundsMax.xyz, 1.0
        // );

        outputStream.Append(input[0]);
        outputStream.Append(input[1]);
        outputStream.Append(input[2]);
    }

    fixed4 frag (v2f i) : SV_Target
    {
        // sample the texture
        fixed4 col = i.color;
        return col;
    }

    ENDCG

    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        Cull Off
        ZWrite On
        ZTest On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            ENDCG
        }
    }
}
