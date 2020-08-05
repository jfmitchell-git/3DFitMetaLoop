using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoop.Common.DataEngine
{
   public class DataConvert
    {
        public static float GetCellValueFloat(object data, string refObjectId = "")
        {
            if (data == null) return 0f;
            if (string.IsNullOrEmpty(data.ToString())) return 0f;
            string value = data.ToString();
            try
            {
                return Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MetaDataImportExeption.Log("Invalid value or cast to float.", refObjectId != "" ? refObjectId : data.ToString());
                return 0f;
            }
        }

        public static float GetCellValueFloat(object data, int splitPosition, string refObjectId)
        {
            if (data == null) return 0f;

            string value = string.Empty;
            try
            {
                value = data.ToString().Split(',')[splitPosition];
            }
            catch { }

            if (string.IsNullOrEmpty(value.ToString())) return 0f;

            try
            {

                return Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MetaDataImportExeption.Log("Invalid value or cast to float.", refObjectId != "" ? refObjectId : data.ToString());
                return 0f;
            }
        }

        public static int GetCellValueInt(object data, int splitPosition, string refObjectId = "Undefined")
        {
            if (data == null) return 0;

            if (!string.IsNullOrEmpty(data.ToString()) && splitPosition == 1)
            {
                bool breakehre = true;
            }

            string value = string.Empty;
            try
            {
                value = data.ToString().Split(',')[splitPosition];
            }
            catch { }

            if (string.IsNullOrEmpty(data.ToString())) return 0;

            try
            {
                return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MetaDataImportExeption.Log("Invalid value or cast to int.", refObjectId != "Undefined" ? refObjectId : data.ToString());
                return 0;
            }
        }

        public static int GetCellValueInt(object data, string refObjectId = "Undefined")
        {
            if (data == null) return 0;
            if (string.IsNullOrEmpty(data.ToString())) return 0;
            string value = data.ToString();
            try
            {
                return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MetaDataImportExeption.Log("Invalid value or cast to int.", refObjectId != "Undefined" ? refObjectId : data.ToString());
                return 0;
            }
        }
    }
}

