using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationEventCleaner : Editor
{
    [MenuItem("Tools/Clean Empty Animation Events")]
    public static void Clean()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip == null) continue;

            // AnimationEventUtility는 내부 클래스이므로 정식 API인 AnimationUtility 사용
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
            List<AnimationEvent> filteredEvents = new List<AnimationEvent>();
            bool changed = false;

            foreach (var ev in events)
            {
                if (string.IsNullOrEmpty(ev.functionName))
                {
                    changed = true;
                    continue; // 함수 이름이 비어있으면 리스트에서 제외
                }
                filteredEvents.Add(ev);
            }

            if (changed)
            {
                AnimationUtility.SetAnimationEvents(clip, filteredEvents.ToArray());
                EditorUtility.SetDirty(clip);
                fixedCount++;
                Debug.Log($"<color=green>[Cleaner] {path} 에서 빈 이벤트를 제거했습니다.</color>");
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("청소 완료", $"총 {fixedCount}개의 애니메이션 파일에서 빈 이벤트를 제거했습니다.", "확인");
    }
}
