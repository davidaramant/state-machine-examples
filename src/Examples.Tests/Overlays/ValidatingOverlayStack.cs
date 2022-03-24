using Examples.Overlays;
using FluentAssertions;
using Xunit;

namespace Examples.Tests.Overlays
{
    public sealed class ValidatingOverlayStack : IOverlayStack
    {
        public bool IsBusyInStack { get; private set; }
        public bool IsPrivacyInStack { get; private set; }

        public void AddBusyOverlay()
        {
            IsPrivacyInStack.Should().BeFalse("cannot add busy overlay over privacy overlay.");
            IsBusyInStack.Should().BeFalse("cannot add busy overlay twice.");
            IsBusyInStack = true;
        }

        public void AddPrivacyOverlay()
        {
            IsPrivacyInStack.Should().BeFalse("cannot add privacy overlay over privacy overlay.");
            IsPrivacyInStack = true;
        }

        public void PopOverlay()
        {
            Assert.True(IsPrivacyInStack || IsBusyInStack, "Attempted to pop empty stack");

            if (IsPrivacyInStack)
            {
                IsPrivacyInStack = false;
            }
            else
            {
                IsBusyInStack = false;
            }
        }
    }
}
