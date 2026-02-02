using Microsoft.EntityFrameworkCore;

namespace Ocuda.PolarisHelper
{
    public class PolarisContext : DbContext
    {
        public PolarisContext(DbContextOptions options) : base(options) { }
    }
}
