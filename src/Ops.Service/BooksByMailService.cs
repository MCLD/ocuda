using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class BooksByMailService : IBooksByMailService
    {
        private readonly IBooksByMailRepository _repository;

        public BooksByMailService(IBooksByMailRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<BooksByMailCustomer> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId)
        {
            return _repository.GetByCustomerLookupIdAsync(customerLookupId);
        }

        public Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer)
        {
            return _repository.AddAsync(customer);
        }

        public Task UpdateAsync(BooksByMailCustomer customer)
        {
            return _repository.UpdateAsync(customer);
        }

        public Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            return _repository.AddCommentAsync(comment);
        }
    }
}