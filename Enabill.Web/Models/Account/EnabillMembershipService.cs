using System;
using System.Web.Security;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class EnabillMembershipService : IMembershipService
	{
		#region IMembershipService Members

		public int MinPasswordLength => 8;

		public bool ValidateUser(string userName, string password)
		{
			var u = new User();

			if (userName.Contains("@"))
			{
				u = UserRepo.GetByEmail(userName);
			}
			else
			{
				u = UserRepo.GetByUserName(userName);
			}

			if (u == null)
				return false;

			if (u.Password == null)
				u.Password = Helpers.HashedString(Settings.DefaultUserPassword);

			return u.Password == Helpers.HashedString(password);
		}

		public MembershipCreateStatus CreateUser(string userName, string password, string email) => throw new NotImplementedException();

		public bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			var u = UserRepo.GetByUserName(userName);

			if (u == null)
				return false;

			if (u.Password == null)
				u.Password = Helpers.HashedString(Settings.DefaultUserPassword);

			if (u.Password != Helpers.HashedString(oldPassword))
				return false;

			if (newPassword.Length < this.MinPasswordLength)
				return false;
			//if (newPassword.Length < MinPasswordLength) throw new ArgumentException(string.Format($"Password does not meet the minimum required length of {MinPasswordLength} characters."));

			u.Password = Helpers.HashedString(newPassword);
			u.MustResetPwd = false;
			u.ResetForgottenPasswordToken();
			u.Save(Settings.Current.CurrentUser);

			Settings.Current.CurrentUser = UserRepo.GetByID(u.UserID);

			return true;
		}

		#endregion IMembershipService Members
	}
}