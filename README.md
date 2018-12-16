# Neural Action 

Neural Action is a real-time CNN-based cross-platform gaze tracking application providing a human-machine interface to improve accessibility.

# Getting Started

## 1. Install Dependencies

### Hardware Requirements

 - CPU: AMD Ryzen 1600 ~

 - RAM: 8GB ~

 - OS: Windows 10

And we suggest using NVidia GPUs that supports CUDA9 and cuDNN.

### Software Requirements

No additional software requirements when using CPU.

If you want to run the program with GPU acceleration, you will need the following libraries.

 - CUDA 9.0

 - cuDNN 7.0

## 2. Download Release

Download latest release from `Releases` tab.

After unzipping the executable, run `NeuralAction.WPF.exe`.

# Build and Test

## Build from source

First, clone our repository.
`git clone https://github.com/NeuralAction/NeuralAction.git --recursive`

Then setup TensorFlowSharp following instruction. Download prebuilt TensorFlow binaries from instruction.
[TensorFlowSharp Instruction](https://github.com/gmlwns2000/TensorFlowSharp)

To build NeuralAction, you need to recover NuGet package first. You need to be careful order of solution to fix NuGet. You must fix NuGet following order.
`Vision/SharpFace/OpenCVSharp > Vision/SharpFace > Vision/TensorFlowSharp > Vision > NeuralAction`

Then open `NeuralAction` solution, set platform target as `x64` and set solution to configure as `Debug`.

## Debug Tracker

You could open tracker debugger by the press `F12` in the setting window.