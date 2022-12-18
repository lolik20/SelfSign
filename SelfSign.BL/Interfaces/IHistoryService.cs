using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Interfaces
{
    public interface IHistoryService
    {
        Task<bool> AddHistory(Guid requestId, string _event);
    }
}
