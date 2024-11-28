using Oculus.Interaction.Input;

namespace BrushingAndLinking
{
    public class ControllerHandler : BrushingOculusHandler
    {

        #region Private variables
        private OVRInput.Axis1D BrushAddButton;
        private OVRInput.Axis1D BrushSubtractButton;
        private readonly float ButtonPressThreshold = 0.35f;
        #endregion

        public override void SetHandedness(Handedness handedness)
        {
            if (handedness == Handedness.Left)
            {
                LeftRayInteractor.enabled = true;
                RightRayInteractor.enabled = false;
                rayInteractorToUse = LeftRayInteractor;
                BrushAddButton = OVRInput.Axis1D.PrimaryIndexTrigger;
                BrushSubtractButton = OVRInput.Axis1D.PrimaryHandTrigger;
            }
            else
            {
                LeftRayInteractor.enabled = false;
                RightRayInteractor.enabled = true;
                rayInteractorToUse = RightRayInteractor;
                BrushAddButton = OVRInput.Axis1D.SecondaryIndexTrigger;
                BrushSubtractButton = OVRInput.Axis1D.SecondaryHandTrigger;
            }

            Handedness = handedness;  
        }
     
        protected override void Interaction()
        {
            if (!isBrushing && !isBrushingLocked)
            {
                if (IsPressedBrushAddTrigger() || IsPressedBrushSubstractTrigger())
                {
                    if (!RaycastTargetingBrushable())
                        isBrushingLocked = true;
                }
            }

            if (isBrushingLocked)
            {
                if (IsUnpressedBrushAddTrigger() && IsUnpressedBrushSubstractTrigger())
                    isBrushingLocked = false;
                return;
            }

            if (IsPressedBrushAddTrigger())
            {
                if (isBrushing && BrushingManager.Instance.SelectionMode == SelectionMode.Subtract)
                {
                    BrushingManager.Instance.StopBrushing();
                    isBrushing = false;
                }

                if (UpdateBrushPoint() && !isBrushing)
                {
                    BrushingManager.Instance.SelectionMode = SelectionMode.Add;
                    BrushingManager.Instance.StartBrushing();
                    isBrushing = true;
                }
            }
            else if (isBrushing && BrushingManager.Instance.SelectionMode == SelectionMode.Add)
            {
                BrushingManager.Instance.StopBrushing();
                isBrushing = false;
            }

            if (!(isBrushing && BrushingManager.Instance.SelectionMode != SelectionMode.Add) ||
                (isBrushing && BrushingManager.Instance.SelectionMode == SelectionMode.Subtract))
            {
                if (IsPressedBrushSubstractTrigger() && UpdateBrushPoint())
                {
                    if (!isBrushing)
                    {
                        BrushingManager.Instance.SelectionMode = SelectionMode.Subtract;
                        BrushingManager.Instance.StartBrushing();
                        isBrushing = true;
                    }
                }
                else if (isBrushing && BrushingManager.Instance.SelectionMode == SelectionMode.Subtract)
                {
                    BrushingManager.Instance.StopBrushing();
                    isBrushing = false;
                }
            }
        }

        protected override bool ValidateInputMode()
        {
            if (InputMode != InputMode.Controller)
            {
                this.enabled = false;
                return false;
            }

            return true; 
        }

        private bool IsPressedBrushAddTrigger()
        {
            return OVRInput.Get(BrushAddButton, OVRInput.Controller.Active) > ButtonPressThreshold;
        }

        private bool IsUnpressedBrushAddTrigger()
        {
            return OVRInput.Get(BrushAddButton, OVRInput.Controller.Active) <= ButtonPressThreshold;
        }

        private bool IsPressedBrushSubstractTrigger()
        {
            return OVRInput.Get(BrushSubtractButton, OVRInput.Controller.Active) > ButtonPressThreshold;
        }

        private bool IsUnpressedBrushSubstractTrigger()
        {
            return OVRInput.Get(BrushSubtractButton, OVRInput.Controller.Active) <= ButtonPressThreshold;
        }
    }
}