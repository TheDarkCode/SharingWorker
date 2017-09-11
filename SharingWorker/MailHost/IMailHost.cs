using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharingWorker.MailHost
{
    interface IMailHost
    {
        string Name { get; }
        bool IsSelected { get; set; }
        
        Task<string> GetMailAddress();
        Task<string> GetMegaSignupMail(string mailAddress);        
    }
}
