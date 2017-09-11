using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using LogManager = NLog.LogManager;

namespace SharingWorker.MailHost
{
    abstract class MailHostBase : PropertyChangedBase, IMailHost
    {
        protected static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
        protected static readonly Random Rnd = new Random();
        
        public abstract string Name { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public abstract Task<string> GetMegaSignupMail(string mailAddress);
        public abstract Task<IEnumerable<string>> GetAvailiableDomains();
        
        protected virtual string GenerateAccount()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var ret = new StringBuilder();
            ret.Append(chars.Where(char.IsLetter).Random());
            for (int i = 0; i < Rnd.Next(6, 9); i++)
            {
                ret.Append(chars.Random());
            }
            return ret.ToString();
        }

        public virtual async Task<string> GetMailAddress()
        {
            var account = GenerateAccount();
            var domain = await GetAvailiableDomains().ConfigureAwait(false);
            if (!domain.Any()) return null;
            return account + domain.Random();
        }

        protected virtual string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash. 
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }
    }
}
