using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;

namespace Examples.Overlays
{
    public sealed class StatelessOverlayController : IOverlayController
    {
        private enum State
        {
            ShowingNothing,
            ShowingBusy,
            ShowingPrivacyOverBusy,
            WorkUnderneath,
            NoLongerBusy,
            ShowingPrivacy,
            NoWork,
            DeferredWork,
        }

        private readonly IOverlayStack _overlayStack;
        private readonly StateMachine<State, OverlayEvent> _machine;
        
        public StatelessOverlayController(IOverlayStack overlayStack)
        {
            _overlayStack = overlayStack;

            _machine = new(State.ShowingNothing);

            _machine.Configure(State.ShowingNothing).Permit(OverlayEvent.Backgrounded, State.NoWork).on;
        }

        public void HandleEvent(OverlayEvent trigger) => _machine.Fire(trigger);
    }
}
