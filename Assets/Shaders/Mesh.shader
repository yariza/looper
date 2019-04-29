Shader "Unlit/Mesh"
{
    Properties
    {
    }
    CGINCLUDE

    #pragma multi_compile __ FLIP

    #include "UnityCG.cginc"

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float4 color : TEXCOORD0;
    };

    StructuredBuffer<float4> _ParticlePositionBuffer;
    StructuredBuffer<uint> _MeshIndicesBuffer;
    float4x4 _ModelMat;

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
        p = UnityWorldToClipPos(p.xyz);

        v2f o;
        o.vertex = p;
        o.color = c;
        return o;
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
            #pragma fragment frag
            ENDCG
        }
    }
}
