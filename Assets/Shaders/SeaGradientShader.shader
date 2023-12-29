Shader "Custom/SeaGradientShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
    }
    SubShader
    {
        Tags {"Queue"="Overlay" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 ps3
            ENDCG

            SetTexture[_Dummy] {
                combine primary
            }
        }
    }
    Fallback "Diffuse"
}