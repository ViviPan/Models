using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models
{
    public class InternalAccount
    {
        private DatabaseAccess repo;
        private readonly List<User> users;
        private readonly List<Account> accounts;

        public InternalAccount()
        {
            repo = new DatabaseAccess();
            accounts = repo.GetAccounts();
            users = repo.GetUsers();
        }

        //Validation Methods
        public bool IsUserValid(string username)
        {
            if (users.Count == 0)
                return false;
            return users.Exists(user => user.Username == username);
        }

        public bool IsPasswordValid(string username, string password)
        {
            if (!IsUserValid(username))
            {
                return false;
            }
            var pass = repo.GetPassword(username);
            if (pass != password)
            {
                return false;
            }
            return true;
        }

        public bool IsUserAdmin(string username)
        {
            if (users.Count == 0)
                return false;

            var user = users.Find(i => i.Username == username);
            if (user.Id == 1)
            {
                return true;
            }
            return false;
        }
        public List<string> GetUsernames()
        {
            if (users.Count == 0)
                return null;
            //returns values only if user is admin
            var usernames = users.GetRange(1, users.Count - 1).Select(i => i.Username).ToList();

            return usernames;
        }

        public int GetUserId(string username)
        {
            return users.Find(i => i.Username == username).Id;
        }

        public bool HasSufficientBalance(string username, decimal amount)
        {
            if (accounts.Count == 0)
                return false;
            var account = accounts.Find(i => i.User.Username == username);
            if (account.Amount < amount)
            {
                return false;
            }
            return true;
        }

        //Logic Methods
        public string ViewAccount(string username)
        {
            if (users.Count == 0)
                return null;
            var account = accounts.Find(i => i.User.Username == username);
            return account.ToString();
        }

        public void DepositToAccount(string username, string otherUsername, decimal amount, out string message)
        {
            var myAccount = accounts.Find(i => i.User.Username == username);
            var otherAccount = accounts.Find(i => i.User.Username == otherUsername);

            // take from my (-) ||| give to other (+)
            repo.UpdateAccount(myAccount.UserId, myAccount.Amount - amount, otherAccount.UserId, otherAccount.Amount + amount);
            var data = new DatabaseAccess();
            var date = data.GetAccounts().Find(i => i.UserId == 1).TransactionDate;
            message = $"Deposit \t {otherUsername}    \t {amount:C}\t {date} \t {(myAccount.Amount - amount):C}\n";
        }

        public void DepositToAll(string username, decimal amount, out string message)
        {
            if (!IsUserAdmin(username))
            {
                message = "";
                return;
            }

            var userId = new int[accounts.Count];
            var finalAmount = new decimal[accounts.Count];

            userId[0] = accounts[0].UserId;
            finalAmount[0] = accounts[0].Amount - amount * (users.Count() - 1);

            for (var i = 1; i < accounts.Count(); i++)
            {
                userId[i] = accounts[i].UserId;
                finalAmount[i] = accounts[i].Amount + amount;
            }

            repo.UpdateAllAccounts(userId, finalAmount);
            var data = new DatabaseAccess();
            var date = data.GetAccounts().Find(i => i.UserId == 1).TransactionDate;
            message = $"Deposit \t All users\t {(amount * (users.Count() - 1)):C}\t {date} \t {finalAmount[0]:C}\n";
        }

        public void Withdraw(string otherUsername, decimal amount, out string message)
        {
            var adminAccount = accounts.Find(i => i.UserId == 1);
            var userAccount = accounts.Find(i => i.User.Username == otherUsername);

            // take from user ( other - ) ||| give to admin( takes + )
            repo.UpdateAccount(userAccount.UserId, userAccount.Amount - amount, adminAccount.UserId, adminAccount.Amount + amount);
            var data = new DatabaseAccess();
            var date = data.GetAccounts().Find(i => i.UserId == 1).TransactionDate;
            message = $"Withdraw\t {otherUsername}    \t {amount:C}\t {date} \t {(adminAccount.Amount + amount):C}\n";
        }

        public void WithdrawFromAll(string username, decimal amount, out string message)
        {
            if (!IsUserAdmin(username))
            {
                message = "";
                return;
            }

            var userId = new int[accounts.Count];
            var finalAmount = new decimal[accounts.Count];

            userId[0] = accounts[0].UserId;
            finalAmount[0] = accounts[0].Amount + amount * (users.Count() - 1);

            for (var i = 1; i < accounts.Count(); i++)
            {
                userId[i] = accounts[i].UserId;
                finalAmount[i] = accounts[i].Amount - amount;
            }

            repo.UpdateAllAccounts(userId, finalAmount);
            var data = new DatabaseAccess();
            var date = data.GetAccounts().Find(i => i.UserId == 1).TransactionDate;
            message = $"Withdraw\t All users\t {(amount * (users.Count() - 1)):C}\t {date} \t {finalAmount[0]:C}\n";
        }
    }
}
