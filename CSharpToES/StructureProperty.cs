namespace TottiWatti.CSharpToES
{
    /// <summary>
    /// Class property of field model
    /// </summary>
    public class StructureProperty
    {
        /// <summary>
        /// Name of property of field
        /// </summary>
        public string Name { get; set; } = "";

        private string _Type = "";
        public string Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                _SetTypeInformation();
            }
        }

        /// <summary>
        /// ES (javascript) type
        /// </summary>
        public string JsType { get; set; } = "";

        /// <summary>
        /// Comments
        /// </summary>
        public List<string> TriviaLines { get; set; } = new List<string>();

        /// <summary>
        /// Initialvalue of property of field if any
        /// </summary>
        public string? InitialValue { get; set; } = null;

        /// <summary>
        /// Property of field is nullable type
        /// </summary>
        public bool IsNullable { get; set; } = false;

        /// <summary>
        /// Numeric type minimum value
        /// </summary>
        public string? TypeMin = null;

        /// <summary>
        /// Numeric type maximum value
        /// </summary>
        public string? TypeMax = null;

        /// <summary>
        /// Property of field is native data type boolean, byte, int, double, string etc.
        /// </summary>
        public bool IsNative = false;

        /// <summary>
        /// Property of field is numeric data type 
        /// </summary>
        public bool IsNumeric = false;

        /// <summary>
        /// Property of field is numeric integer data type byte, int, long etc.
        /// </summary>
        public bool IsInteger = false;

        /// <summary>
        /// Native data type default value
        /// </summary>
        public string BaseTypeDefaultValue = "";

        /// <summary>
        /// Property of field is string data type
        /// </summary>
        public bool IsString = false;

        /// <summary>
        /// Property of field is datetime data type
        /// </summary>
        public bool IsDateTime = false;

        /// <summary>
        /// Property of field class model reference (is instance of another class)
        /// </summary>
        public StructureModel? Class;

        /// <summary>
        /// Property of field enumdel reference (is instance of enum))
        /// </summary>
        public EnumModel? Enum;

        /// <summary>
        /// Property of field is list data type
        /// </summary>
        public bool IsList = false;

        /// <summary>
        /// Property of field list type data type
        /// </summary>
        public StructureProperty? ListType;

        /// <summary>
        /// Property of field is dictionary data type
        /// </summary>
        public bool IsDictionary = false;

        /// <summary>
        /// Property of field dictionary type key data type
        /// </summary>
        public StructureProperty? DictionaryKeyType; // = "";

        /// <summary>
        /// Property of field dictionary type value data type
        /// </summary>
        public StructureProperty? DictionaryValueType; // = "";

        /// <summary>
        /// Property of field is array
        /// </summary>
        public bool IsArray = false;

        /// <summary>
        /// Property of field array is multidimensional array
        /// </summary>
        public bool IsMultiDimensionalArray = false;

        /// <summary>
        /// Property of field array data type
        /// </summary>
        public StructureProperty? ArrayType; // = "";

        /// <summary>
        /// Property of field multidimensional array dimensions
        /// </summary>
        public List<int> ArrayDimensions = new List<int>();

        /// <summary>
        /// Property has range attribute defined
        /// </summary>
        public bool HasRange = false;

        /// <summary>
        /// Property range attribute min
        /// </summary>
        public string RangeMin = "";

        /// <summary>
        /// Property range attribute max
        /// </summary>
        public string RangeMax = "";

        private void _SetTypeInformation()
        {            
            System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
            
            string typeString = _Type; 
            string jsTypeString = typeString;

            // list check
            if (typeString.StartsWith("List<"))
            {
                string[] ss = typeString.Split('<', '>');
                if (ss.Length > 1)
                {
                    IsList = true; 
                    var lType = ss[1].Trim();
                    var lp = new StructureProperty();
                    lp.Type = lType;                    
                    jsTypeString = lp.JsType + "[]";
                    ListType = lp;
                }
            }

            // dictionary check
            if (typeString.StartsWith("Dictionary<"))
            {
                string[] ss = typeString.Split('<', '>');
                if (ss.Length > 1)
                {
                    IsDictionary = true;
                   
                    var ks = ss[1].Split(",")[0].Trim();
                    var kType = new StructureProperty();
                    kType.Type = ks;
                    DictionaryKeyType = kType; 
                    
                    var vs = ss[1].Split(",")[1].Trim();
                    var vType = new StructureProperty();
                    vType.Type = vs;
                    DictionaryValueType = vType;                     

                    jsTypeString = $"Map<{DictionaryKeyType.JsType},{DictionaryValueType.JsType}>";
                }
            }
            
            if (typeString.EndsWith(']'))
            {
                IsArray = true;
                var ais = typeString.IndexOf('[');
                var eis = typeString.LastIndexOf(']');
                if (eis-ais > 1)
                {
                    IsMultiDimensionalArray = true;
                }
            }
            if (IsArray)
            {
                string[] ss = typeString.Split('[');
                if (ss.Length > 1)
                {                         
                    var ats = ss[0].Trim();
                    var at = new StructureProperty();
                    at.Type = ats;
                    if (IsMultiDimensionalArray)
                    {
                        jsTypeString = at.JsType + "[][]";
                    }
                    else
                    {
                        jsTypeString = at.JsType + "[]";
                    }
                }
            }


            switch (typeString)
            {
                case "bool":
                    IsNative = true;
                    BaseTypeDefaultValue = "false";
                    JsType = "boolean";
                    break;
                case "sbyte":
                    TypeMin = sbyte.MinValue.ToString(nfi);
                    TypeMax = sbyte.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "byte":
                    TypeMin = byte.MinValue.ToString(nfi);
                    TypeMax = byte.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "short":
                case "int16":
                    TypeMin = short.MinValue.ToString(nfi);
                    TypeMax = short.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "ushort":
                case "uint16":
                    TypeMin = ushort.MinValue.ToString(nfi);
                    TypeMax = ushort.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "int":
                case "int32":
                    TypeMin = int.MinValue.ToString(nfi);
                    TypeMax = int.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "uint":
                case "uint32":
                    TypeMin = uint.MinValue.ToString(nfi);
                    TypeMax = uint.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "long":
                case "int64":
                    TypeMin = long.MinValue.ToString(nfi);
                    TypeMax = long.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "ulong":
                case "uint64":
                    TypeMin = ulong.MinValue.ToString(nfi);
                    TypeMax = ulong.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    IsInteger = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "float":
                case "Single":
                    TypeMin = float.MinValue.ToString(nfi);
                    TypeMax = float.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "double":
                    TypeMin = double.MinValue.ToString(nfi);
                    TypeMax = double.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "decimal":
                    TypeMin = decimal.MinValue.ToString(nfi);
                    TypeMax = decimal.MaxValue.ToString(nfi);
                    IsNative = true;
                    IsNumeric = true;
                    BaseTypeDefaultValue = "0";
                    JsType = "number";
                    break;
                case "string":
                    IsNative = true;
                    IsString = true;
                    BaseTypeDefaultValue = "''";
                    JsType = "string";
                    break;
                //case "date":
                case "DateTime": 
                case "System.DateTime":
                    TypeMin = "new Date(-8640000000000000)";
                    TypeMax = "new Date(8640000000000000)";
                    IsNative = true;
                    IsDateTime = true;
                    BaseTypeDefaultValue = "new Date()";
                    JsType = "Date";
                    break;
                default:
                    IsNative = false;
                    JsType = jsTypeString; 
                    break;
            }            
        }

        public override string ToString()
        {            
            return $"{this.Name} {this.Type} {this.InitialValue}";            
        }

    }
}
