using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AutoHookAttribute))]
public class AutoHookPropertyDrawer : PropertyDrawer
{
    private AutoHookAttribute _autoHookAttribute;
    private AutoHookAttribute autoHookAttribute
    {
        get
        {
            if (_autoHookAttribute == null)
                _autoHookAttribute = (AutoHookAttribute)attribute;
            return _autoHookAttribute;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (autoHookAttribute.HideWhenFound && property.objectReferenceValue != null)
        {
            return 0;
        }

        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Hide the property if HideWhenFound is true and the component is already found
        if (autoHookAttribute.HideWhenFound && property.objectReferenceValue != null)
        {
            return;
        }

        if (property.objectReferenceValue == null)
        {
            Component component = FindComponent(property);
            if (component != null)
            {
                property.objectReferenceValue = component;
            }
        }
        
        // Disable the property if ReadOnlyWhenFound is true and the component is already found
        EditorGUI.BeginDisabledGroup(autoHookAttribute.ReadOnlyWhenFound && property.objectReferenceValue != null);
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.EndDisabledGroup();
    }

    private Component FindComponent(SerializedProperty property)
    {
        SerializedObject root = property.serializedObject;
        if (!(root.targetObject is Component parent))
        {
            return null;
        }

        Type type = fieldInfo.FieldType;
        
        switch (autoHookAttribute.SearchArea)
        {
            case AutoHookSearchArea.Parent:
                return parent.GetComponentInParent(type);
            case AutoHookSearchArea.Root:
                return parent.transform.root.GetComponent(type);
            case AutoHookSearchArea.Scene:
                return UnityEngine.Object.FindFirstObjectByType(type) as Component;
            case AutoHookSearchArea.FirstChild:
                if (parent.transform.childCount > 0)
                    return parent.transform.GetChild(0).GetComponent(type);
                return null;
            case AutoHookSearchArea.Children:
                return parent.GetComponentInChildren(type, true);
            case AutoHookSearchArea.DirectChildrenOnly:
                return FindComponentInDirectChildren(parent.transform, type);
            case AutoHookSearchArea.AllChildrenOnly:
                return parent.GetComponentInChildren(type, true);
            default:
                return parent.GetComponent(type);
        }
    }

    private Component FindComponentInDirectChildren(Transform parent, Type type)
    {
        int childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Component component = child.GetComponent(type);
            if (component != null)
            {
                return component;
            }
        }
        return null;
    }
}