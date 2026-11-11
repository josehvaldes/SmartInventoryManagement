using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.IntegrationTests.Products
{
    public class ProductData
    {
        public static readonly CreateProductRequest Product1 = new CreateProductRequest
        {
            SKU = "ITM-USB-C-001",
            Name = "USB-C Charging Cable 2m",
            Description = "Braided USB-C cable for daily use",
            Category = ProductCategory.Electronics.ToString(),
            UnitOfMeasure = UnitOfMeasure.Piece.ToString(),
            MinimumStockLevel = 5,
            ReorderPoint = 8,
            ReorderQuantity = 20,
            UnitCost = 9.99m,
        };

        public static readonly List<Product> Products = new List<Product>
        {
            Product.Create(
                sku: "ITM-USB-C-001",
                name: "USB-C Charging Cable 2m",
                description: "Braided USB-C cable for daily use",
                category: ProductCategory.Electronics,
                unitOfMeasure: UnitOfMeasure.Piece,
                minimumStockLevel: 5,
                reorderPoint: 8,
                reorderQuantity: 20,
                unitCost: 9.99m,
                createdBy: "integration-test"),
            Product.Create(
                sku: "ITM-GLOVES-001",
                name: "Safety Gloves",
                description: "Nitrile disposable safety gloves",
                category: ProductCategory.Safety,
                unitOfMeasure: UnitOfMeasure.Box,
                minimumStockLevel: 3,
                reorderPoint: 5,
                reorderQuantity: 10,
                unitCost: 14.50m,
                createdBy: "integration-test")
        };
    }
}
