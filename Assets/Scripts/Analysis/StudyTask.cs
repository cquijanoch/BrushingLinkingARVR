namespace BrushingAndLinking
{
    public class StudyTask
    {
        public string QuestionID;
        public string QuestionText;
        public string[] QuestionAnswers;
        public string DimensionName1;
        public string DimensionDirection1;
        public string DimensionThreshold1;
        public string DimensionName2;
        public string DimensionDirection2;
        public string DimensionThreshold2;

        public StudyTask(string questionID, string questionText, string[] questionAnswers, string dimensionName1, string dimensionDirection1, string dimensionThreshold1, string dimensionName2, string dimensionDirection2, string dimensionThreshold2)
        {
            this.QuestionID = questionID;
            this.QuestionText = questionText;
            this.QuestionAnswers = questionAnswers;
            this.DimensionName1 = dimensionName1;
            this.DimensionDirection1 = dimensionDirection1;
            this.DimensionThreshold1 = dimensionThreshold1;
            this.DimensionName2 = dimensionName2;
            this.DimensionDirection2 = dimensionDirection2;
            this.DimensionThreshold2 = dimensionThreshold2;
        }
    }
}