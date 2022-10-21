namespace Ocuda.Ops.Models.Interfaces
{
    public interface IRosterLocationMapping
    {
        int Id { get; set; }
        int IdInRoster { get; set; }

        int? MapToLocationId { get; set; }
        string Name { get; set; }
    }
}