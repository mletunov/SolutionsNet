using System;

namespace Solutions.Core.IoC
{
    public class Registration
    {
        public Type Service { get; set; }
        public Type Target { get; set; }

        public Func<IScope, object> TargetFunc { get; set; }
    }
}