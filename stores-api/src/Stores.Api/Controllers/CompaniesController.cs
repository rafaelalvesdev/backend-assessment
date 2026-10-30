using Microsoft.AspNetCore.Mvc;
using Stores.Api.Contracts;
using Stores.Application.Common;
using Stores.Application.Companies;
using Stores.Application.Companies.CreateCompany;
using Stores.Application.Companies.DeleteCompany;
using Stores.Application.Companies.GetCompany;
using Stores.Application.Companies.ListCompanies;

namespace Stores.Api.Controllers;

[Route("api/v1/companies")]
[Tags("Companies")]
public sealed class CompaniesController : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List companies")]
    [ProducesResponseType<PagedResult<CompanyResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromServices] IHandler<ListCompaniesQuery, Result<PagedResult<CompanyResponse>>> handler,
        int page = 1,
        int pageSize = Paging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.HandleAsync(new ListCompaniesQuery(page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{companyId:guid}", Name = "GetCompany")]
    [EndpointSummary("Get a company")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid companyId,
        [FromServices] IHandler<GetCompanyQuery, Result<CompanyResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCompanyQuery(companyId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [EndpointSummary("Create a company")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CompanyRequest request,
        [FromServices] IHandler<CreateCompanyCommand, Result<CompanyResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtRoute("GetCompany", new { companyId = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpDelete("{companyId:guid}")]
    [EndpointSummary("Delete a company")]
    [EndpointDescription("Only companies without stores can be deleted; delete their stores first.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid companyId,
        [FromServices] IHandler<DeleteCompanyCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteCompanyCommand(companyId), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }
}
