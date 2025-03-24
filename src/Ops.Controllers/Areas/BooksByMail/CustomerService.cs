using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BooksByMail.Models;
using BooksByMail.QueryFilters;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BooksByMail.Services
{
    public class CustomerService
    {
        public static readonly int BooksByMailPatronID = 5;

        public static readonly string IdParam = "@id";
        public static readonly string SearchParam = "@search";
        public static readonly string SkipParam = "@skip";
        public static readonly string TakeParam = "@take";

        private const string PolarisDbCSName = "polarisdb";
        private readonly IConfiguration _config;

        public CustomerService(IConfiguration configuration)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(
            PolarisPatronFilter filter)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var dataQuery = $"SELECT PT.PatronID AS {nameof(Customer.PatronID)}," +
                                $" PT.Barcode AS {nameof(Customer.Barcode)}," +
                                $" PT.LastActivityDate AS {nameof(Customer.LastActivityDate)}," +
                                $" PTR.NameFirst AS {nameof(Customer.NameFirst)}," +
                                $" PTR.NameLast AS {nameof(Customer.NameLast)}" +
                                " FROM Polaris.Polaris.Patrons AS PT WITH (NOLOCK)" +
                                " JOIN Polaris.Polaris.PatronRegistration AS PTR WITH (NOLOCK)" +
                                " ON PT.PatronID = PTR.PatronID";

            // Select only Books by Mail customers
            var whereClause = $" WHERE PT.PatronCodeID = {BooksByMailPatronID}";
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                whereClause += $" AND (CONCAT(PTR.NameFirst , ' ' , PTR.NameLast) LIKE '%' + {SearchParam} + '%'" +
                                    $" OR PT.Barcode LIKE '%' + {SearchParam} + '%')";
            }

            dataQuery += whereClause;

            var countQuery = $"SELECT COUNT(*) FROM ({dataQuery}) as count";

            dataQuery += $" ORDER BY {filter.OrderBy}";
            if (filter.OrderDesc)
            {
                dataQuery += " DESC";
            }

            dataQuery += $" OFFSET {SkipParam} ROWS FETCH NEXT {TakeParam} ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add(SearchParam, filter.Search);
            parameters.Add(SkipParam, filter.Skip);
            parameters.Add(TakeParam, filter.Take);

            return new DataWithCount<List<Customer>>
            {
                Count = await db.ExecuteScalarAsync<int>(countQuery, parameters),
                Data = (await db.QueryAsync<Customer>(dataQuery, parameters)).ToList()
            };
        }

        public async Task<Customer> GetPatronInfoAsync(int patronID)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var query = $"SELECT PT.PatronID AS {nameof(Customer.PatronID)}," +
                $" PT.Barcode AS {nameof(Customer.Barcode)}," +
                $" PT.LastActivityDate AS {nameof(Customer.LastActivityDate)}," +
                $" PTR.NameFirst AS {nameof(Customer.NameFirst)}," +
                $" PTR.NameLast AS {nameof(Customer.NameLast)}" +
                " FROM Polaris.Polaris.Patrons AS PT WITH (NOLOCK)" +
                " JOIN Polaris.Polaris.PatronRegistration AS PTR WITH (NOLOCK)" +
                " ON PT.PatronID = PTR.PatronID" +
                $" WHERE PT.PatronID = {IdParam}";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, patronID);

            return await db.QueryFirstOrDefaultAsync<Customer>(query, parameters);
        }

        public async Task<List<Material>> GetPatronCheckoutsAsync(int patronID)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var query = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
                $" BR.BrowseTitle AS {nameof(Material.Title)}," +
                $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(Material.Category)}," +
                $" IC.DueDate AS {nameof(Material.DueDate)}" +
                " FROM [Polaris].[Polaris].[ItemCheckouts] AS IC WITH (NOLOCK)" +
                " JOIN [Polaris].[Polaris].[ItemRecordDetails] AS IRD WITH (NOLOCK)" +
                " ON IC.ItemRecordID = IRD.ItemRecordID" +
                " JOIN [Polaris].[Polaris].[CircItemRecords] AS CIR WITH (NOLOCK)" +
                " ON IC.ItemRecordID = CIR.ItemRecordID" +
                " JOIN[Polaris].[Polaris].[BibliographicRecords] AS BR WITH (NOLOCK)" +
                " ON CIR.AssociatedBibRecordID = BR.BibliographicRecordID" +
                $" WHERE IC.PatronID = {IdParam}";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, patronID);

            return (await db.QueryAsync<Material>(query, parameters)).ToList();
        }

        public async Task<int> GetPatronHistoryCountAsync(int patronID)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var query = "SELECT COUNT(*)" +
                " FROM [Polaris].[Polaris].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
                $" WHERE PRH.PatronID = {IdParam}";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, patronID);

            return await db.ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(
            PolarisItemFilter filter)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var dataQuery = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
                $" BR.BrowseTitle AS {nameof(Material.Title)}," +
                $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(Material.Category)}," +
                $" PRH.CheckOutDate AS {nameof(Material.CheckoutDate)}" +
                " FROM [Polaris].[Polaris].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
                " JOIN [Polaris].[Polaris].[ItemRecordDetails] AS IRD WITH (NOLOCK)" +
                " ON PRH.ItemRecordID = IRD.ItemRecordID" +
                " JOIN [Polaris].[Polaris].[CircItemRecords] AS CIR WITH (NOLOCK)" +
                " ON PRH.ItemRecordID = CIR.ItemRecordID" +
                " JOIN[Polaris].[Polaris].[BibliographicRecords] AS BR WITH (NOLOCK)" +
                " ON CIR.AssociatedBibRecordID = BR.BibliographicRecordID";

            var whereClause = $" WHERE PRH.PatronID = {IdParam}";
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                whereClause += $" AND (BR.BrowseTitle LIKE '%' + {SearchParam} + '%'" +
                                $" OR BR.BrowseAuthor LIKE '%' + {SearchParam} + '%')";
            }

            dataQuery += whereClause;

            var countQuery = $"SELECT COUNT(*) FROM ({dataQuery}) as count";

            dataQuery += $" ORDER BY {filter.OrderBy}";
            if (filter.OrderDesc)
            {
                dataQuery += " DESC";
            }

            dataQuery += $" OFFSET {SkipParam} ROWS FETCH NEXT {TakeParam} ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, filter.PatronID);
            parameters.Add(SearchParam, filter.Search);
            parameters.Add(SkipParam, filter.Skip);
            parameters.Add(TakeParam, filter.Take);

            return new DataWithCount<List<Material>>
            {
                Count = await db.ExecuteScalarAsync<int>(countQuery, parameters),
                Data = (await db.QueryAsync<Material>(dataQuery, parameters)).ToList()
            };
        }

        public async Task<List<Material>> GetPatronHoldsAsync(int patronID)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var query = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
                $" BR.BrowseTitle AS {nameof(Material.Title)}," +
                $" SHS.Name AS {nameof(Material.HoldStatus)}" +
                " FROM [Polaris].[Polaris].[SysHoldRequests] AS SHR WITH (NOLOCK)" +
                " JOIN [Polaris].[Polaris].[SysHoldStatuses] AS SHS WITH (NOLOCK)" +
                " ON SHR.SysHoldStatusID = SHS.SysHoldStatusID" +
                " JOIN [Polaris].[Polaris].[BibliographicRecords] AS BR WITH (NOLOCK)" +
                " ON SHR.BibliographicRecordID = BR.BibliographicRecordID" +
                $" WHERE SHR.PatronID = {IdParam}";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, patronID);

            return (await db.QueryAsync<Material>(query, parameters)).ToList();
        }
    }
}
