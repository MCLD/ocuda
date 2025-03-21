using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksByMail.Data;
using BooksByMail.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksByMail.Services
{
    public class CustomerService
    {
        private readonly Context _context;
        public CustomerService(Context context)
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
