using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    /// <summary>
    /// C# class reference to another class of enum definition model
    /// </summary>
    public class Import
    {
        /// <summary>
        /// Reference file name where reference is defined
        /// </summary>
        public string? FileName;

        /// <summary>
        /// Classes and enums reference names in reference file
        /// </summary>
        public List<string> Elements = new List<string>();

        public override string ToString()
        {           
            return $"{this.FileName} {String.Join(",",this.Elements)}";
        }

    }
}
