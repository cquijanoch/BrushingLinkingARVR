using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

namespace BrushingAndLinking
{
    public abstract class BrushingOculusHandler : MonoBehaviour
    {
        public static BrushingOculusHandler Instance { get; private set; }

        #region Public variables
        // The Oculus SDK RayInteractor component. Used to perform raycasts from the controller which contains this component
        public RayInteractor LeftRayInteractor;
        public RayInteractor RightRayInteractor;
        public Handedness Handedness = Handedness.Right;
        public InputMode InputMode = InputMode.Controller;
        #endregion Public variables

        #region Protected variables
        protected RayInteractor rayInteractorToUse;
        protected bool isBrushing = false;
        protected bool isBrushingLocked = false;
        #endregion Protected variables

        #region Private variables
        private readonly string LayerBrusheable = "Brushable";
        #endregion


        public abstract void SetHandedness(Handedness handedness);
        protected abstract void Interaction();
        protected abstract bool ValidateInputMode();

        private void Awake()
        {
            if (ValidateInputMode())
            {
                if (Instance != null && Instance != this) Destroy(this);
                else Instance = this;
            }
        }

        private void Update()
        {
            Interaction();
        }

        /// <summary>
        /// Updates the position of the BrushPoint transform on BrushingManager based on a raycast from the provided RayInteractor component.
        ///
        /// This only raycasts from one single controller, and can only raycast against GameObjects of the "Brushable" layer.
        ///
        /// Returns true if a "Brushable" GameObject was hit, and false if it wasn't
        /// </summary>
        protected bool UpdateBrushPoint()
        {
            if (Physics.Raycast(rayInteractorToUse.Origin, rayInteractorToUse.Forward, out RaycastHit hitInfo, 2f))
            {
                if (hitInfo.collider.CompareTag(LayerBrusheable))
                {
                    BrushingManager.Instance.BrushPoint.transform.position = hitInfo.point;
                    return true;
                }
            }

            return false;
        }

        protected bool RaycastTargetingBrushable()
        {
            if (Physics.Raycast(rayInteractorToUse.Origin, rayInteractorToUse.Forward, out RaycastHit hitInfo, 2f))
            {
                Debug.Log("Targetting to " + hitInfo.collider.name);
                return hitInfo.collider.CompareTag(LayerBrusheable);
            }
                
            return false;
        }
    }
}