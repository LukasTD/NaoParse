using System;
using System.Text.RegularExpressions;

namespace NaoParse
{
	public class DamageMeterRow
	{
		public long entityId;
		public float highestDamage;
		public string skillId;
		public string Name { get; set; }
		public float DamageSum { get; set; }
		public float DamagePercent { get; set; }
		public string MaxHit {
			get {
				return String.Format("{0:n0}", highestDamage) + $" ({Regex.Replace(skillId, "(?!^)([A-Z])", " $1")})";
			}
		}
	}
}
