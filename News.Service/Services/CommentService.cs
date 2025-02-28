using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace News.Service.Services
{
    public class CommentService(ILogger<CommentService> _logger,
        IUnitOfWork _unitOfWork, HttpClient _httpClient , IConfiguration _configuration) : ICommentService
    {
        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            _logger.LogInformation("CommentService --> GetAll called");
            return await _unitOfWork.Repository<Comment>()
        .GetAllAsync(query => query.Include(c => c.User));
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            _logger.LogInformation($"CommentService --> GetById with id : {id} called");
            var comment = await _unitOfWork.Repository<Comment>()
                .FindAsync(c => c.Id == id, query => query.Include(c => c.User));

            return comment.FirstOrDefault() ?? new Comment();
        }
        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(string userId)
        {
            _logger.LogInformation($"CommentService --> GetCommentsByUserIdAsync with userId : {userId} called");
            return await _unitOfWork.Repository<Comment>()
           .FindAsync(c => c.UserId == userId, query => query.Include(c => c.User));
        }
        //public async Task AddAsync(Comment comment)
        //{
        //    _logger.LogInformation("CommentService --> Add called");
        //    await _unitOfWork.Repository<Comment>().AddAsync(comment);
        //    await _unitOfWork.CompleteAsync();
        //}
        public async Task AddAsync(Comment comment)
        {
           (string filteredContent, bool containsBadWords) = await FilterBadWordsAsync(comment.Content);
            comment.Content = filteredContent;
            comment.ContainsBadWords = containsBadWords;
            await _unitOfWork.Repository<Comment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateAsync(Comment comment)
        {
            _logger.LogInformation("CommentService --> Update called");
            var existingComment = await _unitOfWork.Repository<Comment>().GetByIdAsync(comment.Id);
            if (existingComment == null)
            {
                _logger.LogWarning("CommentService --> Update --> existingComment is not found");
                return;
            }
            (string filteredContent, bool containsBadWords) = await FilterBadWordsAsync(comment.Content);
            existingComment.Content = filteredContent;
            existingComment.ContainsBadWords = containsBadWords;
            await _unitOfWork.Repository<Comment>().UpdateAsync(existingComment);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("CommentService --> Update succeeded");
        }
        //public async Task UpdateAsync(Comment comment)
        //{
        //    _logger.LogInformation("CommentService --> Update called");
        //    var existingComment = await _unitOfWork.Repository<Comment>().GetByIdAsync(comment.Id);
        //    if (existingComment == null)
        //    {
        //        _logger.LogWarning("CommentService --> Update --> existingComment is not found");
        //        return;
        //    }
        //    existingComment.Content = comment.Content;
        //    await _unitOfWork.Repository<Comment>().UpdateAsync(existingComment);
        //    await _unitOfWork.CompleteAsync();
        //    _logger.LogInformation("CommentService --> Update succeeded");
        //}
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"CommentService --> Delete with id : {id} called");

            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return;

            await _unitOfWork.Repository<Comment>().DeleteAsync(comment);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"CommentService --> Delete with id : {id} succeeded");
        }
        public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(string articleId)
        {
            _logger.LogInformation($"CommentService --> GetCommentsByArticleIdAsync with articleId: {articleId} called");

            return await _unitOfWork.Repository<Comment>()
                .FindAsync(c => c.ArticleId == articleId, query => query.Include(c => c.User));
        }
        public async Task<(string FilteredComment, bool ContainsBadWords)> FilterBadWordsAsync(string comment)
        {
            var userId = _configuration["NeutrinoAPI:UserID"];
            var apiKey = _configuration["NeutrinoAPI:APIKey"];
            var baseUrl = _configuration["NeutrinoAPI:BaseUrl"];
            var requestData = new Dictionary<string, string>
    {
        { "user-id", userId },
        { "api-key", apiKey },
        { "content", comment }
    };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(baseUrl, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Neutrino API Response: {jsonResponse}");

            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            bool containsBadWords = false;
            if (root.TryGetProperty("bad-words-list", out var badWordsList))
            {
                var badWords = badWordsList.EnumerateArray().Select(word => word.GetString()).ToList();
                foreach (var badWord in badWords)
                {
                    comment = Regex.Replace(comment, $@"\b{badWord}\b", "****", RegexOptions.IgnoreCase);
                    containsBadWords = true;
                }
            }

            return (comment, containsBadWords);
        }

        //public async Task<string> FilterBadWordsAsync(string comment)
        //{
        //    var userId = _configuration["NeutrinoAPI:UserID"];
        //    var apiKey = _configuration["NeutrinoAPI:APIKey"];
        //    var baseUrl = _configuration["NeutrinoAPI:BaseUrl"];
        //    var requestData = new Dictionary<string, string>
        //    {
        //        { "user-id", userId },
        //        { "api-key", apiKey },
        //        { "content", comment }
        //    };

        //    var content = new FormUrlEncodedContent(requestData);
        //    var response = await _httpClient.PostAsync(baseUrl, content);

        //    var jsonResponse = await response.Content.ReadAsStringAsync();
        //   _logger.LogInformation($"Neutrino API Response: {jsonResponse}"); 

        //    using var doc = JsonDocument.Parse(jsonResponse);
        //    var root = doc.RootElement;

        //    // Get the bad words list
        //    if (root.TryGetProperty("bad-words-list", out var badWordsList))
        //    {
        //        var badWords = badWordsList.EnumerateArray().Select(word => word.GetString()).ToList();

        //        foreach (var badWord in badWords)
        //        {
        //            comment = Regex.Replace(comment, $@"\b{badWord}\b", "****", RegexOptions.IgnoreCase);
        //        }
        //    }

        //    return comment; 
        //}
    }
}
