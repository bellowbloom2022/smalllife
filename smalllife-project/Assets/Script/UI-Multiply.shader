Shader "UI/MultiplyOverlay"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (1,1,1,1)
        [HideInInspector]_MainTex ("Sprite Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend DstColor Zero
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Offset 1, 1
        ColorMask RGB
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed alpha = saturate(i.color.a);
                fixed3 tint = lerp(fixed3(1.0, 1.0, 1.0), i.color.rgb, alpha);
                return fixed4(tint, 1.0);
            }
            ENDCG
        }
    }
}
