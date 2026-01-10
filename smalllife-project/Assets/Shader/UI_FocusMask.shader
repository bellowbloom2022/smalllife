Shader "UI/FocusMask"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _FocusCenter ("Focus Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0.1
        _SoftEdge ("Soft Edge", Float) = 0.05
        _Aspect ("Aspect", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float2 _FocusCenter;
            float _Radius;
            float _SoftEdge;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - _FocusCenter;
                // 计算屏幕比例
                float aspect = _ScreenParams.x / _ScreenParams.y;
                // 拉伸 X，让计算空间变成“正圆”
                uv.x *= aspect;

                float dist = length(uv);

                float mask = smoothstep(_Radius, _Radius + _SoftEdge, dist);

                fixed4 col = _Color;
                col.a *= mask;
                return col;
            }
            ENDCG
        }
    }
}
