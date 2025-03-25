using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class BooksByMailRepository : IBooksByMailRepository
    {
        private readonly BooksByMailContext _context;

        public BooksByMailRepository(BooksByMailContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BooksByMailCustomer> GetByIdAsync(int id)
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<BooksByMailCustomer> GetByPatronIdAsync(int patronId)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Include(_ => _.Comments)
                .Where(_ => _.PatronID == patronId)
                .FirstOrDefaultAsync();

            if (customer?.Comments != null)
            {
                customer.Comments = customer.Comments.OrderByDescending(_ => _.CreatedAt).ToList();
            }

            return customer;
        }

        public async Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task UpdateAsync(BooksByMailCustomer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }
    }
}
