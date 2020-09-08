using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSR.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("SchoolContext")
        {
        }
        public DbSet<Function> Functions { set; get; }
    }
    public class Function
    {
        public int ID { get; set; }
    }
}
