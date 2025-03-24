namespace BrushingAndLinking
{
    public enum LogUIMode
    {
        ShowLog,
        None
    }

    public class LogAuxButton : ButtonGroupChild
    {
        public CalibrationSetup CalibrationSetup;
        public LogUIMode LogMode;
        public override void Select()
        {
            base.Select();

            switch (LogMode)
            {
                case LogUIMode.None:
                    CalibrationSetup.ShowLog(false);
                    break;
                case LogUIMode.ShowLog:
                    CalibrationSetup.ShowLog(true);
                    break;
            }
           
        }
    }
}