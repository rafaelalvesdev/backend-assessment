using Microsoft.AspNetCore.Mvc;
using Stores.Api.Contracts;
using Stores.Application.Common;
using Stores.Application.Stores;
using Stores.Application.Stores.CreateStore;
using Stores.Application.Stores.DeleteStore;
using Stores.Application.Stores.GetStore;
using Stores.Application.Stores.ListStores;
using Stores.Application.Stores.UpdateStore;

namespace Stores.Api.Controllers;

[Route("api/v1/companies/{companyId:guid}/stores")]
[Tags("Stores")]
public sealed class StoresController : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List stores of a company")]
    [EndpointDescription("Results are ordered by store code. Use `country` to filter by ISO 3166-1 alpha-2 country code.")]
    [ProducesResponseType<PagedResult<StoreResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(
        Guid companyId,
        [FromServices] IHandler<ListStoresQuery, Result<PagedResult<StoreResponse>>> handler,
        string? country = null,
        int page = 1,
        int pageSize = Paging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.HandleAsync(new ListStoresQuery(companyId, country, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{storeId:guid}", Name = "GetStore")]
    [EndpointSummary("Get a store")]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid companyId,
        Guid storeId,
        [FromServices] IHandler<GetStoreQuery, Result<StoreResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetStoreQuery(companyId, storeId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [EndpointSummary("Create a store")]
    [EndpointDescription("Store codes are case-insensitive and unique within a company.")]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid companyId,
        StoreRequest request,
        [FromServices] IHandler<CreateStoreCommand, Result<StoreResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCreateCommand(companyId), cancellationToken);

        return result.IsSuccess
            ? CreatedAtRoute("GetStore", new { companyId, storeId = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpPut("{storeId:guid}")]
    [EndpointSummary("Replace a store")]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid companyId,
        Guid storeId,
        StoreRequest request,
        [FromServices] IHandler<UpdateStoreCommand, Result<StoreResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToUpdateCommand(companyId, storeId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpDelete("{storeId:guid}")]
    [EndpointSummary("Delete a store")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid companyId,
        Guid storeId,
        [FromServices] IHandler<DeleteStoreCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteStoreCommand(companyId, storeId), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }
}
