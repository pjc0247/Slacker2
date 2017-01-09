using System;
namespace Slacker2
{
	public class NeedsPermissionAttribute : Attribute
	{
		public string PermissionGroupName { get; set; }

		public NeedsPermissionAttribute(string permissionGroupName)
		{
			PermissionGroupName = permissionGroupName;
		}
	}
}
