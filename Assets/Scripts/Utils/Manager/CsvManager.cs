using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

public class CsvManager:SimpleSingleton<CsvManager>
{

    public List<T> Load<T>(string fileName) where T : new()
    {
        List<T> result = new List<T>();
        string path = "UI/Data/" + fileName;

        TextAsset csvData = Resources.Load<TextAsset>(path);

        string[] lines = csvData.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return result;

        // 헤더 파싱
        string[] headers = lines[0].Split(',');

        // Reflection으로 필드 또는 프로퍼티 찾아오기
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            T entry = new T();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                string header = headers[j].Trim();
                string value = values[j].Trim();

                foreach (var field in fields)
                {
                    if (field.Name == header)
                    {
                        object parsed = ConvertString(value, field.FieldType);
                        field.SetValueDirect(__makeref(entry), parsed);
                        break;
                    }
                }
            }

            result.Add(entry);
        }

        return result;
    }

    private object ConvertString(string value, System.Type type)
    {
        if (type.IsEnum)
            return Enum.Parse(type, value, ignoreCase: true);
        if (type == typeof(int))
            return int.TryParse(value, out int i) ? i : 0;
        if (type == typeof(float))
            return float.TryParse(value, out float f) ? f : 0f;
        if (type == typeof(bool))
            return value == "1" || value.ToLower() == "true";
        if (type == typeof(string))
            return value;

        return null;
    }
}