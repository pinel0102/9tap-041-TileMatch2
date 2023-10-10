using System;

namespace NineTap.Common
{
	public readonly struct Unit : IEquatable<Unit>
	{
		#region Inheritances
		public override int GetHashCode()
		{
			return 0;
		}
		#endregion

		#region IEquatable<Unit> Interface
		public bool Equals(Unit other)
		{
			return true;
		}
		#endregion
	}
}
