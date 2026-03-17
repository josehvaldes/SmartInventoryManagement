using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class SeederException:Exception
    {
        public SeederException() { }
        public SeederException(string message) : base(message) { }
        public SeederException(string message, Exception innerException) : base(message, innerException) { }
    }
}
