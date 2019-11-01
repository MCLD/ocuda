using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ISectionRepository : IRepository<Section, int>
    {
        Section GetSectionByStub(string stub);
        Section GetSectionByName(string name);
        List<Section> GetAllSections();
    }
}
