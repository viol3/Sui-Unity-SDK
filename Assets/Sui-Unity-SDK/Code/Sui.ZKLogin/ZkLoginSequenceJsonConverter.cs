using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDive.BCS;

namespace Sui.ZKLogin
{
    public class ZkLoginSequenceJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Sequence);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                if (reader.TokenType == JsonToken.StartArray)
                {
                    var array = JArray.Load(reader);
                    var values = new List<ISerializable>();

                    foreach (var item in array)
                    {
                        if (item.Type == JTokenType.Array)
                        {
                            // Nested array (for "b" field)
                            var nestedArray = (JArray)item;
                            var nestedValues = new List<ISerializable>();

                            foreach (var nestedItem in nestedArray)
                            {
                                // BigInteger olarak parse et ve byte array'e çevir
                                var bigInt = BigInteger.Parse(nestedItem.ToString());
                                nestedValues.Add(new Bytes(bigInt.ToByteArray()));
                            }

                            values.Add(new Sequence(nestedValues.ToArray()));
                        }
                        else
                        {
                            // BigInteger olarak parse et ve byte array'e çevir
                            var bigInt = BigInteger.Parse(item.ToString());
                            values.Add(new Bytes(bigInt.ToByteArray()));
                        }
                    }

                    return new Sequence(values.ToArray());
                }

                throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error converting to Sequence: {ex.Message}", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sequence = value as Sequence;
            if (sequence == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();

            foreach (var item in sequence.Values)
            {
                if (item is Sequence nestedSeq)
                {
                    // Nested sequence
                    writer.WriteStartArray();
                    foreach (var nestedItem in nestedSeq.Values)
                    {
                        if (nestedItem is Bytes bytes)
                        {
                            var bigInt = new BigInteger(bytes.Values);
                            writer.WriteValue(bigInt.ToString());
                        }
                        else
                            serializer.Serialize(writer, nestedItem);
                    }
                    writer.WriteEndArray();
                }
                else if (item is Bytes itemBytes)
                {
                    var bigInt = new BigInteger(itemBytes.Values);
                    writer.WriteValue(bigInt.ToString());
                }
                else
                {
                    serializer.Serialize(writer, item);
                }
            }

            writer.WriteEndArray();
        }
    }
}