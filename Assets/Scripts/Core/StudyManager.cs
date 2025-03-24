using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DxR;
using System.IO;
using Random = System.Random;
using Oculus.Interaction.Input;
using System.Collections;

namespace BrushingAndLinking
{
    /// <summary>
    /// The StudyManager class is the main point of entry for everything associated with running the user study.
    ///
    /// All variables are exposed via the Inspector.
    /// </summary>
    public class StudyManager : MonoBehaviour
    {
        public static StudyManager Instance { get; private set; }

        [Header("User Study Parameters")]
        [Min(1)] public int CurrentParticipantID = 1;
        private Handedness CurrentParticipantHandedness = Handedness.Right;

        [Header("Data Logging")]
        public string FolderPath = "C:/Users/quijancr/Desktop";

        public bool LoggingEnabled = false;
        public bool LogInteractions = false;
        public bool NewFilePerParticipant = true;

        [Header("User Study Input Files")]
        public TextAsset ParticipantInfo;
        public TextAsset TaskInfo;

        [Header("Supermarket Variables")]
        //public Transform ShelvesInfraestructure;
        //public GameObject InLayoutProducts;
        //public GameObject OutLayoutProducts;
        //public GameObject EnvironmentForVR;

        //[Tooltip("Tutorials are the brief period of time where the participant can see the highlighting technique before the trial begins.")] 
        //public GameObject TutorialLayoutProducts;

        //[Tooltip("Training is the stage before the user study begins, where the participant can try and practice with the interactions.")] 
        //public GameObject TrainingLayoutProducts;
        public Vis MainVis;
        public Tablet Tablet;
        public ButtonGroup XButtonGroup;
        public ButtonGroup YButtonGroup;

        [Header("Debug Parameters")]
        public bool AutoStartDemo = false;
        public bool AutoStartUserStudy = false;
        //public bool AutoStartTraining = false;

        public List<StudyTrial> StudyTrials { get; private set; }
        public StudyTrial CurrentTrial { get { return StudyTrials[CurrentTrialIdx]; } }
        public int CurrentTrialIdx { get; private set; }
        public bool StudyActive { get; private set; }
        public bool TrialActive { get; private set; }
        public bool TutorialActive { get; private set; }
        public bool DemoActive { get; private set; }

        public StudyMode Status = StudyMode.Pause;

        // Data variables are using rows x columns (i.e. each list element is a row, each array element is a column within that row)
        private List<string[]> participantInfoData;
        private List<string[]> taskInfoData;
        private string[] participantInfoDataHeaders;
        private string[] taskInfoDataHeaders;
        public List<Product> studyProducts;
        private Random randGen;

        // Key is the configuration of independent variables, value is a list of questions associated with it
        private Dictionary<TaskType, List<StudyTask>> studyTasksDictionary;
        private EnvironmentMode currentEnvironment;

        private StreamWriter mainDataStreamWriter;
        private StreamWriter interactionsDataStreamWriter;
        private TrialDataLog currentTrialDataLog;
        private float _trialStartTime;
        private float _trialFirstObjectSelectionTime;
        private float _trialLastObjectSelectionTime;
        private float _trialLastTabletInteractionTime;
        public  ViewTimeController _viewTimeController;

        private DateTime _startTime;
        private DateTime _endTime;

        private void Awake()
        {
            // Assign this object to the Instance property if it isn't already assigned, otherwise delete this object
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;

            Logger.ReadCsvFromString(ParticipantInfo.text, ref participantInfoData, ref participantInfoDataHeaders);
            Logger.ReadCsvFromString(TaskInfo.text, ref taskInfoData, ref taskInfoDataHeaders);
        }

        private void Start()
        {
            currentEnvironment = MainManager.Instance.EnvironmentMode;
            Status = StudyMode.Pause;

            studyProducts = new List<Product>();
            studyProducts = MainManager.Instance.GetProductsByMode(ApplicationMode.Demo);

            // Set handedness of all interactions
            SetHandedness(CurrentParticipantHandedness);

            SetProductsHidden();
            Tablet.SetOverallVisibility(false);

            if (AutoStartDemo)
                StartDemo();
            else if (AutoStartUserStudy)
                StartStudy();
        }

        public void StartDemo()
        {
            if (StudyActive || TutorialActive || DemoActive)
                return;

            DemoActive = true;

            // Change the vis data set to a training one
            var json = MainVis.GetVisSpecs();
            json["data"]["url"] = "data_source_demo.csv";
            MainVis.UpdateVisSpecsFromJSONNode(json);

            Tablet.SetOverallVisibility(true);

            SetProductsVisibility(ApplicationMode.Demo);
            StartCoroutine(SetHighlightTechnique(HighlightTechnique.AnimatedOutlineLink));
            
            Tablet.SetTaskText("<b>Demo Phase</b>\nPlease practice using the brushing and linking interactions.");

            SetTabletAllVisibility(true, false, false, false, false, false, true);
        }

        public void StopDemo()
        {
            if (!DemoActive)
                return;

            ResetAllBrushingAndLinking();
            SetProductsHidden();
            Tablet.SetOverallVisibility(false);
            DemoActive = false;
            Status = StudyMode.Pause;
            MainManager.Instance.AppMode = ApplicationMode.None;
            MainManager.Instance.StartCalibration();
        }

        public void StartStudy()
        {
            if (StudyActive || DemoActive || Status == StudyMode.Play)
                return;

            StudyActive = true;
            LoadTasks(ref taskInfoData);

            if (CurrentParticipantID < 1)
                CurrentParticipantID = 1;
            CalculateTrials(participantInfoData[CurrentParticipantID - 1]);

            CurrentTrialIdx = 0;
            currentEnvironment = StudyTrials[CurrentTrialIdx].Environment;
            StartCoroutine(MainManager.Instance.ChangeEnvironment(currentEnvironment));


            studyProducts = MainManager.Instance.GetProductsByMode(ApplicationMode.Study);
            Tablet.SetOverallVisibility(true);
            
            // Set initial visibility rules
            SetProductsHidden();
            // Only show the pre-tutorial stuff
            SetTabletAllVisibility(false, true, false, false, false, true, false);

            // Load the first trial in the study
            LoadTrial(StudyTrials[CurrentTrialIdx]);

            // Initialise stream writer for data logging
            InitialiseDataLogging();
        }

        public void NextStudyStep()
        {
            if (DemoActive)
            {
                StopDemo();
                return;
            }

            if (!StudyActive)
                return;

            /// There are three possible states that the study can be in when moving to a next step
            /// 1. The loaded trial is not active and has not been completed -> Start the trial (i.e. pre-trial)
            /// 2. The loaded trial is currently active -> Stop the trial (i.e. mid-trial)
            /// 3. The loaded trial has already completed -> Load the next trial (i.e. post trial)

            // State 1: Start the trial
            if (!TrialActive && !CurrentTrial.IsTrialCompleted)
                StartTrial();
            else if (TrialActive)
                StopTrial();
            else if (!TrialActive && CurrentTrial.IsTrialCompleted)
            {
                CurrentTrialIdx++;

                if (CurrentTrialIdx >= StudyTrials.Count)
                {
                    StopStudy();
                    return;
                }
                
                if (currentEnvironment != StudyTrials[CurrentTrialIdx].Environment)
                {
                    currentEnvironment = StudyTrials[CurrentTrialIdx].Environment;
                    StartCoroutine(MainManager.Instance.ChangeEnvironment(currentEnvironment));
                    studyProducts = MainManager.Instance.GetProductsByMode(ApplicationMode.Study);
                }

                LoadTrial(StudyTrials[CurrentTrialIdx]);
            }
            else
                throw new Exception("[DebugUnity] Error in NextStudyStep. This should not happen.");
        }

        private void LoadTrial(StudyTrial trialToLoad)
        {
            // Change the vis data set
            var json = MainVis.GetVisSpecs();

            if (TaskType.Tutorial == trialToLoad.Task)
                json["data"]["url"] = "data_source_tutorial.csv";
            else
                json["data"]["url"] = "data_source_study.csv";//"ProductData_Tutorial.csv";

            MainVis.UpdateVisSpecsFromJSONNode(json);
            ResetAllBrushingAndLinking();
            SetProductsHidden();
            SetTabletAllVisibility(
                false,      // Always hide the vis, dimension change buttons, etc. when the trial is loaded
                trialToLoad.Task == TaskType.Tutorial,   // Load the pre-tutorial controls if the trial is a tutorial
                false,      // Don't show mid-tutorial controls
                trialToLoad.Task != TaskType.Tutorial,   // Load the pre-trial controls if the trial is a regular trial
                false,      // Don't load hypothesis response controls
                false,      // Don't load post-trial controls
                false        // Don't load exit demo button
            );

            switch (trialToLoad.Task)
            {
                case TaskType.Tutorial:
                    Tablet.SetTaskText(string.Format("<b>Tutorial for {0} technique</b>\nPlease spend a few minutes to practice and get used to the new highlighting technique.", CurrentTrial.Technique));
                    break;
                case TaskType.Single:
                    Tablet.SetTaskText(string.Format("<b>Single selection + {0} technique</b>\n{1}", trialToLoad.Technique, trialToLoad.QuestionText));
                    break;
                case TaskType.Multiple:
                    Tablet.SetTaskText(string.Format("<b>Multiple selection + {0} technique</b>\n{1}", trialToLoad.Technique, trialToLoad.QuestionText));
                    break;
                case TaskType.Hypothesis:
                    Tablet.SetTaskText(string.Format("<b>Statement question + {0} technique</b>\n{1}", trialToLoad.Technique, trialToLoad.QuestionText));
                    break;
            }

            StartCoroutine(SetHighlightTechnique(trialToLoad.Technique));
        }

        private void StartTrial()
        {
            if (CurrentTrial.IsTrialCompleted || TrialActive || Status != StudyMode.Pause)
                return;

            TrialActive = true;
            Status = StudyMode.Play;

            SetProductsVisibility(ApplicationMode.Study);
            SetTabletAllVisibility(
                true,       // Show vis, etc.
                false,      // Don't show pre-tutorial controls
                CurrentTrial.Task == TaskType.Tutorial,   // Load mid-tutorial controls if the trial is a tutorial
                false,      // Don't show pre-trial controls
                CurrentTrial.Task == TaskType.Hypothesis, // Load the response buttons if it is a hypothesis task
                false,      // Don't show post-trial controls
                false       // Don't show demo controls
            );

            // Provide visual cues for the appropriate dimensions to use depending on the given task
            if (CurrentTrial.Task != TaskType.Tutorial)
            {
                XButtonGroup.HighlightButtonByDimensionName(CurrentTrial.DimensionName1);
                XButtonGroup.HighlightButtonByDimensionName(CurrentTrial.DimensionName2);
                YButtonGroup.HighlightButtonByDimensionName(CurrentTrial.DimensionName1);
                YButtonGroup.HighlightButtonByDimensionName(CurrentTrial.DimensionName2);
                FilterSliderManager.Instance.TaskStarted();
            }

            // Start logging variables
            if (CurrentTrial.Task != TaskType.Tutorial)
            {
                currentTrialDataLog = new TrialDataLog();
                _trialStartTime = Time.time;
                _startTime = DateTime.Now;
                _trialFirstObjectSelectionTime = -1;
                _trialLastTabletInteractionTime = -1;
                _trialLastObjectSelectionTime = -1;
                _viewTimeController.Restart();
            }
        }

        public void StopTrial()
        {
            if (!TrialActive && Status != StudyMode.Pause)
                return;

            if (Status == StudyMode.Questionnaire)
                return;

            ResetAllBrushingAndLinking();
            SetProductsHidden();
            ResetHiddenProducts();
            
            // Hide the visual cues for the dimension buttons
            if (CurrentTrial.Task != TaskType.Tutorial)
            {
                XButtonGroup.UnhighlightButtons();
                YButtonGroup.UnhighlightButtons();
            }

            CurrentTrial.TrialCompleted();

            if (CurrentTrial.Task != TaskType.Tutorial)
            {
                currentTrialDataLog.CompletionTime = Time.time - _trialStartTime;
                currentTrialDataLog.TimeUntilLastTabletInteraction = (_trialLastTabletInteractionTime != -1) ? _trialLastTabletInteractionTime - _trialStartTime : -1;
                currentTrialDataLog.TimeUntilFirstObjectSelected = (_trialFirstObjectSelectionTime != -1) ? _trialFirstObjectSelectionTime - _trialStartTime : -1;
                currentTrialDataLog.TimeUntilLastObjectSelected = (_trialLastObjectSelectionTime != -1) ? _trialLastObjectSelectionTime - _trialStartTime : -1;
                currentTrialDataLog.CountOfSelectedObjects = currentTrialDataLog.SelectedObjectNames.Count;
                currentTrialDataLog.TimeOutTabletViewing = currentTrialDataLog.CompletionTime - _viewTimeController.timeViewing;

                if (CurrentTrial.Task != TaskType.Hypothesis)
                    currentTrialDataLog.CountOfWrongSelectedObjects = currentTrialDataLog.SelectedObjectNames.Count - CurrentTrial.QuestionAnswers.Length;
                else
                {
                    if (currentTrialDataLog.SelectedObjectNames.Count > 0)
                        currentTrialDataLog.CountOfWrongSelectedObjects = currentTrialDataLog.SelectedObjectNames[0].ToLower() == CurrentTrial.QuestionAnswers[0].ToLower() ? 0 : 1;
                }

                _viewTimeController.Stop();

                if (Status == StudyMode.Play)
                    WriteDataLogging();
            }

            if (Status == StudyMode.Play && TaskType.Hypothesis == CurrentTrial.Task)
            {
                Status = StudyMode.Questionnaire;
                Tablet.SetTaskText("Please remove the device and wait for instructions.");
            }
            else
            {
                Tablet.SetTaskText("Please return to the middle of the room as indicated by the feet image. Press the button to the right when you have done so.");
                TrialActive = false;
                Status = StudyMode.Pause;
            }

            SetTabletAllVisibility(
                false,   // Don't show vis, etc.
                false,   // Don't show pre-tutorial controls
                false,   // Don't show mid-tutorial controls
                false,   // Don't show pre-trial controls
                false,   // Don't show hypothesis response controls
                Status != StudyMode.Questionnaire,     // Show post-trial controls
                false
            );
        }

        /// <summary>
        /// Once the administrator admits to continue
        /// </summary>
        public void ContinueQuestionnaire()
        {
            Status = StudyMode.Pause;

            SetTabletAllVisibility(
                false,   // Don't show vis, etc.
                false,   // Don't show pre-tutorial controls
                false,   // Don't show mid-tutorial controls
                false,   // Don't show pre-trial controls
                false,   // Don't show hypothesis response controls
                true,     // Show post-trial controls
                false
            );
        }

        /// <summary>
        /// Skips a trial entirely. Used mainly for when something goes wrong and a later part of the study needs to be skipped to
        /// </summary>
        public void SkipTrial()
        {
            if (!TrialActive && !CurrentTrial.IsTrialCompleted)
            {
                CurrentTrialIdx++;
                LoadTrial(StudyTrials[CurrentTrialIdx]);
            }
        }

        public void StopStudy()
        {
            if (!StudyActive)
                return;

            StopDataLogging();
            ResetAllBrushingAndLinking();
            SetProductsHidden();
            _viewTimeController.Stop();


            Tablet.SetOverallVisibility(false);
            StudyActive = false;
            Status = StudyMode.Pause;
        }

        #region Study configuration and loading methods

        private void LoadTasks(ref List<string[]> taskInfoData)
        {
            // Create data structure
            studyTasksDictionary = new Dictionary<TaskType, List<StudyTask>>();

            foreach (string[] row in taskInfoData)
            {
                Enum.TryParse(row[0], out TaskType taskType);
                if (!studyTasksDictionary.TryGetValue(taskType, out List<StudyTask> taskList))
                {
                    taskList = new List<StudyTask>();
                    studyTasksDictionary.Add(taskType, taskList);
                }

                taskList.Add(new StudyTask(row[2], row[3], row[4].Split(';'), row[5], row[6], row[7], row[8], row[9], row[10]));
            }
        }

        private void CalculateTrials(string[] participantInfo)
        {
            StudyTrials = new List<StudyTrial>();

            randGen = new Random(CurrentParticipantID);

            Enum.TryParse(participantInfo[1], out EnvironmentMode environment1);
            Enum.TryParse(participantInfo[2], out EnvironmentMode environment2);
            Enum.TryParse(participantInfo[3], out HighlightTechnique technique1);
            Enum.TryParse(participantInfo[4], out HighlightTechnique technique2);
            Enum.TryParse(participantInfo[5], out HighlightTechnique technique3);

            StudyTrials.Add(new StudyTrial(environment1, technique1, TaskType.Tutorial));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique1, TaskType.Single));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique1, TaskType.Multiple));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique1, TaskType.Hypothesis));

            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique2, TaskType.Single));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique2, TaskType.Multiple));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique2, TaskType.Hypothesis));

            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique3, TaskType.Single));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique3, TaskType.Multiple));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment1, technique3, TaskType.Hypothesis));

            StudyTrials.Add(new StudyTrial(environment2, technique1, TaskType.Tutorial));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique1, TaskType.Single));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique1, TaskType.Multiple));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique1, TaskType.Hypothesis));

            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique2, TaskType.Single));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique2, TaskType.Multiple));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique2, TaskType.Hypothesis));

            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique3, TaskType.Single));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique3, TaskType.Multiple));
            StudyTrials.Add(CreateStudyTrialFromIndependentVariables(environment2, technique3, TaskType.Hypothesis));

            for (int i = 0; i < StudyTrials.Count; i++)
                StudyTrials[i].TrialNumber = i;
        }

        private StudyTrial CreateStudyTrialFromIndependentVariables(EnvironmentMode environment, HighlightTechnique technique, TaskType taskType)
        {
            List<StudyTask> taskList = studyTasksDictionary[taskType];

            int rand = randGen.Next(0, taskList.Count);
            StudyTask studyTask = taskList[rand];
            StudyTrial retVal = new (environment, technique, taskType, studyTask);

            taskList.RemoveAt(rand);

            return retVal;
        }
        private void SetProductsVisibility(ApplicationMode visibilityMode)
        {
            MainManager.Instance.GetVisibility(visibilityMode);
            HighlightManager.Instance.ResetProductReferences();
        }

        private void SetProductsHidden()
        {
            SetProductsVisibility(ApplicationMode.None);
        }
        private void SetTabletContentVisibility(bool visible)
        {
            Tablet.SetContentVisibility(visible);
        }

        private void SetTabletControlsVisibility(TabletControls tabletControls, bool visible)
        {
            Tablet.SetControlsVisibility(tabletControls, visible);
        }

        private void SetTabletAllVisibility(bool content, bool preTutorial, bool midTutorial, bool preTrial, bool hypothesisResponse, bool postTrial, bool demo)
        {
            SetTabletContentVisibility(content);
            SetTabletControlsVisibility(TabletControls.PreTutorial, preTutorial);
            SetTabletControlsVisibility(TabletControls.MidTutorial, midTutorial);
            SetTabletControlsVisibility(TabletControls.PreTrial, preTrial);
            SetTabletControlsVisibility(TabletControls.HypothesisResponse, hypothesisResponse);
            SetTabletControlsVisibility(TabletControls.PostTrial, postTrial);
            SetTabletControlsVisibility(TabletControls.Demo, demo);
        }

        private IEnumerator SetHighlightTechnique(HighlightTechnique technique)
        {
            foreach (Product product in studyProducts)
                product.SetHighlightTechnique(technique);

            yield return new WaitForEndOfFrame();
        }

        private void ResetAllBrushingAndLinking()
        {
            BrushingManager.Instance.RemoveAllBrushing();
            HighlightManager.Instance.UnhighlightAllProducts();
            FilterSliderManager.Instance.ResetFiltering();
            LinkHighlighter.VisMarksChanged();
            //ArrowHighlighter.VisMarksChanged();
        }

        public void PauseStudy()
        {
            TrialActive = false;
            Status = StudyMode.Pause;
            SaveStudyParams();
            _viewTimeController.Stop();

            XButtonGroup.UnhighlightButtons();
            YButtonGroup.UnhighlightButtons();

            ResetAllBrushingAndLinking();
            SetProductsHidden();

            Tablet.SetOverallVisibility(false);
        }

        public void RestoreStudy()
        {
            if (Status == StudyMode.Play)
                return;

            SetHandedness(CurrentParticipantHandedness);

            currentEnvironment = StudyTrials[CurrentTrialIdx].Environment;
            StartCoroutine(MainManager.Instance.ChangeEnvironment(currentEnvironment));
            studyProducts = MainManager.Instance.GetProductsByMode(ApplicationMode.Study);
            Tablet.SetOverallVisibility(true);

            SetProductsHidden();
            LoadTrial(StudyTrials[CurrentTrialIdx]);

            //InitialiseDataLogging();

        }

        //Save StudyParams
        private void SaveStudyParams()
        {

        }

        #endregion Study configuration and loading methods

        #region Participant interaction methods

        public void InteractionOccurred(InteractionType interactionType, string comment = "")
        {
            _trialLastTabletInteractionTime = Time.time;

            // Log to more detailed dataset
            LogInteractionData(Time.time - _trialStartTime, interactionType, comment);
        }

        public void ProductSelected(Product product)
        {
            // For training, we still want to play a sound effect
            //if (TrainingActive)
            //{
            //    SoundEffectPlayer.Instance.PlayCorrectProductSelected();
            //    return;
            //}

            if (!TrialActive)
                return;

            if (CurrentTrial.Task == TaskType.Single || CurrentTrial.Task == TaskType.Multiple)
            {
                currentTrialDataLog.SelectedObjectNames.Add(product.gameObject.name);

                // Log the interaction to more detailed dataset
                // Note that this product selection doesn't count as a tablet interaction, but is still an interaction nonetheless
                LogInteractionData(Time.time - _trialStartTime, InteractionType.ProductSelect, product.gameObject.name);

                // If no value has been logged for the first selection yet, mark the time
                if (_trialFirstObjectSelectionTime == -1)
                {
                    _trialFirstObjectSelectionTime = Time.time;
                }

                if (CurrentTrial.QuestionAnswers.Contains(product.gameObject.name))
                {
                    CurrentTrial.QuestionResponses[Array.IndexOf(CurrentTrial.QuestionAnswers, product.name)] = true;

                    if (CurrentTrial.QuestionResponses.All(b => b))
                    {
                        _endTime = DateTime.Now;
                        _trialLastObjectSelectionTime = Time.time;

                        StopTrial();
                    }
                        
                    else
                        product.SetProductVisibility(false);
                }
            }
        }

        public void ResetHiddenProducts()
        {
            foreach (Product prod in studyProducts)
                if (!prod.gameObject.activeSelf)
                    prod.gameObject.SetActive(true); 
        }

        public void ResponseGiven(string response)
        {
            if (!TrialActive)
                return;

            currentTrialDataLog.SelectedObjectNames.Add(response);

            if (_trialFirstObjectSelectionTime == -1)
            {
                _trialFirstObjectSelectionTime = Time.time;
                _endTime = DateTime.Now;
            }

            // Play a sound effect
            //SoundEffectPlayer.Instance.PlayHypothesisResponseGiven();

            // The trial finishes immediately when a response is given
            StopTrial();
        }

        public void SetHandedness(Handedness ParticipantHandedness)
        {
            CurrentParticipantHandedness = ParticipantHandedness;
            Tablet.SetHandedness(CurrentParticipantHandedness);
            FilterSliderManager.Instance.SetHandedness(CurrentParticipantHandedness);
            BrushingOculusHandler.Instance.SetHandedness(CurrentParticipantHandedness);
        }

        #endregion Participant interaction methods

        #region Data logging functions

        private void InitialiseDataLogging()
        {
            if (!LoggingEnabled)
                return;

#if PLATFORM_ANDROID
            FolderPath = Application.persistentDataPath;
#endif
            if (!FolderPath.EndsWith('/'))
                FolderPath += '/';

            // Get the path, which differs if we use a new file per each participant
            string path = NewFilePerParticipant ? string.Format("{0}MainData_Participant{1}.csv", FolderPath, CurrentParticipantID) : string.Format("{0}MainData.csv", FolderPath);

            bool writeHeaders = !File.Exists(path);
            mainDataStreamWriter = new StreamWriter(path, true);
            if (writeHeaders)
                mainDataStreamWriter.WriteLine("Participant_ID,Technique,Environment,Task_Type,Question_ID,Start_Time,EndTime,Completion_Time,Time_until_last_selection_on_tablet,Time_until_selected_first_object,Time_until_selected_last_object,Time_Out_Tablet_Viewing,Count_selected_Objects,Count_Wrong_Selected_Objects,Name_selected_Objects");
            
            mainDataStreamWriter.AutoFlush = true;

            // Create stream writer for the interactions
            if (LogInteractions)
            {
                path = string.Format("{0}InteractionsData_Participant{1}.csv", FolderPath, CurrentParticipantID);
                writeHeaders = !File.Exists(path);
                interactionsDataStreamWriter = new StreamWriter(path, true);
                if (writeHeaders)
                    interactionsDataStreamWriter.WriteLine("Technique,Environment,Task_Type,Question_ID,Time,Interaction_Type,Comment");
                interactionsDataStreamWriter.AutoFlush = true;
            }
        }

        private void WriteDataLogging()
        {
            if (MainManager.Instance.AppMode != ApplicationMode.Study || Status == StudyMode.Pause)
                return;

            if (!LoggingEnabled || mainDataStreamWriter == null || CurrentTrial.Task == TaskType.Tutorial || CurrentTrial.Task == TaskType.Demo)
                return;

            mainDataStreamWriter.WriteLine(
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                CurrentParticipantID,
                CurrentTrial.Technique.ToString(),
                CurrentTrial.Environment.ToString(),
                CurrentTrial.Task.ToString(),
                CurrentTrial.QuestionID,
                _startTime.ToString("dd/MM/yyyy H:mm:ss"),
                _endTime.ToString("dd/MM/yyyy H:mm:ss"),
                currentTrialDataLog.CompletionTime,
                currentTrialDataLog.TimeUntilLastTabletInteraction,
                currentTrialDataLog.TimeUntilFirstObjectSelected,
                currentTrialDataLog.TimeUntilLastObjectSelected,
                currentTrialDataLog.TimeOutTabletViewing,
                currentTrialDataLog.CountOfSelectedObjects,
                currentTrialDataLog.CountOfWrongSelectedObjects,
                string.Join(';', currentTrialDataLog.SelectedObjectNames)
            ));
        }

        private void LogInteractionData(float time, InteractionType interactionType, string comment = "")
        {
            if (MainManager.Instance.AppMode != ApplicationMode.Study || Status == StudyMode.Pause)
                return;

            if (!LoggingEnabled || interactionsDataStreamWriter == null || CurrentTrial.Task == TaskType.Tutorial || CurrentTrial.Task == TaskType.Demo)
                return;

            interactionsDataStreamWriter.WriteLine(
                string.Format("{0},{1},{2},{3},{4},{5},{6}",
                CurrentTrial.Technique.ToString(),
                CurrentTrial.Environment.ToString(),
                CurrentTrial.Task.ToString(),
                CurrentTrial.QuestionID,
                time,
                interactionType.ToString(),
                comment
            ));
        }

        private void StopDataLogging()
        {
            if (LoggingEnabled && mainDataStreamWriter != null)
            {
                mainDataStreamWriter.Close();
                interactionsDataStreamWriter.Close();
            }
        }

        private void OnApplicationQuit()
        {
            StopDataLogging();
        }

        #endregion Data logging functions
    }
}
