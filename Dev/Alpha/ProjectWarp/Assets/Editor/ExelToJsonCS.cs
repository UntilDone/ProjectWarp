using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class ExcelToJsonCS : Editor
{
    [MenuItem("Tools/Convert Excel to JSON and C# Script")]
    public static void ConvertExcelToJsonAndCS()
    {
        string initialPath = "C:/WorkSpace/UntillDone/Dev/Alpha/ProjectWarp/Assets/Resources/Raw_Datas";
        string excelFilePath = EditorUtility.OpenFilePanel("Select Excel File", initialPath, "xlsx");
        Debug.Log(excelFilePath);
        if (string.IsNullOrEmpty(excelFilePath))
        {
            Debug.LogWarning("No file selected.");
            return;
        }

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(excelFilePath);
        string jsonOutputPath = Path.Combine(Application.dataPath, "Resources/Table", $"{fileNameWithoutExtension}.json");
        string csOutputPath = Path.Combine(Application.dataPath, "Scripts/Data", $"{fileNameWithoutExtension}.cs");


        if (File.Exists(jsonOutputPath) || File.Exists(csOutputPath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Overwrite Confirmation",
                "Files with the same name already exist. Do you want to overwrite them?",
                "Yes", "No"
            );

            if (!overwrite)
            {
                Debug.Log("Operation cancelled by user.");
                return;
            }
        }

        var json = ConvertExcelToJson(excelFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(jsonOutputPath));
        File.WriteAllText(jsonOutputPath, json);
        Debug.Log("JSON file created at: " + jsonOutputPath);

        var fields = ExtractFieldsFromExcel(excelFilePath);
        var csCode = GenerateCSharpClass(fileNameWithoutExtension, fields);
        Directory.CreateDirectory(Path.GetDirectoryName(csOutputPath));
        File.WriteAllText(csOutputPath, csCode);
        Debug.Log("C# class file created at: " + csOutputPath);

        AssetDatabase.Refresh();

    }

    private static string ConvertExcelToJson(string excelFilePath)
    {
        using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = new List<Dictionary<string, object>>();
                var dataSet = reader.AsDataSet();

                if (dataSet.Tables.Count > 0)
                {
                    var table = dataSet.Tables[0];
                    var columnTypes = new Dictionary<string, string>();

                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        string columnName = table.Rows[0][col].ToString();
                        string columnType = table.Rows[1][col].ToString().ToLower();
                        columnTypes[columnName] = columnType;
                    }

                    for (int row = 2; row < table.Rows.Count; row++)
                    {
                        var rowDict = new Dictionary<string, object>();
                        for (int col = 0; col < table.Columns.Count; col++)
                        {
                            string columnName = table.Rows[0][col].ToString();
                            var cellValue = table.Rows[row][col];

                            switch (columnTypes[columnName])
                            {
                                case "int":
                                    rowDict[columnName] = Convert.ToInt32(cellValue);
                                    break;
                                case "float":
                                    rowDict[columnName] = Convert.ToSingle(cellValue);
                                    break;
                                case "bool":
                                    rowDict[columnName] = cellValue?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
                                    break;
                                case "string":
                                default:
                                    rowDict[columnName] = cellValue is double doubleValue
                                        ? doubleValue.ToString("0")
                                        : cellValue?.ToString()?.Trim();
                                    break;
                            }
                        }
                        result.Add(rowDict);
                    }
                }
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
        }
    }

    private static Dictionary<string, string> ExtractFieldsFromExcel(string excelFilePath)
    {
        using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var fields = new Dictionary<string, string>();
                var dataSet = reader.AsDataSet();

                if (dataSet.Tables.Count > 0)
                {
                    var table = dataSet.Tables[0];

                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        string columnName = table.Rows[0][col].ToString();
                        string columnType = table.Rows[1][col].ToString().ToLower();

                        switch (columnType)
                        {
                            case "int":
                                fields[columnName] = "int";
                                break;
                            case "float":
                                fields[columnName] = "float";
                                break;
                            case "bool":
                                fields[columnName] = "bool";
                                break;
                            case "string":
                            default:
                                fields[columnName] = "string";
                                break;
                        }
                    }
                }
                return fields;
            }
        }
    }

    private static string GenerateCSharpClass(string className, Dictionary<string, string> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("public class " + className);
        sb.AppendLine("{");

        foreach (var field in fields)
        {
            sb.AppendLine($"    public {field.Value} {field.Key} {{ get; set; }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
