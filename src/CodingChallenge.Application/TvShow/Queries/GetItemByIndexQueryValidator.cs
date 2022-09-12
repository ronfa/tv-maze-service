using FluentValidation;

namespace CodingChallenge.Application.TvShow.Queries;

public class GetItemByIndexQueryValidator : AbstractValidator<GetItemByIndexQuery>
{
    public GetItemByIndexQueryValidator()
    {
        RuleFor(v => v.Index)
            .NotEmpty(); 
    }
}