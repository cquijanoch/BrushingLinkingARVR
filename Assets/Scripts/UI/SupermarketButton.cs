namespace BrushingAndLinking
{
    public class SupermarketButton : ButtonGroupChild
    {
        public SupermarketVersion buttonVersion;
        public override void Select()
        {
            base.Select();
            MainManager.Instance.ReadShelves(buttonVersion);
        }
    }
}