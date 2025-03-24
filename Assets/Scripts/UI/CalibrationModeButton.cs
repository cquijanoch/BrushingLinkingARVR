namespace BrushingAndLinking
{
    public enum CalibrationStep
    {
        Start,
        FloorPoints,
        ShowShelves,
        MoveShelves,
        Demo,
        StartStudy,
        StopStudy
    }

    public class CalibrationModeButton : ButtonGroupChild
    {
        public CalibrationSetup CalibrationSetup;
        public CalibrationStep Step;

        public override void Select()
        {
            bool status = false;

            switch (Step)
            {
                case CalibrationStep.Start:
                    status = CalibrationSetup.CalibrationInit();
                    break;
                case CalibrationStep.FloorPoints:
                    status = CalibrationSetup.ShowFloorPoints();
                    break;
                case CalibrationStep.ShowShelves:
                    status = CalibrationSetup.ShowShelvesCalibration();
                    break;
                case CalibrationStep.MoveShelves:
                    CalibrationSetup.FinishShelvesCalibration();
                    status = true;
                    break;
                case CalibrationStep.Demo:
                    status = CalibrationSetup.StartDemo();
                    break;
                case CalibrationStep.StartStudy:
                    status = CalibrationSetup.StartStudy();
                    break;
                case CalibrationStep.StopStudy:
                    status = CalibrationSetup.StopStudy();
                    break;
            }

            if (status)
                base.Select();
        }
    }
}