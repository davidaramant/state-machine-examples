using System;
using System.Collections.Generic;
using System.Linq;
using Examples.Overlays;
using Xunit;

namespace Examples.Tests.Overlays
{
    public class OverlayStateMachineTests
    {
        public sealed class Scenario
        {
            private readonly Func<IOverlayStack, IOverlayStateMachine> _buildMachine;
            private readonly string _description;


            public Scenario(Func<IOverlayStack, IOverlayStateMachine> buildMachine, string description)
            {
                _buildMachine = buildMachine;
                _description = description;
            }

            public override string ToString() => _description;

            public IOverlayStateMachine Build(IOverlayStack stack) => _buildMachine(stack);
        }

        public static IEnumerable<object[]> AllMachines =
            new[]
            {
                new object[]{ new Scenario(stack => new NestedSwitchOverlayStateMachine(stack),"Nested Switch")},
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
    }
}
