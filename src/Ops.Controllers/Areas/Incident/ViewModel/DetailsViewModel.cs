using System.Collections.Generic;
using System.Linq;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class DetailsViewModel : IncidentViewModelBase
    {
        public DetailsViewModel()
        {
            Heading = "Incident Details";
        }

        public bool CanAdd { get; set; }
        public bool CanHide { get; set; }
        public Models.Entities.Incident Incident { get; set; }
        public IDictionary<int, string> IncidentTypes { get; set; }
        public IDictionary<int, string> Locations { get; set; }

        public IList<IncidentStaffPublic> Participants
        {
            get
            {
                var staffs = Incident
                    .Staffs?
                    .Where(_ => _.IncidentParticipantType
                        == Models.Entities.IncidentParticipantType.Affected)
                    .Select(_ => new IncidentStaffPublic { Id = _.Id, Name = _.User.Name })
                    ?? Enumerable.Empty<IncidentStaffPublic>();

                var participants = Incident
                    .Participants?
                    .Where(_ => _.IncidentParticipantType
                        == Models.Entities.IncidentParticipantType.Affected)
                    .Select(_ => new IncidentStaffPublic
                    {
                        Barcode = _.Barcode,
                        Name = _.Name,
                        Description = _.Description
                    })
                    ?? Enumerable.Empty<IncidentStaffPublic>();

                return staffs.Union(participants).OrderBy(_ => _.Name).ToList();
            }
        }

        public IList<IncidentStaffPublic> Witnesses
        {
            get
            {
                var staffs = Incident
                    .Staffs?
                    .Where(_ => _.IncidentParticipantType
                        == Models.Entities.IncidentParticipantType.Witness)
                    .Select(_ => new IncidentStaffPublic { Id = _.Id, Name = _.User.Name })
                    ?? Enumerable.Empty<IncidentStaffPublic>();

                var participants = Incident
                    .Participants?
                    .Where(_ => _.IncidentParticipantType
                        == Models.Entities.IncidentParticipantType.Witness)
                    .Select(_ => new IncidentStaffPublic
                    {
                        Barcode = _.Barcode,
                        Name = _.Name,
                        Description = _.Description
                    })
                    ?? Enumerable.Empty<IncidentStaffPublic>();

                return staffs.Union(participants).OrderBy(_ => _.Name).ToList();
            }
        }
    }
}