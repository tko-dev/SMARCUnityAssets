# SMARC Unity Assets
This is a package containing all of the SMARC Unity assets and scripts including vehicles, dynamics, sensors and more. 

If you are new to unity or just want to avoid installation for now and just try things out, I suggest using one of our pre-existing example projects at:

-  https://github.com/martkartasev/SMARCUnityHDRP
-  https://github.com/martkartasev/SMARCUnityStandard

HDRP Is our primary development pipeline, but we also support the Standard Built-In pipeline. 
In general HDRP has more features, including the Unity Water simulation and better graphical fidelity, while Standard is more performant.
The rest of the features are the same.

## Installation

------
We recommend using the the Editor Version 2023.1.13f1.

We rely on a few non unity asset store packages, so follow the instructions below to install the package in your project.

### Install Packages for Unity from Github

1. Open Package Manager window (Window | Package Manager)
2. Click `+` button on the upper-left of a window, and select "Add package from git URL..."
3. Enter the following URL and click `Add` button

```
https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
```
```
https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector
```
```
https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer
```

### Install NuGet packages

Once you have installed NuGet, install the Numerics package for matrix algebra.

1. Click the NuGet dropdown at the top of your Unity Editor (Sometimes requires editor restart to appear).
2. Manage NuGet packages
3. Search and install:
  a. MathNet.Numerics
  b. CoordinateSharp

### Configure the ROS Connector

Our codebase is scripted towards ROS 2. You will need to change some default settings to ensure your messages are compiled for ROS 2.
The code will work with ROS 1, but you will need to update the code to support ROS 1 messages your self - the messages are somewhat different.

1. Open the ROS settings menu (Robotics | ROS Settings)
2. Change the `Protocol` to "ROS 2"
3. Click on "Apply"
4. Wait for the compilation to finish.


## New to Unity?

------

If you have not used unity before I suggest trying out a few tutorials from https://learn.unity.com/

To get started, I suggest you familiarize yourself with the editor from: https://learn.unity.com/tutorial/explore-the-unity-editor-1#

### I cannot see anything!

If you want to jump in quickly to see the sim, follow the next few steps.

1. In the Project Window (tab at the bottom) open: Scenes/WaterSimulation.Unity
2. In the central panel, click on Scene. This is the "Editor" window. The "Game" window is simply a camera that has been set up into the scene.
3. In the Hierarchy (panel on the left side), double click on SAM to pan to it.
4. Click the Play button

From here you should be able to move SAM around using the arrow keys and WASD. You might want to drag and drop the Game window (you can split the view), so you can see both the Scene and Game windows in parallel.

