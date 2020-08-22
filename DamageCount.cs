using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NaoParse
{
    public class DamageCount : INotifyPropertyChanged
    {
        private float damageSum;
        private float highestDamage;
        private string skillId;
        private string percent;
        // TODO record crit and calculate crit rate
        public event PropertyChangedEventHandler PropertyChanged;
        // event for encounter damage sum to update
        public event DamageChangedEventHandler DamageChanged;
        public delegate void DamageChangedEventHandler(object sender, float damage);

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyDamageChanged(float damage)
        {
            DamageChanged?.Invoke(this, damage);
        }

        public float DamageSum {
            get {
                return damageSum;
            }
            set {
                NotifyDamageChanged(value - damageSum);
                damageSum = value;
                NotifyPropertyChanged();
            }
        }

        public float HighestDamage {
            get {
                return highestDamage;
            }
            set {
                highestDamage = value;
                NotifyPropertyChanged();
            }
        }

        public string SkillId {
            get {
                return skillId;
            }
            set {
                skillId = value;
                NotifyPropertyChanged();
            }
        }

        public string DamagePercent {
            get {
                return percent;
            }
            set {
                percent = value;
                NotifyPropertyChanged();
            }
        }


    }

}