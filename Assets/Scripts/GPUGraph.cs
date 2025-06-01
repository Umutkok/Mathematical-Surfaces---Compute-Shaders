using UnityEditor;
using UnityEngine;
public class GPUGraph : MonoBehaviour
{
    [SerializeField]
	ComputeShader computeShader;

    //Compute shader içindeki değişkenlere (_Positions, _Step gibi) ulaşmak için isimler değil ID le kullanılır.
    //Bu ID’ler çalışma boyunca değişmez. Performans için, bu ID’leri statik ve readonly alanlarda saklarız, böylece sürekli tekrar hesaplanmazlar.
    //(readonly) kullanarak bu alanların sabit olduğunu ve değiştirilmemesi gerektiğini belirtiyoruz.
    //shader özelliklerine erişim için Shader.PropertyToID(string) fonksiyonunu kullanılır.
    //Bu fonksiyon string olarak isim verdiğimiz shader değişkenlerinin benzersiz tamsayı(int) ID’lerini döndürür.
    static readonly int positionsId = Shader.PropertyToID("_Positions");
    static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    static readonly int stepId = Shader.PropertyToID("_Step");
    static readonly int timeId = Shader.PropertyToID("_Time");

    static readonly int transitionProgressId = Shader.PropertyToID("_TransitionProgress");

	[SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;
    const int maxResolution = 1000;


    //eski başı
    [SerializeField, Range(10, maxResolution)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;
    float duration;
    bool transitioning;
    FunctionLibrary.FunctionName transitionFunction;


    public enum TransitionMode { Cycle, Random }

    [SerializeField]
    TransitionMode transitionMode;

	ComputeBuffer positionsBuffer;

	void OnEnable ()
    {
		positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
	}

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
	}

    void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration; // 0 olmamasının sebebi fazladan geçen zamanı çöpe atmadan bir sonraki hesaplamaya taşıyarak zamanlama doğruluğunu korumak.
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }
        
        UpdateFunctionOnGPU();

    }

    void PickNextFunction()
    {
        //Eğer Cycle ise sıra ile, değilse random fonksiyon göster
        function = transitionMode == TransitionMode.Cycle ? FunctionLibrary.GetNextFunctionName(function) : FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }    

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);

		if (transitioning) {
			computeShader.SetFloat(
				transitionProgressId,
				Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
			);
		}

        //daha önce her zaman sıfır (0) olan kernelIndex yerine, artık seçilen fonksiyona göre değişen bir index kullanmamız gerekiyor.
        //+ dan sonrası compute shader içindeki doğru kernel fonksiyonunu seçmek için Çünkü her geçiş için ayrı bir kernel fonksiyonu yazıldı,
        //ve bu fonksiyonların sırası belirli bir düzene göre yapıldı.
        var kernelIndex = (int)function + (int)(transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;

        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer); //0(sonradan değişti) kernel indexi için compute shader birden fazla kernele sahip olabilir(biz zaten 1 tane kullancaz). 
                                                                  // sonraki ikili buffer’ın compute shader daki adı(positionsId) ile buradaki değişken(positionsBuffer) ile eşlemek

        int groups = Mathf.CeilToInt(resolution / 8f);//8’in katı değilse, grup sayısını yukarı yuvarlayarak eksik hesaplama yapmamayı sağlıyoruz.
                                                      // 8x8 numthred grubu oluşturduğumuz için sekize bölüp her boyut için gereken grup sayısını bulduk.
                                                      // örnek: X boyutunda: Mathf.CeilToInt(24 / 8f) = 3, Y boyutunda: Mathf.CeilToInt(24 / 8f) = 3 Yani, toplamda 3x3 = 9 grup gerekir.
        computeShader.Dispatch(kernelIndex, groups, groups, 1); // ilk parametre yine kernel index sini temsil ediyor(sonradan değişti). diğerleri Her boyut için kaç grup çalıştırılacağını söylüyor. x ve y yi kullanıyoruz.

        
        material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution); //neden resulation lar çarpılıyo incele !!
        

	}
}
