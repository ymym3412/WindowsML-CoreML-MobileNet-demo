from coremltools.models.utils import load_spec
from winmltools import convert_coreml
from winmltools.utils import save_model, save_text

# ml model load
model_coreml = load_spec("MobileNet.mlmodel")
# convert coreml models to onnx model
model_onnx = convert_coreml(model_coreml, name="mobilenet")

# save onnx format
save_model(model_onnx, "mobilenet.onnx")
save_text(model_onnx, "mobilenet.txt")
