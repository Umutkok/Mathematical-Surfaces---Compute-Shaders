Shader "MyShaders/BPR shader-2"
{
    Properties {
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _PixelSize ("Pixel Size", Float) = 50

        _ColorTop ("Top Color", Color) = (1,1,1,1)
        _ColorMiddle ("Middle Color", Color) = (0,0.5,1,1)
        _ColorBottom ("Bottom Color", Color) = (0,1,1,1)
    }

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float3 worldPos;
        };

        float _Smoothness;
        float _PixelSize;

        fixed4 _ColorTop;
        fixed4 _ColorMiddle;
        fixed4 _ColorBottom;

        void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
            // Dünyadaki pozisyonu pikselleştir
            float3 pixelatedPos = floor(input.worldPos.xyz * _PixelSize) / _PixelSize;

            // Yüksekliği normalize et (-1 ile 1 aralığını 0 ile 1'e çeker)
            float amount = saturate(pixelatedPos.y * 0.5 + 0.5);

            fixed3 color;

            // 3 bölgeli geçiş: alt - orta - üst
            if (amount < 0.3)
            {
                // Alt bölgeden orta bölgeye geçiş
                float t = saturate(amount / 0.33);
                color = lerp(_ColorBottom.rgb, _ColorMiddle.rgb, t);
            }
            else if (amount < 0.4)
            {
                // Orta bölge - sabit renk
                color = _ColorMiddle.rgb;
            }
            else
            {
                // Orta bölgeden üst bölgeye geçiş
                float t = saturate((amount - 0.66) / 0.34); // dikkat: 1.0 - 0.66 = 0.34
                color = lerp(_ColorMiddle.rgb, _ColorTop.rgb, t);
            }

            surface.Albedo = color;
            surface.Smoothness = _Smoothness;
        }

        ENDCG
    }

    FallBack "Diffuse"
}