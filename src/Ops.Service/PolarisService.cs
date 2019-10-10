using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Service.Models;
using Ocuda.Ops.Service.Filters;

namespace Ops.Service
{
    public class PolarisService
    {
        public static readonly int BooksByMailPatronID = 5;

        public static readonly string IdParam = "@id";
        public static readonly string SearchParam = "@search";
        public static readonly string SkipParam = "@skip";
        public static readonly string TakeParam = "@take";

        private static readonly string PolarisDbCSName = "polarisdb";
        private readonly IConfiguration _config;

        public PolarisService(IConfiguration configuration)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Ocuda.Ops.Models.DataWithCount<List<PolarisPatron>>> GetPaginatedPatronListAsync(
            PolarisPatronFilter filter)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName)))
            {
                var dataQuery = $"SELECT PT.PatronID AS {nameof(PolarisPatron.PatronID)}," +
                                    $" PT.Barcode AS {nameof(PolarisPatron.Barcode)}," +
                                    $" PT.LastActivityDate AS {nameof(PolarisPatron.LastActivityDate)}," +
                                    $" PTR.NameFirst AS {nameof(PolarisPatron.NameFirst)}," +
                                    $" PTR.NameLast AS {nameof(PolarisPatron.NameLast)}" +
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

                if (filter.OnlyNoCheckouts)
                {
                    dataQuery = $"SELECT  t1.{nameof(PolarisPatron.PatronID)}," +
                                    $"t1.{nameof(PolarisPatron.Barcode)}," +
                                    $"t1.{nameof(PolarisPatron.LastActivityDate)}," +
                                    $"t1.{nameof(PolarisPatron.NameFirst)}," +
                                    $"t1.{nameof(PolarisPatron.NameLast)}" +
                                    $" FROM(" + dataQuery +
                                    $") as t1 left join(Select distinct PCO.PatronID from Polaris.Polaris.ItemCheckouts AS PCO With (NOLOCK)) t2 on t1." +
                                    nameof(PolarisPatron.PatronID) +
                                    " = t2.PatronID WHERE t2.PatronID IS NULL";
                }

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

                return new Ocuda.Ops.Models.DataWithCount<List<PolarisPatron>>
                {
                    Count = await db.ExecuteScalarAsync<int>(countQuery, parameters),
                    Data = (await db.QueryAsync<PolarisPatron>(dataQuery, parameters)).ToList()
                };
            }
        }

        public async Task<PolarisPatron> GetPatronInfoAsync(int patronID)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName)))
            {
                var query = $"SELECT PT.PatronID AS {nameof(PolarisPatron.PatronID)}," +
                    $" PT.Barcode AS {nameof(PolarisPatron.Barcode)}," +
                    $" PT.LastActivityDate AS {nameof(PolarisPatron.LastActivityDate)}," +
                    $" PTR.NameFirst AS {nameof(PolarisPatron.NameFirst)}," +
                    $" PTR.NameLast AS {nameof(PolarisPatron.NameLast)}" +
                    " FROM Polaris.Polaris.Patrons AS PT WITH (NOLOCK)" +
                    " JOIN Polaris.Polaris.PatronRegistration AS PTR WITH (NOLOCK)" +
                    " ON PT.PatronID = PTR.PatronID" +
                    $" WHERE PT.PatronID = {IdParam}";

                var parameters = new DynamicParameters();
                parameters.Add(IdParam, patronID);

                return await db.QueryFirstOrDefaultAsync<PolarisPatron>(query, parameters);
            }
        }

        public async Task<List<PolarisItem>> GetPatronCheckoutsAsync(int patronID)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName)))
            {
                var query = $"SELECT BR.BrowseAuthor AS {nameof(PolarisItem.Author)}," +
                    $" BR.BrowseTitle AS {nameof(PolarisItem.Title)}," +
                    $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(PolarisItem.Category)}," +
                    $" IC.DueDate AS {nameof(PolarisItem.DueDate)}" +
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

                return (await db.QueryAsync<PolarisItem>(query, parameters)).ToList();
            }
        }

        public async Task<int> GetPatronHistoryCountAsync(int patronID)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName)))
            {
                var query = "SELECT COUNT(*)" +
                    " FROM [Polaris].[Polaris].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
                    $" WHERE PRH.PatronID = {IdParam}";

                var parameters = new DynamicParameters();
                parameters.Add(IdParam, patronID);

                return await db.ExecuteScalarAsync<int>(query, parameters);
            }
        }

        public async Task<Ocuda.Ops.Models.DataWithCount<List<PolarisItem>>> GetPaginatedPatronHistoryAsync(
            PolarisItemFilter filter)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName)))
            {
                var dataQuery = $"SELECT BR.BrowseAuthor AS {nameof(PolarisItem.Author)}," +
                    $" BR.BrowseTitle AS {nameof(PolarisItem.Title)}," +
                    $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(PolarisItem.Category)}," +
                    $" PRH.CheckOutDate AS {nameof(PolarisItem.CheckoutDate)}" +
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


                return new Ocuda.Ops.Models.DataWithCount<List<PolarisItem>>
                {
                    Count = await db.ExecuteScalarAsync<int>(countQuery, parameters),
                    Data = (await db.QueryAsync<PolarisItem>(dataQuery, parameters)).ToList()
                };
            }
        }

        public async Task<List<PolarisItem>> GetPatronHoldsAsync(int patronID)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName)))
            {
                var query = $"SELECT BR.BrowseAuthor AS {nameof(PolarisItem.Author)}," +
                    $" BR.BrowseTitle AS {nameof(PolarisItem.Title)}," +
                    $" SHS.Name AS {nameof(PolarisItem.HoldStatus)}" +
                    " FROM [Polaris].[Polaris].[SysHoldRequests] AS SHR WITH (NOLOCK)" +
                    " JOIN [Polaris].[Polaris].[SysHoldStatuses] AS SHS WITH (NOLOCK)" +
                    " ON SHR.SysHoldStatusID = SHS.SysHoldStatusID" +
                    " JOIN [Polaris].[Polaris].[BibliographicRecords] AS BR WITH (NOLOCK)" +
                    " ON SHR.BibliographicRecordID = BR.BibliographicRecordID" +
                    $" WHERE SHR.PatronID = {IdParam}";

                var parameters = new DynamicParameters();
                parameters.Add(IdParam, patronID);

                return (await db.QueryAsync<PolarisItem>(query, parameters)).ToList();
            }
        }
    }
}
