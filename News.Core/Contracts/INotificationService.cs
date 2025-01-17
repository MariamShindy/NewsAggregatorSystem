using News.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Contracts
{
    public interface INotificationService
    {
        Task SendNotificationsAsync();
    }
}
