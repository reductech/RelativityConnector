﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Flurl.Http;
using Reductech.EDR.Connectors.Relativity.Errors;
using Reductech.EDR.Connectors.Relativity.ManagerInterfaces;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Relativity.Environment.V1.Matter.Models;
using Relativity.Environment.V1.Workspace.Models;
using Relativity.Services.Folder;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Relativity
{

public static class RelativityEntityConversionHelpers
{
    public static Entity ConvertToEntity(this WorkspaceResponse workspaceResponse)
    {
        var w = new
        {
            workspaceResponse.Name,
            workspaceResponse.ArtifactID,
            workspaceResponse.Notes,
            workspaceResponse.CreatedOn,
            workspaceResponse.DownloadHandlerUrl
        };

        return ConvertToEntity(w);
    }

    public static Entity ConvertToEntity(this DisplayableObjectIdentifier obj)
    {
        var w = new
        {
            obj.Name,
            obj.ArtifactID
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this DeleteItem obj)
    {
        var w = new
        {
            obj.ObjectTypeName,
            obj.Action,
            obj.Count,
            obj.Connection
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this DeleteResult obj)
    {
        var w = new
        {
            obj.Report.DeletedItems.Count,
            DeletedItems = obj.Report.DeletedItems.Select(x=>x.ConvertToEntity()).ToList()
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this WorkspaceSummary obj)
    {
        var w = new
        {
            obj.DocumentCount, obj.FileSize
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this MatterResponse obj)
    {
        var w = new
        {
            obj.ArtifactID,
            obj.Name,
            obj.Keywords,
            obj.Notes,
            obj.Number
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this KeywordSearch obj)
    {
        var w = new
        {
            obj.ArtifactID,
            obj.Name,
            obj.ArtifactTypeID,
            obj.Notes,
            obj.Keywords,
            obj.QueryHint,
            obj.RequiresManualRun,
            obj.Scope,
            obj.SearchText,
            obj.SearchType
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this FolderMoveResultSet obj)
    {
        var w = new
        {
            obj.TotalOperations,
            obj.OperationsCompleted,
            obj.ProcessState
        };

        return ConvertToEntity(w);
    }
    
    public static Entity ConvertToEntity(this Folder x)
    {
        var w = new
        {
            x.Name,
            x.ArtifactID,
            x.HasChildren, 
            x.SystemCreatedOn,
            x.SystemLastModifiedOn, 
            x.Selected
        };

        return ConvertToEntity(w);
    }

    public static Entity ConvertToEntity<T>(T thing) where T : class
    {
        var properties = new List<(EntityPropertyKey key, object? value)>();

        foreach (var propertyInfo in typeof(T).GetProperties())
        {
            var v = propertyInfo.GetValue(thing);

            properties.Add(
                new ValueTuple<EntityPropertyKey, object?>(new EntityPropertyKey(propertyInfo.Name), v)
            );
        }

        return Entity.Create(properties);
    }
}

/// <summary>
/// Contains methods to help with API requests
/// </summary>
public static class APIRequestHelpers
{
    public static async Task<Result<TResult, IErrorBuilder>> TrySendRequest<TManager, TResult>(
        this TManager manager,
        Func<TManager, Task<TResult>> sendRequest)
        where TManager : IManager
    {
        TResult result;

        try
        {
            result = await sendRequest(manager);
        }
        catch (FlurlHttpException flurlHttpException)
        {
            IDictionary<string, object?> responseException =
                await flurlHttpException.GetResponseJsonAsync();

            var responseMessage = responseException?["Message"]?.ToString() ?? "";

            return ErrorCode_Relativity.RequestFailed
                .ToErrorBuilder(
                    flurlHttpException.StatusCode ?? 0,
                    flurlHttpException.Message,
                    responseMessage
                );
        }

        catch (Exception ex)
        {
            return ErrorCode.Unknown.ToErrorBuilder(ex);
        }

        return result;
    }
}

}
