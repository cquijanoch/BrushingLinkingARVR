namespace BrushingAndLinking
{
    public class EnvironmentButton : ButtonGroupChild
    {
        public EnvironmentMode buttonMode;
        public override void Select()
        {
            base.Select();

            switch (buttonMode)
            {
                case EnvironmentMode.AR:
                    StartCoroutine(MainManager.Instance.ChangeEnvironment(buttonMode));
                    break;
                case EnvironmentMode.VR:
                    StartCoroutine(MainManager.Instance.ChangeEnvironment(buttonMode));
                    break;
            }
        }
    }
}