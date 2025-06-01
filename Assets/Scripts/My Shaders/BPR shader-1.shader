Shader "MyShaders/BPR shader-1"
{
    Properties {
        _PixelSize ("Pixel Size", Float) = 50
    }

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float3 worldPos;
            float2 uv; // Gerekirse kullanılabilir
        };

        float _PixelSize;

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) 
        {
            // Pozisyonu "pikselize" etmek için dünyadaki konumu böleceğiz
            float3 pixelatedPos = floor(input.worldPos.xyz * _PixelSize) / _PixelSize;

            // Buradan x değerini alıp 0-1 aralığına çeviriyoruz
            float Amount = saturate(pixelatedPos.y * 0.1 + 0.2);

            // Renk kırmızıdan siyaha geçer (siyah: 0,0,0 - kırmızı: 1,0,0)
            surface.Albedo = float3(Amount, Amount, Amount);

            surface.Smoothness = 0.3; // pixel tarzı için pürüzsüz olmasın
            surface.Metallic = 1.0;
            surface.Occlusion = 0.3; 
        }

        ENDCG
    }

    FallBack "Diffuse"
}