using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;

namespace porting.Models
{
    public static class Utils
    {
        public static string ConvertToString(this FPackageIndex index)
        {
            return index.ToString().SubstringAfter(" "); // ExportType somepath we only need path
        }
    }
}