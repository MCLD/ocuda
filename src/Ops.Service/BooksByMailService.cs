using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;

namespace Ocuda.Ops.Service
{
    public class BooksByMailService : BaseService<BooksByMailService>, IBooksByMailService
    {
        private readonly IBooksByMailCommentRepository _booksByMailCommentRepository;
        private readonly IBooksByMailCustomerRepository _booksByMailCustomerRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public BooksByMailService(ILogger<BooksByMailService> logger,
            IHttpContextAccessor httpContextAccessor,
            IBooksByMailCommentRepository booksByMailCommentRepository,
            IBooksByMailCustomerRepository booksByMailCustomerRepository,
            IDateTimeProvider dateTimeProvider)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(booksByMailCommentRepository);
            ArgumentNullException.ThrowIfNull(booksByMailCustomerRepository);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);

            _booksByMailCommentRepository = booksByMailCommentRepository;
            _booksByMailCustomerRepository = booksByMailCustomerRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer)
        {
            ArgumentNullException.ThrowIfNull(customer);

            customer.CreatedAt = _dateTimeProvider.Now;
            customer.CreatedBy = GetCurrentUserId();

            await _booksByMailCustomerRepository.AddAsync(customer);
            await _booksByMailCustomerRepository.SaveAsync();

            return customer;
        }

        public async Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);

            comment.CreatedAt = _dateTimeProvider.Now;
            comment.CreatedBy = GetCurrentUserId();

            await _booksByMailCommentRepository.AddAsync(comment);
            return comment;
        }

        public async Task<BooksByMailCustomer> GetAsync(int booksByMailCustomerId)
        {
            var customer = await _booksByMailCustomerRepository.FindAsync(booksByMailCustomerId);

            customer.Comments = await _booksByMailCommentRepository
                .GetAllAsync(booksByMailCustomerId);

            return customer;
        }

        public async Task<BooksByMailCustomer> UpdateCustomerAsync(BooksByMailCustomer customer)
        {
            ArgumentNullException.ThrowIfNull(customer);

            customer.UpdatedAt = _dateTimeProvider.Now;
            customer.UpdatedBy = GetCurrentUserId();

            _booksByMailCustomerRepository.Update(customer);
            await _booksByMailCustomerRepository.SaveAsync();

            return customer;
        }
    }
}