namespace Ocuda.Promenade.Controllers.ViewModels.Locations.Volunteer
{
    public class FormSubmissionSuccessViewModel
    {
        public FormSubmissionSuccessViewModel(string LocationStub, string VolunteerType)
        {
            this.LocationStub = LocationStub;
            this.VolunteerType = VolunteerType;
        }

        public string LocationStub { get; set; }
        public string VolunteerType { get; set; }
    }
}