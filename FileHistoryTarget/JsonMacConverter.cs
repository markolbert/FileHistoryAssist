using System;
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace J4JSoftware.FileHistory
{
    public class JsonMacConverter : JsonConverter
    {
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            var macAddress = value as PhysicalAddress;

            writer.WriteValue( macAddress == null ? PhysicalAddress.None.ToString() : macAddress.ToString() );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer )
        {
            var retVal = PhysicalAddress.None;

            if ( objectType != typeof(PhysicalAddress) ) return retVal;

            try
            {
                retVal = PhysicalAddress.Parse( reader.ReadAsString() );
            }
            catch
            {
            }

            return retVal;
        }

        public override bool CanConvert( Type objectType )
        {
            return objectType == typeof(PhysicalAddress);
        }
    }
}