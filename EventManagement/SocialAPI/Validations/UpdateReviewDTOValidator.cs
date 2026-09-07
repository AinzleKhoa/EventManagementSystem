using FluentValidation;
using SocialAPI.DTOs;

namespace SocialAPI.Validations
{
    public class UpdateReviewDTOValidator : AbstractValidator<UpdateReviewDTO>
    {
        public UpdateReviewDTOValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Review comment cannot be empty.")
                .MinimumLength(5).WithMessage("Comment is too short (minimum 5 characters required).")
                .MaximumLength(500).WithMessage("Comment is too long (maximum 500 characters allowed).");

            RuleFor(x => x.Comment)
                .MinimumLength(20)
                .When(x => x.Rating == 1)
                .WithMessage("Since you rated 1 star, please provide at least 20 characters explaining your experience so we can improve.");

            RuleFor(x => x.Comment)
                .Must(NotContainBadWords)
                .WithMessage("Your comment contains inappropriate language. Please keep it polite.");
        }

        private bool NotContainBadWords(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return true;
            var badWords = new List<string> { "fuck you", "shit", "bitch", "asshole", "đmmmmmm", "vkl", "ngu lắm", "như cặc" };
            var lowerComment = comment.ToLower();
            return !badWords.Any(word => lowerComment.Contains(word));
        }
    }
}