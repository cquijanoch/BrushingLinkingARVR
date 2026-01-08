# Situated Brushing and Linking in Virtual and Augmented Reality

This repository contains supplemental material for a research paper **Situated Brushing and Linking in Virtual and Augmented Reality** by [Carlos Quijano Ch.](mailto:cquijanochavez@gmail.com), [Benjamin Lee], [Nina Doerr], [Wolfgang Büschel], [Michael Sedlmair] and [Dieter Schmalstieg]. 

Please, check the presentation [video](TBA)

It contains including source code, the software used in the experiment as well as the experimental data and analysis scripts.

## Source Code

A Unity project application for evaluating Brushing and Linking in Virtual and Augmented Reality in Meta Quest with passthrough devices.  

1. Navigate to *Application* folder.

2. To run in editor mode, ensure that Unity Version: 6000.2.8f1 and Meta Horizon Link are installed.

3. Open the scene *BasicSceneARVR* and update the folder logging directory in *StudyManager*.

4. Run.

## Application

Optionally, an apk file can be found in *Installer* folder, ready to be installed in Meta Quest devices.

1. Once installed, open *BrushingandLinking* application (unknown sources).

2. The application will run in AR mode, where a *admin menu* is displayed on the left controller.

3. Using *Points* button will display the calibration cues. After grabbing the red cue to the center of the floor, *Show Shelves* button will show the outline of all the products.

4. To start the demo, *Demo* button will run the application in AR using visual linking highlighting.

5. *Start Study* will start the study trials considering the handedness and participant ID (Note: the task order is determined by the specified participant ID and is defined in the *Assets/UserStudy/ParticipantInfo.csv* file).

6. Its possible to pause the study to display the *admin menu* pressing the buttons *X, A and B at the same time*. To return to the study trial, just press *Start Study* button.

7. Once the applications finishes a condition, it asks to remove the headsets (to answer questions), to continue press *Y*
 
8. Logging data are being recorded in the same device.

## Analysis Data

This folder contais the statistical scripts used to analyze results.

1. Navigate to Analysis folder.

2. Ensure that [R](https://www.r-project.org/) is installed.

3. In the R terminal, `> source(main.R)`, which will ingest CSVs files (tasks, tracking and forms), compute (bootstrap) CIs, and export plots.

4. All of the .png charts will appear in this directory.
### Acknowledgements
Development of the prototype was led by Benjamin Lee, with assistance from Nina Doerr and Katarina Baricova.
