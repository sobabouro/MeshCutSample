using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// デバッグ用のログを出力するためのユーティリティクラス
/// </summary>
public class Log {

    public static void PrintCollection<T>(IEnumerable<T> collection, string field) {
        if (collection == null) {
            Debug.Log("Collection is null");
            return;
        }

        StringBuilder stringBuilder = new StringBuilder();
        System.Type type = typeof(T);

        FieldInfo fieldInfo = type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null) {
            Debug.LogError($"Field '{field}' not found in type '{type.Name}'");
            return;
        }

        foreach (var item in collection) {
            if (item == null) {
                stringBuilder.AppendLine("null");
                continue;
            }
            var value = fieldInfo.GetValue(item);
            stringBuilder.AppendLine($"{field}: {value} ");
        }
        Debug.Log(stringBuilder.ToString());
    }
}
    
