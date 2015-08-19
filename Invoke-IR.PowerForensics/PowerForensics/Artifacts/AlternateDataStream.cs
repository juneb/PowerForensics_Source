using System;

namespace InvokeIR.PowerForensics.Artifacts
{
    public class AlternateDataStream
    {
        public readonly string FullName;
        public readonly string Name;
        public readonly string StreamName;

        internal AlternateDataStream(string fullName, string name, string streamName)
        {
            FullName = fullName;
            Name = name;
            StreamName = streamName;
        }
    }
}
