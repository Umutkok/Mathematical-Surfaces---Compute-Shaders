using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
	public delegate Vector3 Function(float u, float v, float t);

	public enum FunctionName { Wave, MultiWave, Ripple, Sphere, Torus }
	static Function[] functions = { Wave, MultiWave, Ripple, Sphere, Torus };

	public static Function GetFunction(FunctionName name)
	{
		return functions[(int)name];
	}

	public static Vector3 Wave(float u, float v, float t)
	{
		Vector3 p;
		p.x = u;
		p.y = Sin(PI * (u + v + t));
		p.z = v;
		return p;
	}
	public static Vector3 MultiWave(float u, float v, float t)
	{
		Vector3 p;
		p.x = u;
		p.y = Sin(PI * (u + t));
		p.y += 0.5f * Sin(2f * PI * (v + t));
		p.y += Sin(PI * (u + v + 0.25f * t));
		p.y *= 1f / 2.5f;
		p.z = v;
		return p;
	}

	/*public static Vector3 Ripple(float u, float v, float t)
	{
		float d = Sqrt(u * u + v * v);
		Vector3 p;
		p.x = u;
		p.y = Sin(PI * (4f * d - t));
		p.y /= 1f + 10f * d; // =/ operatörü py = py / ...  anlamına gelir
		p.z = v;
		return p;
	}*/


	public static Vector3 Sphere(float u, float v, float t)
	{
		//float r = 0.5f + 0.5f * Sin(PI * t); 							//Küre
		//float r = 0.9f + 0.1f * Sin(8f * PI * u);						//dikey çıkıntılı küre	
		//float r = 0.9f + 0.1f * Sin(8f * PI * v);						//yatay çıkıntılı küre
		float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));        //hem dikey hem yatay twisted küre


		float s = r * Cos(0.5f * PI * v);       //Bu s, her bir v yüksekliğine göre hesaplanan yatay çemberin yarıçapıdır. 
												//Cos(π/2 * v) ile bu çemberlerin yarıçapı en tepede ve en altta sıfır olur, ortada maksimum olur.
		Vector3 p;
		p.x = s * Sin(PI * u);
		p.y = r * Sin(0.5f * PI * v);
		p.z = s * Cos(PI * u);
		return p;
	}

	/*/public static Vector3 Ripple (float x, float z, float t) 
		{
			float d = Sqrt(x * x + z * z);                // 1. d: orijinden uzaklık (radyal mesafe)
			float y = Sin(PI * (4f * d - t));            // 2. Sinüs dalgası: zamana ve mesafeye bağlı
			return y / (1f + 10f * d);                  // 3. Genliği mesafeye göre azalt
		}*/


	public static Vector3 Torus(float u, float v, float t)
	{
		//normal torus
		//float r1 = 0.75f;
		//float r2 = 0.25f;
		//spiral torus
		float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
		float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));

		float s = r1 + r2 * Cos(PI * v); //yarım daire tam daireye çevirdik dikkat!
		Vector3 p;
		p.x = s * Sin(PI * u);
		p.y = r2 * Sin(PI * v);
		p.z = s * Cos(PI * u);
		return p;
	}

	public static FunctionName GetNextFunctionName(FunctionName name)
	{
		if ((int)name < functions.Length - 1)
		{
			return name + 1;
		}
		else
		{
			return 0;
		}
		//return (int)name < functions.Length - 1 ? name + 1 : 0;  //tek satırda da yazılabilir
	}
	//public static FunctionName GetNextFunctionName (FunctionName name) => (int)name < functions.Length - 1 ? name + 1 : 0;   // üstteki tüm foksiyonun çok daha sade hali (set olmadığı için)


	public static FunctionName GetRandomFunctionNameOtherThan(FunctionName name)
	{
		var choice = (FunctionName)Random.Range(1, functions.Length);
		return choice == name ? 0 : choice; // aynı fonksiyonu tekrar seçmediğimize emin olduk
	}

	public static Vector3 Morph(float u, float v, float t, Function from, Function to, float progress) //fonksiyon karıştırma. Burada FunctionName yerine neden Function kullanıldı? araştır 
	{
		return Vector3.LerpUnclamped(from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress)); //LerpUnclamped neden?
	}







	public static int FunctionCount
	{
		get
		{
			return functions.Length;
		}
		//get => functions.Length;  //üstteki get bolğunun daha kısa yazımı
	}

	//public static int FunctionCount => functions.Length; // üstteki tüm fonksiyonun "set" olmadığı için daha da sade hali.





	public static Vector3 Ripple(float u, float v, float t)
	{
    float d = Sqrt(u * u + v * v); // Merkezden uzaklık
    Vector3 p;
    p.x = u;
    p.z = v;

    // Zamanla solan bir tümsek (Gaussian dağılım benzeri)
    p.y = Exp(-5f * d * d) * Cos(t); 

    return p;
	}
}

