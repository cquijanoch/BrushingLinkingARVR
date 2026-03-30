# Situated Brushing and Linking in Virtual and Augmented Reality

This repository contains supplemental material for a research paper **Situated Brushing and Linking in Virtual and Augmented Reality** by [Carlos Quijano Ch.](mailto:cquijanochavez@gmail.com), [Benjamin Lee](mailto:benjaminchlee@gmail.com), [Nina Doerr](mailto:nina.doerr@visus.uni-stuttgart.de), [Wolfgang Büschel](mailto:wolfgang.bueschel@visus.uni-stuttgart.de), [Michael Sedlmair](mailto:michael.sedlmair@visus.uni-stuttgart.de) and [Dieter Schmalstieg](mailto:schmaldr@visus.uni-stuttgart.de). 

Please, check the presentation [video](https://youtu.be/jv8a4tl3gng?si=AcgVtOACAZX0fpPH)

It contains including source code, the software used in the experiment as well as the logging data and analysis scripts.

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

3. The calibration of the virtual and physical shelves is supported by visual cues. *Points* button displays the calibration cues. After grabbing the red cue to the center of the floor, *Show Shelves* button will show the outline of all the products.

4. To start the demo, *Demo* button will run the application in AR using visual linking highlighting.

5. *Start Study* will run the study trials considering the handedness and participant ID (Note: the task order is determined by the specified participant ID and is defined in the *Assets/UserStudy/ParticipantInfo.csv* file).

6. Its possible to pause the study to display the *admin menu* pressing the buttons *X, A and B at the same time*. To return to the study trial, just press *Start Study* button again.

7. Once the applications finishes a condition, it asks to remove the headsets (to answer questions), to continue press *Y*
 
8. Logging data are being recorded in the same device.

## Analysis Data

This folder contais the statistical scripts used to analyze results.

1. Navigate to Analysis folder.

2. Ensure that [R](https://www.r-project.org/) is installed.

3. In the R terminal, `> source(main.R)`, which will ingest CSVs files (tasks, tracking and forms), compute (bootstrap) CIs, and export plots.

4. All of the .pdf charts will appear in this directory.

## Acknowledgements:

This software is based on *VisHigh* by [Doerr et al.](https://github.com/doerrna/Visual-Highlighting-for-Situated-Brushing-and-Linking) 