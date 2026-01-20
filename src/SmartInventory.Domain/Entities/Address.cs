using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// Represents a physical address.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Street: string (required, max 200 chars)
        /// City: string (required, max 100 chars)
        /// State: string (optional, max 100 chars)
        /// PostalCode: string (optional, max 20 chars)
        /// Country: string (required, max 100 chars)
        /// </summary>

        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Full address as a single string.
        /// </summary>
        /// <returns>
        /// {street}, {city}, {state}, {postalCode}, {country}
        /// </returns>
        public string GetFullAddress()
        {
            StringBuilder fullAddress = new StringBuilder();
            fullAddress.Append(Street);
            fullAddress.Append(", ");
            fullAddress.Append(City);
            if (!string.IsNullOrWhiteSpace(State))
            {
                fullAddress.Append(", ");
                fullAddress.Append(State);
            }
            if (!string.IsNullOrWhiteSpace(PostalCode))
            {
                fullAddress.Append(", ");
                fullAddress.Append(PostalCode);
            }
            fullAddress.Append(", ");
            fullAddress.Append(Country);
            return fullAddress.ToString();
        }

        public bool Equals(Address other)
        {
            if (other == null) return false;
            return Street == other.Street &&
                   City == other.City &&
                   State == other.State &&
                   PostalCode == other.PostalCode &&
                   Country == other.Country;
        }
    }
}
