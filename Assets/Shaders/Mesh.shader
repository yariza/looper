Shader "Unlit/Mesh"
{
    Properties
    {
        _DistanceThreshold("Dist Threshold", Range(0, 1)) = 1
    }
    CGINCLUDE

    #pragma multi_compile __ FLIP

    #include "UnityCG.cginc"

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float4 color : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
    };

    StructuredBuffer<float4> _ParticlePositionBuffer;
    StructuredBuffer<uint> _MeshIndicesBuffer;
    float4x4 _ModelMat;

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

        float4 c = UnpackColor(asuint(particle.w));

        p = float4(mul(_ModelMat, p).xyz, 1.0);
        v2f o;
        o.worldPos = p.xyz;
        p = UnityWorldToClipPos(p.xyz);

        o.vertex = p;
        o.color = c;
        return o;
    }

    [maxvertexcount(3)]
    void geom (triangle v2f input[3], inout TriangleStream<v2f> outputStream)
    {
        float3 pos0 = input[0].worldPos;
        float3 pos1 = input[1].worldPos;
        float3 pos2 = input[2].worldPos;

        float3 d01 = pos0 - pos1;
        float3 d12 = pos2 - pos1;
        float3 d02 = pos2 - pos0;

        float l01 = dot(d01, d01);
        float l12 = dot(d12, d12);
        float l02 = dot(d02, d02);

        float thresh = _DistanceThreshold * _DistanceThreshold;

        if (l01 > thresh ||
            l12 > thresh ||
            l02 > thresh)
        {
            return;
        }

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
