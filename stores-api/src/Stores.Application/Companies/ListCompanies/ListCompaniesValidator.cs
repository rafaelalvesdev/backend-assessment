using FluentValidation;
using Stores.Application.Common;

namespace Stores.Application.Companies.ListCompanies;

public sealed class ListCompaniesValidator : AbstractValidator<ListCompaniesQuery>
{
    public ListCompaniesValidator() => this.PageRules(x => x.Page, x => x.PageSize);
}
