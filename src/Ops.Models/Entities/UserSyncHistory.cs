namespace Ocuda.Ops.Models.Entities
{
    public class UserSyncHistory : Abstract.BaseEntity
    {
        public int AddedUsers { get; set; }
        public int DeletedUsers { get; set; }
        public string Log { get; set; }
        public int TotalRecords { get; set; }
        public int UndeletedUsers { get; set; }
        public int UpdatedUsers { get; set; }
    }
}