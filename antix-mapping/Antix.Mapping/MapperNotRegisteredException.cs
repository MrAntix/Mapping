using System;
using System.Runtime.Serialization;
using Antix.Mapping.Properties;

namespace Antix.Mapping
{
    [Serializable]
    public class MapperNotRegisteredException : Exception
    {
        public MapperNotRegisteredException(Tuple<Type, Type> key)
            : base(
                string.Format(
                    Resources.MapperNotRegisteredException,
                    key.Item1.FullName,
                    key.Item2.FullName))
        {
        }

        protected MapperNotRegisteredException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}