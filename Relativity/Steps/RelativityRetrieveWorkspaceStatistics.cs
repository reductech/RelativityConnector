﻿using OneOf;
using Reductech.EDR.Connectors.Relativity.ManagerInterfaces;
using Relativity.Environment.V1.Workspace.Models;

namespace Reductech.EDR.Connectors.Relativity.Steps;

[SCLExample(
    "RelativityRetrieveWorkspaceStatistics Workspace: 42",
    expectedOutput: "(DocumentCount: 1234 FileSize: 5678)",
    ExecuteInTests = false
)]
public sealed class
    RelativityRetrieveWorkspaceStatistics : RelativityApiRequest<int, IWorkspaceManager1,
        WorkspaceSummary, Entity>
{
    

    /// <inheritdoc />
    public override IStepFactory StepFactory =>
        new SimpleStepFactory<RelativityRetrieveWorkspaceStatistics, Entity>();

    /// <inheritdoc />
    public override Result<Entity, IErrorBuilder> ConvertOutput(WorkspaceSummary serviceOutput)
    {
        return serviceOutput.ConvertToEntity();
    }

    /// <inheritdoc />
    public override async Task<WorkspaceSummary> SendRequest(
        IStateMonad stateMonad,
        IWorkspaceManager1 service,
        int requestObject,
        CancellationToken cancellationToken)
    {
        return await service.GetWorkspaceSummaryAsync(requestObject);
    }

    /// <inheritdoc />
    public override Task<Result<int, IError>> TryCreateRequest(
        IStateMonad stateMonad,
        CancellationToken cancellation)
    {
        return Workspace.WrapArtifact(ArtifactType.Case,stateMonad, this).Run(stateMonad, cancellation);
    }

    /// <summary>
    /// The Workspace to retrieve.
    /// You can provide either the Artifact Id or the name
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<OneOf<int, StringStream>> Workspace { get; set; } = null!;
}
