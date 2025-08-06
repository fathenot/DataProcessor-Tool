using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.UserSettings.DefaultValsGenerator;
namespace DataProcessor.source.UserSettings
{
    /// <summary>
    /// this class holds user settings for the DataProcessor.
    /// </summary>
    public static class UserConfig 
    {
        /// <summary>
        /// if user wants to normalize unicode strings by default, this property should be set to <see langword="true"/>.
        /// </summary>
        public static bool NormalizeUnicode { get; set; } = true;

    }
}
