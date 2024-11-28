using Oculus.Interaction.Input;
using UnityEngine;

namespace BrushingAndLinking
{
    public class HandGesturesHandler : BrushingOculusHandler
    {

        #region Private variables
        //private OVRInput.Axis1D BrushAddButton;
        //private OVRInput.Axis1D BrushSubtractButton;
        private readonly float ButtonPressThreshold = 0.0f;
        private OVRHand handToUse;
        #endregion

        public OVRHand HandLeft;
        public OVRHand HandRight;
        public float PressingIndexFingerActionTime = 0f;
        public float PressingMiddleFingerActionTime = 0f;

        public override void SetHandedness(Handedness handedness)
        {
            if (handedness == Handedness.Left)
            {
                LeftRayInteractor.enabled = true;
                RightRayInteractor.enabled = false;
                rayInteractorToUse = LeftRayInteractor;
                HandLeft.enabled = true;
                HandRight.enabled = false;
                handToUse = HandLeft;
                //BrushAddButton = OVRInput.Axis1D.PrimaryIndexTrigger;
                //BrushSubtractButton = OVRInput.Axis1D.PrimaryHandTrigger;
            }
            else
            {
                LeftRayInteractor.enabled = false;
                RightRayInteractor.enabled = true;
                rayInteractorToUse = RightRayInteractor;
                HandLeft.enabled = false;
                HandRight.enabled = true;
                handToUse = HandRight;
                //BrushAddButton = OVRInput.Axis1D.SecondaryIndexTrigger;
                //BrushSubtractButton = OVRInput.Axis1D.SecondaryHandTrigger;
            }

            Handedness = handedness;  
        }
     
        protected override void Interaction()
        {
            //Debug.Log("INDEX FINGER PINCH " + OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Hands));
            //Debug.Log("Middle FINGER PINCH " + handToUse.GetFingerIsPinching(OVRHand.HandFinger.Middle));

            //rayInteractorToUse.InjectSelector()

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
                if (!IsPressedBrushAddTrigger() && !IsPressedBrushSubstractTrigger())
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
            if (InputMode != InputMode.Hand)
            {
                this.enabled = false;
                return false;
            }

            return true; 
        }

        private bool IsPressedBrushAddTrigger()
        {
            if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Hands))
            {
                PressingIndexFingerActionTime += Time.deltaTime;
                PressingMiddleFingerActionTime = 0f;
            }  
            else
                PressingIndexFingerActionTime = 0f;

            return PressingIndexFingerActionTime > ButtonPressThreshold;
            //return OVRInput.Get(BrushAddButton, OVRInput.Controller.Active) > ButtonPressThreshold;
        }

        private bool IsUnpressedBrushAddTrigger()
        {
            return PressingIndexFingerActionTime <= ButtonPressThreshold;
            //return OVRInput.Get(BrushAddButton, OVRInput.Controller.Active) <= ButtonPressThreshold;
        }

        private bool IsPressedBrushSubstractTrigger()
        {
            if (handToUse.GetFingerIsPinching(OVRHand.HandFinger.Middle))
            {
                PressingMiddleFingerActionTime += Time.deltaTime;
                PressingIndexFingerActionTime = 0f;
            } 
            else
                PressingMiddleFingerActionTime = 0f;

            return PressingMiddleFingerActionTime > ButtonPressThreshold;
            //return OVRInput.Get(BrushSubtractButton, OVRInput.Controller.Active) > ButtonPressThreshold;
        }

        private bool IsUnpressedBrushSubstractTrigger()
        {
            return PressingMiddleFingerActionTime <= ButtonPressThreshold;
            //return OVRInput.Get(BrushSubtractButton, OVRInput.Controller.Active) <= ButtonPressThreshold;
        }
    }
}