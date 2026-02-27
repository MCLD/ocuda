using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class SubjectService : BaseService<SubjectService>, ISubjectService
    {
        private readonly IEmediaSubjectRepository _emediaSubjectRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ISubjectTextRepository _subjectTextRepository;

        public SubjectService(ILogger<SubjectService> logger,
            IHttpContextAccessor httpContextAccessor,
            ISubjectRepository subjectRepository,
            ISubjectTextRepository subjectTextRepository,
            IEmediaSubjectRepository emediaSubjectRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(emediaSubjectRepository);
            ArgumentNullException.ThrowIfNull(subjectRepository);
            ArgumentNullException.ThrowIfNull(subjectTextRepository);

            _emediaSubjectRepository = emediaSubjectRepository;
            _subjectRepository = subjectRepository;
            _subjectTextRepository = subjectTextRepository;
        }

        public async Task<Subject> CreateAsync(Subject subject)
        {
            ArgumentNullException.ThrowIfNull(subject);
            subject.Name = subject.Name?.Trim();

            await _subjectRepository.AddAsync(subject);
            await _subjectRepository.SaveAsync();

            return subject;
        }

        public async Task DeleteAsync(int id)
        {
            var subject = await _subjectRepository.FindAsync(id)
                ?? throw new OcudaException("Subject does not exist.");

            var subjectTexts = await _subjectTextRepository.GetAllForSubjectAsync(subject.Id);

            var emediaCategories = await _emediaSubjectRepository
                .GetBySubjectIdAsync(subject.Id);

            _subjectTextRepository.RemoveRange(subjectTexts);
            _emediaSubjectRepository.RemoveRange(emediaCategories);
            _subjectRepository.Remove(subject);

            await _subjectRepository.SaveAsync();
        }

        public async Task<Subject> EditAsync(Subject subject)
        {
            ArgumentNullException.ThrowIfNull(subject);
            var currentSubject = await _subjectRepository.FindAsync(subject.Id);

            currentSubject.Name = subject.Name?.Trim();

            _subjectRepository.Update(currentSubject);
            await _subjectRepository.SaveAsync();

            return currentSubject;
        }

        public async Task<ICollection<Subject>> GetAllAsync()
        {
            return await _subjectRepository.GetAllAsync();
        }

        public async Task<Subject> GetByIdAsync(int id)
        {
            return await _subjectRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<Subject>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _subjectRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<string>> GetSubjectEmediasAsync(int id)
        {
            return await _emediaSubjectRepository.GetEmediasForSubjectAsync(id);
        }

        public async Task<ICollection<string>> GetSubjectLanguagesAsync(int id)
        {
            return await _subjectTextRepository.GetUsedLanguagesForSubjectAsync(id);
        }

        public async Task<SubjectText> GetTextBySubjectAndLanguageAsync(int subjectId,
                            int languageId)
        {
            return await _subjectTextRepository.GetBySubjectAndLanguageAsync(subjectId,
                languageId);
        }

        public async Task SetSubjectTextAsync(SubjectText subjectText)
        {
            ArgumentNullException.ThrowIfNull(subjectText);

            var currentText = await _subjectTextRepository
                .GetBySubjectAndLanguageAsync(subjectText.SubjectId, subjectText.LanguageId);

            if (currentText == null)
            {
                subjectText.Text = subjectText.Text?.Trim();

                await _subjectTextRepository.AddAsync(subjectText);
            }
            else
            {
                currentText.Text = subjectText.Text?.Trim();

                _subjectTextRepository.Update(subjectText);
            }

            await _subjectTextRepository.SaveAsync();
        }
    }
}