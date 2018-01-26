# Neural Action 

Neural Action is a real-time CNN-based cross-platform gaze tracking application providing human-machine interface to improve accessibility.

# Getting Started

## 1. Installation process

First, clone our repository.
`git clone `

Then setup TensorFlowSharp following instruction. Download prebuilt TensorFlow binaries from instruction.
[TensorFlowSharp Instruction](https://github.com/gmlwns2000/TensorFlowSharp)

## 2.	Latest releases
Lastest release is on the `isef_k` branch.

# Build and Test

To build NeuralAction, you need to recover nuget package first. You need to be careful order of solution to fix nuget. You must fix nuget following order.
`Vision/SharpFace/OpenCVSharp > Vision/SharpFace > Vision/TensorFlowSharp > Vision > NeuralAction`

Then open `NeuralAction` solution, set platform target as `x64` and set solution configure as `Debug`.
