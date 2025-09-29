          Shader "Custom/DrawCircleInstances"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4x4 _ObjectToWorld;
            uniform float _NumInstances;

            struct BodyInputData
            {
                float2 pos;
                float mass;
                float radius;
                float2 last;
            };

            StructuredBuffer<BodyInputData> Circles;


            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                v.vertex.xyz *= Circles[instanceID].radius * 3;

                float4 wpos = mul(_ObjectToWorld, v.vertex + float4(Circles[instanceID].last,0.0f,0.0f));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {               


                 float t = 1 - smoothstep(0.25, 0.5,length(i.uv - float2(0.5,0.5)));

                 t = clamp(t, 0, 1);
             
                 return float4(1.0f,1.0f,1.0f,t);

            }
            ENDCG
        }
    }
}