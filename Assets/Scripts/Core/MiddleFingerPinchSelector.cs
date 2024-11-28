using Oculus.Interaction;
using Oculus.Interaction.Input;
using System;
using UnityEngine;

namespace BrushingAndLinking
{
    /// <summary>
    /// Broadcasts whether the hand's middle finger is selecting or unselecting. The middle finger is selecting if it's pinching.
    /// </summary>
    public class MiddlePinchSelector : MonoBehaviour, ISelector
    {
        /// <summary>
        /// The hand to check.
        /// </summary>
        [Tooltip("The hand to check.")]
        [SerializeField, Interface(typeof(IHand))]
        private UnityEngine.Object _hand;
        public IHand Hand { get; private set; }

        private bool _isMiddleFingerPinching;

        public event Action WhenSelected = delegate { };
        public event Action WhenUnselected = delegate { };

        protected bool _started = false;

        protected virtual void Awake()
        {
            Hand = _hand as IHand;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(Hand, nameof(Hand));
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                Hand.WhenHandUpdated += HandleHandUpdated;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                Hand.WhenHandUpdated -= HandleHandUpdated;
            }
        }

        private void HandleHandUpdated()
        {
            var prevPinching = _isMiddleFingerPinching;
            _isMiddleFingerPinching = Hand.GetFingerIsPinching(HandFinger.Middle);
            if (prevPinching != _isMiddleFingerPinching)
            {
                if (_isMiddleFingerPinching)
                {
                    WhenSelected();
                }
                else
                {
                    WhenUnselected();
                }
            }
        }

        #region Inject

        public void InjectAllMiddlePinchSelector(IHand hand)
        {
            InjectHand(hand);
        }

        public void InjectHand(IHand hand)
        {
            _hand = hand as UnityEngine.Object;
            Hand = hand;
        }

        #endregion
    }
}
