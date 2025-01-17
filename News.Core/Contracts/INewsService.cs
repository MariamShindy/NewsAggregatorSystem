﻿using News.Core.Dtos;
using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface INewsService
    {
        //Task<string> GetAllNewsAsync(int? page, int? pageSize);
        Task<IEnumerable<ArticleDto>> GetAllCategorizedArticlesAsync(int? page, int? pageSize);
        Task<ArticleDto> GetArticleByIdAsync(string id);
        Task<Dictionary<string, string>> GetSourceCategoriesAsync();
        Task<bool> CheckArticleExistsAsync(string newsId);
        //Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<bool> AddCategoryAsync(AddOrUpdateCategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> UpdateCategoryAsync(int id, AddOrUpdateCategoryDto categoryDto);
        Task<IEnumerable<Article>> GetArticlesByCategoriesAsync(IEnumerable<CategoryDto> preferredCategories);
    }
}
