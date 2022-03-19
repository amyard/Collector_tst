using System;
using System.Linq;
using Collector.Models;
using Collector.Models.Instant;
using JobInfo = Collector.Models.JobInfo;

namespace Collector
{
    public class Mapper
    {
        public static InstantPyCardExportData MapImportedDataToExport(InstantPayCardImportData importedEmployeeData)
        {
            var terminated = (importedEmployeeData.EmployeeInfo.Terminated.TerminatedDate != null &&
                              Convert.ToDateTime(importedEmployeeData.EmployeeInfo.Terminated.TerminatedDate) <= DateTime.Now)
                ? importedEmployeeData.EmployeeInfo.Terminated.TerminatedDate
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
                    BirthDate = importedEmployeeData.EmployeeInfo.BirthDate,
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