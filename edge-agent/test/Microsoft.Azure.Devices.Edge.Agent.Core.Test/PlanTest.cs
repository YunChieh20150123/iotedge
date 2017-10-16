// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Agent.Core.Test
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.Azure.Devices.Edge.Util.Test.Common;
    using Xunit;
    using Microsoft.Azure.Devices.Edge.Util;
    using Moq;

    public class PlanTest
    {
        static readonly ConfigurationInfo DefaultConfigurationInfo = new ConfigurationInfo("1");

        [Fact]
        [Unit]
        public async void TestPlanExecution()
        {
            Option<TestPlanRecorder> recordKeeper = Option.Some(new TestPlanRecorder());
            var moduleExecutionList = new List<TestRecordType>
            {
                new TestRecordType(TestCommandType.TestCreate, new TestModule("module1", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image1"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestCreate, new TestModule("module2", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image2"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestCreate, new TestModule("module3", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image3"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestCreate, new TestModule("module4", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image4"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestCreate, new TestModule("module5", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image5"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),

            };
            var commandList = new List<ICommand>
            {
                new TestCommand(moduleExecutionList[0].TestType, moduleExecutionList[0].Module, recordKeeper),
                new TestCommand(moduleExecutionList[1].TestType, moduleExecutionList[1].Module, recordKeeper),
                new TestCommand(moduleExecutionList[2].TestType, moduleExecutionList[2].Module, recordKeeper),
                new TestCommand(moduleExecutionList[3].TestType, moduleExecutionList[3].Module, recordKeeper),
                new TestCommand(moduleExecutionList[4].TestType, moduleExecutionList[4].Module, recordKeeper)
            };

            var plan1 = new Plan(commandList);
            var plan2 = new Plan(new List<ICommand>());

            Assert.False(plan1.IsEmpty);
            Assert.True(Plan.Empty.IsEmpty);
            Assert.True(plan2.IsEmpty);

            var token = new CancellationToken();

            await plan1.ExecuteAsync(token);
            Assert.All(commandList,
                command =>
                {
                    var c = command as TestCommand;
                    Assert.NotNull(c);
                    Assert.True(c.CommandExecuted);
                });
            recordKeeper.ForEach( r => Assert.Equal(moduleExecutionList, r.ExecutionList));
        }

        [Fact]
        [Unit]
        public async void TestPlanFactoryCommands()
        {
            var factory = new TestCommandFactory();
            var moduleExecutionList = new List<TestRecordType>
            {
                new TestRecordType(TestCommandType.TestCreate, new TestModule("module1", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image1"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestPull, new TestModule("module2", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image2"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestUpdate, new TestModule("module3", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image3"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestRemove, new TestModule("module4", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image4"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestStart, new TestModule("module5", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image5"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
                new TestRecordType(TestCommandType.TestStop, new TestModule("module6", "version1", "type1", ModuleStatus.Stopped, new TestConfig("image6"), RestartPolicy.OnUnhealthy, DefaultConfigurationInfo)),
            };
            var identity = new Mock<IModuleIdentity>();
            var commandList = new List<ICommand>
            {
                factory.Create(new ModuleWithIdentity(moduleExecutionList[0].Module, identity.Object)),
                factory.Pull(moduleExecutionList[1].Module),
                factory.Update(moduleExecutionList[0].Module, new ModuleWithIdentity(moduleExecutionList[2].Module, identity.Object)),
                factory.Remove(moduleExecutionList[3].Module),
                factory.Start(moduleExecutionList[4].Module),
                factory.Stop(moduleExecutionList[5].Module),
            };
            var plan1 = new Plan(commandList);
            var token = new CancellationToken();
            await plan1.ExecuteAsync(token);
            Assert.All(commandList,
                command =>
                {
                    var c = command as TestCommand;
                    Assert.NotNull(c);
                    Assert.True(c.CommandExecuted);
                });
            factory.Recorder.ForEach( r => Assert.Equal(moduleExecutionList, r.ExecutionList));
        }
    }
}
