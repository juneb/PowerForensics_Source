namespace InvokeIR.PowerForensics.Artifacts
{
    public class Timezone
    {
        public readonly string RegistryTimezone;
        public readonly string dotNetStandardTimezone;
        public readonly string dotNetDaylightTimezone;
        public readonly bool IsDaylightSavingTime;
        
        internal Timezone(string registry, string standard, string daylight, bool dst)
        {
            RegistryTimezone = registry;
            dotNetStandardTimezone = standard;
            dotNetDaylightTimezone = daylight;
            IsDaylightSavingTime = dst;
        }
    }
}
