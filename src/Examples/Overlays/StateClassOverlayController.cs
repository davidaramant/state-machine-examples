namespace Examples.Overlays
{
    public sealed class StateClassOverlayController : IOverlayController
    {
        abstract class State
        {
            private int _workCount = 0;
            protected readonly IOverlayStack _overlayStack;

            protected State(IOverlayStack overlayStack) => _overlayStack = overlayStack;

            public abstract State Handle(OverlayEvent trigger);

            protected void IncrementWorkCount() => _workCount++;
            protected void DecrementWorkCount() => _workCount--;
        }

        sealed class ShowingNothing : State
        {

            public ShowingNothing(IOverlayStack overlayStack) : base(overlayStack) { }

            public override State Handle(OverlayEvent trigger)
            {
                switch (trigger)
                {
                    case OverlayEvent.WorkStarted:
                        IncrementWorkCount();
                        _overlayStack.AddBusyOverlay();
                        return new ShowingBusy(_overlayStack, workCount);

                    case OverlayEvent.Backgrounded:
                        _overlayStack.AddPrivacyOverlay();
                        return new NoWork();

                    case OverlayEvent.WorkEnded:
                    case OverlayEvent.Foregrounded:
                    default:
                        return this;
                }
            }
        }

        sealed class ShowingBusy : State
        {
            public State Handle(OverlayEvent trigger) => throw new NotImplementedException();
        }

        abstract class ShowingPrivacyOverBusy : State
        {
            public State Handle(OverlayEvent trigger) => throw new NotImplementedException();
        }

        sealed class WorkUnderneath : ShowingPrivacyOverBusy
        {

        }

        sealed class NoLongerBusy : ShowingPrivacyOverBusy
        {

        }

        abstract class ShowingPrivacy : State
        {
            public State Handle(OverlayEvent trigger) => throw new NotImplementedException();
        }

        sealed class NoWork : ShowingPrivacy
        {

        }

        sealed class DeferredWork : ShowingPrivacy
        {

        }

        private State _currentState;

        public StateClassOverlayController(IOverlayStack overlayStack)
        {
            _currentState = new ShowingNothing(overlayStack);
        }

        public void HandleEvent(OverlayEvent trigger) =>
            _currentState = _currentState.Handle(trigger);

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
    }
}
