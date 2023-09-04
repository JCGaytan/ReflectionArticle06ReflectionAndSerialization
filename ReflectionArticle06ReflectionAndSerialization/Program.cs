using System.Reflection;
using System.Xml.Serialization;

namespace ReflectionArticle06ReflectionAndSerialization
{
    /// <summary>
    /// Custom attribute to control serialization and deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializableTypeAttribute : Attribute { }

    // First version of Person class
    [SerializableType]
    public class PersonV1
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    // Second version of Person class with an additional property
    [SerializableType]
    public class PersonV2
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
            Console.ReadLine();
        }

        /// <summary>
        /// Main execution method.
        /// </summary>
        private void Run()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] serializableTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<SerializableTypeAttribute>() != null)
                .ToArray();

            foreach (Type serializableType in serializableTypes)
            {
                ProcessSerializableType(serializableType);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Process a serializable type.
        /// </summary>
        /// <param name="serializableType">The type to process.</param>
        private void ProcessSerializableType(Type serializableType)
        {
            Serialize(serializableType);
            Deserialize(serializableType);
            DeserializeInvalidVersion(serializableType);
            Console.WriteLine();
        }

        /// <summary>
        /// Serialize an object of the specified type.
        /// </summary>
        /// <param name="serializableType">The type to serialize.</param>
        private void Serialize(Type serializableType)
        {
            object instance = SendValues(serializableType);
            string xmlFileName = $"serialized_{serializableType.Name}.xml";

            XmlSerializer serializer = new XmlSerializer(serializableType);
            using (TextWriter writer = new StreamWriter(xmlFileName))
            {
                serializer.Serialize(writer, instance);
            }

            Console.WriteLine($"Serialized {serializableType.Name} to {xmlFileName}");
        }

        /// <summary>
        /// Create and send values for serialization.
        /// </summary>
        /// <param name="serializableType">The type for which to send values.</param>
        /// <returns>The instance with values for serialization.</returns>
        private object SendValues(Type serializableType)
        {
            if (serializableType == typeof(PersonV1))
            {
                PersonV1 personV1 = new PersonV1
                {
                    FirstName = "John",
                    LastName = "Doe"
                };
                Console.WriteLine($"Sending values for {serializableType.Name}");
                return personV1;
            }
            else if (serializableType == typeof(PersonV2))
            {
                PersonV2 personV2 = new PersonV2
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Age = 30
                };
                Console.WriteLine($"Sending values for {serializableType.Name}");
                return personV2;
            }

            return null;
        }

        /// <summary>
        /// Deserialize an object of the specified type.
        /// </summary>
        /// <param name="serializableType">The type to deserialize.</param>
        private void Deserialize(Type serializableType)
        {
            string xmlFileName = $"serialized_{serializableType.Name}.xml";
            string xmlContent = File.ReadAllText(xmlFileName);

            if (ValidateMetadata(serializableType, xmlContent))
            {
                XmlSerializer serializer = new XmlSerializer(serializableType);
                using (TextReader reader = new StringReader(xmlContent))
                {
                    object deserializedInstance = serializer.Deserialize(reader);

                    Console.WriteLine($"Deserialized values for {serializableType.Name}:");

                    PropertyInfo[] properties = serializableType.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        Console.WriteLine($"{property.Name}: {property.GetValue(deserializedInstance)}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Metadata validation failed for {serializableType.Name}. Skipping deserialization.");
            }
        }

        /// <summary>
        /// Attempt to deserialize an invalid version of the specified type.
        /// </summary>
        /// <param name="serializableType">The type to attempt deserialization with.</param>
        private void DeserializeInvalidVersion(Type serializableType)
        {
            // Attempt to deserialize an invalid object version
            string xmlFileName = $"serialized_{serializableType.Name}_invalid.xml";
            string invalidXmlContent = "<InvalidRootElement>...</InvalidRootElement>";

            XmlSerializer serializer = new XmlSerializer(serializableType);
            using (TextWriter writer = new StreamWriter(xmlFileName))
            {
                writer.Write(invalidXmlContent);
            }

            Console.WriteLine($"Attempt to deserialize an invalid version of {serializableType.Name}...");
            try
            {
                using (TextReader reader = new StreamReader(xmlFileName))
                {
                    object deserializedInstance = serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while deserializing invalid version: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate metadata in the XML content.
        /// </summary>
        /// <param name="serializableType">The type to validate metadata for.</param>
        /// <param name="xmlContent">The XML content to validate.</param>
        /// <returns>True if metadata is valid, false otherwise.</returns>
        private bool ValidateMetadata(Type serializableType, string xmlContent)
        {
            string expectedElementName = GetElementName(serializableType);

            return xmlContent.Contains(expectedElementName);
        }

        /// <summary>
        /// Get the expected XML element name based on the type's attribute.
        /// </summary>
        /// <param name="serializableType">The type to retrieve attribute from.</param>
        /// <returns>The expected XML element name.</returns>
        private string GetElementName(Type serializableType)
        {
            SerializableTypeAttribute attribute = serializableType.GetCustomAttribute<SerializableTypeAttribute>();
            if (attribute != null)
            {
                return serializableType.Name;
            }
            return string.Empty;
        }
    }
}
