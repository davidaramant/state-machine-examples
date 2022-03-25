using System;
using System.Collections.Generic;
using System.Linq;
using Examples.Overlays;
using FluentAssertions;
using Xunit;

namespace Examples.Tests.Overlays
{
    public class OverlayControllerTests
    {
        public sealed class Scenario
        {
            private readonly Func<IOverlayStack, IOverlayController> _buildMachine;
            private readonly string _description;


            public Scenario(Func<IOverlayStack, IOverlayController> buildMachine, string description)
            {
                _buildMachine = buildMachine;
                _description = description;
            }

            public override string ToString() => _description;

            public IOverlayController Build(IOverlayStack stack) => _buildMachine(stack);
        }

        public static IEnumerable<object[]> AllMachines =
            new[]
            {
                new object[]{ new Scenario(stack => new NestedSwitchOverlayController(stack),"Nested Switch")},
            };

        [Theory]
        [MemberData(nameof(AllMachines))]
        public void ShouldHandleRandomEventStream(Scenario scenario)
        {
            var machine = scenario.Build(new ValidatingOverlayStack());
            const int NumEvents = 100;
            var random = new Random();
            var maxEventNum = Enum.GetValues(typeof(OverlayEvent)).Cast<int>().Max();

            var eventStream = Enumerable.Range(0, NumEvents).Select(_ => (OverlayEvent)random.Next(maxEventNum));
            foreach (var overlayEvent in eventStream)
            {
                machine.HandleEvent(overlayEvent);
            }
        }

        [Theory]
        [MemberData(nameof(AllMachines))]
        public void ShouldDisplayBusyOverlayWhileWorkIsOngoing(Scenario scenario)
        {
            var stack = new ValidatingOverlayStack();
            var machine = scenario.Build(stack);

            machine.HandleEvent(OverlayEvent.WorkStarted);
            stack.IsBusyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.WorkStarted);
            stack.IsBusyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.WorkEnded);
            stack.IsBusyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.WorkEnded);
            stack.IsBusyInStack.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(AllMachines))]
        public void ShouldHandlePrivacyOverlayOnTopOfOngoingWork(Scenario scenario)
        {
            var stack = new ValidatingOverlayStack();
            var machine = scenario.Build(stack);

            machine.HandleEvent(OverlayEvent.WorkStarted);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeFalse();

            machine.HandleEvent(OverlayEvent.Backgrounded);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.Foregrounded);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeFalse();

            machine.HandleEvent(OverlayEvent.WorkEnded);
            stack.IsBusyInStack.Should().BeFalse();
            stack.IsPrivacyInStack.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(AllMachines))]
        public void ShouldHandlePrivacyOverlayOnTopOfWorkThatEnds(Scenario scenario)
        {
            var stack = new ValidatingOverlayStack();
            var machine = scenario.Build(stack);

            machine.HandleEvent(OverlayEvent.WorkStarted);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeFalse();

            machine.HandleEvent(OverlayEvent.Backgrounded);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.WorkEnded);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.Foregrounded);
            stack.IsBusyInStack.Should().BeFalse();
            stack.IsPrivacyInStack.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(AllMachines))]
        public void ShouldHandleWorkThatStartsWhileShowingPrivacyOverlay(Scenario scenario)
        {
            var stack = new ValidatingOverlayStack();
            var machine = scenario.Build(stack);

            machine.HandleEvent(OverlayEvent.Backgrounded);
            stack.IsBusyInStack.Should().BeFalse();
            stack.IsPrivacyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.WorkStarted);
            stack.IsBusyInStack.Should().BeFalse();
            stack.IsPrivacyInStack.Should().BeTrue();

            machine.HandleEvent(OverlayEvent.Foregrounded);
            stack.IsBusyInStack.Should().BeTrue();
            stack.IsPrivacyInStack.Should().BeFalse();

            machine.HandleEvent(OverlayEvent.WorkEnded);
            stack.IsBusyInStack.Should().BeFalse();
            stack.IsPrivacyInStack.Should().BeFalse();
        }
    }
}
