using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Verse
{
	public static class DirectXmlSaver
	{
		public static bool IsSimpleTextType(Type type)
		{
			return type == typeof(float) || type == typeof(double) || type == typeof(long) || type == typeof(ulong) || type == typeof(char) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(int) || type == typeof(uint) || type == typeof(bool) || type == typeof(short) || type == typeof(ushort) || type == typeof(string) || type.IsEnum;
		}

		public static void SaveDataObject(object obj, string filePath)
		{
			try
			{
				XDocument xDocument = new XDocument();
				XElement content = XElementFromObject(obj, obj.GetType());
				xDocument.Add(content);
				xDocument.Save(filePath);
			}
			catch (Exception ex)
			{
				Log.Error("Exception saving data object " + obj.ToStringSafe() + ": " + ex);
				GenUI.ErrorDialog("ProblemSavingFile".Translate(filePath, ex.ToString()));
			}
		}

		public static XElement XElementFromObject(object obj, Type expectedClass)
		{
			return XElementFromObject(obj, expectedClass, expectedClass.Name);
		}

		public static XElement XElementFromObject(object obj, Type expectedType, string nodeName, FieldInfo owningField = null, bool saveDefsAsRefs = false)
		{
			DefaultValueAttribute customAttribute;
			if (owningField != null && owningField.TryGetAttribute(out customAttribute) && customAttribute.ObjIsDefault(obj))
			{
				return null;
			}
			if (obj == null)
			{
				XElement xElement = new XElement(nodeName);
				xElement.SetAttributeValue("IsNull", "True");
				return xElement;
			}
			Type type = obj.GetType();
			XElement xElement2 = new XElement(nodeName);
			if (IsSimpleTextType(type))
			{
				xElement2.Add(new XText(obj.ToString()));
			}
			else if (saveDefsAsRefs && typeof(Def).IsAssignableFrom(type))
			{
				string defName = ((Def)obj).defName;
				xElement2.Add(new XText(defName));
			}
			else
			{
				if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(List<>))
				{
					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
					{
						Type expectedType2 = type.GetGenericArguments()[0];
						Type expectedType3 = type.GetGenericArguments()[1];
						IEnumerator enumerator = (obj as IEnumerable).GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object current = enumerator.Current;
								object value = current.GetType().GetProperty("Key").GetValue(current, null);
								object value2 = current.GetType().GetProperty("Value").GetValue(current, null);
								XElement xElement3 = new XElement("li");
								xElement3.Add(XElementFromObject(value, expectedType2, "key", null, saveDefsAsRefs: true));
								xElement3.Add(XElementFromObject(value2, expectedType3, "value", null, saveDefsAsRefs: true));
								xElement2.Add(xElement3);
							}
							return xElement2;
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
					}
					if (type != expectedType)
					{
						XAttribute content = new XAttribute("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(obj.GetType()));
						xElement2.Add(content);
					}
					{
						foreach (FieldInfo item in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						orderby f.MetadataToken
						select f)
						{
							try
							{
								XElement xElement4 = XElementFromField(item, obj);
								if (xElement4 != null)
								{
									xElement2.Add(xElement4);
								}
							}
							catch
							{
								throw;
							}
						}
						return xElement2;
					}
				}
				Type expectedType4 = type.GetGenericArguments()[0];
				int num = (int)type.GetProperty("Count").GetValue(obj, null);
				for (int i = 0; i < num; i++)
				{
					object[] index = new object[1]
					{
						i
					};
					object value3 = type.GetProperty("Item").GetValue(obj, index);
					XNode content2 = XElementFromObject(value3, expectedType4, "li", null, saveDefsAsRefs: true);
					xElement2.Add(content2);
				}
			}
			return xElement2;
		}

		private static XElement XElementFromField(FieldInfo fi, object owningObj)
		{
			if (Attribute.IsDefined(fi, typeof(UnsavedAttribute)))
			{
				return null;
			}
			object value = fi.GetValue(owningObj);
			return XElementFromObject(value, fi.FieldType, fi.Name, fi);
		}
	}
}
