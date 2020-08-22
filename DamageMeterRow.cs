using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NaoParse
{
    public class DamageMeterRow : INotifyPropertyChanged
    {
        private DamageCount damageCount;

		public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ListenToDamageCountChange(DamageCount damageCount)
        {
            damageCount.PropertyChanged += new PropertyChangedEventHandler(DamageCountChangeHandler);
        }
        private void DamageCountChangeHandler(object o, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged();
        }

        public DamageMeterRow(DamageCount damageCount, string name)
        {
            this.damageCount = damageCount;
            this.Name = name;

            ListenToDamageCountChange(damageCount);
        }

		public string Name { get; }

		public string DamageSum {
            get {
                return String.Format("{0:n0}", damageCount.DamageSum);
            }
        }

        public string DamagePercent {
            get {
                return damageCount.DamagePercent;
            }
        }

        public string MaxHit {
            get {
                return String.Format("{0:n0}", damageCount.HighestDamage)  + $" ({Regex.Replace(damageCount.SkillId.ToString(), "(?!^)([A-Z])", " $1")})";
            }
        }

        public DamageCount GetDamageCount()
        {
            return damageCount;
        }
    }
}
