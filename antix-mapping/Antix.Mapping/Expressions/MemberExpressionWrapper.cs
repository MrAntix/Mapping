using System;
using System.Reflection;

namespace Antix.Mapping.Expressions
{
    public class MemberExpressionWrapper<T>
    {
        readonly MemberInfo _member;
        readonly object _subject;
        readonly Func<object, object> _getValue;
        readonly Action<object, object> _setValue;
        readonly Type _type;

        public MemberExpressionWrapper(MemberInfo member, Object subject)
        {
            _member = member;
            _subject = subject;

            switch (_member.MemberType)
            {
                default:
                    throw new NotSupportedException("Member type not supported");
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo) member;
                    _type = propertyInfo.PropertyType;
                    _getValue = o => propertyInfo.GetValue(o, new object[] {});
                    _setValue = (o, v) => propertyInfo.SetValue(o, v, new object[] {});
                    break;
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo) member;
                    _type = fieldInfo.FieldType;
                    _getValue = fieldInfo.GetValue;
                    _setValue = fieldInfo.SetValue;
                    break;
            }
        }

        public MemberInfo Member
        {
            get { return _member; }
        }

        public object Subject
        {
            get { return _subject; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public T GetValue()
        {
            return (T) _getValue(_subject);
        }

        public void SetValue(T value)
        {
            _setValue(_subject, value);
        }
    }
}