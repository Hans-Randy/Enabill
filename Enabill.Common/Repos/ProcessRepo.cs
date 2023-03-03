using System.Data.SqlClient;
using System.Linq;

namespace Enabill.Repos
{
	public class ProcessRepo : BaseRepo
	{
		#region PASSPHRASS DB FUNCTIONS

		internal static string GetFirstRecord(string passphrase)
		{
			if (string.IsNullOrEmpty(passphrase))
				return null;

			return DB.Database.SqlQuery<decimal?>("EXEC dbo.Passphrase_RSP_Check @PassPhrase",
								new SqlParameter("PassPhrase", passphrase))
						.SingleOrDefault()
						.ToString();
		}

		internal static byte[] GetEncryptedValue(string passphrase, double value) => DB.Database.SqlQuery<byte[]>("EXEC dbo.EnCryptDecSP @Phrase, @Value",
								new SqlParameter("Phrase", passphrase),
								new SqlParameter("Value", (decimal)value))
						.SingleOrDefault();

		internal static string GetDecryptedValue(string passphrase, byte[] valueToDecrypt) => DB.Database.SqlQuery<string>("EXEC dbo.DeCryptDecSP @Phrase, @Value",
								new SqlParameter("Phrase", passphrase),
								new SqlParameter("Value", valueToDecrypt))
						.SingleOrDefault()
;

		#endregion PASSPHRASS DB FUNCTIONS
	}
}