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
        /// this property holds the default normalization form for unicode strings. if unchanged it will be <see cref="NormalizationForm.FormC"/>.
        /// </summary>
        public static NormalizationForm DefaultNormalizationForm { get; set; } = NormalizationForm.FormC;
        /// <summary>
        /// if user wants to normalize unicode strings by default, this property should be set to <see langword="true"/>.
        /// </summary>
        public static bool NormalizeUnicode { get; set; } = true;

    }
}
