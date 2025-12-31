using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Codexus.Game.Launcher.Services.Java.RPC.Events;

public class SimpleUnpack
{
	private readonly byte[] _data;

	private int _index;

	private ushort _lastLength;

	public SimpleUnpack(byte[] bytes)
	{
		_data = bytes;
		_index = 0;
		_lastLength = 0;
	}

	public void Unpack<T>(ref T content)
	{
		FieldInfo[] fields = typeof(T).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(content);
			Type fieldType = fieldInfo.FieldType;
			if (value != null)
			{
				InnerUnpack(ref value, fieldType);
				fieldInfo.SetValue(content, value);
			}
		}
		PropertyInfo[] properties = typeof(T).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite)
			{
				object value2 = propertyInfo.GetValue(content);
				Type propertyType = propertyInfo.PropertyType;
				if (value2 != null)
				{
					InnerUnpack(ref value2, propertyType);
					propertyInfo.SetValue(content, value2);
				}
			}
		}
		if (content != null)
		{
			content = ConvertValue<T>(content);
		}
	}

	private static T ConvertValue<T>(object value)
	{
		return (T)Convert.ChangeType(value, typeof(T));
	}

	private void InnerUnpack(ref object value, Type type)
	{
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Object:
			if (type == typeof(byte[]))
			{
				value = _data.Skip(_index).Take(_lastLength).ToArray();
				_index += _lastLength;
			}
			else if (type == typeof(List<uint>))
			{
				ushort num = _lastLength;
				List<uint> list = new List<uint>();
				while (num > 0)
				{
					list.Add(BitConverter.ToUInt32(_data, _index));
					_index += 4;
					num -= 4;
				}
				value = list;
			}
			break;
		case TypeCode.Byte:
			value = _data[_index++];
			break;
		case TypeCode.Int16:
			value = BitConverter.ToInt16(_data, _index);
			_index += 2;
			break;
		case TypeCode.UInt16:
			value = BitConverter.ToUInt16(_data, _index);
			_index += 2;
			_lastLength = (ushort)value;
			break;
		case TypeCode.Int32:
			value = BitConverter.ToInt32(_data, _index);
			_index += 4;
			break;
		case TypeCode.UInt32:
			value = BitConverter.ToUInt32(_data, _index);
			_index += 4;
			break;
		case TypeCode.String:
			value = Encoding.UTF8.GetString(_data, _index, _lastLength);
			_index += _lastLength;
			break;
		}
	}
}
