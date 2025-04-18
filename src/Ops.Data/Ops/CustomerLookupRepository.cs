using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class CustomerLookupRepository : OpsRepository<OpsContext, BooksByMailCustomer, int>, ICustomerRepository
    {
        //private const string PolarisDbCSName = "polarisdb";
        //private static readonly int BooksByMailPatronID = 5;

        //private static readonly string IdParam = "@id";
        //private static readonly string SearchParam = "@search";
        //private static readonly string SkipParam = "@skip";
        //private static readonly string TakeParam = "@take";
        public CustomerLookupRepository(Repository<OpsContext> repositoryFacade,
            ILogger<CustomerLookupRepository> logger) : base(repositoryFacade, logger)
        {
        }


        public async Task<DataWithCount<List<Customer>>> GetPaginatedCustomerLookupListAsync(CustomerLookupFilter filter)
        {
            //using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            //var dataQuery = $"SELECT PT.PatronID AS {nameof(Customer.PatronID)}," +
            //                    $" PT.Barcode AS {nameof(Customer.Barcode)}," +
            //                    $" PT.LastActivityDate AS {nameof(Customer.LastActivityDate)}," +
            //                    $" PTR.NameFirst AS {nameof(Customer.NameFirst)}," +
            //                    $" PTR.NameLast AS {nameof(Customer.NameLast)}" +
            //                    " FROM polarisdb.dbo.Patrons AS PT WITH (NOLOCK)" +
            //                    " JOIN polarisdb.dbo.PatronRegistration AS PTR WITH (NOLOCK)" +
            //                    " ON PT.PatronID = PTR.PatronID";

            //var whereClause = $" WHERE PT.PatronCodeID = {BooksByMailPatronID}";
            //if (!string.IsNullOrWhiteSpace(filter.Search))
            //{
            //    whereClause += $" AND (CONCAT(PTR.NameFirst , ' ' , PTR.NameLast) LIKE '%' + {SearchParam} + '%'" +
            //                        $" OR PT.Barcode LIKE '%' + {SearchParam} + '%')";
            //}

            //dataQuery += whereClause;

            //var countQuery = $"SELECT COUNT(*) FROM ({dataQuery}) AS count";

            //dataQuery += $" ORDER BY {filter.OrderBy}";
            //if (filter.OrderDesc)
            //{
            //    dataQuery += " DESC";
            //}

            //dataQuery += $" OFFSET {SkipParam} ROWS FETCH NEXT {TakeParam} ROWS ONLY";

            //var parameters = new DynamicParameters();
            //parameters.Add(SearchParam, filter.Search);
            //parameters.Add(SkipParam, filter.Skip);
            //parameters.Add(TakeParam, filter.Take);

            //return new DataWithCount<List<Customer>>
            //{
            //    Count = await db.ExecuteScalarAsync<int>(countQuery, parameters),
            //    Data = (await db.QueryAsync<Customer>(dataQuery, parameters)).ToList()
            //};
            return await Task.FromResult(new DataWithCount<List<Customer>>
            {
                Count = 1,
                Data = new List<Customer>
            {
            new Customer
            {
                CustomerLookupID = 101,
                Barcode = "1234567890",
                LastActivityDate = DateTime.UtcNow.AddDays(-1),
                NameFirst = "Jane",
                NameLast = "Smith"
            }
            }
            });
        }

        public async Task<Customer> GetCustomerLookupInfoAsync(int customerLookupID)
        {
            //using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            //var query = $"SELECT PT.PatronID AS {nameof(Customer.PatronID)}," +
            //    $" PT.Barcode AS {nameof(Customer.Barcode)}," +
            //    $" PT.LastActivityDate AS {nameof(Customer.LastActivityDate)}," +
            //    $" PTR.NameFirst AS {nameof(Customer.NameFirst)}," +
            //    $" PTR.NameLast AS {nameof(Customer.NameLast)}" +
            //    " FROM polarisdb.dbo.Patrons AS PT WITH (NOLOCK)" +
            //    " JOIN polarisdb.dbo.PatronRegistration AS PTR WITH (NOLOCK)" +
            //    " ON PT.PatronID = PTR.PatronID" +
            //    $" WHERE PT.PatronID = {IdParam}";

            //var parameters = new DynamicParameters();
            //parameters.Add(IdParam, patronID);

            //return await db.QueryFirstOrDefaultAsync<Customer>(query, parameters);
            return await Task.FromResult(new Customer
            {
                CustomerLookupID = customerLookupID,
                Barcode = "1234567890",
                LastActivityDate = DateTime.UtcNow,
                NameFirst = "Jane",
                NameLast = "Smith"
            });
        }

        public async Task<List<Material>> GetCustomerLookupCheckoutsAsync(int customerLookupID)
        {
            //using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            //var query = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
            //    $" BR.BrowseTitle AS {nameof(Material.Title)}," +
            //    $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(Material.Category)}," +
            //    $" IC.DueDate AS {nameof(Material.DueDate)}" +
            //    " FROM [polarisdb].[dbo].[ItemCheckouts] AS IC WITH (NOLOCK)" +
            //    " JOIN [polarisdb].[dbo].[ItemRecordDetails] AS IRD WITH (NOLOCK)" +
            //    " ON IC.ItemRecordID = IRD.ItemRecordID" +
            //    " JOIN [polarisdb].[dbo].[CircItemRecords] AS CIR WITH (NOLOCK)" +
            //    " ON IC.ItemRecordID = CIR.ItemRecordID" +
            //    " JOIN [polarisdb].[dbo].[BibliographicRecords] AS BR WITH (NOLOCK)" +
            //    " ON CIR.AssociatedBibRecordID = BR.BibliographicRecordID" +
            //    $" WHERE IC.PatronID = {IdParam}";

            //var parameters = new DynamicParameters();
            //parameters.Add(IdParam, patronID);

            //return (await db.QueryAsync<Material>(query, parameters)).ToList();
            return await Task.FromResult(new List<Material>
            {
            new Material
            {
            Author = "Author One",
            Title = "Checkout Book One",
            Category = "FIC",
            DueDate = DateTime.UtcNow.AddDays(14)
            }
            });
        }

        public async Task<int> GetCustomerLookupHistoryCountAsync(int customerLookupID)
        {
            //using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            //var query = "SELECT COUNT(*)" +
            //    " FROM [polarisdb].[dbo].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
            //    $" WHERE PRH.PatronID = {IdParam}";

            //var parameters = new DynamicParameters();
            //parameters.Add(IdParam, patronID);

            //return await db.ExecuteScalarAsync<int>(query, parameters);
            return await Task.FromResult(3);
        }

        public async Task<DataWithCount<List<Material>>> GetPaginatedCustomerLookupHistoryAsync(MaterialFilter filter)
        {
            //using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            //var dataQuery = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
            //    $" BR.BrowseTitle AS {nameof(Material.Title)}," +
            //    $" COALESCE(IRD.ClassificationNumber, IRD.CutterNumber) AS {nameof(Material.Category)}," +
            //    $" PRH.CheckOutDate AS {nameof(Material.CheckoutDate)}" +
            //    " FROM [polarisdb].[dbo].[PatronReadingHistory] AS PRH WITH (NOLOCK)" +
            //    " JOIN [polarisdb].[dbo].[ItemRecordDetails] AS IRD WITH (NOLOCK)" +
            //    " ON PRH.ItemRecordID = IRD.ItemRecordID" +
            //    " JOIN [polarisdb].[dbo].[CircItemRecords] AS CIR WITH (NOLOCK)" +
            //    " ON PRH.ItemRecordID = CIR.ItemRecordID" +
            //    " JOIN [polarisdb].[dbo].[BibliographicRecords] AS BR WITH (NOLOCK)" +
            //    " ON CIR.AssociatedBibRecordID = BR.BibliographicRecordID";

            //var whereClause = $" WHERE PRH.PatronID = {IdParam}";
            //if (!string.IsNullOrWhiteSpace(filter.Search))
            //{
            //    whereClause += $" AND (BR.BrowseTitle LIKE '%' + {SearchParam} + '%'" +
            //                    $" OR BR.BrowseAuthor LIKE '%' + {SearchParam} + '%')";
            //}

            //dataQuery += whereClause;

            //var countQuery = $"SELECT COUNT(*) FROM ({dataQuery}) AS count";

            //dataQuery += $" ORDER BY {filter.OrderBy}";
            //if (filter.OrderDesc)
            //{
            //    dataQuery += " DESC";
            //}

            //dataQuery += $" OFFSET {SkipParam} ROWS FETCH NEXT {TakeParam} ROWS ONLY";

            //var parameters = new DynamicParameters();
            //parameters.Add(IdParam, filter.PatronID);
            //parameters.Add(SearchParam, filter.Search);
            //parameters.Add(SkipParam, filter.Skip);
            //parameters.Add(TakeParam, filter.Take);

            //return new DataWithCount<List<Material>>
            //{
            //    Count = await db.ExecuteScalarAsync<int>(countQuery, parameters),
            //    Data = (await db.QueryAsync<Material>(dataQuery, parameters)).ToList()
            //};
            return await Task.FromResult(new DataWithCount<List<Material>>
            {
                Count = 2,
                Data = new List<Material>
            {
            new Material
            {
                Author = "Author A",
                Title = "History Book A",
                Category = "SCI",
                CheckoutDate = DateTime.UtcNow.AddDays(-10)
            },
            new Material
            {
                Author = "Author B",
                Title = "History Book B",
                Category = "BIO",
                CheckoutDate = DateTime.UtcNow.AddDays(-20)
            }
            }
            });
        }

        public async Task<List<Material>> GetCustomerLookupHoldsAsync(int customerLookupID)
        {
            //    using IDbConnection db = new SqlConnection(_config.GetConnectionString(PolarisDbCSName));
            //    var query = $"SELECT BR.BrowseAuthor AS {nameof(Material.Author)}," +
            //        $" BR.BrowseTitle AS {nameof(Material.Title)}," +
            //        $" SHS.Name AS {nameof(Material.HoldStatus)}" +
            //        " FROM [polarisdb].[dbo].[SysHoldRequests] AS SHR WITH (NOLOCK)" +
            //        " JOIN [polarisdb].[dbo].[SysHoldStatuses] AS SHS WITH (NOLOCK)" +
            //        " ON SHR.SysHoldStatusID = SHS.SysHoldStatusID" +
            //        " JOIN [polarisdb].[dbo].[BibliographicRecords] AS BR WITH (NOLOCK)" +
            //        " ON SHR.BibliographicRecordID = BR.BibliographicRecordID" +
            //        $" WHERE SHR.PatronID = {IdParam}";

            //    var parameters = new DynamicParameters();
            //    parameters.Add(IdParam, patronID);

            //    return (await db.QueryAsync<Material>(query, parameters)).ToList();
            //}
            return await Task.FromResult(new List<Material>
            {
            new Material
            {
            Author = "Author X",
            Title = "Hold Book X",
            HoldStatus = "Pending"
            },
            new Material
            {
            Author = "Author Y",
            Title = "Hold Book Y",
            HoldStatus = "Ready"
            }
            });
        }
    }
}