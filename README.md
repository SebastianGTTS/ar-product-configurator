# AR Product Configurator
This Unity-based Android app allows you to configure a product in Augmented Reality.

## Introduction
This Android app was developed as a part of a Master thesis exploring product configuration in Augmented Reality using kitchen furniture as a case study.
It is built on top of Unity engine and Google's ARCore SDK. It can be deployed to [any Android device that supports ARCore][ARCore Supported Devices].
The app requires two types of input files to function:
1. The feature model of the product augmented with special information on how the product parts combine into a final configuration. The model has to be exported in the JSON format.
Only one model/product is allowed at a single time, but you can use your own.
2. 3D models of every part of the overall product. Each 3D model has to be referenced in the feature model by file name.

The app also supports overlaying textures over the 3D models. If you use this option, the textures will need to be saved as image files and referenced in the feature model.

For the sake of convenience, the feature model, 3D models and textures used in the original case study have been uploaded to this repository and can be used out-of-the-box.
They are located in the `Assets/Input/CaseStudyModels` folder.

## Installation
To use this app, you will need to install Unity. This will require registering for a Unity Developer Network (UDN) account. Registration is free for personal use.
Follow the [official Unity manual][Unity Install] to perform the following steps:

1. Install Unity Hub.
2. From the Hub, install Unity Editor version **2019.3.0f6**.  
   _The project will not open in any other editor version. You will need to add the **Android Build Support** module, which can be done both during and after installation.
   The project does not require any other modules, so feel free to choose any additional modules you might need._
3. Clone or download/unzip this project to a folder on your hard drive.
4. Open Unity Hub, navigate to the Projects tab and click on the Add button. In the following folder selection window, choose the folder in which you have saved the project
to add it to Unity Hub.
5. Once the project has been added to the list, click on it to open it in the editor. If you did not install the correct editor version, you will be prompted to do so.
6. Inside the Unity Editor, navigate to `Assets/Scenes` folder and open the `AR Configurator` scene.

## Version Control
This project uses [GitHub for Unity][GitHub Unity] plugin to facilitate version control. After downloading the project, you will have version control tools out-of-the-box working
directly from inside the Unity Editor. Please read the official GitHub for Unity documentation in case you need to change any settings. Since this plugin is still under
development, it is especially advisable to be informed of the [known issues][Issues] and perform backups to prevent files from being corrupted.
If it causes any problems, simply remove the plugin from the project.

## Deploying the Project
In order to use this app, you will need to set it up to download the input over an internet connection. The app relies on Unity's [Asset Bundle][Asset Bundles] mechanism
to download its contents during runtime. This means you will have to compile your models into an asset bundle and host it on a remote server.
Follow these steps to deploy the app:

1. Open the project in the Unity Editor.
2. Set up the app's input by performing *one* of the following.
   1. If you do not have your own models yet, the project already contains the requisite models from the case study. In this case, proceed to step 3.
   2. If you have your own models, empty the `Assets/Input` folder and place your models/JSON/textures inside it. The folder structure does not matter. Note that
   only one JSON file is permitted.
3. In the Unity Editor's main toolbar, select Product Configurator &rarr; Create Database Bundle. Wait for a popup dialog saying that the process is finished.
4. The previous step will create a new folder named `Bundles` inside the project's `Assets` folder. Inside that folder you will find a file named `elements.db`.
Upload this file to a remote server that supports hotlinking/direct download over HTTP.
5. Inside the Hierarchy view, select the Asset Database game object. In the Inspector view you will see a field named Database Location. Paste the remote URL of the
`elements.db` file you have uploaded in step 4 into the field and save the project.
6. Connect your AR-capable Android device. Make sure to enable developer options and USB debugging first.
7. Select Build and Run menu option of the editor's File menu and follow the instructions if prompted.

Once you have deployed the app, you can disconnect your device and use the app independently. If you wish to make any changes to the models or use your own,
open the project, delete the `Bundles` folder and repeat steps 2.ii&ndash;4 with your new set of models.
You do *not* have to re-deploy the app, unless the URL of the asset bundle changes, in which case follow steps 5&ndash;7 as well.
The app will load the new bundle upon next start.

[ARCore Supported Devices]: https://developers.google.com/ar/discover/supported-devices
[Unity Install]: https://docs.unity3d.com/Manual/GettingStartedInstallingUnity.html
[GitHub Unity]: https://unity.github.com/
[Issues]: https://github.com/github-for-unity/Unity/issues?q=is%3Aissue+is%3Aopen+label%3Abug
[Asset Bundles]: https://docs.unity3d.com/Manual/AssetBundlesIntro.html
