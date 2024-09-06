using Ocuda.Utility.Models;

namespace Ocuda.Utility.Keys
{
    public static class Template
    {
        public static readonly KeyWithDescription LocationName
            = new(nameof(LocationName), "The name of the selected location");
    }
}