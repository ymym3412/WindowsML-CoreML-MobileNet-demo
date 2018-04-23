# WindowsML-CoreML-MobileNet-demo
Sample code of converting CoreML MobileNet into Windows Machine Learning.  
We prepare CoreML MobileNet for image classification from [here](https://coreml.store/mobilenet).  

![mobilenet_demo](mobilenet_demo.gif)

## Prepare model
Download MobileNet model from [Core ML Store](https://coreml.store/mobilenet).  
Then you see `MobileNet.mlmodel` , move model file to `./convert_model` directory.  

We use `coremltools` for loading CoreML model and `winmltools` for converting CoreML model into ONNX format.  
First, install `coremltools` and `winmltools` by pip.  

```shell
pip install coremltools winmltools
```

Second, execute python file for converting CoreML model.  

```shell
python coreml2onnx.py
```

After execution, you will get two files `mobilenet.onnx` and `mobilenet.txt`.  
Third, create C# code describing ONNX model.  

```shell
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.17125.0\x64\mlgen.exe" -i mobilenet.onnx -l CS -n mobilenet -o mobilenet.cs
```

`mobilenet.cs` describes three classes `MobilenetModelInput` and `MobilenetModelOutput`and `MobilenetModel`.  
Each class correspond `Network input(image)`, `Network output(class label and probability)` and `MobileNet Network and inference method`.  

Finally, move each files.  
`mobilenet.cs -> {solution dir}`  
`mobilenet.onnx -> {solution dir}/Assets`  

## Run demo
Start solution application.  

### Deploying the sample
- Select Build > Deploy Solution.
