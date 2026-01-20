using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Represents the various units of measure used in the inventory system.
    /// </summary>    
    public enum UnitOfMeasure
    {
        Piece = 1,      // Individual items
        Box = 2,        // Box/carton
        Pallet = 3,     // Pallet
        Kilogram = 10,  // Weight
        Gram = 11,
        Pound = 12,
        Liter = 20,     // Volume
        Milliliter = 21,
        Gallon = 22,
        Meter = 30,     // Length
        Centimeter = 31,
        Foot = 32,
        SquareMeter = 40, // Area
        CubicMeter = 50   // Volume (cubic)
    }
}
