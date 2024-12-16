namespace BrushingAndLinking
{
    public enum HighlightTechnique
    {
        None,
        Color,
        Outline,
        Link,
        //Arrow,
        Size,
        Calibration
    }

    public enum LinkTechnique
    {
        None,
        Line
    }

    public enum ShelfLayout
    {
        None,
        Tutorial,
        In,
        Out,
        Training,
        Demo
    }

    public enum TaskType
    {
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
