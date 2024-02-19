using HR.LeaveManagement.Application.Contracts.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.LeaveManagement.Infrastructure.Logging
{
    public class LoggerAdapter<T> : IAppLogger<T>
    {
        public LoggerAdapter(ILoggerFactory loggerFactory)
        {
            
        }
        public void LogInformation(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string message, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
