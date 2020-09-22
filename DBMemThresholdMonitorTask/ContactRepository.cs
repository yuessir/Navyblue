using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DBMemThresholdMonitorTask.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NavyBule.Core.Domain;

namespace DBMemThresholdMonitorTask
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetContacts();
        Task<IEnumerable<ContactThresholdMap>> GetContactsMapByThresholdInfoId(int thresholdInfoId);

    }
    public class ContactRepository : MssqlRepositoryBase<BaseEntity>, IContactRepository
    {

        public ContactRepository(IConfiguration configuration, ILogger<ContactRepository> logger) : base(configuration, logger)
        {
        }

        public async Task<IEnumerable<Contact>> GetContacts()
        {
            return await GetList<Contact>(@"SELECT [Id]
      ,[Name]
      ,[Email]
      ,[Cellphone]
  FROM [dbo].[Contact] where Enabled=1");

        }
        public async Task<IEnumerable<ContactThresholdMap>> GetContactsMapByThresholdInfoId(int thresholdInfoId)
        {
          var  strSQL= @"SELECT [Id]
      ,[ContactId]
      ,[ThresholdInfoId]
  FROM [dbo].[ContactThresholdMap] WHERE thresholdInfoId = @thresholdInfoId";

            DynamicParameters dps = new DynamicParameters();
            dps.Add("thresholdInfoId", thresholdInfoId);
           return await GetList<ContactThresholdMap>(strSQL, dps);


        }
    }
}
