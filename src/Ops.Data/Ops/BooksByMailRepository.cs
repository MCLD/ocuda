using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class BooksByMailRepository : IBooksByMailRepository
    {
        private readonly OpsContext _context;

        public BooksByMailRepository(OpsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BooksByMailCustomer> GetByIdAsync(int id)
        {
            return await _context.BooksByMailCustomers
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<BooksByMailCustomer> GetByPatronIdAsync(int patronId)
        {
            var booksbymailcustomer = await _context.BooksByMailCustomers
                .AsNoTracking()
                .Include(_ => _.Comments)
                .Where(_ => _.PatronID == patronId)
                .FirstOrDefaultAsync();

            if (booksbymailcustomer?.Comments != null)
            {
                booksbymailcustomer.Comments = booksbymailcustomer.Comments.OrderByDescending(_ => _.CreatedAt).ToList();
            }

            return booksbymailcustomer;
        }

        public async Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer)
        {
            await _context.BooksByMailCustomers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task UpdateAsync(BooksByMailCustomer customer)
        {
            _context.BooksByMailCustomers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            await _context.BooksByMailComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }
    }
}