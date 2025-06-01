using UnityEditor;
using UnityEngine;
public class Graph : MonoBehaviour
{
    Transform[] points;

    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 100)]
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


    void Awake()
    {
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        points = new Transform[resolution * resolution];

        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab);  // "i" ile sürekli küp üretimi sağlanıyor
            point.localScale = scale;
            point.SetParent(transform, false);
        }
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

        if (transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }
        
        

    }

    void UpdateFunction()
    {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u, v, time);
        }
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?         //Eğer Cycle ise sıra ile değilse random fonksiyon göster
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
    
    void UpdateFunctionTransition()
    {
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction) , to = FunctionLibrary.GetFunction(function); //
        float progress = duration / transitionDuration; //
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }





    //artık x ve z de update de güncelleneceği için awakede tanımlamaya gerek kalmadaı ve awakde sadeleşti ancak update ve FunctionLibrary 3 vektörü ile güncellendi 
    //alttaki eski update
    /*void Update()
    {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            //position.y = Mathf.Sin(Mathf.PI * (position.x + time)); //xküp yerine sin kullanıyoruz ki tekrarlansın
            position.y = f(position.x, position.z, time);
            point.localPosition = position;
        }


    }*/
}
