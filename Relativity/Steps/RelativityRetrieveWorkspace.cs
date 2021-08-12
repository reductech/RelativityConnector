﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Connectors.Relativity.ManagerInterfaces;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Relativity.Environment.V1.Workspace.Models;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Relativity.Steps
{

[SCLExample(
    "RelativityRetrieveWorkspace Workspace: 11 IncludeMetadata: True IncludeActions: True",
    ExecuteInTests = false,
    ExpectedOutput =
        "(Client: \"\" ClientNumber: \"\" DownloadHandlerUrl: \"\" EnableDataGrid: False Matter: \"\" MatterNumber: \"\" ProductionRestrictions: \"\" ResourcePool: \"\" DefaultFileRepository: \"\" DataGridFileRepository: \"\" DefaultCacheLocation: \"\" SqlServer: \"\" AzureCredentials: \"\" AzureFileSystemCredentials: \"\" SqlFullTextLanguage: \"\" Status: \"\" WorkspaceAdminGroup: \"\" Keywords: \"\" Notes: \"\" CreatedOn: 0001-01-01T00:00:00.0000000 CreatedBy: \"\" LastModifiedBy: \"\" LastModifiedOn: 0001-01-01T00:00:00.0000000 Meta: (Unsupported: \"\" ReadOnly: [\"Meta\", \"Data\"]) Actions: [(Name: \"MyAction\" Href: \"\" Verb: \"Post\" IsAvailable: True Reason: \"\")] Name: \"\" ArtifactID: 11 Guids: \"\")"
)]
public sealed class
    RelativityRetrieveWorkspace : RelativityApiRequest<(int workspaceId, bool includeMetadata, bool
        includeActions),
        IWorkspaceManager1, WorkspaceResponse, Entity>
{
    /// <summary>
    /// The Workspace to retrieve.
    /// You can provide either the Artifact Id or the name
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<OneOf<int, StringStream>> Workspace { get; set; } = null!;

    /// <summary>
    /// Whether to include metadata in the result
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("false")]
    public IStep<bool> IncludeMetadata { get; set; } = new BoolConstant(false);

    /// <summary>
    /// Whether to include actions in the result
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> IncludeActions { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory =>
        new SimpleStepFactory<RelativityRetrieveWorkspace, Entity>();

    /// <inheritdoc />
    public override Result<Entity, IErrorBuilder> ConvertOutput(WorkspaceResponse serviceOutput)
    {
        var r = APIRequestHelpers.TryConvertToEntity(serviceOutput);
        return r;
    }

    /// <inheritdoc />
    public override async Task<WorkspaceResponse> SendRequest(
        IStateMonad stateMonad,
        IWorkspaceManager1 service,
        (int workspaceId, bool includeMetadata, bool includeActions) requestObject,
        CancellationToken cancellationToken)
    {
        return await service.ReadAsync(
            requestObject.workspaceId,
            requestObject.includeMetadata,
            requestObject.includeActions
        );
    }

    /// <inheritdoc />
    public override async
        Task<Result<(int workspaceId, bool includeMetadata, bool includeActions), IError>>
        TryCreateRequest(IStateMonad stateMonad, CancellationToken cancellation)
    {
        return await stateMonad.RunStepsAsync(
            Workspace.WrapWorkspace(stateMonad, this),
            IncludeMetadata,
            IncludeActions,
            cancellation
        );
    }
}

}
