namespace BrushingAndLinking
{
    public enum CalibrationStep
    {
        Start,
        FloorPoints,
        Shelves
    }

    public class CalibrationModeButton : ButtonGroupChild
    {
        public CalibrationSetup CalibrationSetup;
        public CalibrationStep Step;
        public override void Select()
        {
            base.Select();

            switch (Step)
            {
                case CalibrationStep.Start:
                    CalibrationSetup.CalibrationInit();
                    break;
                case CalibrationStep.FloorPoints:
                    CalibrationSetup.ShowFloorPoints();
                    break;
                case CalibrationStep.Shelves:
                    CalibrationSetup.ShelvesCalibration();
                    break;

            }
           
        }
    }
}