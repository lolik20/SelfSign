using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Interfaces
{
    public interface IItMonitoringService
    {
        Task<Tuple<bool, string>> CreateRequest(object request);
        Task<bool> TwoFactor(string requestId,object request);

    }
}
