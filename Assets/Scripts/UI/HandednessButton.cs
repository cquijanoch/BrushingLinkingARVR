using Oculus.Interaction.Input;

namespace BrushingAndLinking
{

    public class HandednessButton : ButtonGroupChild
    {
        public Handedness buttonMode;
        public override void Select()
        {
            base.Select();
            
            switch (buttonMode)
            {
                case Handedness.Right:
                    MainManager.Instance.SetHandedness(Handedness.Right);
                    break;
                case Handedness.Left:
                    MainManager.Instance.SetHandedness(Handedness.Left);
                    break;
            }
        }
    }
}