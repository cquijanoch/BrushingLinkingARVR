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
        Training
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
