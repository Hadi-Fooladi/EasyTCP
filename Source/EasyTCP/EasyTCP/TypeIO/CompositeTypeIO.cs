using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace EasyTCP
{
	class CompositeTypeIO : ITypeIO
	{
		public CompositeTypeIO(Type t) => _t = t;

		#region Fields
		readonly Type _t;
		readonly SortedList<int, CField> _fields = new SortedList<int, CField>();
		#endregion

		#region ITypeIO Members
		public void Init(TypeIOs container)
		{
			foreach (var field in _t.GetFields())
			{
				var attr = field.GetCustomAttribute<EasyTCPAttribute>();
				if (attr != null)
					_fields.Add(attr.Order, new CField(field, container));
			}
		}

		public object Read(BinaryReader br)
		{
			var instance = Activator.CreateInstance(_t);

			foreach (var field in _fields.Values)
				field.Info.SetValue(instance, field.IO.Read(br));

			return instance;
		}

		public void Write(BinaryWriter bw, object value)
		{
			foreach (var field in _fields.Values)
				field.IO.Write(bw, field.Info.GetValue(value));
		}
		#endregion

		#region Nested Classes
		class CField
		{
			public readonly ITypeIO IO;
			public readonly FieldInfo Info;

			public CField(FieldInfo info, TypeIOs container)
			{
				Info = info;
				IO = container.GetOrCreate(info.FieldType);
			}
		}
		#endregion
	}
}
