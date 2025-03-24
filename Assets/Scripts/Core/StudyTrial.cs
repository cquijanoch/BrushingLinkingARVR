namespace BrushingAndLinking
{
    public class StudyTrial
    {
        public int TrialNumber;

        public EnvironmentMode Environment;
        public HighlightTechnique Technique;
        public TaskType Task;
        
        public string QuestionID;
        public string QuestionText;
        public string[] QuestionAnswers;
        public bool[] QuestionResponses;
        public string DimensionName1;
        public string DimensionDirection1;
        public string DimensionThreshold1;
        public string DimensionName2;
        public string DimensionDirection2;
        public string DimensionThreshold2;
        public bool IsTrialCompleted = false;

        public StudyTrial(HighlightTechnique technique, TaskType task)
        {
            Technique = technique;
            Task = task;
        }

        public StudyTrial(EnvironmentMode environment, HighlightTechnique technique, TaskType task)
        {
            Environment = environment;
            Technique = technique;
            Task = task;
        }

        public StudyTrial(HighlightTechnique technique, TaskType task, StudyTask studyTask)
        {
            Technique = technique;
            Task = task;
            QuestionID = studyTask.QuestionID;
            QuestionText = studyTask.QuestionText;
            QuestionAnswers = studyTask.QuestionAnswers;
            QuestionResponses = new bool[studyTask.QuestionAnswers.Length];
            DimensionName1 = studyTask.DimensionName1;
            DimensionDirection1 = studyTask.DimensionDirection1;
            DimensionThreshold1 = studyTask.DimensionThreshold1;
            DimensionName2 = studyTask.DimensionName2;
            DimensionDirection2 = studyTask.DimensionDirection2;
            DimensionThreshold2 = studyTask.DimensionThreshold2;
        }

        public StudyTrial(EnvironmentMode environment, HighlightTechnique technique, TaskType task, StudyTask studyTask)
        {
            Technique = technique;
            Environment = environment;
            Task = task;
            QuestionID = studyTask.QuestionID;
            QuestionText = studyTask.QuestionText;
            QuestionAnswers = studyTask.QuestionAnswers;
            QuestionResponses = new bool[studyTask.QuestionAnswers.Length];
            DimensionName1 = studyTask.DimensionName1;
            DimensionDirection1 = studyTask.DimensionDirection1;
            DimensionThreshold1 = studyTask.DimensionThreshold1;
            DimensionName2 = studyTask.DimensionName2;
            DimensionDirection2 = studyTask.DimensionDirection2;
            DimensionThreshold2 = studyTask.DimensionThreshold2;
        }


        public StudyTrial(HighlightTechnique technique, TaskType task, string questionID, string questionText, string[] questionAnswers, string dimensionName1, string dimensionDirection1, string dimensionThreshold1, string dimensionName2, string dimensionDirection2, string dimensionThreshold2)
        {
            Technique = technique;
            Task = task;
            QuestionID = questionID;
            QuestionText = questionText;
            QuestionAnswers = questionAnswers;
            QuestionResponses = new bool[questionAnswers.Length];
            DimensionName1 = dimensionName1;
            DimensionDirection1 = dimensionDirection1;
            DimensionThreshold1 = dimensionThreshold1;
            DimensionName2 = dimensionName2;
            DimensionDirection2 = dimensionDirection2;
            DimensionThreshold2 = dimensionThreshold2;
        }

        public void TrialCompleted()
        {
            IsTrialCompleted = true;
        }

        public string ToFormattedString()
        {
            if (Task == TaskType.Tutorial)
                return string.Format("Trial Number: {0}\nEnvironment: {1}\nTechnique: {2}\nTask Type: {3}",
                    TrialNumber,
                    Environment.ToString(),
                    Technique.ToString(),
                    Task.ToString());
            
            return string.Format("Trial Number: {0}\nEnvironment: {1}\nTechnique: {2}\nTask Type: {3}\nTask ID: {4}\nTask Question: {5}\nTask Answers: {6}",
                TrialNumber,
                Environment.ToString(),
                Technique.ToString(),
                Task.ToString(),
                QuestionID,
                QuestionText,
                string.Join(", ", QuestionAnswers)
                );
        }
    }
}
    