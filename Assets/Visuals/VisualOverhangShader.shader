Shader "Custom/Visual"
{
    Properties
    {
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _GradientStart ("ColorStart", Color) = (1,1,0,1)
        _GradientEnd ("ColorEnd", Color) = (1,0,0,1)
        _GradientMin ("GradientMin", float) = 0.65 // 50 degrees
        _GradientMax ("GradientMax", float) = 0.985 // 10 degrees
    }
    SubShader
    {
        Pass
        {
            Name "Pass 1"
            Tags
            {
                "Queue"="Opaque"
                "RenderType"="Opaque"
                "LightMode"="ForwardBase"
            }
            Cull Off
            CGPROGRAM

            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            #include "UnityLightingCommon.cginc"
            #pragma fragment frag
            #pragma vertex vert

            float _GradientMin;
            float _GradientMax;
            fixed4 _GradientStart;
            fixed4 _GradientEnd;

            struct v2f
            {
                fixed4 diff:COLOR0;
                float4 vertex:SV_POSITION;
                float overhang:TEXCOORD0;
            };
            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                half4 wpos = mul(unity_ObjectToWorld , v.vertex);
                half3 worldNormal = UnityObjectToWorldNormal(-v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));
                o.overhang = wpos.z<=0.001 ? -1 : dot(worldNormal, float3(0.0, 0.0, 1.0));
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float o = i.overhang;
                float a = (o-_GradientMin) / (_GradientMax-_GradientMin);
                clip(a);
                fixed4 col = lerp(_GradientStart, _GradientEnd, a);
                col.a = 1.0f;
                return col * i.diff;
            }
            ENDCG
        }
        Pass
        {
            Name "Pass 2"
            Tags
            {
                "Queue" = "Transparent"
                "RenderType"="Transparent"
                "LightMode"="ForwardBase"
            }
            ZWrite Off
            ZTest OFf
            Blend One One // SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            #include "UnityLightingCommon.cginc"
            #pragma fragment frag
            #pragma vertex vert
            #pragma alpha:fade

            float _GradientMin;
            fixed4 _Color;

            struct v2f
            {
                fixed4 diff:COLOR0;
                float4 vertex:SV_POSITION;
                float overhang:TEXCOORD0;
            };
            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));
                o.overhang = dot(worldNormal, float3(0.0, -1.0, 0.0));
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float o = i.overhang;
                float a = _GradientMin-o;
                clip(a);
                return _Color * i.diff;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}