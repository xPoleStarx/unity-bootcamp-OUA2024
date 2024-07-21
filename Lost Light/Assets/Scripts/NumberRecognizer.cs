using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Linq;

public class NumberRecognizer : MonoBehaviour
{
    private InferenceSession session;

    void Start()
    {
        session = new InferenceSession("C:/Users/seyfullahkorkmaz/UNITY_PROJECTS/Lost Light/Assets/Plugins/mnist-8.onnx");
    }

    public int RecognizeNumber(Tensor<float> inputTensor)
    {
        var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", inputTensor) }; // Giriþ adýný burada güncelleyin
        using (var results = session.Run(input))
        {
            var output = results.First().AsEnumerable<float>().ToArray();
            return output.ToList().IndexOf(output.Max());
        }
    }
}
