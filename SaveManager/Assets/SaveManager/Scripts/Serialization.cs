using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SaveManager
{
    public enum SerializationFormat
    {
        Binary
    }

    /// <summary>
    /// Holds up methods for serializing data.
    /// </summary>
    public static class Serialization
    {
        public static byte[] SerializeToBytes(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentException("Object can't be null.", "obj");
            }

            if (!obj.GetType().IsSerializable)
            {
                throw new SerializationException(
                    string.Format(
                        "Type '{0}' is not serializable! " +
                            "Check that the type are marked with [Serializable] attribute."
                        , obj.GetType()
                    )
                );
            }

            var formatter = new BinaryFormatter();

            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public static string SerializeToJson(object obj)
        {
            throw new NotImplementedException();
        }

        public static T DeserializeFromBytes<T>(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException(
                    "Data can't be null.",
                    "data"
                );
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(
                    "Data array can't be empty.",
                    "data"
                );
            }

            var formatter = new BinaryFormatter();

            using (var memoryStream = new MemoryStream(data))
            {
                return (T)formatter.Deserialize(memoryStream);
            }
        }

        public static T DeserializeFromJson<T>(string json)
        {
            throw new NotImplementedException();
        }
    }
}

