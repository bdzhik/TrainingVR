Shader "TrainingVR/Selection Outline"
{
    Properties
    {
        [HDR] _OutlineColor("Outline Color", Color) = (1, 0.65, 0.05, 1)
        _OutlineWidth("Outline Width (pixels)", Range(1, 10)) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "SelectionOutline"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float2 normalCS = normalize(mul((float3x3)UNITY_MATRIX_VP, normalWS).xy);
                normalCS.x *= _ScreenParams.y / _ScreenParams.x;
                positionHCS.xy += normalCS * (_OutlineWidth * 2.0 / _ScreenParams.y) * positionHCS.w;
                output.positionHCS = positionHCS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
