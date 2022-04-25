using System;
using System.Linq;
using Collector.Models;
using Collector.Models.Instant;

namespace Collector
{
    public class Mapper
    {
        public static InstantPyCardExportData MapImportedDataToExport(EmployeeData importedEmployeeData)
        {
            bool terminatedDateSuccess = DateTime.TryParse(importedEmployeeData.EmployeeInfo.Terminated.TerminatedDate,
                out DateTime terminatedDate);
            
            bool birthDateSuccess = DateTime.TryParse(importedEmployeeData.EmployeeInfo.BirthDate, out DateTime birthDate);

            var terminated = (importedEmployeeData.EmployeeInfo.Terminated.IsTerminated && (terminatedDateSuccess && terminatedDate <= DateTime.Now))
                ? terminatedDate.ToString("yyyy-MM-dd")
                : importedEmployeeData.EmployeeInfo.Terminated.IsTerminated.ToString().ToLower();

            var job = importedEmployeeData.JobInfo.FirstOrDefault(j => j.IsDefault) ??
                      importedEmployeeData.JobInfo.First();
            
            return new InstantPyCardExportData()
            {
                EmployeeId =  importedEmployeeData.EmployeeInfo.EmployeeId,
                ExternalLocationId =  importedEmployeeData.ExternalLocationId,
                EmployeeInfo = new Models.Instant.EmployeeInfo
                {
                    FirstName = importedEmployeeData.EmployeeInfo.FirstName,
                    LastName = importedEmployeeData.EmployeeInfo.LastName,
                    BirthDate = birthDateSuccess && birthDate <= DateTime.Now ? birthDate.ToString("yyyy-MM-dd") : null,
                    BlockInstantPay = importedEmployeeData.EmployeeInfo.BlockInstantPay,
                    PaidOnInstant = importedEmployeeData.EmployeeInfo.PaidInstant,
                    Terminated = terminated
                },
                JobCode = job.Code,
                JobInfo = new Models.Instant.JobInfo
                {
                    Name = job.Name,
                    AnnualSalary = !job.IsHourly ? job.AnnualSalary : null,
                    HourlyRate = job.IsHourly ? job.HourlyRate : null
                }
            };
        }
    }
}