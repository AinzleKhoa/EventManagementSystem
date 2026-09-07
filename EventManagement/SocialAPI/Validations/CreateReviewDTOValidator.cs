using FluentValidation;
using SocialAPI.DTOs;

namespace SocialAPI.Validations
{
    public class CreateReviewDTOValidator : AbstractValidator<CreateReviewDTO>
    {
        public CreateReviewDTOValidator()
        {
            // 1. Kiểm tra EventId hợp lệ
            RuleFor(x => x.EventId)
                .GreaterThan(0).WithMessage("Invalid Event ID.");

            // 2. Kiểm tra Rating (1-5 sao)
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars.");

            // 3. Kiểm tra độ dài Comment cơ bản
            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Review comment cannot be empty.")
                .MinimumLength(5).WithMessage("Comment is too short (minimum 5 characters required).")
                .MaximumLength(500).WithMessage("Comment is too long (maximum 500 characters allowed).");

            // 4. Custom Logic: Nếu đánh giá 1 sao thì bắt buộc phải viết dài hơn để giải thích
            RuleFor(x => x.Comment)
                .MinimumLength(20)
                .When(x => x.Rating == 1)
                .WithMessage("Since you rated 1 star, please provide at least 20 characters explaining your experience so we can improve.");

            // 5. Custom Logic: Lọc từ ngữ thô tục (Bad word filter)
            RuleFor(x => x.Comment)
                .Must(NotContainBadWords)
                .WithMessage("Your comment contains inappropriate language. Please keep it polite.");
        }

        // Hàm hỗ trợ lọc từ khóa
        private bool NotContainBadWords(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return true;

            // Danh sách các từ cấm (Cả tiếng Anh lẫn tiếng Việt)
            var badWords = new List<string> { "fuck", "shit", "bitch", "asshole", "đm", "vkl", "ngu" };

            var lowerComment = comment.ToLower();
            return !badWords.Any(word => lowerComment.Contains(word));
        }
    }
}