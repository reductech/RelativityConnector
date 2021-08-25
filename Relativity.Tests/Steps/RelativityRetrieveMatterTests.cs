﻿using System.Collections.Generic;
using System.Net.Http;
using Reductech.EDR.Connectors.Relativity.ManagerInterfaces;
using Reductech.EDR.Connectors.Relativity.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Relativity.Environment.V1.Matter.Models;

namespace Reductech.EDR.Connectors.Relativity.Tests.Steps
{

public partial class RelativityRetrieveMatterTests : StepTestBase<RelativityRetrieveMatter, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Retrieve Matter",
                    TestHelpers.LogEntity(
                        new RelativityRetrieveMatter()
                        {
                            MatterArtifactId = StaticHelpers.Constant(1234)
                        }
                    ),
                    Unit.Default,
                    "('Client': \"\" 'Number': \"My Number\" 'Status': \"\" 'Keywords': \"\" 'Notes': \"\" 'Meta': \"\" 'Actions': \"\" 'CreatedOn': 0001-01-01T00:00:00.0000000 'CreatedBy': \"\" 'LastModifiedBy': \"\" 'LastModifiedOn': 0001-01-01T00:00:00.0000000 'Name': \"\" 'ArtifactID': 1234 'Guids': \"\")"
                ).WithTestRelativitySettings()
                .WithService(
                    new MockSetup<IMatterManager1, MatterResponse>(
                        x => x.ReadAsync(1234),
                        new MatterResponse() { ArtifactID = 1234, Number = "My Number", }
                    )
                );

            yield return new StepCase(
                    "Retrieve Matter with HTTP",
                    TestHelpers.LogEntity(
                        new RelativityRetrieveMatter()
                        {
                            MatterArtifactId = StaticHelpers.Constant(1234)
                        }
                    ),
                    Unit.Default,
                    "('Client': \"\" 'Number': \"My Number\" 'Status': \"\" 'Keywords': \"\" 'Notes': \"\" 'Meta': \"\" 'Actions': \"\" 'CreatedOn': 0001-01-01T00:00:00.0000000 'CreatedBy': \"\" 'LastModifiedBy': \"\" 'LastModifiedOn': 0001-01-01T00:00:00.0000000 'Name': \"\" 'ArtifactID': 1234 'Guids': \"\")"
                ).WithTestRelativitySettings()
                .WithFlurlMocks(
                    x => x.ForCallsTo(
                            "http://TestRelativityServer/Relativity.REST/api/relativity-environment/v1/workspaces/-1/matters/1234"
                        )
                        .WithVerb(HttpMethod.Get)
                        .RespondWithJson(
                            new MatterResponse() { ArtifactID = 1234, Number = "My Number", }
                        )
                );
        }
    }
}

}
