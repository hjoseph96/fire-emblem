/*
 * Authors:  Jesse Graupmann & Tim Graupmann
 * Just Good Design, @copyright 2012-2013  All rights reserved.
 *
*/
 
using UnityEditor;
using UnityEngine;  

public static class GoodEditorPrefs
{
    [MenuItem("Window/Just Good Design/Good Prefs [Editor]/Delete All Keys", priority = 510)]
    public static void DelteAllPlayerPrefs()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        EditorPrefs.DeleteAll();
    }

    // bool
    public static void SetBool(string key, bool value)
    {
        EditorPrefs.SetBool(key, value);
    }
    public static bool GetBool(string key, bool defaultValue)
    {
        return EditorPrefs.GetBool(key, defaultValue);
    }

    // float
    public static void SetFloat(string key, float value)
    {
        EditorPrefs.SetFloat(key, value);
    }
    public static float GetFloat(string key, float defaultValue)
    {
        return EditorPrefs.GetFloat(key, defaultValue);
    }

    // int
    public static void SetInt(string key, int value)
    {
        EditorPrefs.SetInt(key, value);
    }
    public static int GetInt(string key, int defaultValue)
    {
        return EditorPrefs.GetInt(key, defaultValue);
    }

    // String
    public static void SetString(string key, string str)
    {
        EditorPrefs.SetString(key, str);
    }
    public static string GetString(string key, string defaultValue)
    {
        return EditorPrefs.GetString(key, defaultValue);
    }

    // Rect
    public static void SetRect(string key, Rect rect)
    {
        EditorPrefs.SetFloat(key + "_Rect.x", rect.x);
        EditorPrefs.SetFloat(key + "_Rect.y", rect.y);
        EditorPrefs.SetFloat(key + "_Rect.width", rect.width);
        EditorPrefs.SetFloat(key + "_Rect.height", rect.height);
    }
    public static Rect GetRect(string key, Rect defaultValue)
    {
        Rect rect = new Rect();

        rect.x = EditorPrefs.GetFloat(key + "_Rect.x", defaultValue.x);
        rect.y = EditorPrefs.GetFloat(key + "_Rect.y", defaultValue.y);
        rect.width = EditorPrefs.GetFloat(key + "_Rect.width", defaultValue.width);
        rect.height = EditorPrefs.GetFloat(key + "_Rect.height", defaultValue.height);

        return rect;
    }  

    // Vector 2
    public static void SetVector2(string key, Vector2 vect)
    {
        EditorPrefs.SetFloat(key + "_Vector2.x", vect.x);
        EditorPrefs.SetFloat(key + "_Vector2.y", vect.y);
    }
    public static Vector2 GetVector2(string key, Vector2 defaultValue)
    {
        Vector2 vect = new Vector2();

        vect.x = EditorPrefs.GetFloat(key + "_Vector2.x", defaultValue.x);
        vect.y = EditorPrefs.GetFloat(key + "_Vector2.y", defaultValue.y);
 
        return vect;
    }

    // Vector 3
    public static void SetVector3(string key, Vector3 vect)
    {
        EditorPrefs.SetFloat(key + "_Vector3.x", vect.x);
        EditorPrefs.SetFloat(key + "_Vector3.y", vect.y);
        EditorPrefs.SetFloat(key + "_Vector3.z", vect.z);
    }
    public static Vector3 GetVector3(string key, Vector3 defaultValue)
    {
        Vector3 vect = new Vector3();

        vect.x = EditorPrefs.GetFloat(key + "_Vector3.x", defaultValue.x);
        vect.y = EditorPrefs.GetFloat(key + "_Vector3.y", defaultValue.y);
        vect.z = EditorPrefs.GetFloat(key + "_Vector3.z", defaultValue.z);

        return vect;
    }

    // Color
    public static void SetColor(string key, Color color)
    {
        EditorPrefs.SetFloat(key + "_Color.a", color.a);
        EditorPrefs.SetFloat(key + "_Color.r", color.r);
        EditorPrefs.SetFloat(key + "_Color.g", color.g);
        EditorPrefs.SetFloat(key + "_Color.b", color.b);
    }
    public static Color GetColor(string key, Color defaultValue)
    { 
        var color = new Color();

        color.a = EditorPrefs.GetFloat(key + "_Color.a", defaultValue.a);
        color.r = EditorPrefs.GetFloat(key + "_Color.r", defaultValue.r);
        color.g = EditorPrefs.GetFloat(key + "_Color.g", defaultValue.g);
        color.b = EditorPrefs.GetFloat(key + "_Color.b", defaultValue.b);

        return color;
    }
}