using UnityEngine;

/// <summary>
/// SpriteRenderer의 스프라이트가 바뀔 때마다 PolygonCollider2D를
/// 해당 스프라이트의 Physics Shape으로 자동 동기화합니다.
///
/// 사용 방법:
///   1. Sprite Editor > Physics Shape 탭에서 각 프레임의 충돌 모양을 그려두세요.
///   2. 이 컴포넌트를 SpriteRenderer와 PolygonCollider2D가 있는 오브젝트에 추가하세요.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SpriteColliderSync : MonoBehaviour
{
    private SpriteRenderer _sr;
    private PolygonCollider2D _col;
    private Sprite _lastSprite;

    private void Awake()
    {
        _sr  = GetComponent<SpriteRenderer>();
        _col = GetComponent<PolygonCollider2D>();
    }

    private void LateUpdate()
    {
        if (_sr.sprite == _lastSprite) return;
        _lastSprite = _sr.sprite;
        UpdateCollider(_sr.sprite);
    }

    private void UpdateCollider(Sprite sprite)
    {
        if (sprite == null) return;

        int pathCount = sprite.GetPhysicsShapeCount();
        if (pathCount == 0) return;

        _col.pathCount = pathCount;
        var path = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < pathCount; i++)
        {
            sprite.GetPhysicsShape(i, path);
            _col.SetPath(i, path);
        }
    }
}
