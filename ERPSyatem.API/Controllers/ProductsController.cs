using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Products.Product.Read)]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Products.Product.Read)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        [Authorize(Policy =Permissions.Products.Product.Manage)]
      //  [Authorize(Policy = Permissions.Products.Create)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var id = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id}")]
        [Authorize(Policy =Permissions.Products.Product.Manage)]
       // [Authorize(Policy = Permissions.Products.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            await _productService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = Permissions.Products.Delete)]
        [Authorize(Policy =Permissions.Products.Product.Manage)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }
}
