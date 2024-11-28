namespace BrushingAndLinking
{
    public class NextTaskButton : Button
    {
        public override void Select()
        {
            StudyManager.Instance.NextStudyStep();
        }
    }
}