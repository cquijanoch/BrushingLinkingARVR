using UnityEngine;
using UnityEditor;

namespace BrushingAndLinking
{
    [CustomEditor(typeof(StudyManager))]
    [CanEditMultipleObjects]
    public class StudyManagerEditor : Editor
    {
        private StudyManager studyManagerScript;
        private string productToHighlight;

        private void OnEnable()
        {
            studyManagerScript = (StudyManager)target;
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                if (!studyManagerScript.StudyActive && !studyManagerScript.DemoActive)
                {
                    if (GUILayout.Button("Start Study", GUILayout.Height(40)))
                    {
                        studyManagerScript.gameObject.SetActive(true);
                        studyManagerScript.StartStudy();
                    }

                    if (GUILayout.Button("Start Demo"))
                    {
                        studyManagerScript.gameObject.SetActive(true);
                        studyManagerScript.StartDemo();
                    }
                }
                else
                {
                    if (studyManagerScript.StudyActive)
                    {
                        if (!studyManagerScript.TrialActive && !studyManagerScript.CurrentTrial.IsTrialCompleted)
                        {
                            if (GUILayout.Button("Start Trial", GUILayout.Height(40)))
                            {
                                studyManagerScript.NextStudyStep();
                            }
                        }
                        else if (studyManagerScript.TrialActive)
                        {
                            if (GUILayout.Button("Stop Trial", GUILayout.Height(40)))
                            {
                                studyManagerScript.NextStudyStep();
                            }
                        }
                        else if (!studyManagerScript.TrialActive && studyManagerScript.CurrentTrial.IsTrialCompleted)
                        {
                            if (GUILayout.Button("Next Trial", GUILayout.Height(40)))
                            {
                                studyManagerScript.NextStudyStep();
                            }
                        }
                    }
                    else if (studyManagerScript.DemoActive)
                    {
                        if (GUILayout.Button("Stop Demo"))
                        {
                            studyManagerScript.StopDemo();
                        }

                    }
                }

                if (studyManagerScript.StudyActive)
                {
                    EditorGUILayout.HelpBox(studyManagerScript.CurrentTrial.ToFormattedString(), MessageType.Info);
                }

                if (studyManagerScript.TrialActive)
                {
                    if (studyManagerScript.LoggingEnabled)
                    {
                        EditorGUILayout.HelpBox("Trial is currently in progress and being logged!", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Trial is currently in progress but is NOT being logged. Make sure this is intentional!", MessageType.Error);
                    }
                }

                if (studyManagerScript.DemoActive)
                {
                    EditorGUILayout.HelpBox("Demo is currently active.", MessageType.Info);
                }

                if (studyManagerScript.StudyActive && !studyManagerScript.TrialActive && !studyManagerScript.CurrentTrial.IsTrialCompleted)
                {
                    if (GUILayout.Button("Skip This Trial"))
                    {
                        studyManagerScript.SkipTrial();
                    }
                }

                if (studyManagerScript.StudyActive && studyManagerScript.TrialActive)
                {
                    if (GUILayout.Button("Reset Filtering"))
                    {
                        FilterSliderManager.Instance.ResetFiltering();
                    }
                }
            }

            DrawDefaultInspector();
        }
    }
}