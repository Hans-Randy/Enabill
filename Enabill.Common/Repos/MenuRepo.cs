using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class MenuRepo : BaseRepo
	{
		#region MENU SPECIFIC

		public static List<Menu> GetByRoleBW(int roleBW) => DB.Menus
					.Where(m => (m.RoleBW & roleBW) > 0)
					.OrderBy(m => m.CustomSort)
					.ToList();

		internal static void Save(Menu menu)
		{
			if (menu.MenuID == 0)
				DB.Menus.Add(menu);

			DB.SaveChanges();
		}

		internal static void Delete(Menu menu)
		{
			try
			{
				DB.Menus.Remove(menu);
				DB.SaveChanges();
			}
			catch
			{
				throw new NullReferenceException("This menu item could not be found in the records");
			}
		}

		#endregion MENU SPECIFIC
	}
}