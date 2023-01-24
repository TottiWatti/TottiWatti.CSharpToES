using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    /// <summary>
    /// Model of .cs file containing classes and enums
    /// </summary>
    public class StructureFileModel
    {        
        /// <summary>
        /// Source file path
        /// </summary>
        public string SourceFile = "";

        /// <summary>
        /// Destination file path
        /// </summary>
        public string DestinationFile = "";

        /// <summary>
        /// Relative destination file path
        /// </summary>
        public string RelativeDestinationFile = "";

        /// <summary>
        /// References to classes and enums defined in another .cs file
        /// </summary>
        public List<Import> Imports = new List<Import>();

        /// <summary>
        /// Class models in file
        /// </summary>
        public List<StructureModel> ClassModels = new List<StructureModel>();

        /// <summary>
        /// Enum models in file
        /// </summary>
        public List<EnumModel> EnumModels = new List<EnumModel>();
    }
}
