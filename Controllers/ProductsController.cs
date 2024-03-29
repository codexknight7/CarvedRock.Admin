using CarvedRock.Admin.Logic;
using CarvedRock.Admin.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarvedRock.Admin.Controllers;
[Authorize]
public class ProductsController : Controller
{
    private readonly IProductLogic _logic;
    private readonly ILogger<ProductsController> _logger;

    //public List<ProductModel> Products {get;set;}
    public ProductsController(IProductLogic logic, ILogger<ProductsController> logger)
    {
        //Products = GetSampleProducts();
        _logic = logic;
        _logger = logger;
    }
    public async Task<IActionResult> Index()
    {
        var products = await _logic.GetAllProducts();
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _logic.GetProductById(id);
        if (product == null)
        {
            _logger.LogInformation("Details not found for id {id}", id);
            return View("NotFound");
        }
        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _logic.InitializeProductModel();
        return View(model);
    }

    // POST: ProductsData/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductModel product)
    {
        if (!ModelState.IsValid)
        {
            await _logic.GetAvailableCategories(product);
            return View(product);
        }
        try
        {
            await _logic.AddNewProduct(product);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException valEx)
        {
            var results = new ValidationResult(valEx.Errors);
            results.AddToModelState(ModelState, null);
            await _logic.GetAvailableCategories(product);
            return View(product);
        }
    }

    // GET: ProductsData/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            _logger.LogInformation("No id passed for edit");
            return View("NotFound");
        }

        var productModel = await _logic.GetProductById(id.Value);
        if (productModel == null)
        {
            _logger.LogInformation("Edit details not found for id {id}", id);
            return View("NotFound");
        }

        await _logic.GetAvailableCategories(productModel);
        return View(productModel);
    }

    // POST: ProductsData/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductModel product)
    {
        if (id != product.Id)
        {
            _logger.LogInformation("Id mismatch in passed information. " +
                "Id value {id} did not match model value of {productId}",
                id, product.Id);
            return View("NotFound");
        }

        if (!ModelState.IsValid)
        {
            await _logic.GetAvailableCategories(product);
            return View(product);
        }
        try
        {
            await _logic.UpdateProduct(product);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException valEx)
        {
            var results = new ValidationResult(valEx.Errors);
            results.AddToModelState(ModelState, null);
            await _logic.GetAvailableCategories(product);
            return View(product);
        }
    }

    // GET: ProductsData/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            _logger.LogInformation("No id value was passed for deletion.");
            return View("NotFound");
        }

        var productModel = await _logic.GetProductById(id.Value);
        if (productModel == null)
        {
            _logger.LogInformation("Id to be deleted ({id}) does not exist in database.",
                id);
            return View("NotFound");
        }

        return View(productModel);
    }

    // POST: ProductsData/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _logic.RemoveProduct(id);
        return RedirectToAction(nameof(Index));
    }
    // private List<ProductModel> GetSampleProducts()
    // {
    //     return new List<ProductModel> 
    //     {
    //         new ProductModel {Id = 1, Name = "Trailblazer", Price = 69.99M, IsActive = true,
    //             Description = "Great support in this high-top to take you to great heights and trails." },
    //         new ProductModel {Id = 2, Name = "Coastliner", Price = 49.99M, IsActive = true,
    //             Description = "Easy in and out with this lightweight but rugged shoe with great ventilation to get your around shores, beaches, and boats."},
    //         new ProductModel {Id = 3, Name = "Woodsman", Price = 64.99M, IsActive = true,
    //             Description = "All the insulation and support you need when wandering the rugged trails of the woods and backcountry." },
    //         new ProductModel {Id = 4, Name = "Basecamp", Price = 249.99M, IsActive = true,
    //             Description = "Great insulation and plenty of room for 2 in this spacious but highly-portable tent."},                            
    //     };
    // }
}