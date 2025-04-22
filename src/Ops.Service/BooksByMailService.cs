using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class BooksByMailService : BaseService<BooksByMailService>, IBooksByMailService
    {
        private readonly IBooksByMailRepository _booksByMailRepository;

        public BooksByMailService(ILogger<BooksByMailService> logger,
            IHttpContextAccessor httpContextAccessor,
            IBooksByMailRepository booksByMailRepository)
            : base(logger, httpContextAccessor)
        {
            _booksByMailRepository = booksByMailRepository
                ?? throw new ArgumentNullException(nameof(booksByMailRepository));
        }

        public Task<BooksByMailCustomer> FindAsync(int id)
        {
            return _booksByMailRepository.FindAsync(id);
        }

        public Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId)
        {
            return _booksByMailRepository.GetByCustomerLookupIdAsync(customerLookupId);
        }

        public async Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));

            await _booksByMailRepository.AddAsync(customer);
            await _booksByMailRepository.SaveAsync();

            return customer;
        }

        public void Update(BooksByMailCustomer customer)
        {
            _booksByMailRepository.Update(customer);
        }

        public Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            return _booksByMailRepository.AddCommentAsync(comment);
        }
    }
}