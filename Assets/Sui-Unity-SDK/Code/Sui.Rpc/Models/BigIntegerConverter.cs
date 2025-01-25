using System;
using System.Numerics;
using Unity.Plastic.Newtonsoft.Json;

namespace Sui.Rpc
{
    public class BigIntegerConverter : JsonConverter<BigInteger>
    {
        public override BigInteger ReadJson(JsonReader reader, Type objectType, BigInteger existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    return new BigInteger((long)reader.Value);
                case JsonToken.String:
                    return string.IsNullOrEmpty((string)reader.Value)
                        ? BigInteger.Zero
                        : BigInteger.Parse((string)reader.Value);
                default:
                    return BigInteger.Zero;
            }
        }

        public override void WriteJson(JsonWriter writer, BigInteger value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}