Shader "Custom/IgnoreZ" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
//		Tags { "RenderType"="Transparent" }
		Tags { "Queue"="Transparent" }

	    Pass {
	        Blend SrcAlpha OneMinusSrcAlpha
	        Lighting Off
	        ZWrite Off
		    ZTest Always
		    CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members viewAngle)
			// #pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;

			struct data 
			{
			    float4 vertex : POSITION;
			    float3 normal : NORMAL;
			};

			struct v2f
			{
			    float4 position : SV_POSITION;
			};

			v2f vert(data i)
			{
			    v2f o;
			    o.position = UnityObjectToClipPos(i.vertex);
			    return o;
			}

			float4 frag( v2f i ) : COLOR
			{   
			    return _Color;
			}

			ENDCG
        }
	}
    FallBack "Diffuse"
}
