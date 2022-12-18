using SelfSign.BL.Interfaces;
using SelfSign.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly ApplicationContext _context;
        public HistoryService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> AddHistory(Guid requestId, string _event)
        {
            var request = _context.Requests.FirstOrDefault(x => x.Id == requestId);
            if (request == null) return false;
            _context.History.Add(new Common.Entities.History { RequestId = requestId, Event = _event });
            throw new NotImplementedException();
        }
    }
}
