﻿using News.Core.Dtos;
using News.Core.Entities;
using News.Core.Entities.NewsCatcher;

namespace News.Core.Contracts
{
    public interface INewsService
    {
        Task<IEnumerable<ArticleDto>> GetAllCategorizedArticlesAsync(int? page, int? pageSize);
        Task<ArticleDto> GetArticleByIdAsync(string id);
        Task<Dictionary<string, string>> GetSourceCategoriesAsync();
        Task<bool> CheckArticleExistsAsync(string newsId);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<bool> AddCategoryAsync(AddOrUpdateCategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> UpdateCategoryAsync(int id, AddOrUpdateCategoryDto categoryDto);
        Task<IEnumerable<Article>> GetArticlesByCategoriesAsync(IEnumerable<CategoryDto> preferredCategories);
        byte[] GenerateArticlePdf(ArticleDto article);

    }
}
