namespace EasyCaching.Bus.Zookeeper
{
    public class RegistryCenterHealthCheckModel
    {
        public RegistryCenterHealthCheckModel()
        {
            UnHealthTimes = 0;
        }

        public RegistryCenterHealthCheckModel(bool isHealth)
        {
            IsHealth = isHealth;
        }

        public RegistryCenterHealthCheckModel(bool isHealth, int unHealthTimes)
        {
            IsHealth = isHealth;
            UnHealthTimes = unHealthTimes;
        }

        public bool IsHealth { get; set; }

        public int UnHealthTimes { get; set; }

        public HealthTypeEnum? HealthType { get; set; }

        public string UnHealthReason { get; set; }

        public void SetHealth()
        {
            IsHealth = true;
            UnHealthTimes = 0;
            UnHealthReason = null;
            HealthType = HealthTypeEnum.Connected;
        }

        public void SetUnHealth(HealthTypeEnum healthType, string unHealthReason)
        {
            IsHealth = false;
            UnHealthTimes += 1;
            UnHealthReason = unHealthReason;
            HealthType = healthType;
        }
    }
}
