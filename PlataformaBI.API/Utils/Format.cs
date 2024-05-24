using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace PlataformaBI.API.Utils
{
    public class Format
    {
        [NonAction]
        public static string GetCNPJ(string CNPJ)
        {
            if (string.IsNullOrEmpty(CNPJ))
                return string.Empty;

            CNPJ = CNPJ.Replace("%2", "");
            var result = Regex.Replace(CNPJ, "[^0-9]", "");
            return result;
        }
    }
}
