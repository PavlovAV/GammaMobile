using System;

namespace gamma_mob.Common
{
    public sealed class DesignMode
    {
        public static bool IsTrue
        {
            get
            {
                return AppDomain.CurrentDomain
                                .FriendlyName.Contains("DefaultDomain");
            }
        }
    }
}