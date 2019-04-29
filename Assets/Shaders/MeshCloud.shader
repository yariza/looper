Shader "Unlit/MeshCloud"
{
    Properties
    {
        _PositionBuffer("-", 2D) = "black"{}
        _ColorBuffer("-", 2D) = "black"{}
        _SlopeThreshold("Slope Threshold", Range(0, 100)) = 1000
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

    sampler2D _PositionBuffer;
    float4 _PositionBuffer_TexelSize;
    sampler2D _ColorBuffer;

    float4x4 _ModelMat;

    float3 _BoundsMin;
    float3 _BoundsMax;

    float _SlopeThreshold;

    static uint2 pixelOffset[6] =
    {
        uint2(0, 0),
        uint2(0, 1),
        uint2(1, 0),
        uint2(1, 1),
        uint2(1, 0),
        uint2(0, 1)
    };

    v2f vert (uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
    {
        uint id = instance_id * 3 + vertex_id;
        uint pixelIndex = id / 6;
        uint2 pixelCoord = uint2(
            pixelIndex % ((uint)_PositionBuffer_TexelSize.z - 1),
            pixelIndex / ((uint)_PositionBuffer_TexelSize.z - 1)
        );
        pixelCoord += pixelOffset[id % 6];

        float4 uv = float4(
            ((float2)pixelCoord + 0.5) * _PositionBuffer_TexelSize.x,
            0, 0
        );

        // float4 particle = _ParticlePositionBuffer[index];
        float4 p = float4(tex2Dlod(_PositionBuffer, uv).xyz, 1.0);
        // float4 p = float4(uv.xy, 0, 1.0);
        // float4 p = float4(particle.xyz, 1.0);

        #if !FLIP
        p.x = -p.x;
        #endif
        v2f o;

        float4 c = tex2Dlod(_ColorBuffer, uv);
        // float4 c = UnpackColor(asuint(particle.w));

        o.localPos = p.xyz;
        p = float4(mul(_ModelMat, p).xyz, 1.0);
        o.worldPos = p.xyz;

        if (any(p.xyz < _BoundsMin || p.xyz > _BoundsMax))
        {
            p.xyz = float3(0.0 / 0.0, 0.0 / 0.0, 0.0 / 0.0);
        }

        p = UnityWorldToClipPos(p.xyz);

        o.vertex = p;
        o.color = c;
        return o;
    }

    [maxvertexcount(3)]
    void geom (triangle v2f input[3], inout TriangleStream<v2f> outputStream)
    {
        float3 pos0 = input[0].localPos;
        float3 pos1 = input[1].localPos;
        float3 pos2 = input[2].localPos;

        float x0 = pos0.x / pos0.z;
        float x1 = pos1.x / pos1.z;
        float x2 = pos2.x / pos2.z;

        float dz = max(abs(pos0.z - pos1.z), max(abs(pos1.z - pos2.z), abs(pos0.z - pos2.z)));
        float dx = max(abs(x0 - x1), max(abs(x1 - x2), abs(x0 - x2)));

        float slope = dz / dx;

        if (slope > _SlopeThreshold)
        {
            return;
        }

        // input[0].color = input[1].color = input[2].color = float4(
        //     slope.xxx, 1.0
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
