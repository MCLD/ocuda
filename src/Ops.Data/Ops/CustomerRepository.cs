using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;
using Dapper;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Data.Ops
{
    public class CustomerRepository : ICustomerRepository
    {
        private const string PolarisDbCSName = "polarisdb";
        private static readonly int BooksByMailPatronID = 5;

        private static readonly string IdParam = "@id";
        private static readonly string SearchParam = "@search";
        private static readonly string SkipParam = "@skip";
        private static readonly string TakeParam = "@take";

        private readonly IConfiguration _config;

        public CustomerRepository(IConfiguration configuration)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(PolarisPatronFilter filter)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var dataQuery = $"SELECT PT.PatronID AS {nameof(Customer.PatronID)}," +
                                $" PT.Barcode AS {nameof(Customer.Barcode)}," +
                                $" PT.LastActivityDate AS {nameof(Customer.LastActivityDate)}," +
                                $" PTR.NameFirst AS {nameof(Customer.NameFirst)}," +
                                $" PTR.NameLast AS {nameof(Customer.NameLast)}" +
                                " FROM polarisdb.dbo.Patrons AS PT WITH (NOLOCK)" +
                                " JOIN polarisdb.dbo.PatronRegistration AS PTR WITH (NOLOCK)" +
                                " ON PT.PatronID = PTR.PatronID";

            var whereClause = $" WHERE PT.PatronCodeID = {BooksByMailPatronID}";
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                whereClause += $" AND (CONCAT(PTR.NameFirst , ' ' , PTR.NameLast) LIKE '%' + {SearchParam} + '%'" +
                                    $" OR PT.Barcode LIKE '%' + {SearchParam} + '%')";
            }

            dataQuery += whereClause;

            var countQuery = $"SELECT COUNT(*) FROM ({dataQuery}) AS count";

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
                " FROM polarisdb.dbo.Patrons AS PT WITH (NOLOCK)" +
                " JOIN polarisdb.dbo.PatronRegistration AS PTR WITH (NOLOCK)" +
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
                " FROM [polarisdb].[dbo].[ItemCheckouts] AS IC WITH (NOLOCK)" +
                " JOIN [polarisdb].[dbo].[ItemRecordDetails] AS IRD WITH (NOLOCK)" +
                " ON IC.ItemRecordID = IRD.ItemRecordID" +
                " JOIN [polarisdb].[dbo].[CircItemRecords] AS CIR WITH (NOLOCK)" +
                " ON IC.ItemRecordID = CIR.ItemRecordID" +
                " JOIN [polarisdb].[dbo].[BibliographicRecords] AS BR WITH (NOLOCK)" +
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
                " FROM [polarisdb].[dbo].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
                $" WHERE PRH.PatronID = {IdParam}";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, patronID);

            return await db.ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(PolarisItemFilter filter)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            var dataQuery = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
                $" BR.BrowseTitle AS {nameof(Material.Title)}," +
                $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(Material.Category)}," +
                $" PRH.CheckOutDate AS {nameof(Material.CheckoutDate)}" +
                " FROM [polarisdb].[dbo].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
                " JOIN [polarisdb].[dbo].[ItemRecordDetails] AS IRD WITH (NOLOCK)" +
                " ON PRH.ItemRecordID = IRD.ItemRecordID" +
                " JOIN [polarisdb].[dbo].[CircItemRecords] AS CIR WITH (NOLOCK)" +
                " ON PRH.ItemRecordID = CIR.ItemRecordID" +
                " JOIN [polarisdb].[dbo].[BibliographicRecords] AS BR WITH (NOLOCK)" +
                " ON CIR.AssociatedBibRecordID = BR.BibliographicRecordID";

            var whereClause = $" WHERE PRH.PatronID = {IdParam}";
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                whereClause += $" AND (BR.BrowseTitle LIKE '%' + {SearchParam} + '%'" +
                                $" OR BR.BrowseAuthor LIKE '%' + {SearchParam} + '%')";
            }

            dataQuery += whereClause;

            var countQuery = $"SELECT COUNT(*) FROM ({dataQuery}) AS count";

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
                " FROM [polarisdb].[dbo].[SysHoldRequests] AS SHR WITH (NOLOCK)" +
                " JOIN [polarisdb].[dbo].[SysHoldStatuses] AS SHS WITH (NOLOCK)" +
                " ON SHR.SysHoldStatusID = SHS.SysHoldStatusID" +
                " JOIN [polarisdb].[dbo].[BibliographicRecords] AS BR WITH (NOLOCK)" +
                " ON SHR.BibliographicRecordID = BR.BibliographicRecordID" +
                $" WHERE SHR.PatronID = {IdParam}";

            var parameters = new DynamicParameters();
            parameters.Add(IdParam, patronID);

            return (await db.QueryAsync<Material>(query, parameters)).ToList();
        }
    }
}
