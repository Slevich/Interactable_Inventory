using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class HorizontalAlignment : MonoBehaviour
{
    #region Fields
    [Header("Settings.")]
    [SerializeField, Range(0f, 100f)] 
    private float _spacing = 1f;
    
    [Header("References.")]
    [SerializeField] 
    private List<HorizontalAlignmentTarget> _targets = new();
    [SerializeField] 
    private HorizontalAlignmentTarget _previewTarget;
    #endregion

    #region Properties
    public List<HorizontalAlignmentTarget> Targets => _targets;
    public HorizontalAlignmentTarget PreviewTarget => _previewTarget;
    #endregion
    
    #region Methods
    private void Awake()
    {
        RemovePreviewTargets();
    }

    private void OnValidate()
    {
        Align();
    }

    public void Align()
    {
        if (_targets == null || _targets.Count == 0)
            return;

        float totalWidth = (_targets.Count - 1) * _spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < _targets.Count; i++)
        {
            HorizontalAlignmentTarget target = _targets[i];
            if (target == null)
                continue;

            Vector3 localPosition = new Vector3(startX + i * _spacing, 0f, 0f);
            target.transform.localPosition = localPosition;
        }
    }

    public void RegisterTarget(HorizontalAlignmentTarget target, int index = -1)
    {
        if (target == null || _targets.Contains(target))
            return;

        if(index == -1)
            _targets.Add(target);
        else
        {
            index = Mathf.Clamp(index, 0, _targets.Count - 1);
            _targets.Insert(index, target);
        }
        
        if(target.transform.parent != transform)
            target.transform.SetParent(transform);
        
        Align();
    }

    public void RegisterTargetFirst(HorizontalAlignmentTarget target)
    {
        int index = 0;
        RegisterTarget(target, index);
    }

    public void RegisterTargetLast(HorizontalAlignmentTarget target)
    {
        RegisterTarget(target);
    }
    
    public void UnregisterTarget(HorizontalAlignmentTarget target)
    {
        if (target == null || !_targets.Contains(target))
            return;

        _targets.Remove(target);
        
        if(target.transform.parent == transform)
            target.transform.SetParent(null);
        
        Align();
    }

    public void RemovePreviewTargets()
    {
        if (_targets == null)
            return;

        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            if (_targets[i] != null && _targets[i].IsPreview)
            {
                if (Application.isPlaying)
                    Destroy(_targets[i].gameObject);
                else
                    DestroyImmediate(_targets[i].gameObject);
                
                _targets.RemoveAt(i);
            }
        }
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(HorizontalAlignment))]
public class HorizontalAlignmentEditor : Editor
{
    private static readonly string spacingPropertyName = "_spacing";
    private static readonly string targetsPropertyName = "_targets";
    private static readonly string previewTargetPropertyName = "_previewTarget";
    
    private SerializedProperty _spacingProperty;
    private SerializedProperty _targetsProperty;
    private SerializedProperty _previewTargetProperty;
    

    private void OnEnable()
    {
        _spacingProperty = serializedObject.FindProperty(spacingPropertyName);
        _targetsProperty = serializedObject.FindProperty(targetsPropertyName);
        _previewTargetProperty = serializedObject.FindProperty(previewTargetPropertyName);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(_spacingProperty);
        EditorGUILayout.PropertyField(_targetsProperty);
        
        DrawPreviewControls();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPreviewControls()
    {
        Color defaultColor = GUI.backgroundColor;
        
        GUILayout.Space(20);
        GUIStyle boxStyle = new(EditorStyles.helpBox);
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Alignment Preview", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_previewTargetProperty, new GUIContent("Preview Target"));

        GUILayout.Space(5);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Preview Target...", GUILayout.Height(30)))
        {
            AddPreviewTarget();
        }
        GUI.backgroundColor = defaultColor;
        
        GUILayout.Space(5);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove preview targets!", GUILayout.Height(30)))
        {
            ((HorizontalAlignment)target).RemovePreviewTargets();
        }
        GUI.backgroundColor = defaultColor;

        EditorGUILayout.EndVertical();
    }

    private void AddPreviewTarget()
    {
        HorizontalAlignment alignment = (HorizontalAlignment)target;
        HorizontalAlignmentTarget previewTarget = alignment.PreviewTarget;

        if (previewTarget == null)
        {
            Debug.LogWarning("⚠️ No preview target assigned!");
            return;
        }

        Debug.Log("Woop");
        GameObject original = previewTarget.gameObject;
        Transform parent = previewTarget.transform.parent;
        
        GameObject clone = MonoBehaviour.Instantiate(original, parent);
        clone.name = original.name + "_Preview";
        clone.SetActive(true);

        HorizontalAlignmentTarget newTarget = clone.GetComponent<HorizontalAlignmentTarget>();
        if (newTarget == null)
        {
            newTarget = clone.AddComponent<HorizontalAlignmentTarget>();
        }

        newTarget.IsPreview = true;
        alignment.RegisterTarget(newTarget);
        alignment.Align();
        
        EditorUtility.SetDirty(alignment);
        EditorSceneManager.MarkSceneDirty(alignment.gameObject.scene);
    }
}
#endif