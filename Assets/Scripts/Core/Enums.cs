namespace BrushingAndLinking
{
    public enum HighlightTechnique
    {
        None,
        Color,
        Outline,
        AnimatedOutline,
        AnimatedOutlineLink,
        Link,
        //Arrow,
        //Size,
        //Calibration
    }

    public enum EnvironmentMode
    {
        AR,
        VR
    }

    public enum LinkTechnique
    {
        None,
        Line
    }

    public enum ShelfLayout
    {
        None,
        A,
        B,
        C,
        D,
        E
    }

    public enum ApplicationMode
    {
        Demo,
        Study,
        None
    }

    public enum StudyMode
    {
        Play,
        Pause,
        Questionnaire
    }

    public enum TaskType
    {
        None,
        Demo,
        Tutorial,
        Single,
        Multiple,
        Hypothesis
    }

    public enum InteractionType
    {
        Brushing,
        Filter,
        DimensionChange,
        BrushChange,
        ProductSelect
    }
}
