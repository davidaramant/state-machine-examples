namespace Examples.Overlays
{
    public sealed class NestedSwitchOverlayController : IOverlayController
    {
        enum State
        {
            ShowingNothing,
            ShowingBusy,
            ShowingPrivacyOverBusyWorkUnderneath,
            ShowingPrivacyOverBusyNoLongerBusy,
            ShowingPrivacyNoWork,
            ShowingPrivacyDeferredWork
        }

        private readonly IOverlayStack _overlayStack;
        private State _currentState = State.ShowingNothing;
        private int _workCount = 0;

        public NestedSwitchOverlayController(IOverlayStack overlayStack) => _overlayStack = overlayStack;
        
        public void HandleEvent(OverlayEvent trigger) =>
            _currentState = _currentState switch
            {
                State.ShowingNothing => ShowingNothing(trigger),
                State.ShowingBusy => ShowingBusy(trigger),
                State.ShowingPrivacyOverBusyWorkUnderneath => ShowingPrivacyOverBusyWorkUnderneath(trigger),
                State.ShowingPrivacyOverBusyNoLongerBusy => ShowingPrivacyOverBusyNoLongerBusy(trigger),
                State.ShowingPrivacyNoWork => ShowingPrivacyNoWork(trigger),
                State.ShowingPrivacyDeferredWork => ShowingPrivacyDeferredWork(trigger),
                _ => throw new ArgumentOutOfRangeException()
            };

        private State ShowingNothing(OverlayEvent trigger)
        {
            switch (trigger)
            {
                case OverlayEvent.WorkStarted:
                    IncrementWorkCount();
                    _overlayStack.AddBusyOverlay();
                    return State.ShowingBusy;

                case OverlayEvent.Backgrounded:
                    _overlayStack.AddPrivacyOverlay();
                    return State.ShowingPrivacyNoWork;

                case OverlayEvent.WorkEnded:
                case OverlayEvent.Foregrounded:
                default:
                    return _currentState;
            }
        }

        private State ShowingBusy(OverlayEvent trigger)
        {
            switch (trigger)
            {
                case OverlayEvent.WorkStarted:
                    IncrementWorkCount();
                    return _currentState;

                case OverlayEvent.WorkEnded:
                    DecrementWorkCount();
                    if (_workCount > 0)
                    {
                        return _currentState;
                    }
                    else
                    {
                        _overlayStack.PopOverlay();
                        return State.ShowingNothing;
                    }
                    
                case OverlayEvent.Backgrounded:
                    _overlayStack.AddPrivacyOverlay();
                    return State.ShowingPrivacyOverBusyWorkUnderneath;

                case OverlayEvent.Foregrounded:
                default:
                    return _currentState;
            }
        }

        private State ShowingPrivacyOverBusyWorkUnderneath(OverlayEvent trigger)
        {
            switch (trigger)
            {
                case OverlayEvent.WorkStarted:
                    IncrementWorkCount();
                    return _currentState;

                case OverlayEvent.WorkEnded:
                    DecrementWorkCount();
                    return _workCount > 0 ? _currentState : State.ShowingPrivacyOverBusyNoLongerBusy;

                case OverlayEvent.Foregrounded:
                    _overlayStack.PopOverlay();
                    return State.ShowingBusy;

                case OverlayEvent.Backgrounded:
                default:
                    return _currentState;
            }
        }

        private State ShowingPrivacyOverBusyNoLongerBusy(OverlayEvent trigger)
        {
            switch (trigger)
            {
                case OverlayEvent.WorkStarted:
                    IncrementWorkCount();
                    return State.ShowingPrivacyOverBusyWorkUnderneath;

                case OverlayEvent.Foregrounded:
                    _overlayStack.PopOverlay();
                    _overlayStack.PopOverlay();
                    return State.ShowingNothing;
                
                case OverlayEvent.WorkEnded:
                case OverlayEvent.Backgrounded:
                default:
                    return _currentState;
            }
        }

        private State ShowingPrivacyNoWork(OverlayEvent trigger)
        {
            switch (trigger)
            {
                case OverlayEvent.WorkStarted:
                    IncrementWorkCount();
                    return State.ShowingPrivacyDeferredWork;

                case OverlayEvent.Foregrounded:
                    _overlayStack.PopOverlay();
                    return State.ShowingNothing;
                
                case OverlayEvent.Backgrounded:
                case OverlayEvent.WorkEnded:
                default:
                    return _currentState;
            }
        }        
        
        private State ShowingPrivacyDeferredWork(OverlayEvent trigger)
        {
            switch (trigger)
            {
                case OverlayEvent.WorkStarted:
                    IncrementWorkCount();
                    return _currentState;

                case OverlayEvent.WorkEnded:
                    DecrementWorkCount();
                    return _workCount > 0 ? _currentState : State.ShowingPrivacyNoWork;

                case OverlayEvent.Foregrounded:
                    _overlayStack.PopOverlay();
                    _overlayStack.AddBusyOverlay();
                    return State.ShowingBusy;

                case OverlayEvent.Backgrounded:
                default:
                    return _currentState;
            }
        }


        private void IncrementWorkCount() => _workCount++;
        private void DecrementWorkCount() => _workCount--;
    }
}
