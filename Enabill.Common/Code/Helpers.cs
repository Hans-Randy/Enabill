using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Enabill.Models.Dto;
using Enabill.Repos;
using Newtonsoft.Json;
using NLog;

namespace Enabill
{
	public static class Helpers
	{
		#region DATE RELATED

		//Loads all the days in the month for the calendar
		public static List<DateTime> GetDaysInMonth(int year, int month) => GetDaysInMonth(new DateTime(year, month, 1));

		public static List<DateTime> GetDaysInMonth(DateTime date)
		{
			var startDate = date.ToFirstDayOfMonth();
			var endDate = date.ToLastDayOfMonth();

			return GetDaysInDateSpan(startDate, endDate);
		}

		public static List<DateTime> GetDaysInDateSpan(DateTime startDate, DateTime endDate)
		{
			var days = new List<DateTime>();

			var date = startDate.Date;

			while (date <= endDate.Date)
			{
				days.Add(date);
				date = date.AddDays(1);
			}

			return days;
		}

		public static List<DateTime> GetDaysInDateSpanForWorkSessionStatus(DateTime startDate, DateTime endDate, int userID, int workSessionStatus)
		{
			var days = new List<DateTime>();

			var date = startDate.Date;

			while (date <= endDate.Date)
			{
				if (UserRepo.GetWorkSessionStatus(userID, date) == workSessionStatus)
				{
					if (workSessionStatus != 3)
					{
						days.Add(date);
					}
					else
					{
						//if exceptions ensure to exclude non workable days
						var user = UserRepo.GetByID(userID);

						if (date.IsWorkableDay() && !user.IsAnyLeaveTakenOnDate(date) && !user.IsFlexiDayTakenOnDate(date))
						{
							days.Add(date);
						}
					}
				}

				date = date.AddDays(1);
			}

			return days;
		}

		public static List<DateTime> GetDaysInDateSpanNoWorkSession(DateTime startDate, DateTime endDate, int userID)
		{
			var days = new List<DateTime>();

			var date = startDate.Date;

			while (date <= endDate.Date)
			{
				if (!UserRepo.GetWorkSessionsForDate(userID, date).Any())
				{
					days.Add(date);
				}
				date = date.AddDays(1);
			}

			return days;
		}

		#endregion DATE RELATED

		#region DOMAIN RELATED

		public static DateTime ConfigureDate(this DateTime date)
		{
			if (date < EnabillSettings.SiteStartDate)
				return EnabillSettings.SiteStartDate;

			return date;
		}

		public static int CalculateColumnsRequired(int numerator, int denominator)
		{
			decimal rem = decimal.Remainder(numerator, denominator);

			int result = numerator / denominator;
			if (rem > 0)
				result++;

			return result;
		}

		#endregion DOMAIN RELATED

		#region ENCRYPTION

		public static string HashSha512(string rawData)
		{
			// Create a SHA256. SHA = Secure Hash Algorithm.
			using (var sha512Hash = SHA512.Create())
			{
				// ComputeHash - returns byte array
				byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

				// Convert byte array to a string
				var builder = new StringBuilder();

				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}

				return builder.ToString().ToUpper();
			}
		}

		// This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
		// 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
		private const string initVector = "pemgail9uzpgzl88";

		// This constant is used to determine the keysize of the encryption algorithm
		private const int keysize = 256;

		//Encrypt
		public static string EncryptString(string plainText, string passPhrase)
		{
			if (string.IsNullOrEmpty(plainText) && (plainText?.EndsWith("=") == false && plainText?.Length != 64))
			{
				return plainText;
			}
			else
			{
				byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
				byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
				var password = new PasswordDeriveBytes(passPhrase, null);
				byte[] keyBytes = password.GetBytes(keysize / 8);
				var symmetricKey = new RijndaelManaged
				{
					Mode = CipherMode.CBC
				};
				var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
				var memoryStream = new MemoryStream();
				var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

				cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
				cryptoStream.FlushFinalBlock();

				byte[] cipherTextBytes = memoryStream.ToArray();

				memoryStream.Close();
				cryptoStream.Close();

				return Convert.ToBase64String(cipherTextBytes);
			}
		}

		//Decrypt
		public static string DecryptString(string cipherText, string passPhrase)
		{
			if (cipherText?.EndsWith("=") == true || cipherText?.Length == 64)
			{
				byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
				byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
				var password = new PasswordDeriveBytes(passPhrase, null);
				byte[] keyBytes = password.GetBytes(keysize / 8);
				var symmetricKey = new RijndaelManaged
				{
					Mode = CipherMode.CBC
				};
				var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
				var memoryStream = new MemoryStream(cipherTextBytes);
				var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
				byte[] plainTextBytes = new byte[cipherTextBytes.Length];
				int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

				memoryStream.Close();
				cryptoStream.Close();

				return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
			}
			else
			{
				return cipherText;
			}
		}

		#endregion ENCRYPTION

		#region PASSPHRASE FUNCTIONS

		public static bool ConfirmPassphraseIsValid(string passphrase) => !string.IsNullOrEmpty(ProcessRepo.GetFirstRecord(passphrase));

		#endregion PASSPHRASE FUNCTIONS

		#region LOGGING

		public static void Log(LogItem logItem)
		{
			if (Code.Constants.CONFIG == "Release" || Code.Constants.CONFIG == "Staging")
			{
				var _logger = LogManager.GetCurrentClassLogger();
				_logger.Info(JsonConvert.SerializeObject(logItem));
			}
		}

		#endregion LOGGING

		public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
		{
			for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
				yield return day;
		}
	}
}