using Examples.Overlays;
using FluentAssertions;
using Xunit;

namespace Examples.Tests.Overlays
{
    public sealed class ValidatingOverlayStack : IOverlayStack
    {
        private bool _busyInStack = false;
        private bool _privacyInStack = false;

        public void AddBusyOverlay()
        {
            _privacyInStack.Should().BeFalse("cannot add busy overlay over privacy overlay.");
            _busyInStack.Should().BeFalse("cannot add busy overlay twice.");
            _busyInStack = true;
        }

        public void AddPrivacyOverlay()
        {
            _privacyInStack.Should().BeFalse("cannot add privacy overlay over privacy overlay.");
            _privacyInStack = true;
        }

        public void PopOverlay()
        {
            Assert.True(_privacyInStack || _busyInStack, "Attempted to pop empty stack");

            if (_privacyInStack)
            {
                _privacyInStack = false;
            }
            else
            {
                _busyInStack = false;
            }
        }
    }
}
