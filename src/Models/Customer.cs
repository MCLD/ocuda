using System;
using System.Collections.Generic;

namespace Ocuda.Models
{
    public class Customer
    {
        public IEnumerable<CustomerAddress> Addresses { get; set; }
        public DateTime? AddressVerificationDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BlockingNotes { get; set; }
        public double ChargeBalance { get; set; }
        public int CustomerCodeId { get; set; }
        public string CustomerIdNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int Id { get; set; }
        public bool IsBlocked { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public string Notes { get; set; }
        public string UserDefinedField1 { get; set; }
        public string UserDefinedField2 { get; set; }
        public string UserDefinedField3 { get; set; }
        public string UserDefinedField4 { get; set; }
        public string UserDefinedField5 { get; set; }
    }
}