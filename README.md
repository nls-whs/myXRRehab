# myXRRehab

_Unity Version_: 2020.3.4f1 <br/>
_Body sensors_: Orbbec Astra Pro / Orbbec Astra S <br/>
_Astra SDK_: v2.1.2 <br/>
_Vuforia version_: 9.7.4 <br/>
_Platform_: Server (Desktop App for Windows), Client (UWP for HoloLens 2) <br/>

# Introduction
This prototype has been developed as part of [Next level Sports](https://hci.w-hs.de/research/projects/nextlevelsports/) project. The main goal of this app is to support patient rehabilitation by allowing them to perform the exercise correctly. The application is designed for only one exercise i.e. Leg press, for prototype. In future, it can be extended to more exercises. 

It is based on server-client architecture. The server application uses Orbbec Astra Pro/S sensor and tracks body joints which are then transformed mathematically into HoloLens coordinate system. After that, the client app shows certain instructions or visualization to perform the exercise correctly. This prototype is still in progress and can accommodate many contributions with respect to visualizations for client app. 

## Server

The open source Astra Unity sample for Windows application has been modified according to the requirements. Since the knee angle is really important to know in the leg press exercise, therefore, it has been added in the view. All other mathematical calculations and transformations also take place on server side. The scene in Unity project is found in `Assets/Frameworks/Orbbec/Samples/MainScene.unity`

![image](https://user-images.githubusercontent.com/104509917/176886603-be7b6819-f27a-4505-8754-318cdace8ddb.png)

## Client

The client app starts with calibration to receive the data from Orbbec sensor and then shows spherical visualization on knee, hip and ankle joints. The start and end position angle is proved further and marks the sphere with a certain color accordingly. 

To position the foot placement in the middle of the Leg press exercise machine, four AR markers have been used to determine the board position which can help to position the feet right in the middle. For this purpose, Vuforia API has been used.

The scene in Unity project is found in `Assets/Scenes/SampleScene.unity`

![image](https://user-images.githubusercontent.com/104509917/176899054-59f0690f-0ab5-4225-8580-4f82bf8f7b4b.png)

## Calibration

The calibration is performed by capturing three head positions. The user captures the first head position of HoloLens P(H) by standing in the middle where the sensor could detect the head properly and in return receives the head position from Orbbec as P(O). The user can then move a bit left or right to capture second and third point respectively. All three points should have a minimum 0.5m distance to perform the calibration correctly. If incorrectly performed, the resulting spherical visualization would not overlay onto the body. 

# Interaction

The common interaction for selecting 2D based UI is using hand rays to target the button and then use air-tap gesture to select it. Voice commands can also be used interchangeably and are discussed below. More details about what voice commands to use are shown when the user hovers over the button. <br/>

<details><summary>Voice commands</summary> <br/>

Voice commands are used globally in the app. There is no need to focus the gaze cursor on the button to initiate the voice command. <br/>
1. Calibrate: To start the app and be ready for calibration <br/>		
2. Capture: Capture the head position from HoloLens and get the Orbbec head position in return <br/>
3. Finish: Finish calibration <br/>
4. Reset: Reset calibration  <br/>
5. Close: Close the calibration menu <br/>
6. Menu: Opens the calibration menu <br/>
7. Follow Me: The menu follows the user's head movement <br/>

</details>

# How to use the app?
 
1. Clone the git repo. 
2. Open Client app in Unity and select  `Connection Manager` in Hierarchy window.
3. Change the IP Address field with the server machine IP address in the Inspector Window of `Broadcast Receiver` component.
4. Now save the scene and build for UWP app.
5. Open Visual Studio`.sln` file in the folder you build the app and add the IP address of HoloLens (remote machine) in  `Project Properties`.
6. First run the server app and then deploy the client app to HoloLens. Once the connection is successful, the menu will show successful connection message and can proceed with the calibration. 

# Documentation

Astra Wiki: https://readthedocs.org/projects/astra-wiki/downloads/pdf/latest/ <br/>
AR Marker generator: https://shawnlehner.github.io/ARMaker/ <br/>
Vuforia Marker tracking: https://arvrjourney.com/hololens-2-marker-tracking-with-vuforia-engine-and-mrtk-fb582c8f8ac0 <br/>
Orbbec Astra SDK: https://orbbec3d.com/index/download.html <br/>

# Credits

The application uses all open source contents and is licensed under [MIT License](https://opensource.org/licenses/MIT).

Mathematical calculation for coordinate transformation matrix between client (HoloLens) and server (Windows app) is done by [Prof. Dr. Gregor Lux](https://www.w-hs.de/service/informationen-zur-person/person/lux/).


## Developer
[Asma Rafi](https://github.com/asmarf6)


