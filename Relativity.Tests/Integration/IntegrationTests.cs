﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Connectors.Relativity.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Relativity.Tests.Integration
{

public partial class IntegrationTests
{
    public IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; set; }
        
    public const string SkipAll = "manual";

    [Fact(Skip = SkipAll)]
    public async void TestImportConcordance()
    {
        var step = new RelativityImport()
        {
            Workspace = TestSteps.IntegrationTestWorkspace,
            FilePath =
                Constant("C:\\Users\\wainw\\source\\repos\\Examples\\Concordance\\Carla2\\loadfile.dat"),
            SettingsFilePath =
                Constant("C:\\Users\\wainw\\source\\repos\\Examples\\Concordance\\CarlaSettings.kwe"),
            FileImportType = Constant(FileImportType.Object)
        };

        await TestSCLSequence(step);
    }

    [Fact(Skip = SkipAll)]
    public async void TestDeleteAllIntegrationTestWorkspace()
    {
        await TestSCLSequence(TestSteps.DeleteAllIntegrationTestWorkspace);
    }

    [Fact(Skip = SkipAll)]
    public async void TestDeleteAllTestMatter()
    {
        await TestSCLSequence(TestSteps.DeleteAllTestMatter);
    }

    [Fact(Skip = SkipAll)]
    public async void TestImportEntities()
    {
        string filePath = Path.Combine(
            Assembly.GetAssembly(typeof(IntegrationTests))!.Location,
            "..",
            "Data",
            "TestDocument.pdf"
        );

        var step = TestSteps.ImportEntities(filePath);
        step.Workspace = new OneOfStep<int, StringStream>(Constant(123));

        await TestSCLSequence(step);
    }

    [Fact(Skip = SkipAll)]
    public async void TestExportEntities()
    {
        var exportStep = new RelativityExport()
        {
            Workspace  = TestSteps.IntegrationTestWorkspace,
            Condition  = Constant("'Title' LIKE 'Bond'"),
            FieldNames = Array("Title")
        };

        var step = new ForEach<Entity>()
        {
            Array = exportStep,
            Action = new LambdaFunction<Entity, Unit>(
                null,
                new Log<Entity>() { Value = GetEntityVariable }
            )
        };

        await TestSCLSequence(step);
    }

    [Fact(Skip = SkipAll)]
    public async void CreateField()
    {
        var step = new RelativityCreateField()
        {
            FieldName = Constant("MyTestField"),
            //ObjectType = new OneOfStep<ArtifactType, int>(Constant(ArtifactType.Document)),
            Workspace = TestSteps.IntegrationTestWorkspace
        };
        await TestSCLSequence(step);
    }

    [Fact(Skip = SkipAll)]
    public async void CreateWorkspace()
    {
        var step = new Sequence<Unit>()
        {
            InitialSteps = new List<IStep<Unit>>()
            {
                TestSteps.DeleteAllIntegrationTestWorkspace,
                TestSteps.MaybeCreateTestMatter,
                TestSteps.MaybeCreateIntegrationTestWorkspace,
            }
        };
        await TestSCLSequence(step);
    }

    [Fact(Skip = SkipAll)]
    [Trait("Category", "Integration")]
    public async void ShortIntegrationTest()
    {
        string filePath = Path.Combine(
            Assembly.GetAssembly(typeof(IntegrationTests))!.Location,
            "..",
            "Data",
            "TestDocument.pdf"
        );

        var step = new Sequence<Unit>()
        {
            InitialSteps = new List<IStep<Unit>>()
            {
                TestSteps.DeleteAllIntegrationTestWorkspace,
                TestSteps.MaybeCreateTestMatter,
                TestSteps.MaybeCreateIntegrationTestWorkspace,
                TestSteps.AssertDocumentCount(0),
                //TestSteps.AssertFolderCount(0),

                new RunStep<int>
                {
                    Step = new RelativityCreateFolder
                    {
                        FolderName = Constant("MyFolder"),
                        Workspace  = TestSteps.IntegrationTestWorkspace,
                    }
                },
                //TestSteps.AssertFolderCount(1),

                TestSteps.ImportEntities(filePath),
                TestSteps.AssertFolderCount(2),
                TestSteps.AssertDocumentCount(1),
                TestSteps.LogWorkspaceStatistics,
                TestSteps.DeleteDocuments("Test Document"),
                TestSteps.AssertDocumentCount(0),
                new RelativityDeleteUnusedFolders()
                {
                    Workspace = TestSteps.IntegrationTestWorkspace
                },
                //TestSteps.AssertFolderCount(0),
                TestSteps.DeleteAllIntegrationTestWorkspace,
            }
        };

        await TestSCLSequence(step);
    }

    [Fact(Skip = SkipAll)]
    public async void TestSearchAndTag()
    {
        var step = new ForEach<Entity>()
        {
            Array = new RelativitySendQuery()
            {
                ArtifactType =
                    new OneOfStep<ArtifactType, int>(Constant(ArtifactType.Document)),
                Condition = Constant("'Title' LIKE 'Bond'"),
                Workspace = TestSteps.IntegrationTestWorkspace,
                Start     = Constant(0),
                Length    = Constant(100),
                Fields = new OneOfStep<Array<int>, Array<StringStream>>(
                    Array("Title", "Control Number")
                )
            },
            Action = new LambdaFunction<Entity, Unit>(
                null,
                new RelativityUpdateObject()
                {
                    ObjectArtifactId = new EntityGetValue<int>()
                    {
                        Entity   = new GetAutomaticVariable<Entity>(),
                        Property = Constant("ArtifactId")
                    },
                    Workspace   = TestSteps.IntegrationTestWorkspace,
                    FieldValues = Constant(Entity.Create(("Tags", "My Tag")))
                }
            )
        };

        await TestSCLSequence(step);
    }

    private async Task TestSCLSequence(IStep step)
    {
        var logger =
            TestOutputHelper.BuildLogger(new LoggingConfig() { LogLevel = LogLevel.Trace });

        var scl = step.Serialize();
        logger.LogInformation(scl);

        var sfs = StepFactoryStore.Create(
            new ConnectorData(
                new ConnectorSettings()
                {
                    Id      = "Reductech.EDR.Connectors.Relativity",
                    Enable  = true,
                    Version = "0.10.0",
                    Settings = new RelativitySettings
                    {
                        RelativityUsername = "relativity.admin@relativity.com",
                        //RelativityUsername = "mark@reduc.tech",
                        RelativityPassword = "Test1234!",
                        Url                = "http://relativitydevvm/",
                        APIVersionNumber   = 1,
                        ImportClientPath =
                            "C:\\Users\\wainw\\source\\repos\\Reductech\\entityimportclient\\EntityImportClient\\bin\\Debug\\EntityImportClient.exe",
                        DesktopClientPath =
                            @"C:\Program Files\kCura Corporation\Relativity Desktop Client\Relativity.Desktop.Client.exe"
                    }.ToDictionary()
                },
                typeof(RelativityGetClients).Assembly
            )
        );

        var runner = new SCLRunner(
            logger,
            sfs,
            ExternalContext.Default with
            {
                InjectedContexts = new ConnectorInjection().TryGetInjectedContexts()
                    .Value.ToArray()
            },
            DefaultRestClientFactory.Instance
        );

        var r = await runner.RunSequenceFromTextAsync(
            scl,
            new Dictionary<string, object>(),
            CancellationToken.None
        );

        r.ShouldBeSuccessful();
    }
}

}