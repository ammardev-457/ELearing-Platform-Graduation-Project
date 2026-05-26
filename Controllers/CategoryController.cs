using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }


        [HttpGet("Categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await unitOfWork.Categories.GetAllCategoriesAsync();
            return Ok(result.Select(c => new
            {
                id = c.Id,
                name = c.Name
            }));
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            var result = await unitOfWork.Categories.GetByIdAsync(categoryId);
            return Ok(new { id = result.Id, name = result.Name });
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("Add-Category/{categoryName:alpha}")]
        public async Task<IActionResult> AddCategory(string categoryName)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null)
                return Unauthorized("You do not have permission to add a new category.");

            var category = new Category { Name = categoryName };

            try
            {
                await unitOfWork.Categories.AddAsync(category);
                await unitOfWork.CompleteAsync();

                return CreatedAtAction(nameof(GetCategory), new { categoryId = category.Id }, category);
            }
            catch
            {
                return StatusCode(500, "An error occurred while adding the category. Please try again.");
            }

        }

        [Authorize(Roles ="Admin")]
        [HttpPut("update/{categoryId}/{categoryName:alpha}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, string categoryName)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null)
                return Unauthorized("You do not have permission to update the category.");

            var category = await unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
                return BadRequest("Invalid Category");


            category.Name = categoryName ?? category.Name;

            try
            {
                unitOfWork.Categories.Update(category);
                await unitOfWork.CompleteAsync();
                return CreatedAtAction(nameof(GetCategory), new { categoryId = category.Id }, category);
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the category. Please try again.");
            }
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null)
                return Unauthorized("You do not have permission to delete the category.");

            var category = await unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
                return BadRequest("Invalid Category");

            try
            {
                unitOfWork.Categories.Remove(category);
                await unitOfWork.CompleteAsync();
                return Ok("Category deleted successfully.");
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the category. Please try again.");
            }
        }

    }
}
