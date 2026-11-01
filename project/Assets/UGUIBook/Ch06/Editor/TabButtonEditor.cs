// 继承 Selectable/Button 的组件，Inspector 默认被 SelectableEditor 接管，子类新增的字段不会显示。
// 写一个继承 SelectableEditor 的编辑器，把自己的字段补画出来。
using UnityEditor;
using UnityEditor.UI;

namespace UGUIBook.Ch06
{
    //#region main
    [CustomEditor(typeof(TabButton), true)]
    [CanEditMultipleObjects]
    public class TabButtonEditor : SelectableEditor
    {
        static readonly string[] Fields =
        {
            "m_Icon", "m_Label", "m_Underline", "m_Hover", "m_FocusRing", "m_OnColor", "m_OffColor",
        };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();                       // 先画 Selectable 自带的部分（Interactable、Transition、Navigation）
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tab Button", EditorStyles.boldLabel);
            foreach (var f in Fields) EditorGUILayout.PropertyField(serializedObject.FindProperty(f));
            serializedObject.ApplyModifiedProperties();
        }
    }
    //#endregion
}
